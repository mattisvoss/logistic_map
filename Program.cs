using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


/*--------------------------------------------------------------------------------------------------------------------
|                                            Final Exam                   |
|                                            Course:     AM6007                                                    |
|                                            Lecturer:   Dr Kieran Mulchrone                                       |
|                                                                                                                  |
|                                            Name:       Mattis Voss                                               |
|                                            Student #:  121128764                                                 |
|                                            Date:       09/12/2021                                                |
--------------------------------------------------------------------------------------------------------------------*/


namespace FinalExam
{
    class Program
    {
        static void Main(string[] args)
        {
            //test of Vector and FunctionVector
            FunctionVector fv = new FunctionVector(3);
            fv[0] = x => x[0] + x[1] + x[2];
            fv[1] = x => x[0] + x[1] * x[2];
            fv[2] = x => x[0] * x[1] + x[2];
            Vector tmp = fv.Evaluate(new Vector(new double[] { 1, -1, 3 }));

            Console.WriteLine("tmp = {0}", tmp);

            //predator prey simulation
            //(1) single value
            PredPrey p = new PredPrey();
            p.Nsettle = 1000;
            Vector v0 = new Vector(new double[] { 0.83, 0.55 });
            p.Delta = 1.38;
            p.run1sim(v0, "outfile.csv");

            //(2) produce bifurcation plot data use default values
            p.runsimDrange(v0, 1.26, 1.4, 1000, "outfile1.csv");

            //(3) produce second bifurcation plot
            p.R = 3;
            p.B = 3.5;
            p.D = 2;
            v0 = new Vector(new double[] { 0.57, 0.37 });
            p.runsimDrange(v0, 0.5, 0.95, 1000, "outfile2.csv");

        }
    }
    // Delegate for a method that takes a double and two vectors as input
    public delegate double Function(Vector arg);


    /*=======================================================================================================================*/
    /*                                          PredPrey CLASS                                                               */
    /*=======================================================================================================================*/

    /// <summary>
    /// Generates data to produce a bifurcation diagram for discrete predator-prey model
    /// Public methods:
    /// run1sim(Vector v0, string filename): runs simulation for one value of delta
    /// runsimDrange(Vector v0, double deltafrom, double deltato, int numsteps, string filename):
    /// runs simulation for a range of values of delta
    /// </summary>
    class PredPrey
    {
        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Private data                                                             */
        /*-------------------------------------------------------------------------------------------------------------------*/

        private FunctionVector fv = new FunctionVector();
        private double delta = 0.5;
        private double r = 2;
        private double b = 0.6;
        private double d = 0.5;
        private int nsettle = 200;
        private int nreps = 200;
        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Properties                                                               */
        /*-------------------------------------------------------------------------------------------------------------------*/
        public double Delta
        {
            get => delta;
            set
            {
                if (value > 0) delta = value;
                else throw new Exception("delta must be greater than 0");
            }
        }
        public double R { get => r; set => r = value; }
        public double B { get => b; set => b = value; }
        public double D { get => d; set => d = value; }
        public int Nsettle
        {
            get => nsettle;
            set
            {
                if (value > 1) nsettle = value;
                else nsettle = 200;
            }
        }
        public int Nreps
        {
            get => nreps;
            set
            {
                if (value > 1) nreps = value;
                else nreps = 200;
            }
        }

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Constructors                                                             */
        /*-------------------------------------------------------------------------------------------------------------------*/

        public PredPrey()
        {
            fv[0] = v => (R * v[0] * (1 - v[0])) - (B * v[0] * v[1]);
            fv[1] = v => (-D + B * v[0]) * v[1];
        }

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Public methods                                                           */
        /*-------------------------------------------------------------------------------------------------------------------*/

        public void runsimDrange(Vector v0, double deltafrom, double deltato, int numsteps, string filename)
        {
            double range = deltato - deltafrom;
            // Run simulation for the given values of delta
            for (double del = deltafrom; del <= deltato; del += (range / (numsteps - 1)))
            {
                this.delta = del;
                run1sim(v0, filename);
            }
        }


