using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        double E = 200E9;                 // 200 GPa
        double W = 0.2;                   // 20 cm
        double H = 0.4;                   // 40 cm
        double A = W*H;                   // area
        double I = W*Math.Pow(H,3)/12;    // inertia

        double length = 10;              // beam length
        double ratio = 0.5;             // roller position ratio
        double zF_s0 = ratio * length;   // roller position
        
        double zF_s1 = length - zF_s0;          // force position
        double force = 8E2;
        double moment = 5E2;

        double[] absolutePositions = new double[] {0, zF_s0, length}; // absolute position for the mathematical segment splitting
    
        int numtratti = 2;
        int dimMat = 6*numtratti;

        double[,] Matrix = new double[dimMat, dimMat];

        NonExtSupportType non_ext_support = NonExtSupportType.Hinge;
        ExtSupportType ext_support = ExtSupportType.Fixed;
        LoadType load = LoadType.Moment;


        List<List<double>> abs_conditions = non_ext_support.get_absolute_beam_equations(zF_s0, E, I, A);
        List<List<double>> rel_conditions = non_ext_support.get_relative_beam_equations(zF_s0, E, I, A);
        List<List<double>> ext_conditions = ext_support.get_extreme_beam_equations(0,E,I,A);
        List<List<double>> load_conditions = load.get_extreme_load_equations(length,E,I,A);

        double[] vector = load.get_vector(force/(E*I));

        int offset = 0;
        //Console.WriteLine(offset);


        for(int i=0; i<ext_conditions.Count; i++){
            for(int j=0; j<ext_conditions[i].Count;j++){
                Matrix[i, j] = ext_conditions[i][j]; 
            } 
        }
        offset += ext_conditions.Count;

        for(int i=0; i<load_conditions.Count; i++){
            for(int j=0; j<load_conditions[i].Count;j++){
                Matrix[i+offset, j+6] = load_conditions[i][j]; 
            }
        }
         offset += load_conditions.Count;

        //Console.WriteLine(offset);

        // get and process (fill Matrix) absolute equations
        for(int i=0; i<abs_conditions.Count; i++){
            for(int j=0; j<abs_conditions[i].Count;j++){
                Matrix[i+offset, j] = abs_conditions[i][j]; 
            }
        }
        offset += abs_conditions.Count;

        //Console.WriteLine(offset);

        //get and process (fill Matrix on both segments) relative equations
        
        for(int i=0; i<rel_conditions.Count; i++){
            for(int j=0; j<rel_conditions[i].Count;j++){
                Matrix[i+offset, j] = (-1)*rel_conditions[i][j];
                Matrix[i+offset, j+6] = rel_conditions[i][j]; 
            }
        }
        offset += rel_conditions.Count;

        //Console.WriteLine(offset);
    

        Console.WriteLine();
        Console.WriteLine();
        
        for(int i=0; i<Matrix.GetLength(0); i++){
            for(int j=0; j<Matrix.GetLength(1); j++){
                Console.Write($"{Matrix[i, j],10:E2} "); 
            }
            Console.WriteLine();
        }

        double[] solution = MatrixSolver.Solve(Matrix, vector);      // solve Ax = b
        

        Console.WriteLine("Solution:");
            foreach (var value in solution)
            {
                Console.Write($"{value:E2}  ");
            }
            Console.WriteLine();
            Console.WriteLine();

        // plot the beam
        List<PlotHelper.ResultsGroup> groups = PlotHelper.GetSolutionGroups(solution, absolutePositions, E, I, A); 
        PlotHelper.PlotBeamFunction(groups);

    }
    
}



public enum BeamEquationType
{
    V, VI, VII, VIII, W, WI
}

public static class BeamEquationTypeExtensions {
    public static double[] get_equation(this BeamEquationType type, double z_in, double E, double I, double A) {
        double[] list =  new double[6];

        switch (type) {
            case BeamEquationType.V: {
                list[0] = (1/(6*E*I)) * Math.Pow(z_in, 3);    //coeff di c1 
                list[1] = (1/(2*E*I)) * Math.Pow(z_in, 2);    //coeff di c2
                list[2] = (1/(E*I)) * z_in;    //coeff di c3
                list[3] = (1/(E*I)) ;    //coeff di c4
                list[4] = 0;    //coeff di c5
                list[5] = 0;    //coeff di c6
                break;
            }
            case BeamEquationType.VI: {
                list[0] = (1/(2*E*I)) * Math.Pow(z_in, 2);
                list[1] = (1/(E*I)) * z_in;
                list[2] = (1/(E*I));
                list[3] = 0;
                list[4] = 0;
                list[5] = 0;
                break;
            }
            case BeamEquationType.VII: {
                list[0] = (1/(E*I)) * z_in;
                list[1] = (1/(E*I));
                list[2] = 0;
                list[3] = 0;
                list[4] = 0;
                list[5] = 0;
                break;
            }
            case BeamEquationType.VIII: {
                list[0] = (1/(E*I));
                list[1] = 0;
                list[2] = 0;
                list[3] = 0;
                list[4] = 0;
                list[5] = 0;
                break;
            }
            case BeamEquationType.W: {
                list[0] = 0;
                list[1] = 0;
                list[2] = 0;
                list[3] = 0;
                list[4] = (1/(E*A)) * z_in;
                list[5] = (1/(E*A));
                break;
            }
            case BeamEquationType.WI: {
                list[0] = 0;
                list[1] = 0;
                list[2] = 0;
                list[3] = 0;
                list[4] = (1/(E*A));
                list[5] = 0;
                break;
            }
        }

        return list;
    }
}


