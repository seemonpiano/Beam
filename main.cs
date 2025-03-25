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

        double ratio_F = 0.7;
        double zF = ratio_F*length; 
        
        double zF_s1 = length - zF_s0;          // force position
        double ext_load = 8E2;


        double[] absolutePositions = new double[] {0, zF_s0, zF, length}; // absolute position for the mathematical segment splitting

       /*  int numRows = 12;   // define rows of the matrix
        int numCols = 12;   // define columns of the matrix
     */
        
        int numtratti = 3;
        int dimMat = 6*numtratti;

        double[,] Matrix = new double[dimMat, dimMat];

        NonExtSupportType non_ext_support = NonExtSupportType.Hinge;
        ExtSupportType ext_support = ExtSupportType.Fixed;
        LoadType load = LoadType.Moment;
        
        //se voglio un estremo libero
        //LoadType end = LoadType.Force;

        ExtSupportType end_support = ExtSupportType.Fixed;


        List<List<double>> abs_conditions = non_ext_support.get_absolute_beam_equations(zF_s0, E, I, A);
        List<List<double>> rel_conditions = non_ext_support.get_relative_beam_equations(zF_s0, E, I, A);
        List<List<double>> ext_conditions = ext_support.get_extreme_beam_equations(0,E,I,A);
        List<List<double>> load_conditions = load.get_relative_load_equations(zF,E,I,A);
        List<List<double>> end_conditions = end_support.get_extreme_beam_equations(length,E,I,A);

        double[] vector = load.get_vector(ext_load/(E*I));

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
                Matrix[i+offset, j+6] = (-1)*load_conditions[i][j];
                Matrix[i+offset, j+12] = load_conditions[i][j]; 
            }
        }
         offset += load_conditions.Count;

        for(int i=0; i<end_conditions.Count; i++){
            for(int j=0; j<end_conditions[i].Count;j++){
                Matrix[i+offset, j+12] = end_conditions[i][j]; 
            }
        }
         offset += end_conditions.Count;
 

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