        public void run1sim(Vector v0, string filename)
        {
            // Create storage as v0 gets passed by reference
            Vector vn = new Vector(v0.Length);
            vn = v0;
            // Create file to take output, "true" means existing information in file is not overwritten
            // between calls to run1sim by other methods
            StreamWriter output = new StreamWriter(@filename, true);

            // Check file exists
            FileInfo finfo = new FileInfo(@filename);
            try
            {
                if (!finfo.Exists) throw new Exception("File does not exist");
            }
            catch (Exception)
            {
                Console.WriteLine("{0}, cannot proceed");
            }

            // Iterate solution until it settles
            Vector vnp1 = new Vector(vn.Length);
            for (int n = 0; n < nsettle; n++)
            {
                vnp1 = vn + delta * (fv.Evaluate(vn));
                vn = vnp1;
            }

            // Iterate for nreps
            for (int n = 0; n < nreps; n++)
            {
                vnp1 = vn + delta * (fv.Evaluate(vn));
                output.WriteLine("{0}, {1}, {2}", delta, vn[0], vn[1]);

                vn = vnp1;
            }
            output.Close();
        }
    }


    /*=======================================================================================================================*/
    /*                                          FunctionVector CLASS                                                         */
    /*=======================================================================================================================*/

    /// <summary>
    /// Creates a vector of functions. The functions are assigned to a delegate of type 'Function'.
    /// Has a public method 'Evaluate()', which evaluates each function in the FunctionVector and returns a vector of doubles.
    /// Allows use of [] notation for indexing (public Function this[])
    /// </summary>
    /// 
    public class FunctionVector
    {
        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Private data                                                             */
        /*-------------------------------------------------------------------------------------------------------------------*/
        // Array of function elements
        private Function[] functionVector;

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Constructors                                                             */
        /*-------------------------------------------------------------------------------------------------------------------*/

        // Make FunctionVector size 2 with default lambda functions returning 0
        public FunctionVector()
        {
            this.functionVector = new Function[2];
            this.functionVector[0] = (x) => 0;
            this.functionVector[1] = (x) => 0;
        }
        // Size array of delegates, or makes it size 2 if argument less than one
        public FunctionVector(int size)
        {
            size = size >= 1 ? size : 2;
            Function[] funcs = new Function[size];
            this.functionVector = funcs;
        }
        // Size array of delegates according to the size of functions, and make 
        // array delegates reference the delegates in functions
        public FunctionVector(Function[] functions)
        {
            this.functionVector = functions;
        }

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Indexing and public methods                                              */
        /*-------------------------------------------------------------------------------------------------------------------*/

        // Indexer for function array
        public Function this[int index]
        {
            get { return this.functionVector[index]; }
            set { this.functionVector[index] = value; }
        }
        // Method to evaluate each function in the FunctionVector
        public Vector Evaluate(Vector values)
        {
            Vector tmp = new Vector(this.functionVector.Length);
            for (int i = 0; i < this.functionVector.Length; i++)
                tmp[i] = this.functionVector[i](values);
            return tmp;
        }
    }

    /*=======================================================================================================================*/
    /*                                           Vector CLASS                                                                */
    /*=======================================================================================================================*/
    /// <summary>
    /// Represents a vector. 
    /// Allows use of [] notation for indexing (public double this[])
    /// Default constructor creates a zero-Vector length 2.
    /// 
    /// Overloaded operators:
    /// + Binary Vector addition
    /// - Binary Vector subtraction
    /// * Vector multiplication (assume dot product is meant)
    /// * Post-multiply Vector by double
    /// 
    /// Public methods:
    /// ToString(): Returns vector as a row in curly brackets.
    /// SetSize(): Resizes vector and copies over as many elements as possible
    /// </summary>
    /// 
    public class Vector
    {
        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Private data and properties                                              */
        /*-------------------------------------------------------------------------------------------------------------------*/
        // Array of vector elements
        private double[] values;