public enum NonExtSupportType
{
    // Simple constraints
    Roller, RotationLock,
    // Double constraints
    Hinge, RollerWithRotationLock
}

public enum ExtSupportType
{
    // Double constraints
    Hinge, RollerWithRotationLock,
    // Triple constraint
    Fixed
}

public enum LoadType
{
    Force,

    Moment
}

public static class SupportTypeExtensions
{
    public static List<List<double>> get_absolute_beam_equations(this NonExtSupportType support_type, double z_in, double E, double I, double A) {
        List<List<double>> absolute_equations = new List<List<double>>();

        foreach (BeamEquationType eq_type in support_type.get_absolute_beam_equation_types()) {
            absolute_equations.Add(eq_type.get_equation(z_in, E, I, A).ToList());
        }

        return absolute_equations;
    }

    public static List<List<double>> get_relative_beam_equations(this NonExtSupportType support_type, double z_in, double E, double I, double A) {
        List<List<double>> relative_equations = new List<List<double>>();

        foreach (BeamEquationType eq_type in support_type.get_relative_beam_equation_types()) {
            relative_equations.Add(eq_type.get_equation(z_in, E, I, A).ToList());
        }

        return relative_equations;
    }

    public static List<List<double>> get_extreme_beam_equations(this ExtSupportType support_type, double z_in, double E, double I, double A) {
        List<List<double>> extreme_equations = new List<List<double>>();

        foreach (BeamEquationType eq_type in support_type.get_extreme_beam_equation_types()) {
            extreme_equations.Add(eq_type.get_equation(z_in, E, I, A).ToList());
        }

        return extreme_equations;
    }

public static List<List<double>> get_extreme_load_equations(this LoadType load_type, double z_in, double E, double I, double A) {
        List<List<double>> load_equations = new List<List<double>>();

        foreach (BeamEquationType eq_type in load_type.get_extreme_load_equation_types()) {
            load_equations.Add(eq_type.get_equation(z_in, E, I, A).ToList());
        }

        return load_equations;
    }

public static double[] get_vector(this LoadType load_type, double load) {

    return load_type switch{
        
        LoadType.Force => [0, 0, 0, 0 ,-load ,0, 0, 0, 0, 0, 0, 0],
        LoadType.Moment => [0, 0, 0, -load, 0 ,0, 0, 0, 0, 0, 0, 0],
        _ =>[0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0]
    };
}
    

    public static List<BeamEquationType> get_extreme_beam_equation_types(this ExtSupportType support_type){
        return support_type switch
        {
            ExtSupportType.Hinge => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.W,
                BeamEquationType.VII
                },
            ExtSupportType.Fixed => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.W
                },
            ExtSupportType.RollerWithRotationLock => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.WI
                },
            _ => new List<BeamEquationType>()
            
        };        
    }

     public static List<BeamEquationType> get_extreme_load_equation_types(this LoadType load_type){
        return load_type switch
        {
            LoadType.Force => new List<BeamEquationType> {
                BeamEquationType.VII,
                BeamEquationType.VIII,
                BeamEquationType.WI
                },
            LoadType.Moment => new List<BeamEquationType> {
                BeamEquationType.VII,
                BeamEquationType.VIII,
                BeamEquationType.WI
                },
            _ => new List<BeamEquationType>()
            
        };        
    }

    public static List<BeamEquationType> get_absolute_beam_equation_types(this NonExtSupportType support_type)
    {
        return support_type switch
        {
            NonExtSupportType.Roller => new List<BeamEquationType> { BeamEquationType.V },
            NonExtSupportType.RotationLock => new List<BeamEquationType> { BeamEquationType.VI },
            NonExtSupportType.Hinge => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.W
                },
            NonExtSupportType.RollerWithRotationLock => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI
                },
            _ => new List<BeamEquationType>()
        };
    }

    public static List<BeamEquationType> get_relative_beam_equation_types(this NonExtSupportType support_type)
    {
        return support_type switch
        {
            NonExtSupportType.Roller => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.VII,
                BeamEquationType.W,
                BeamEquationType.WI
                },
            NonExtSupportType.RotationLock => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.VIII,
                BeamEquationType.W,
                BeamEquationType.WI
                },
            NonExtSupportType.Hinge => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.VII,
                BeamEquationType.W,
                },
            NonExtSupportType.RollerWithRotationLock => new List<BeamEquationType> {
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.W,
                BeamEquationType.WI
                },
            _ => new List<BeamEquationType>()
        };
    }
}