/*         double[] row1 = get_V("V",0,E, I);

        for (int i = 0; i < row1.GetLength(0); i++)
        {
            Console.Write($"{row1[i]} "); // Use scientific notation for clarity
        }  */
        
   /*      // v(0) = 0
        double[] row1 = { 0, 0, 0, 1/(E*I), 0, 0, 0, 0, 0, 0, 0, 0 };
        // w(0) = 0
        double[] row2 = {0, 0, 0, 0, 0, -1/(E*A), 0, 0, 0, 0, 0, 0 };
        // M(0) = 0
        double[] row3 = { 0, 1/(E*I), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        // v(zF_s0) = 0
        double[] row4 = { 1/(E*I)*Math.Pow(zF_s0,3)/6,  1/(E*I)*Math.Pow(zF_s0,2)/2, 1/(E*I)*zF_s0,  1/(E*I), 0, 0, 0, 0, 0, 0, 0, 0 };
        // Delta_w() = w_s1(0) - w_s0(zF_s0) = 0
        double[] row5 = { 0, 0, 0, 0, -(-1/(E*A)) *zF_s0,  -(-1/(E*A)), 0, 0, 0, 0, 0, -1/(E*A) };
        // Delta_phi() = phi_s1(0) - phi_s0(zF_s0) = 0 //erano sbagliati i segni
        double[] row6 = { (1/(E*I))*Math.Pow(zF_s0,2)/2, (1/(E*I))*zF_s0, (1/(E*I)), 0, 0, 0, 0, 0,  -1/(E*I), 0, 0, 0 };
        // Delta_v() = v_s1(0) - v_s0(zF_s0) = 0
        double[] row7 = {(1/(E*I))*Math.Pow(zF_s0,3)/6, (1/(E*I))*Math.Pow(zF_s0,2)/2, (1/(E*I))*zF_s0, (1/(E*I)), 0, 0, 0, 0, 0,  -1/(E*I), 0, 0 };
        // Delta_M() = M_s1(0) - M_s0(zF_s0) = 0
        double[] row8 = { -(-zF_s0), -(-1), 0, 0, 0, 0, 0, -1, 0, 0, 0, 0 };
        // Delta_N() = N_s1(0) - N_s0(zF_s0) = 0
        double[] row9 = { 0, 0, 0, 0, -(-1), 0, 0, 0, 0, 0, -1, 0 };
        // M(zF_s1) = 0
        double[] row10 = {0, 0, 0, 0, 0, 0, -zF_s1, -1, 0, 0, 0, 0 };
        // T(zF_s1) = F //NON CI VUOLE UN EI???
        double[] row11 = {0, 0, 0, 0, 0, 0, -(1/(E*I)), 0, 0, 0, 0, 0 };
        // N(zF_s1) = 0
        double[] row12 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0 };

        // Set rows collection for the matrix
        List<double[]> rows = new List<double[]>
        {
            row1, row2, row3, row4, row5, row6, row7, row8, row9, row10, row11, row12
        };

        // define the vector
        double[] vector = new double[]
        {
           0, 0, 0, 0, 0, 0, 0, 0, 0, 0, force, 0
        };

        
        // reshape the matrix from [][] to [,]
        double[,] matrix = new double[numRows, numCols];
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < rows[i].Length; j++)
            {
                matrix[i, j] = rows[i][j];
            }
        } */
        

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

    /* public static double[] get_V(string type, double z_in, double E, double I){
       
    } */
    
    /*
    -- old boundary_condition logic (absolute + relative all together --)
    public static List<List<double>> boundary_conditions(SupportType support_type, double z_in, double E, double I){
        List<List<double>> matrix = new List<List<double>>();

        switch (support_type) {
            case SupportType.Roller {
                double[] cond1 = get_V("V", z_in, E, I);    //v(z_in)=0
                double[] cond2 = get_V("V", z_in, E, I);    //delta_v(z_in)=0, devo vedere come aggiungere le condizioni relative
                double[] cond3 = get_V("VI", z_in, E, I);   //delta_phi(z_in)
                double[] cond4 = get_V("VII", z_in, E, I);   //delta_M
                double[] cond5 = get_V("WI", z_in, E, I);   //delta_N
                double[] cond6 = get_V("W", z_in, E, I);   //delta_W

                matrix.Add(cond1.ToList());
                matrix.Add(cond2.ToList());
                matrix.Add(cond3.ToList());
                matrix.Add(cond4.ToList());
                matrix.Add(cond5.ToList());
                matrix.Add(cond6.ToList());
                break;
            }
            default: new List(new List());
        }

        return matrix;
    }*/
    
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

    public static List<List<double>> get_relative_load_equations(this LoadType load_type, double z_in, double E, double I, double A) {
        List<List<double>> load_equations = new List<List<double>>();

        foreach (BeamEquationType eq_type in load_type.get_relative_load_equation_types()) {
            load_equations.Add(eq_type.get_equation(z_in, E, I, A).ToList());
        }

        return load_equations;
    }

public static double[] get_vector(this LoadType load_type, double load) {

    return load_type switch{
        
        LoadType.Force => [0, 0, 0, 0 ,load ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        LoadType.Moment => [0, 0, 0, load, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        _ =>[0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
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

    public static List<BeamEquationType> get_relative_load_equation_types(this LoadType load_type){
        return load_type switch
        {
            LoadType.Force => new List<BeamEquationType> {
                BeamEquationType.VII,
                BeamEquationType.VIII,
                BeamEquationType.WI,
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.W
                },
            LoadType.Moment => new List<BeamEquationType> {
                BeamEquationType.VII,
                BeamEquationType.VIII,
                BeamEquationType.WI,
                BeamEquationType.V,
                BeamEquationType.VI,
                BeamEquationType.W
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