        public double[] Values
        {
            get
            {
                return values;
            }
            set
            {
                if (value.Length == this.Length) values = value;
                else Console.WriteLine("Input array must have same length as existing vector");
            }
        }
        public int Length { get => values.Length; }

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Constructors                                                             */
        /*-------------------------------------------------------------------------------------------------------------------*/

        // Default constructor, creates vector length 2
        public Vector()
        {
            this.values = new double[2];
        }

        // Constructor takes an integer defining length as its argument. Defaults to 2 if 
        // incorrect input  
        public Vector(int length)
        {
            length = length >= 1 ? length : 2;
            double[] values = new double[length];
            this.values = values;
        }
        // Constructor takes an array of doubles
        public Vector(double[] values)
        {
            this.values = values;
        }

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                           Public methods                                                          */
        /*-------------------------------------------------------------------------------------------------------------------*/

        // Resize vector and copy over as many elements as possible
        public void SetSize(int size)
        {
            if (size <= 1)
                throw new ArgumentException("Vector must be length 1 or greater");
            double[] tmp = new double[size];
            int len = Math.Min(size, this.Length);
            for (int i = 0; i < len; i++)
            {
                tmp[i] = this.values[i];
            }
            this.values = tmp;
        }

        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Operator overloading                                                     */
        /*-------------------------------------------------------------------------------------------------------------------*/

        // Overload + operator for binary addition
        public static Vector operator +(Vector left, Vector right)
        {
            Vector sum = new Vector(left.Values.Length);
            if (left.Length != right.Length)
                throw new ArgumentException("Addition only possible for vectors of equal length");
            for (int i = 0; i < left.Length; i++)
            {
                sum[i] = left[i] + right[i];
            }
            return sum;
        }
        // Overload - operator for binary subtraction
        public static Vector operator -(Vector left, Vector right)
        {
            Vector diff = new Vector(left.Values.Length);
            if (left.Length != right.Length)
                throw new ArgumentException("Subtraction only possible for vectors of equal length");
            for (int i = 0; i < left.Length; i++)
            {
                diff.Values[i] = left[i] - right[i];
            }
            return diff;
        }


        // Overload * operator for inner product
        public static double operator *(Vector left, Vector right)
        {
            double dotProd = 0;
            if (left.Length != right.Length)
                throw new ArgumentException("Inner product only possible for vectors of equal dimension");
            for (int i = 0; i < left.Length; i++)
            {
                dotProd += left[i] * right[i];
            }
            return dotProd;
        }

        // Overload * operator to postmultiply Vector by double
        public static Vector operator *(Vector left, double right)
        {
            Vector result = new Vector(left.Length);
            for (int i = 0; i < left.Length; i++)
            {
                result[i] = left[i] * right;
            }
            return result;
        }
        // Overload * operator to premultiply Vector by double
        public static Vector operator *(double left, Vector right)
        {
            Vector result = new Vector(right.Length);
            for (int i = 0; i < right.Length; i++)
            {
                result[i] = right[i] * left;
            }
            return result;
        }



        /*-------------------------------------------------------------------------------------------------------------------*/
        /*                                          Indexing and overrides                                                   */
        /*-------------------------------------------------------------------------------------------------------------------*/

        // Indexer to allow use of [] notation
        public double this[int i]
        {
            get { return this.Values[i]; }
            set { this.Values[i] = value; }
        }

        // Override array string representation to return a row vector in curly brackets to three decimal places
        public override string ToString()
        {
            string tmp = "{";
            for (int i = 0; i < values.Length - 1; i++)
            {
                tmp += String.Format("{0:0.0}", this[i]);
                tmp += ", ";
            }
            tmp += String.Format("{0:0.0}", this[values.Length - 1]);
            return tmp += "}";
        }


    }

}
