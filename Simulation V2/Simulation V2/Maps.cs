using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation_V2
{ 
    public static class Maps
    {
        static System.Random rand = new System.Random();  // generate Random object

        // Information about the simulation 
        public static int SpeedLimit = Vars.SpeedLimit;  // speed limit in km/h - not strict/rigid but more of the maximum velocity where the cars should still be accelerating
        //// maybe add pointers to variables in vars so that all adjustability is kept to vars but the code mimics actual leveraging of map services ?????
        public static string[] Directions = new string[] {"N","E","S","W"}; // for use by the simulation

        // Randomly Generated ID's for Intersections (Arbitrary, unique, and random IDs)
        public static int IntersectionID = rand.Next(100000, 200000);
        public static int IntersectionNorthID = rand.Next(200000, 300000);
        public static int IntersectionEastID = rand.Next(300000, 400000);
        public static int IntersectionSouthID = rand.Next(400000, 500000);
        public static int IntersectionWestID = rand.Next(500000, 600000);
        public static int RoadNSID = rand.Next(100000, 500000);
        public static int RoadEWID = rand.Next(500000, 1000000);
        public static double[] Roads = new double[] { RoadNSID, RoadEWID };
        public static double[] Intersections = new double[] { IntersectionNorthID, IntersectionEastID, IntersectionSouthID, IntersectionWestID };
        
    }
}
