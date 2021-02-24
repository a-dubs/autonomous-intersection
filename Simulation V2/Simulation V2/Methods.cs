using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simulation_V2
{
    class Methods 
    {

        public static double Distance(Vector2 p1, Vector2 p2)
        {
            double dist = Math.Sqrt(Math.Pow(p1.X-p2.X,2)+ Math.Pow(p1.Y - p2.Y, 2));
            return dist;
        }
        public static Vector2 Midpoint(Vector2 p1, Vector2 p2)
        {
            return new Vector2((float)(p1.X+p2.X)/ (float)2.0, (float)(p1.Y + p2.Y) / (float)2.0);
        }
        public static bool AreSimilar(double num1, double num2, double buffer, bool percentage)
        {
            if (Math.Abs(num1-num2)<num1*buffer || Math.Abs(num1 - num2) < num2 * buffer)
            {
                return true;
            }
            else
                return false;
        }
        public static bool AreSimilar(double num1, double num2, double buffer)
        {
            if (Math.Abs(num1 - num2) < buffer)
            {
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deg"> degrees to convert</param>
        /// <returns></returns>
        public static double DegToRad(double deg)
        {
            return deg * Vars.DegtoRad;
        }
        /// <summary>Conver Radians to Degrees</summary>
        public static double RadToDeg(double rad)
        {
            return rad * Vars.RadtoDeg;
        }
        /// <summary>Convert km/h to m/s</summary>
        public static double KmhToMs(double kmh)
        {
            return kmh * Vars.KmhtoMs;
        }
        /// <summary>Convert m/s to km/h</summary>
        public static double MsToKmh(double ms)
        {
            return ms * Vars.MstoKmh;
        }
        /// <summary> Print out a string with an identifying string in front </summary>
        public static void Print(string identifier, string str)
        {
            System.Diagnostics.Debug.WriteLine(identifier + ": " + str);
        }
        /// <summary> Print out a value with an identifying string in front </summary>
        public static void Print(string identifier, double val)
        {
            System.Diagnostics.Debug.WriteLine(identifier + ": " + val);
        }
        /// <summary> Print out a value</summary>
        public static void Print(double val)
        {
            System.Diagnostics.Debug.WriteLine(val);
        }
        /// <summary> Print out a value</summary>
        public static void Print(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }
        /// <summary> Print out an array</summary>
        public static void Print(double[] array)
        {
            System.Diagnostics.Debug.WriteLine(array);
        }
        /// <summary> Print out an array</summary>
        public static void Print(string[] array)
        {
            System.Diagnostics.Debug.WriteLine(array);
        }
        /// <summary> Print out an array</summary>
        public static void Print(int[] array)
        {
            System.Diagnostics.Debug.WriteLine(array);
        }
    }
}
