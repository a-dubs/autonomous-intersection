using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Simulation_V2
{
    public static class Vars
    {
        static System.Random rand = new System.Random();  // generate Random object

        // Simulation Key Variables
        public static int WindowWidth = 1920;
        public static int WindowHeight = 1050; 
        public static float PPM = 4;  // Pixels Per Meter; Gives scale to the simulation;
        public static int NumberOfLanes = 15;
        public static double CarFrequency = 100;  // cars spawning in per minute 
        public static int MaxNumberOfVehicles = 100;  // max possible number of car on screen
        public static double DeltaTime = .0166667;  // 1/60th of a second b/c simulation runs at 60 fps  
        public static int SpeedLimit = 70;  // speed limit in km/h - not strict/rigid but more of the maximum velocity where the cars should still be accelerating
        public static double LiDARUncertainty = .1;  // uncertainty of LiDAR bounding boxes (larger % makes boxes larger)
        public static double VehicleProcessorCyclesPerSec = 30; // Forces vehicle to only think / run code set number of times per second
        public static double ETABuffer = 1.6;  // window of time (in seconds) that cars must guarantee between themselves when avoid potential collisions
        public static double PedestrianThiccness = 1;  // width of pedestrian in LIDAR 
        public static double AvgPedestrianSpeed = 8;  // speed of pedestrian in km/h
        public static double PedestrianFrequency = 6;  // pedestrians spawning in per minute 

        public static double LeftTurnPct = .1;
        public static double StraightPct = .5;
        public static double RightTurnPct = .4;

        // Global Counting Variables
        public static int VehicleNumber = 1;
        public static double ElapsedGameTime = 0;
        public static int LaneNumber = 0;
        public static int SelectedVehicleID = 0;
        public static bool SecondElapsed = false;
        public static int NumberOfActiveVehicles = 0;
        public static int DirectionNumber = 0;
        public static bool Paused = false;
        public static int VehiclesThrough = 0;
        public static bool PrintETAs = false;
        
        // Road Drawing Specifications
        public static double LaneWidth = 4.5; //width of lanes in meters
        public static float StreetWidthPixels = (float) LaneWidth * NumberOfLanes * PPM;  // width in pixels of each half of the street
        public static double DashedLineLength = .5;  // length of dashed lane divider lines in meters
        public static double DashedLineThickness = 1; // thickness of dashed lane divider lines in pixels
        public static double DashedLineSpacing = 1;  // spacing between dashed lines in meters
        public static double IntersectionCurve = 4; // radius curvature at corners of intersection in meters

        // Origins 
        public static Vector2 Origin = new Vector2(Vars.WindowWidth/ 2, Vars.WindowHeight / 2);
        public static Vector2 OriginNE = new Vector2((int)(Vars.WindowWidth / 2 + StreetWidthPixels + IntersectionCurve * PPM), (int)(Vars.WindowHeight / 2 - StreetWidthPixels - IntersectionCurve * PPM));
        public static Vector2 OriginSE = new Vector2((int)(Vars.WindowWidth / 2 + StreetWidthPixels + IntersectionCurve * PPM), (int)(Vars.WindowHeight / 2 + StreetWidthPixels + IntersectionCurve * PPM));
        public static Vector2 OriginSW = new Vector2((int)(Vars.WindowWidth / 2 - StreetWidthPixels - IntersectionCurve * PPM), (int)(Vars.WindowHeight / 2 + StreetWidthPixels + IntersectionCurve * PPM));
        public static Vector2 OriginNW = new Vector2((int)(Vars.WindowWidth / 2 - StreetWidthPixels - IntersectionCurve * PPM), (int)(Vars.WindowHeight / 2 - StreetWidthPixels - IntersectionCurve * PPM));
        public static Vector2 InnerOriginNE = new Vector2((int)(OriginNE.X - IntersectionCurve * PPM), (int)(OriginNE.Y + IntersectionCurve * PPM));
        public static Vector2 InnerOriginSE = new Vector2((int)(OriginSE.X - IntersectionCurve * PPM), (int)(OriginSE.Y - IntersectionCurve * PPM));
        public static Vector2 InnerOriginSW = new Vector2((int)(OriginSW.X + IntersectionCurve * PPM), (int)(OriginSW.Y - IntersectionCurve * PPM));
        public static Vector2 InnerOriginNW = new Vector2((int)(OriginNW.X + IntersectionCurve * PPM), (int)(OriginNW.Y + IntersectionCurve * PPM));

        // Vehicle Specifications
        public static int MaxMassVehicle = 2080;  // max weight of car in kg
        public static int MinMassVehicle = 1680;  // min weight of car in kg
        
        // Conversion Factors
        public static double MstoKmh = 3.6;
        public static double KmhtoMs = .278;
        public static double RadtoDeg = 180 / Math.PI;
        public static double DegtoRad = Math.PI / 180;

    }

}
    

