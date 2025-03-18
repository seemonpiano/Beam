using ScottPlot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#nullable enable
public class PlotHelper
{

    // vertical displacement function
    public static double v(double x, ResultsGroup group)
    {
        double c1 = group.c1;
        double c2 = group.c2;
        double c3 = group.c3;
        double c4 = group.c4;
        double E = group.E;
        double I = group.I;
        double qIIIInt = group.qIIIInt;
        // Example computation using E and I
        return (1/(E*I)) * (qIIIInt + c1*Math.Pow(x, 3) / 6 + c2*Math.Pow(x, 2) / 2 + c3*x +c4);
    }

    // horizontal displacement function
    public static double w(double x, ResultsGroup group)
    {

        double c5 = group.c5;
        double c6 = group.c6;
        double E = group.E;
        double A = group.A;
        double qIIIInt = group.qIIIInt;
        double pIInt = group.pIInt;
        // Example computation using E and I
        return -(1/(E*A)) * (pIInt + c5*x + c6);
    }

    // bending moment function
    public static double M(double x, ResultsGroup group)
    {
        double c1 = group.c1;
        double c2 = group.c2;
        double qIInt = group.qIInt;
        // Example computation using E and I
        return -(c1*x+c2); // x
    }

    // shear force function
    public static double T(double x, ResultsGroup group)
    {
        double c1 = group.c1;
        double qInt = group.qInt;
        
        return -  (qInt + c1);
    }

    // axial force function
    public static double N(double x, ResultsGroup group)
    {
        double c5 = group.c5;
        double pInt = group.pInt;
        
        return -  (pInt + c5);
    }
    public static void PlotBeamFunction(List<ResultsGroup> groups)
    {
        
        ScottPlot.Multiplot multiplot = new(); // start a new multiplot
        double dx = 0.001; // resolution of the plot
        
        ScottPlot.Plot displacement  = PlotDisplacement( groups, dx); // prepare the displacement plot
        ScottPlot.Plot N  = PlotN( groups, dx); // prepare the axial force plot
        ScottPlot.Plot T  = PlotT( groups, dx); // prepare the shear force plot
        ScottPlot.Plot M  = PlotM( groups, dx); // prepare the bending moment plot
        
        multiplot.AddPlot(displacement); // add the displacement plot to the multiplot
        multiplot.AddPlot(N); // add the axial force plot to the multiplot
        multiplot.AddPlot(T); // add the shear force plot to the multiplot
        multiplot.AddPlot(M); // add the bending moment plot to the multiplot

        // string imagePath = "plot.png"; // path to save the image
        var baseDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
        if (baseDirectory == null || baseDirectory.Parent == null || baseDirectory.Parent.Parent == null)
        {
            throw new InvalidOperationException("Unable to determine the project root directory.");
        }
        string projectRoot = baseDirectory.Parent.Parent.FullName;
        string imagePath = Path.Combine(projectRoot, "plot.png");
        multiplot.SavePng(imagePath, 600, 1200); // save the image
        OpenImage(imagePath); // open the image
    }

    

