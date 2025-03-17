using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public static class MatrixSolver
{
    public static double[] Solve(double[,] matrix, double[] vector)
    {
/*         Console.WriteLine("--------------------------------------------- ");
        Console.WriteLine("             M2M beam solver");
        Console.WriteLine("--------------------------------------------- ");
        // Print the matrix
        Console.WriteLine("");
        Console.WriteLine("Matrix:");
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            Console.Write("        ");
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write($"{matrix[i, j],10:E2} "); // Use scientific notation for clarity
            }
            Console.WriteLine();
        }*/
        Console.WriteLine(""); 
        Console.WriteLine("Vector:");
        foreach (var value in vector)
        {
            Console.Write($"{value:E2}  ");
        }
        Console.WriteLine();
        Console.WriteLine();
        
        // Convert the input matrix and vector to Math.NET Numerics types
        var A = DenseMatrix.OfArray(matrix);
        var b = DenseVector.OfArray(vector);

        // Solve the linear system Ax = b
        var x = A.Solve(b);

        // Convert the result back to a double array and return
        return x.ToArray();
    }
}