    public static void OpenImage(string imagePath){
        Console.WriteLine();
        Console.WriteLine($"Plot save in: {imagePath}");
        Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFullPath(imagePath),
            UseShellExecute = true
        });
    }
    
    public static ScottPlot.Plot PlotDisplacement(List<ResultsGroup> groups, double dx)
    {   
        // function to plot the displacement make use of the GeneratePlot function
        // the function is called with the specific logic for the displacement
        // it also add some customization to the plot
        return GeneratePlot(
            groups,
            dx,
            computeY: (x, group) => v(x, group), // Specific logic for M
            title : "v",
            yLabel : "v(z)",
            lineColor : Colors.Navy,
            InvertedYAxisQ : true,
            FillQ : false,
            computeX: (x, group) => w(x, group)
            );

        }

    public static ScottPlot.Plot PlotM(List<ResultsGroup> groups, double dx)
    {
        // function to plot the bending moment make use of the GeneratePlot function
        // the function is called with the specific logic for the bending moment
        // it also add some customization to the plot
        return GeneratePlot(
            groups,
            dx,
            computeY: (x, group) => M(x, group), // Specific logic for M
            title : "M",
            yLabel : "M(z)",
            lineColor : Colors.DarkGreen,
            FillQ : true,
            InvertedYAxisQ : true);
    }


    public static ScottPlot.Plot PlotT(List<ResultsGroup> groups, double dx)
    {
        // function to plot the shear force make use of the GeneratePlot function
        // the function is called with the specific logic for the shear force
        // it also add some customization to the plot
        return GeneratePlot(
            groups,
            dx,
            computeY: (x, group) => T(x, group), // Specific logic for T
            title : "T",
            yLabel : "T(z)",
            lineColor : Colors.DarkGreen,
            FillQ: true,
            InvertedYAxisQ : false);
    }

    public static ScottPlot.Plot PlotN(List<ResultsGroup> groups, double dx)
    {
        // function to plot the axial force make use of the GeneratePlot function
        // the function is called with the specific logic for the axial force
        // it also add some customization to the plot
        return GeneratePlot(
            groups,
            dx,
            computeY: (x, group) => N(x, group), // Specific logic for M
            title : "N",
            yLabel : "N(z)",
            lineColor : Colors.DarkGreen,
            FillQ: true,
            InvertedYAxisQ : false);
    }

    // Function to generate a plot
    // it takes as input the groups of results, the resolution of the plot, the logic to compute the Y values
    public static ScottPlot.Plot GeneratePlot(
        List<ResultsGroup> groups,
        double dx,
        Func<double, ResultsGroup, double> computeY, // Delegate for Y computation
        string title,
        string yLabel,
        Color lineColor,
        bool InvertedYAxisQ,
        bool FillQ,
        Func<double, ResultsGroup, double>? computeX = null // Optional delegate for X computation
        )
    
    {
        ScottPlot.Plot plot = new();
        // initialize the min and max values for the axes
        double minX = 0;
        double maxX = 0;
        double maxY = 0;
        double minY = 0;
        double max_xAxis = 0;
        double max_yAxis = 0;
        double min_yAxis = 0;

        foreach (var group in groups)
        {
            maxX = group.ZEnd - group.ZStart; // compute the length of the segment
            int nPoints = (int)((maxX - minX) / dx) + 1; // compute the number of points
            double[] dataX = new double[nPoints]; // initialize the x values
            double[] dataY = new double[nPoints]; // initialize the y values
            double[] dataY_Zero = new double[nPoints]; // initialize the y zero-values

            for (int i = 0; i < nPoints; i++)
            {
                dataX[i] = i * dx; // x values
                dataX[i] += group.ZStart; // translate x values with respect to each segment
                dataY[i] = computeY(dataX[i], group); // use the provided computation logic
                
                // apply computeX if provided
                if (computeX != null)
                {
                    dataX[i] += computeX(dataX[i], group); // Adjust x values based on computeX
                }
                dataY_Zero[i] = 0;
            }
            // the following code set max and min values for the axes
            maxY = dataY.Max();
            minY = dataY.Min();
            if (minY >= 0 - 0.1*maxY)
            {
                minY = -0.1 * maxY;
            }
            if (maxY <= 0 + 0.1*Math.Abs(maxY))
            {
                maxY = 0.1*Math.Abs(maxY);
            }

            // add scatter plot with specified line style and colors
            plot.Add.Scatter(dataX, dataY_Zero, Colors.Gray);
            var scatterPlot = plot.Add.Scatter(dataX, dataY, lineColor);
            if (FillQ){
                scatterPlot.FillY = true;
                scatterPlot.FillYColor = scatterPlot.Color.WithAlpha(.2);
            }

            max_xAxis = Math.Max(max_xAxis, group.ZEnd);
            max_yAxis = Math.Max(max_yAxis, maxY);
            min_yAxis = Math.Min(min_yAxis, minY);
        }
        // increase the max and min values for the axes to have a better visualization
        min_yAxis = 1.15*min_yAxis;
        max_yAxis = 1.15*max_yAxis;
        
        // customize axes
        plot.Axes.Right.FrameLineStyle.Width = 0;
        plot.Axes.Top.FrameLineStyle.Width = 0;
        if (InvertedYAxisQ) {
            plot.Axes.SetLimits(minX, max_xAxis, max_yAxis, min_yAxis);
        }
        else{
            plot.Axes.SetLimits(minX, max_xAxis, min_yAxis, max_yAxis);
        }
        
        plot.Axes.SetupMultiplierNotation(plot.Axes.Left);

        // add titles and labels
        plot.Title(title);
        plot.XLabel("z");
        plot.YLabel(yLabel);

        return plot;
    }
    
    

    // class to store the results of the solution
    // it allows to group each segment by its 6 coefficients
    // and the initial and final position
    public class ResultsGroup
    {
        public double[] Coefficients { get; set; } // The list of coefficients for this group
        public double ZStart { get; set; }
        public double ZEnd { get; set; }
        public double E { get; set; }
        public double I { get; set; }
        public double A { get; set; }
        public double qIIIInt { get; set; }
        public double qIIInt { get; set; }
        public double pIInt { get; set; }
        public double qIInt { get; set; }
        public double qInt { get; set; }
        public double pInt { get; set; }
        public double c1 { get; set; }
        public double c2 { get; set; }
        public double c3 { get; set; }
        public double c4 { get; set; }
        public double c5 { get; set; }
        public double c6 { get; set; }

        public ResultsGroup(double[] coefficients, double zStart, double zEnd, double Ein, double Iin, double Ain)
        {
            Coefficients = coefficients;
            c1 = Coefficients[0];
            c2 = Coefficients[1];
            c3 = Coefficients[2];
            c4 = Coefficients[3];
            c5 = Coefficients[4];
            c6 = Coefficients[5];
            ZStart = zStart;
            ZEnd = zEnd;
            E = Ein;
            I = Iin;
            A = Ain;
            qInt = 0;
            qIInt= 0;
            qIIInt = 0;
            qIIIInt= 0;
            pInt= 0;
            pIInt= 0;
        }
    }

    // function to split the global solution in groups
    // each group is a segment of the beam
    public static List<ResultsGroup> GetSolutionGroups(double[] solution, double[] z_list, double E, double I, double A)
    {
        if (solution == null || solution.Length == 0)
            throw new ArgumentException("Segments cannot be null or empty.", nameof(solution));
       
        List<ResultsGroup> groups = new List<ResultsGroup>();
        int groupSize = 6;
        int noGroupd = solution.Length / groupSize;
        // split each segment in a group
        for (int i = 0; i < noGroupd; i++)
        {
            // Extract coefficients for the current segment
            double[] coefficients = solution
                .Skip(i * groupSize)
                .Take(groupSize)
                .ToArray();

            // create the new group and add it to the list
            var group = new ResultsGroup(coefficients, z_list[i], z_list[i+1], E, I, A);
            groups.Add(group);
        }

        Console.WriteLine("Solution found!");
        Console.WriteLine(" ");

        int k = 0;
        foreach (var group in groups)
        {
            k++;
            Console.Write($"Segment {k} from z: {group.ZStart} to z: {group.ZEnd}, coefficients: ");
            for (int i = 0; i < group.Coefficients.Length; i++)
            {  
                if (i!=0)
                {
                    Console.Write(", ");
                }
                Console.Write($"c{i+1} = {group.Coefficients[i]:E2}");
            }
            Console.WriteLine("");
        }

        return groups;
    }

    
}



