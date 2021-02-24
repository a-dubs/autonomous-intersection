using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Simulation_V2
{
    class Vehicle
    {
        //atrributes and values and characteristics

        public double Time = 0;
        public Vector2 Position;
        public double[] Velocity = new double[2];  // speed km/h , car's rotation
        public double Acceleration = 0;  // in km/h/s
        public string Direction;  // either N , E , S , W to indicate direction facing, giving relativity to a certain direction to make moving and turning easier
        public int DirectionNumber;  // N = 1, E = 2, S = 3, W = 4
        public double Mass = 2000;  // in kg
        public bool Active = true;
        public int ID = 0;
        public int CurrentIntersectionID;
        public int PreviousIntersectionID;
        public int RoadID;
        public int LaneNumber = 1; // starts at 1
        public string Action = "Idle";
        public string Destination;
        public int DestinationNumber;
        public double MaxTurningSpeed, SuggestedTurningSpeed;
        public double TurningRadius;  // Radius in Meters
        public double IntersectionDistance;  // Distance in Meters
        public double RelativeAngle;
        public Vector2 TurningPoint;
        public bool Turned = false;
        public Vector2 TopLeftCorner, TopCenter, TopRightCorner, RightCenter, LeftCenter, BottomLeftCorner, BottomCenter, BottomRightCorner;
        public Vector2 Center;
        public Vector2[] BoundingBox;
        public Color Hue = Color.White;
        public bool Crashed = false;
        public Vector2 NorthCorner, EastCorner, SouthCorner, WestCorner;
        public double AppliedThrottle = 0.0; // net throttle output of car (this value gradually increases as the pedal is pressed down)
        public double ThrottleTarget = 0.0; // target throttle value of car (is more or less the same value as ThrottleCommand but this value should never be modified)
        public double ThrottleCommand = 0.0; // throttle value inputted to car/drive function
        public int VehiclesBehind, VehiclesAhead, VehiclesQueuedAhead, VehiclesQueuedBehind, VehiclesQueuedTotal;
        public Vehicle VehicleAhead, VehicleBehind;
        public double VehicleAheadDistance, VehicleBehindDistance, BrakingDistance;
        public bool LeavingIntersection = false;
        public bool InIntersection = false;
        public double Score; // value indicating whether car should speed up or slow down
        public double ScoreBuffer = 0;  // value that will buffer the car's response to score
        public double Priority;
        public byte[][] State = new byte[0][];
        List<Vehicle> Vehicles = new List<Vehicle>();
        public List<Vector2[]> LiDAR = new List<Vector2[]>();
        public bool InFront = false;
        public double ObjectAheadDistance; // in meters
        public double FollowingDistance;
        public Vehicle CarAhead;
        public List<Vehicle> RelevantVehicles = new List<Vehicle>();  // list of vehicles
        public List<Vehicle>[] ImportantVehicles = new List<Vehicle>[0];  // array of lists
        public double ProcessorCycleCount = 0;
        public double ThrottleModification = 0;
        public double PedalTime = 1;
        public List<Vehicle> CarsFromLeft = new List<Vehicle>();
        public List<Vehicle> CarsFromRight = new List<Vehicle>();
        public List<Vehicle> CarsOpposite = new List<Vehicle>();
        public List<Vehicle> CarsInIntersection = new List<Vehicle>();
        public int ETATally = 0;
        public double ETADiffTotal = 0;
        public string PotentialCollisions = "";
        public double NumberOfETAs = 0;
        public double SpeedInaccuracy = 0;
        public Vector2 LeftUpperCenter = new Vector2();
        public Vector2 LeftLowerCenter = new Vector2();
        public Vector2 RightUpperCenter = new Vector2();
        public Vector2 RightLowerCenter = new Vector2();
        public int ModelNumber=0;
        public double DistanceTravelled = 0;

        // constants and information and details
        public static Color[] ModelColors = new Color[] { Color.White, Color.RoyalBlue, Color.DeepSkyBlue, Color.OrangeRed, Color.Purple, Color.MediumVioletRed, Color.SlateGray}; 
        public static float Width = (float)1.9 * Vars.PPM;
        public static float Height = (float)5.5 * Vars.PPM;
        public static float SpriteWidth = 100;
        public static float SpriteHeight = 200;
        public static string SpriteFileName = "images/white_car";
        public static Texture2D Sprite;
        public static float Xscale = (float)(Vehicle.Width / Vehicle.SpriteWidth) * (float)1.1111;
        public static float Yscale = Vehicle.Height / Vehicle.SpriteHeight;
        public static Vector2 Origin = new Vector2(SpriteWidth / 2, (float)(SpriteHeight * .75));
        public static float DiagonalTop = (float)Math.Sqrt(Math.Pow((.75 * Height), 2) + Math.Pow((.5 * Width), 2));
        public static float DiagonalBottom = (float)Math.Sqrt(Math.Pow((.25 * Height), 2) + Math.Pow((.5 * Width), 2));

        public Vehicle(int id, int dn = 0, int lane = 0, double speed = 0)
        {
            System.Random rand = new System.Random();

            // randomly generating numbers to create random information and characteristics about each car
            SpeedInaccuracy = (rand.Next(0, 200) - 100) / 100.0;
            // if direction value is not specified 
            if (dn == 0)
                DirectionNumber = rand.Next(1, 5); // generate random int from 1-4 (NESW)
            else
                DirectionNumber = dn;


            if (speed != 0)
                Velocity[0] = speed;
            else// randomly generated initial velocity (Vi) based on speed limit
                Velocity[0] = rand.Next(Maps.SpeedLimit - 50, Maps.SpeedLimit - 40);
            // randomly generated lane number

            // set ID value to given id parameter
            ID = id;
            // set IntersectionID value to that of the main intersection
            CurrentIntersectionID = Maps.IntersectionID;

            ModelNumber = rand.Next(0, ModelColors.Length);
           
            //// apply random values to object fields ////

            //Methods.Print(vi);
            LaneNumber = lane;
            //Mass = weight;
            //Methods.Print(lane + " vs " + LaneNumber);
            //Vector2[] Hitboxes = {TopLeftCorner, TopCenter, TopRightCorner, RightCenter, BottomRightCorner, BottomCenter, BottomLeftCorner, LeftCenter};

            //// assign destinations ////
            int destination = 0; // assigned arbitrary value 
            int n = rand.Next(0, 100);
            if (lane == 0)
            {
                lane = rand.Next(1, Vars.NumberOfLanes + 1);
                if (Vars.NumberOfLanes == 1)  // 1 lane
                {
                    LaneNumber = 1;
                    if (n < Vars.LeftTurnPct * 100)
                    {
                        destination = 0; // left 
                    }
                    else if (n > 100 - (Vars.RightTurnPct * 100)) // right turn
                    {
                        destination = 2; // right
                    }
                    else // straight
                    {
                        destination = 1; // straight
                    }
                    // destination = rand.Next(0, 3); // 0 = left, 1 = straight, 2 = right
                }
                else if (Vars.NumberOfLanes == 2)  // 2 lanes
                {
                    if (n < Vars.LeftTurnPct * 100)
                    {
                        destination = 0; // left 
                        LaneNumber = 1;
                    }
                    else if (n > 100 - (Vars.RightTurnPct * 100)) // right turn
                    {
                        destination = 2; // right
                        LaneNumber = 2;
                    }
                    else // straight
                    {
                        destination = 1; // straight
                        LaneNumber = rand.Next(1, 3);
                    }
                }
                else  // 3 lanes or more
                {
                    if (n < Vars.LeftTurnPct * 100)
                    {
                        destination = 0; // left 
                        LaneNumber = 1;
                    }
                    else if (n > 100 - (Vars.RightTurnPct * 100)) // right turn
                    {
                        destination = 2; // right
                        LaneNumber = Vars.NumberOfLanes;
                    }
                    else // straight
                    {
                        destination = 1; // straight
                        LaneNumber = rand.Next(1, Vars.NumberOfLanes + 1);
                    }
                }
            }
            else
            {
                LaneNumber = lane;
                if (Vars.NumberOfLanes == 1)  // 1 lane
                {
                    if (n < Vars.LeftTurnPct * 100)
                    {
                        destination = 0; // left 
                    }
                    else if (n > 100 - (Vars.RightTurnPct * 100)) // right turn
                    {
                        destination = 2; // right
                    }
                    else // straight
                    {
                        destination = 1; // straight
                    }
                    // destination = rand.Next(0, 3); // 0 = left, 1 = straight, 2 = right
                }
                /*else if (Vars.NumberOfLanes == 2)  // 2 lanes
                {
                    if(LaneNumber == 1) { }
                    if (n < Vars.LeftTurnPct * 100)
                    {
                        destination = 0; // left 
                    }
                    else if (n > 100 - (Vars.RightTurnPct * 100)) // right turn
                    {
                        destination = 2; // right
                    }
                    else // straight
                    {
                        destination = 1; // straight
                    }
                }*/
                else  // 3 lanes or more
                {
                    if (LaneNumber == 1)
                    {
                        n = rand.Next(0, (int)(Vars.LeftTurnPct * 100 + Vars.StraightPct*100));
                        if (n < Vars.LeftTurnPct * 100)
                        {
                            destination = 0; // left 
                        }
                        else // straight
                        {
                            destination = 1; // straight
                        }
                    }
                    else if (LaneNumber == Vars.NumberOfLanes)
                    {
                        n = rand.Next(0, (int)(Vars.RightTurnPct * 100 + Vars.StraightPct*100));
                        if (n < Vars.RightTurnPct * 100) // right turn
                        {
                            destination = 2; // right
                        }
                        else // straight
                        {
                            destination = 1; // straight
                        }
                    }
                    else
                    {
                        destination = 1; // straight
                    }
                }
            }


            DestinationNumber = destination;
            //// based on direction, assign position and rotation values ////
            if (DirectionNumber == 1)  // North
            { // Facing North; Spawning from the South
                PreviousIntersectionID = Maps.IntersectionSouthID;
                Direction = "N";
                Position = new Vector2(Vars.WindowWidth / 2 + (float)(((LaneNumber - 1) * Vars.PPM * Vars.LaneWidth) + (Vars.PPM * Vars.LaneWidth * .5)), Vars.WindowHeight + (float)(Height * .75));
                Velocity[1] = 0;
                RelativeAngle = 0;
            }
            else if (DirectionNumber == 2)  // East
            { // Facing East; Spawning from the West
                PreviousIntersectionID = Maps.IntersectionWestID;
                Direction = "E";
                Position = new Vector2(0 - (float)(Height * .75), Vars.WindowHeight / 2 + (float)(((LaneNumber - 1) * Vars.PPM * Vars.LaneWidth) + (Vars.PPM * Vars.LaneWidth * .5)));
                Velocity[1] = 270 * Vars.DegtoRad;
                RelativeAngle = 270 * Vars.DegtoRad;
            }
            else if (DirectionNumber == 3)  // South
            { // Facing South; Spawning from the North
                PreviousIntersectionID = Maps.IntersectionNorthID;
                Direction = "S";
                Position = new Vector2(Vars.WindowWidth / 2 - ((float)(((LaneNumber - 1) * Vars.PPM * Vars.LaneWidth) + (Vars.PPM * Vars.LaneWidth * .5))), -(float)(Height * .75));
                Velocity[1] = 180 * Vars.DegtoRad;
                RelativeAngle = 180 * Vars.DegtoRad;
            }
            else  // West
            { // Facing West; Spawning from the East
                PreviousIntersectionID = Maps.IntersectionEastID;
                Direction = "W";
                Position = new Vector2(Vars.WindowWidth + (float)(Height * .75), (Vars.WindowHeight / 2) - (float)(((LaneNumber - 1) * Vars.PPM * Vars.LaneWidth) + (Vars.PPM * Vars.LaneWidth * .5)));
                Velocity[1] = 90 * Vars.DegtoRad;
                RelativeAngle = 90 * Vars.DegtoRad;
            }

            // Assign Destination
            UpdateDestination(destination);

            // Set Max Turning Information
            //MaxTurningSpeed = Math.Pow(6.86 * TurningRadius, .5);


        }

        public Vehicle()
        {

        }

        public static string DirectionConverter(double DN)
        {
            string DS = "";
            if (DN == 1)
            {
                DS = "N";
            }
            else if (DN == 2)
            {
                DS = "E";
            }
            else if (DN == 3)
            {
                DS = "S";
            }
            else if (DN == 4)
            {
                DS = "W";
            }
            return DS;
        }

        public void UpdateBoundingBox()
        {
            Center = new Vector2(Position.X + (float)(.25 * Height * Math.Cos(90 * Vars.DegtoRad + Velocity[1])), Position.Y - (float)(.25 * Height * Math.Sin(90 * Vars.DegtoRad + Velocity[1])));
            TopLeftCorner = new Vector2(Position.X + (DiagonalTop * (float)Math.Cos(Velocity[1] + 90 * Vars.DegtoRad + Math.Atan((2 * Width) / (3 * Height)))), (float)(Position.Y - (DiagonalTop * Math.Sin(Velocity[1] + 90 * Vars.DegtoRad + Math.Atan((2 * Width) / (3 * Height))))));
            TopRightCorner = new Vector2(Position.X + (DiagonalTop * (float)Math.Cos(Velocity[1] + 90 * Vars.DegtoRad - Math.Atan((2 * Width) / (3 * Height)))), (float)(Position.Y - (DiagonalTop * Math.Sin(Velocity[1] + 90 * Vars.DegtoRad - Math.Atan((2 * Width) / (3 * Height))))));
            BottomLeftCorner = new Vector2(Position.X + (DiagonalBottom * (float)Math.Cos(Velocity[1] + 270 * Vars.DegtoRad - Math.Atan(2 * Width / Height))), Position.Y - (DiagonalBottom * (float)Math.Sin(Velocity[1] + 270 * Vars.DegtoRad - Math.Atan(2 * Width / Height))));
            BottomRightCorner = new Vector2(Position.X + (DiagonalBottom * (float)Math.Cos(Velocity[1] + 270 * Vars.DegtoRad + Math.Atan(2 * Width / Height))), Position.Y - (DiagonalBottom * (float)Math.Sin(Velocity[1] + 270 * Vars.DegtoRad + Math.Atan(2 * Width / Height))));
            TopCenter = new Vector2(Center.X + (float)(.5 * Height * Math.Cos(90 * Vars.DegtoRad + Velocity[1])), Center.Y - (float)(.5 * Height * Math.Sin(90 * Vars.DegtoRad + Velocity[1])));
            LeftCenter = new Vector2(Center.X + (float)(.5 * Width * Math.Cos(180 * Vars.DegtoRad + Velocity[1])), Center.Y - (float)(.5 * Width * Math.Sin(180 * Vars.DegtoRad + Velocity[1])));
            RightCenter = new Vector2(Center.X + (float)(.5 * Width * Math.Cos(Velocity[1])), Center.Y - (float)(.5 * Width * Math.Sin(Velocity[1])));
            BottomCenter = new Vector2(Center.X + (float)(.5 * Height * Math.Cos(270 * Vars.DegtoRad + Velocity[1])), Center.Y - (float)(.5 * Height * Math.Sin(270 * Vars.DegtoRad + Velocity[1])));
            LeftUpperCenter = Methods.Midpoint(LeftCenter, TopLeftCorner);
            LeftLowerCenter = Methods.Midpoint(LeftCenter, BottomLeftCorner);
            RightUpperCenter = Methods.Midpoint(RightCenter, TopLeftCorner);
            RightUpperCenter = Methods.Midpoint(RightCenter, BottomLeftCorner);
        }

        public Vector2[] GetBoundingBox()
        {
            UpdateBoundingBox();
            // return all 4 corners and all 4 mid points of bounding box
            BoundingBox = new Vector2[] {TopLeftCorner, TopCenter, TopRightCorner, RightUpperCenter, RightCenter, RightLowerCenter, BottomRightCorner, BottomCenter, BottomLeftCorner, LeftLowerCenter, LeftCenter, LeftUpperCenter };
            // return all 4 corners of bounding box and center
            //Vector2[] Hitboxes = { new Vector2(TopLeftCorner.X, TopLeftCorner.Y), new Vector2(TopRightCorner.X, TopRightCorner.Y), new Vector2(BottomRightCorner.X, BottomRightCorner.Y), new Vector2(BottomLeftCorner.X, BottomLeftCorner.Y), new Vector2(Center.X, Center.Y) };

            // return all 4 corners of bounding box
            //BoundingBox = new Vector2[]{ new Vector2(TopLeftCorner.X*(float)(1-uncertainty), TopLeftCorner.Y), new Vector2(TopRightCorner.X, TopRightCorner.Y), new Vector2(BottomRightCorner.X, BottomRightCorner.Y), new Vector2(BottomLeftCorner.X, BottomLeftCorner.Y), new Vector2(Center.X, Center.Y) };
            return BoundingBox;
        }

        public void UpdateDestination(int dn)
        {
            DestinationNumber = dn;
            if (DestinationNumber == 0)
            {
                Destination = "Left";
                ImportantVehicles = new List<Vehicle>[] { new List<Vehicle>(), new List<Vehicle>(), new List<Vehicle>(), new List<Vehicle>() };
                //ImportantVehicles = new List<Vehicle>[4];
                TurningRadius = ((((Vars.LaneWidth * Vars.PPM) / 2) + (Vars.LaneWidth * Vars.PPM * (LaneNumber - 1 + Vars.NumberOfLanes))) / Vars.PPM) + Vars.IntersectionCurve;
                {
                    if (Direction == "N")
                    {
                        TurningPoint = Vars.OriginSW;
                    }
                    if (Direction == "E")
                    {
                        TurningPoint = Vars.OriginNW;
                    }
                    if (Direction == "S")
                    {
                        TurningPoint = Vars.OriginNE;
                    }
                    if (Direction == "W")
                    {
                        TurningPoint = Vars.OriginSE;
                    }
                }
            }
            else if (DestinationNumber == 1)
            {
                Destination = "Straight";
                ImportantVehicles = new List<Vehicle>[3];
            }
            else
            {
                ImportantVehicles = new List<Vehicle>[2];
                Destination = "Right";
                TurningRadius = ((Vars.NumberOfLanes - LaneNumber + .5) * Vars.LaneWidth) + Vars.IntersectionCurve;
                if (Direction == "N")
                {
                    TurningPoint = Vars.OriginSE;
                }
                if (Direction == "E")
                {
                    TurningPoint = Vars.OriginSW;
                }
                if (Direction == "S")
                {
                    TurningPoint = Vars.OriginNW;
                }
                if (Direction == "W")
                {
                    TurningPoint = Vars.OriginNE;
                }
            }
        }

        public void RotateHitbox()
        {
            if (Velocity[1] < 0 && Velocity[1] > -90)  // Facing North East
            {
                NorthCorner = TopLeftCorner;
                EastCorner = TopRightCorner;
                SouthCorner = BottomRightCorner;
                WestCorner = BottomLeftCorner;
            }
            else if (Velocity[1] < 90 && Velocity[1] > 0)  // Facing North West
            {
                NorthCorner = TopRightCorner;
                EastCorner = BottomRightCorner;
                SouthCorner = BottomLeftCorner;
                WestCorner = TopLeftCorner;
            }
            else if (Velocity[1] < 180 && Velocity[1] > 90)  // Facing South West
            {
                NorthCorner = BottomRightCorner;
                EastCorner = BottomLeftCorner;
                SouthCorner = TopLeftCorner;
                WestCorner = TopRightCorner;
            }
            else if (Velocity[1] < 270 && Velocity[1] > 180)  // Facing South East
            {
                NorthCorner = BottomLeftCorner;
                EastCorner = TopLeftCorner;
                SouthCorner = TopRightCorner;
                WestCorner = BottomRightCorner;
            }
            else if (Velocity[1] < 360 && Velocity[1] > 270)  // Facing North East
            {
                NorthCorner = TopLeftCorner;
                EastCorner = TopRightCorner;
                SouthCorner = BottomRightCorner;
                WestCorner = BottomLeftCorner;
            }
        }

        public void Drive(string action, double inputthrottle, double pedaltime = 1)
        {
           
            ThrottleTarget = inputthrottle;
            // if throttle target hasn't been reached
            if (Math.Sign(AppliedThrottle) != Math.Sign(ThrottleTarget))
                AppliedThrottle = 0;
            if (Math.Abs(AppliedThrottle) < Math.Abs(ThrottleTarget))
                AppliedThrottle += (double)Math.Sign(ThrottleTarget) * (Vars.DeltaTime / pedaltime);
            else
            {
                AppliedThrottle = ThrottleTarget;
            }
            if (ThrottleTarget < 0)
            {
                Acceleration = AppliedThrottle * (39.2);
                Acceleration -= ((.2 + ((.03 * Math.Pow(Velocity[0], 2)) / Mass)) * 3.6) * (1 - AppliedThrottle);
            }
            else
            {
                Acceleration = (double)AppliedThrottle * 58.019 * Math.Pow(2.718, (-0.012 * Velocity[0]));
                Acceleration -= ((.2 + ((.03 * Math.Pow(Velocity[0], 2)) / Mass)) * 3.6) * (1 - AppliedThrottle);
            }
            Velocity[0] += Acceleration * Vars.DeltaTime;
            if (Velocity[0] <= 0)
            {
                Acceleration = 0;
                Velocity[0] = 0;
            }

            //// Turning Left ////
            if (action == "TurningLeft")  // Turning Left
            {
                double pheta;
                if (Math.Abs(Velocity[1] - RelativeAngle) < 90 * Vars.DegtoRad)
                {
                    Action = "TurningLeft";
                    double r; // adjustable radius of turn circle in meters. each lane will need a different value of r
                    float y;
                    float x;
                    double angle = Velocity[1] - RelativeAngle;
                    r = TurningRadius;
                    pheta = (((Velocity[0] * .287 * Vars.DeltaTime) / ((2 * (r * 3.14159) / 4))) * (3.14159 / 2)) + (angle);
                    if (Direction == "N")
                    {
                        // pheta = (( velocity (m/s) * elapsed time ) / quarter circle perimeter) -> radians + current rotation of car

                        x = (float)(TurningPoint.X + (Math.Cos(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y - (Math.Sin(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle + pheta;

                    }
                    else if (Direction == "E")
                    {


                        x = (float)(TurningPoint.X + (Math.Sin(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y + (Math.Cos(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                        //System.Diagnostics.Debug.WriteLine("pheta: " + pheta + " , " + "x: " + x + " , " + "y: " + y);
                    }
                    else if (Direction == "S")
                    {


                        x = (float)(TurningPoint.X - (Math.Cos(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y + (Math.Sin(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                        //System.Diagnostics.Debug.WriteLine("pheta: " + pheta + " , " + "x: " + x + " , " + "y: " + y);
                    }
                    else  // Direction == West
                    {


                        x = (float)(TurningPoint.X - (Math.Sin(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y - (Math.Cos(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                        //System.Diagnostics.Debug.WriteLine("pheta: " + pheta + " , " + "x: " + x + " , " + "y: " + y);
                    }

                    Position = new Vector2(x, y);
                    Velocity[1] = RelativeAngle + pheta;

                }
                else
                {
                    RelativeAngle += (90 * Vars.DegtoRad);
                    Velocity[1] = RelativeAngle;
                    Turned = true;
                    DestinationNumber = 1;
                    DirectionNumber -= 1;
                    if (DirectionNumber == 0)  // reset DirectionNumber to 4 
                    {
                        DirectionNumber = 4;
                    }
                    Direction = DirectionConverter(DirectionNumber);
                    if (RelativeAngle == 360 * Vars.DegtoRad)
                    {
                        RelativeAngle = 0;
                        Velocity[1] = 0;

                    }
                }
            }

            //// Turning Right ////
            else if (action == "TurningRight")  // Turning Right
            {
                double pheta = 0;

                if (Math.Abs(RelativeAngle - Velocity[1]) < 90 * Vars.DegtoRad)
                {
                    Action = "TurningRight";
                    double r; // adjustable radius of turn circle in meters. each lane will need a different value of r
                    float y = 0;
                    float x = 0;
                    double angle = RelativeAngle - Velocity[1];

                    // radius of turning circle  = wheelbase length(in) -> (m) * sin(90-angle of wheel))/ 2*sin(angle of wheel)
                    // double r = (106.8 * .0254 * Math.Sin(((90 - angle)/180)*3.14159) / (2 * Math.Sin((angle/180) * 3.14159)));
                    r = TurningRadius;


                    // pheta (radians) = distance traveled around circumference in Vars.DeltaTime seconds divided by 1/4 the circumference of the turning circle based on r to get a %, -> radians + already traveled circumference 
                    pheta = (((Velocity[0] * .287 * Vars.DeltaTime) / ((.5 * (r * 3.14159)))) * (3.14159 / 2)) + (angle);
                    if (Direction == "N")
                    {


                        x = (float)(TurningPoint.X - (Math.Cos(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y - (Math.Sin(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                    }
                    else if (Direction == "E")
                    {

                        x = (float)(TurningPoint.X + (Math.Sin(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y - (Math.Cos(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                        //System.Diagnostics.Debug.WriteLine("pheta: " + pheta + " , " + "x: " + x + " , " + "y: " + y);
                    }
                    else if (Direction == "S")
                    {

                        x = (float)(TurningPoint.X + (Math.Cos(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y + (Math.Sin(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                        //System.Diagnostics.Debug.WriteLine("pheta: " + pheta + " , " + "x: " + x + " , " + "y: " + y);
                    }
                    else //west
                    {

                        x = (float)(TurningPoint.X - (Math.Sin(pheta) * r * Vars.PPM));
                        y = (float)(TurningPoint.Y + (Math.Cos(pheta) * r * Vars.PPM));
                        Velocity[1] = RelativeAngle - pheta;
                        //System.Diagnostics.Debug.WriteLine("pheta: " + pheta + " , " + "x: " + x + " , " + "y: " + y);
                    }
                    Position = new Vector2(x, y);
                }
                else  // vehicle has completed its turn
                {
                    RelativeAngle -= (90 * Vars.DegtoRad);  // readjust relative angle value
                    Velocity[1] = RelativeAngle;  // assign rotation value to relative angle b/c vehicle is now continuing straight in that direction
                    Turned = true;
                    DirectionNumber = (DirectionNumber + 1) % 4;
                    DestinationNumber = 1;
                    Direction = DirectionConverter(DirectionNumber);
                    if (RelativeAngle == -90 * Vars.DegtoRad)
                    {
                        RelativeAngle = 270 * Vars.DegtoRad;
                        Velocity[1] = 270 * Vars.DegtoRad;

                    }
                }
            }

            //// Driving Straight ////
            else  // Driving Straight
            {
                float dy = 0;
                float dx = 0;
                //Vector2 NewPos = new Vector2(0, 0);

                if (RelativeAngle * Vars.RadtoDeg == 0)  // North
                {
                    dy = (float)(-Math.Sin((Velocity[1] + 90) * (3.14159 / 180)) * Velocity[0] * .278 * Vars.PPM * Vars.DeltaTime);
                    dx = 0;
                }
                if (RelativeAngle * Vars.RadtoDeg == 270)  // East
                {
                    dy = 0;
                    dx = (float)(-Math.Cos((Velocity[1] - 180) * (3.14159 / 180)) * Velocity[0] * .278 * Vars.PPM * Vars.DeltaTime);
                }
                if (RelativeAngle * Vars.RadtoDeg == 180)  // South
                {
                    dy = (float)(-Math.Sin((Velocity[1] - 90) * (3.14159 / 180)) * Velocity[0] * .278 * Vars.PPM * Vars.DeltaTime);
                    dx = 0;
                }
                if (RelativeAngle * Vars.RadtoDeg == 90)  // West
                {
                    dy = 0;
                    dx = (float)(-Math.Cos((Velocity[1]) * (3.14159 / 180)) * Velocity[0] * .278 * Vars.PPM * Vars.DeltaTime);
                }
                DistanceTravelled += Math.Abs(dx) + Math.Abs(dy);
                Position = new Vector2(Position.X + dx, Position.Y + dy);
            }

        }

        private bool AreObjectsObstructing(double maxangle, double maxdist)
        {
            bool obstruction = false;
            maxdist = maxdist * Vars.PPM;
            foreach (Vector2[] boundingbox in Simulation.Objects)
            {
                foreach (Vector2 p in boundingbox)
                {
                    double dx = p.X - this.Center.X;
                    double dy = this.Center.Y - p.Y;

                    if (Methods.Distance(this.Center, p) < maxdist)
                    {
                        //Methods.Print("Car " + ID + " distance to object: " + Methods.Distance(this.Center, p)/Vars.PPM+ " at angle: " + Methods.RadToDeg(Math.Atan(dy / dx) + (Math.PI / 2)));
                        if (Math.Atan(dy / dx) + (Math.PI / 2) > -maxangle * Vars.DegtoRad && Math.Atan(dy / dx) + (Math.PI / 2) < maxangle * Vars.DegtoRad)
                        {
                            //Methods.Print("Car " + ID + " has object in the way");
                            obstruction = true;
                            break;
                        }
                    }

                }
                if (obstruction)
                    break;
            }
            return obstruction;
        }

        private double ClosestObjectDistanceAhead(double maxangle)
        {
            double dist = 0;
            //Methods.Print(LiDAR.Count);
            foreach (Vector2[] boundingbox in LiDAR)
            {
                foreach (Vector2 p in boundingbox)
                {
                    double dx = Math.Abs(p.X - this.Center.X);
                    double dy = Math.Abs(this.Center.Y - p.Y);
                    //Methods.Print(Methods.RadToDeg(Math.Atan(dy / dx) - (Math.PI / 2)) + " vs " + maxangle);
                    //Methods.Print((Math.PI / 2) * (DirectionNumber % 2));
                    if ((Math.Atan(dy / dx) - ((Math.PI / 2) * (DirectionNumber % 2))) > -(maxangle * Vars.DegtoRad) && (Math.Atan(dy / dx) - ((Math.PI / 2) * (DirectionNumber % 2))) < (maxangle * Vars.DegtoRad))// && (Methods.Distance(this.Center, p) < dist || dist == 0) && Methods.Distance(p,this.TopCenter) < Methods.Distance(p, this.BottomCenter));     
                    {
                        if (Methods.Distance(this.Center, p) < dist || dist == 0)
                        {
                            if (Methods.Distance(p, this.Center) < Methods.Distance(p, this.BottomCenter))
                                dist = Methods.Distance(this.Center, p);
                        }
                    }

                }
            }

            if (dist == 0)
            {
                return 6942066613;
            }
            else
            {
                return (dist - .5 * Vehicle.Height) / Vars.PPM;  // convert from pixels to m & subtract half a vehicle length from the distance b/c lidar is centered on car
            }
        }

        public void Update(GameTime gameTime)
        {
            //Methods.Print("AppliedThrottle :" + AppliedThrottle + " ; ThrottleTarget: " +ThrottleTarget);
            UpdateBoundingBox();
            ProcessorCycleCount += (1.0 * (Vars.DeltaTime / (1.0 / 60.0)));  //// Force vehicle to only think / run code set number of times per second ////
            if (ProcessorCycleCount >= 60.0 / Vars.VehicleProcessorCyclesPerSec)
            {
                ProcessorCycleCount = 0;


                ///////////////////////// UPDATE SENSOR VALUES ////////////////////////////

                // LIDAR 
                // make array of positions?? of bounding boxes (hitboxes) ??? 
                // bool yuh = AreObjectsObstructing(30, 15);
                ObjectAheadDistance = ClosestObjectDistanceAhead(15);

                // update the center position(x,y) of vehicle; the normal position(x,y)/origin of the car is centered on the back axle
                Center = new Vector2(Position.X + (float)(.25 * Height * Math.Cos(90 * Vars.DegtoRad + Velocity[1])), Position.Y - (float)(.25 * Height * Math.Sin(90 * Vars.DegtoRad + Velocity[1])));

                // intersection distance
                if(IntersectionDistance>-.75*Vehicle.Height|| DestinationNumber == 1) {
                    if (Direction == "N") { IntersectionDistance = Center.Y - Vars.OriginSE.Y; }
                    else if (Direction == "E") { IntersectionDistance = Vars.OriginNW.X - Center.X; }
                    else if (Direction == "S") { IntersectionDistance = Vars.OriginNW.Y - Center.Y; }
                    else if (Direction == "W") { IntersectionDistance = Center.X - Vars.OriginNE.X; }
                    // set intersectin distance to be from the front bumber of the vehicle
                    IntersectionDistance -= .5 * Vehicle.Height; // distance is calculated from center of vehicle, so subtract half the vehicle's height to get the distance from the front end of the vehicle to the beginning of the intersection
                }
                else 
                {
                    IntersectionDistance = -TurningRadius * Vars.PPM * Math.Abs(Velocity[1]) - .75 * Vehicle.Height;
                }
                
               
               
                if (IntersectionDistance < 0 && !LeavingIntersection)
                {
                    InIntersection = true;
                }
                if ((IntersectionDistance < -((Vars.StreetWidthPixels * 2) + Vars.IntersectionCurve * Vars.PPM + Vehicle.Height)&&DestinationNumber==1)||Turned)
                {
                    LeavingIntersection = true;
                    InIntersection = false;
                }

                // Vehicle creates its own state 

                State = Protocol.EncodeState(this);
                //System.Diagnostics.Debug.WriteLine(State[7]);
                // stop pushing this vehicle's old state to the airwaves 
                Simulation.Airwaves.Remove(Simulation.Airwaves.Find(x => Protocol.DecodeState(x).ID == this.ID));  // finds its own state in the airwave list by finding it's id and then removes this element from the list 
                UpdateBoundingBox();                                                                                          // push this vehicle's new state to the airwaves so it can be accessed by other cars
                Simulation.Airwaves.Add(State);

                // reset all lists of vehicles
                Vehicles.Clear();
                CarsFromLeft.Clear();
                CarsFromRight.Clear();
                CarsOpposite.Clear();
                CarsInIntersection.Clear();

                // get all states from the airwaves
                foreach (byte[][] state in Simulation.Airwaves)
                {
                    Vehicle v = Protocol.DecodeState(state);
                    //Methods.Print(v.SpeedInaccuracy);
                    // if vehicle is going to the same intersection OR if it is going the same direction AND on the same road
                    if ((v.CurrentIntersectionID == this.CurrentIntersectionID || (v.RoadID == this.RoadID && v.Direction == this.Direction)) && v.ID != this.ID)
                    {  // only keep relevent vehicles
                        Vehicles.Add(v);  // add to list of relevant vehicles 
                    }
                }




                //////////////////////////// MISCELLANEOUS //////////////////////////////

                //////////////////////////// LOGIC OF CAR ////////////////////////////////
                ThrottleCommand = 0;
                ThrottleModification = 0;


                // find car ahead and store in variable
                double dist = -100000; // temp variable to find closeset vehicle
                CarAhead = new Vehicle();
                foreach (Vehicle v in Vehicles)
                {

                    // if in the same lane as vehicle
                    // get car ahead & see if this vehicle is in front in it's lane
                    //if (v.LaneNumber == this.LaneNumber && v.Direction == this.Direction && (((this.InIntersection || this.LeavingIntersection) && (v.InIntersection || v.LeavingIntersection))||(!(this.InIntersection && this.LeavingIntersection) && !(v.InIntersection || v.LeavingIntersection))))
                    if (v.LaneNumber == this.LaneNumber && v.Direction == this.Direction && this.CurrentIntersectionID == v.CurrentIntersectionID && v.IntersectionDistance > 0)
                    {
                        //Methods.Print("car in lane");
                        if (v.IntersectionDistance > dist && v.IntersectionDistance < this.IntersectionDistance)
                        {
                            dist = v.IntersectionDistance;
                            CarAhead = v;
                        }

                    }
                }

                // if nothing closer found
                if (dist == -100000 && !InIntersection && !LeavingIntersection)
                    InFront = true;
                // car in front was found
                else
                    InFront = false;

                RelevantVehicles.Clear();

                //// get relevant vehicles ////
                for (int i = 0; i < Vehicles.Count; i++)
                {
                    Vehicle v = Vehicles[i];

                    //Methods.Print(Vehicles.Count);
                    // if other vehicle is in the front of its lane or in the intersection
                    if ((v.InIntersection || v.InFront) && !v.LeavingIntersection)

                        RelevantVehicles.Add(Vehicles[i]);
                }

                //Methods.Print(RelevantVehicles.Count);
                List<Vehicle> temp = new List<Vehicle>(RelevantVehicles);
                //Methods.Print(" for car " + ID + " temp length: " + temp.Count);
                //// get important vehicles ////
                for (int i = 0; i < RelevantVehicles.Count; i++)
                {
                    Vehicle v = RelevantVehicles[i];
                    //Methods.Print(v.DirectionNumber);
                    // if this vehicle is turning right
                    if (DestinationNumber == 2)
                    {
                        if (v.LaneNumber == Vars.NumberOfLanes)
                        {
                            if ((DirectionNumber + 1) % 4 == v.DirectionNumber % 4 && v.DestinationNumber == 1)
                            {
                                //ImportantVehicles[1].Add(RelevantVehicles[i]);
                                CarsFromLeft.Add(v);
                                temp.Remove(v);
                            }
                        }
                        if ((v.DirectionNumber + 2) % 4 == DirectionNumber % 4 && v.DestinationNumber == 0 && Vars.NumberOfLanes == 1)
                        {
                            CarsOpposite.Add(v);
                            temp.Remove(v);
                        }
                    }


                    // if this vehicle is going straight
                    else if (DestinationNumber == 1)
                    {  // add vehicles from left and right

                        //// add vehicles from left ////
                        // if other vehicle is coming from left // if other vehicle not turning right // if this vehicle in left lane and other vehicle is turning left
                        if ((DirectionNumber + 1) % 4 == v.DirectionNumber % 4 && (v.DestinationNumber == 1 || (v.DestinationNumber == 0 && LaneNumber == 1)))
                        {
                            CarsFromLeft.Add(v);
                            temp.Remove(v);
                        }

                        //// add vehicles from right ////
                        // if other vehicle is coming from the right // if other vehicle is not turning right // if other car turning right and this car is in right most lane 
                        if ((DirectionNumber + 3) % 4 == v.DirectionNumber % 4 && (v.DestinationNumber != 2 || LaneNumber == Vars.NumberOfLanes))
                        { // works!
                            CarsFromRight.Add(v);
                            temp.Remove(v);
                        }

                        if ((v.DirectionNumber + 2) % 4 == DirectionNumber % 4 && v.DestinationNumber == 0)
                        {
                            CarsOpposite.Add(v);
                            temp.Remove(v);
                        }
                    }

                    // if this vehicle is turning left
                    else
                    {   // add vehicles from left, (closest lane) right, and opposite

                        //// add vehicles from left ////
                        if ((DirectionNumber + 1) % 4 == v.DirectionNumber % 4 && v.DestinationNumber != 2)
                        {
                            CarsFromLeft.Add(v);
                            temp.Remove(v);
                        }

                        //// add vehicles from right (closest lane) ////
                        if ((DirectionNumber + 3) % 4 == v.DirectionNumber % 4 && v.DestinationNumber != 2 && v.LaneNumber == 1)
                        {
                            CarsFromRight.Add(v);
                            temp.Remove(v);
                        }

                        //// opposite ////
                        // if not turning left or if 2 lanes or more so that right turns dont interfere with left turns
                        if ((DirectionNumber + 2) % 4 == v.DirectionNumber % 4 && v.DestinationNumber != 0 && (v.DestinationNumber == 1 || Vars.NumberOfLanes == 1))
                        {
                            CarsOpposite.Add(v);
                            temp.Remove(v);
                        }
                    }
                    // cars in intersection // all cars that havent been deleted from temporary relevant vehicle copy list
                    if (temp.Count > 0)
                        CarsInIntersection.AddRange(temp);
                    //Methods.Print(" for car " +ID+ " temp length: " + temp.Count);
                }


                ////////////////////////////// ARTIFICIAL INTELLIGENCE /////////////////////////////////////
                double ThisETA = 0, OtherETA = 0;
                PotentialCollisions = "";
                PotentialCollisions += " Left: ";
                foreach (Vehicle v in CarsFromLeft)
                    PotentialCollisions += v.ID + " ";
                PotentialCollisions += " Right: ";
                foreach (Vehicle v in CarsFromRight)
                    PotentialCollisions += v.ID + " ";
                PotentialCollisions += " Opposite: ";
                foreach (Vehicle v in CarsOpposite)
                    PotentialCollisions += v.ID + " ";
                //Methods.Print("(Car " + ID + ")" + PotentialCollisions);

                List<double[]> ETAs = new List<double[]>();
                ETATally = 0;
                ETADiffTotal = 0;
                NumberOfETAs = 0;
                double thisvelocity;
                double othervelocity;
                /*
                if (Velocity[0] < 5)
                    thisvelocity = 5;
                else*/
                thisvelocity = Velocity[0]+SpeedInaccuracy;
                for (int i = 0; i < CarsFromLeft.Count; i++)
                {
                    Vehicle v = CarsFromLeft[i];
                    /*if (v.Velocity[0] < 5)
                    {
                        Methods.Print("Vehicle " + v.ID + " is adjusting eta for near / full stoppage");
                        othervelocity = 5;
                    }
                    else
                    {
                        
                    }*/
                    othervelocity = v.Velocity[0]+v.SpeedInaccuracy;
                    // tested :)
                    if (this.DestinationNumber == 1 && v.DestinationNumber == 0)
                    {
                        ThisETA = ((IntersectionDistance / Vars.PPM) + v.TurningRadius*.55 + (Vars.StreetWidthPixels / Vars.PPM)) / Methods.KmhToMs(thisvelocity);
                        OtherETA = (((v.IntersectionDistance) / Vars.PPM) + (v.TurningRadius * Math.Atan(v.TurningRadius / (v.TurningRadius - Vars.LaneWidth)))) / Methods.KmhToMs(othervelocity);
                    }
                    // tested :)
                    else if (this.DestinationNumber != 0)
                    {
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + (((Vars.NumberOfLanes + LaneNumber - 1) * Vars.LaneWidth))) / Methods.KmhToMs(othervelocity);
                        ThisETA = ((IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + ((Vars.NumberOfLanes - v.LaneNumber) * Vars.LaneWidth)) / Methods.KmhToMs(thisvelocity);
                    }
                    // tested :)
                    else if (this.DestinationNumber == 0 && v.DestinationNumber == 1) // turning left
                    {
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + ((v.LaneNumber - 1) * Vars.LaneWidth)) / Methods.KmhToMs(othervelocity);
                        ThisETA = ((IntersectionDistance / Vars.PPM) + TurningRadius * Math.Atan((Vars.IntersectionCurve + ((Vars.NumberOfLanes - v.LaneNumber) * Vars.LaneWidth)) / TurningRadius)) / Methods.KmhToMs(thisvelocity);
                    }
                    // tested :)
                    else if (this.DestinationNumber == 0 && v.DestinationNumber == 0) // turning left
                    {
                        ThisETA = (((IntersectionDistance) / Vars.PPM) + (TurningRadius * Math.Atan(TurningRadius / (TurningRadius - Vars.LaneWidth)))) / Methods.KmhToMs(thisvelocity);
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + v.TurningRadius * Math.Atan((Vars.IntersectionCurve +((Vehicle.Height/Vars.PPM)*.5))/ v.TurningRadius)) / Methods.KmhToMs(othervelocity);
                    }
                    if(ThisETA* Methods.KmhToMs(thisvelocity)<(Vehicle.Height/Vars.PPM)*.7 && OtherETA* Methods.KmhToMs(othervelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        if (ThisETA < OtherETA)
                        {
                            ThisETA = 0;
                            OtherETA = Vars.ETABuffer;
                        }
                        else
                        {
                            ThisETA = Vars.ETABuffer;
                            OtherETA = 0;
                        }
                
                    }
                    else if (ThisETA * Methods.KmhToMs(thisvelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        ThisETA = 0;
                        OtherETA = Vars.ETABuffer;
                
                    }
                    else if (OtherETA * Methods.KmhToMs(othervelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        ThisETA = Vars.ETABuffer;
                        OtherETA = 0;
                    }
                    ETAs.Add(new double[] { v.ID, ThisETA, OtherETA });
                    //Methods.Print("Car" + ID + " vs Car " + v.ID + " : This ETA vs Other ETA: " + ThisETA + " vs " + OtherETA);
                }


                for (int i = 0; i < CarsFromRight.Count; i++)
                {
                    Vehicle v = CarsFromRight[i];
                    /*if (v.Velocity[0] < 5)
                    {
                        Methods.Print("Vehicle " + v.ID + " is adjusting eta for near / full stoppage");
                        othervelocity = 5;
                    }
                    else
                    {
                        
                    }*/
                    othervelocity = v.Velocity[0] + v.SpeedInaccuracy;
                    // tested :)
                    if (this.DestinationNumber == 1 && v.DestinationNumber != 0) // if this is going straight, and cars from right are going straight or turning right
                    {
                        ThisETA = ((IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + (((Vars.NumberOfLanes + v.LaneNumber - 1) * Vars.LaneWidth))) / Methods.KmhToMs(thisvelocity);
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + ((Vars.NumberOfLanes - LaneNumber) * Vars.LaneWidth)) / Methods.KmhToMs(othervelocity);
                    }
                    /*else if (this.DestinationNumber == 1 && v.DestinationNumber == 1)
                    {

                    }*/
                    // tested :)
                    if (this.DestinationNumber == 1 && v.DestinationNumber == 0) // if this is going straight, and cars from right are turning left
                    {
                        ThisETA = ((IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + ((LaneNumber - 1) * Vars.LaneWidth)) / Methods.KmhToMs(thisvelocity);
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + v.TurningRadius * Math.Atan((Vars.IntersectionCurve + ((Vars.NumberOfLanes - LaneNumber) * Vars.LaneWidth)) / v.TurningRadius)) / Methods.KmhToMs(othervelocity);
                    }
                    // tested :)
                    else if (this.DestinationNumber == 0 && v.DestinationNumber == 0) // if both cars turning left
                    {
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + (v.TurningRadius * Math.Atan(v.TurningRadius / (v.TurningRadius - Vars.LaneWidth)))) / Methods.KmhToMs(othervelocity);
                        ThisETA = ((IntersectionDistance / Vars.PPM) + TurningRadius * Math.Atan((Vars.IntersectionCurve + ((Vehicle.Height / Vars.PPM) * .5)) / TurningRadius)) / Methods.KmhToMs(thisvelocity);
                    }
                    // tested :)
                    else if (this.DestinationNumber == 0 && v.DestinationNumber == 1)
                    {
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + TurningRadius*.55 + (Vars.StreetWidthPixels / Vars.PPM)) / Methods.KmhToMs(othervelocity);
                        ThisETA = (((IntersectionDistance) / Vars.PPM) + (TurningRadius * Math.Atan(TurningRadius / (TurningRadius - Vars.LaneWidth)))) / Methods.KmhToMs(thisvelocity);
                    }
                    if (ThisETA * Methods.KmhToMs(thisvelocity) < (Vehicle.Height / Vars.PPM) * .7 && OtherETA * Methods.KmhToMs(othervelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        if (ThisETA < OtherETA)
                        {
                            ThisETA = 0;
                            OtherETA = Vars.ETABuffer;
                        }
                        else
                        {
                            ThisETA = Vars.ETABuffer;
                            OtherETA = 0;
                        }
                  
                    }
                    else if (ThisETA * Methods.KmhToMs(thisvelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        ThisETA = 0;
                        OtherETA = Vars.ETABuffer;
                   
                    }
                    else if (OtherETA * Methods.KmhToMs(othervelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        ThisETA = Vars.ETABuffer;
                        OtherETA = 0;
                       
                    }
                    //Methods.Print("Car" + ID + " vs Car " + v.ID + " : This ETA vs Other ETA: " + ThisETA + " vs " + OtherETA);
                    ETAs.Add(new double[] { v.ID, ThisETA, OtherETA });
                }


                for (int i = 0; i < CarsOpposite.Count; i++)
                {
                    Vehicle v = CarsOpposite[i];
                    /*if (v.Velocity[0] < 5)
                    {
                        Methods.Print("Vehicle " + v.ID + " is adjusting eta for near / full stoppage");
                        othervelocity = 5;
                    }
                    else
                    {
                        
                    }*/
                    othervelocity = v.Velocity[0] + v.SpeedInaccuracy;
                    // left and straight collisions 
                    // tested :)
                    if (DestinationNumber == 0 && v.DestinationNumber == 1)
                    {
                        if(Vars.NumberOfLanes == 1)
                            ThisETA = ((IntersectionDistance / Vars.PPM) + (TurningRadius * Math.Atan((Vars.IntersectionCurve) / (TurningRadius - ((v.LaneNumber - 1) * Vars.LaneWidth))))) / Methods.KmhToMs(thisvelocity);
                        else 
                            ThisETA = ((IntersectionDistance / Vars.PPM) + (TurningRadius * Math.Atan((((Vars.StreetWidthPixels / Vars.PPM) * ((double)(v.LaneNumber - 1) / (double)(Vars.NumberOfLanes - 1))) + Vars.IntersectionCurve) / (TurningRadius - ((v.LaneNumber - 1) * Vars.LaneWidth))))) / Methods.KmhToMs(thisvelocity);
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + ((Vars.NumberOfLanes- 1 + ((Vars.NumberOfLanes-1)*(1-((double)v.LaneNumber/Vars.NumberOfLanes))))*Vars.LaneWidth)) / Methods.KmhToMs(othervelocity);
                    }
                    // tested :)
                    if (DestinationNumber == 1 && v.DestinationNumber == 0)
                    {
                        if (Vars.NumberOfLanes == 1)
                            OtherETA = ((v.IntersectionDistance / Vars.PPM) + (v.TurningRadius * Math.Atan((Vars.IntersectionCurve) / (v.TurningRadius - ((LaneNumber - 1) * Vars.LaneWidth))))) / Methods.KmhToMs(othervelocity);
                        else
                            OtherETA = ((v.IntersectionDistance / Vars.PPM) + (v.TurningRadius * Math.Atan((((Vars.StreetWidthPixels / Vars.PPM) * ((double)(LaneNumber - 1) / (double)(Vars.NumberOfLanes - 1))) + Vars.IntersectionCurve) / (v.TurningRadius - ((LaneNumber - 1) * Vars.LaneWidth))))) / Methods.KmhToMs(othervelocity);
                        ThisETA = ((IntersectionDistance / Vars.PPM) + Vars.IntersectionCurve + ((Vars.NumberOfLanes - 1 + ((Vars.NumberOfLanes - 1) * (1 - ((double)LaneNumber / Vars.NumberOfLanes)))) * Vars.LaneWidth)) / Methods.KmhToMs(thisvelocity);
                    }
                    // left and right collision (1 lane intersections only)
                    if (DestinationNumber == 0 && v.DestinationNumber == 2)
                    {
                        ThisETA = ((IntersectionDistance / Vars.PPM) + (TurningRadius * Math.PI * .25)) / Methods.KmhToMs(thisvelocity); 
                        OtherETA = ((v.IntersectionDistance + Vars.IntersectionCurve) / Vars.PPM) / Methods.KmhToMs(othervelocity);
                    }
                    if (DestinationNumber == 2 && v.DestinationNumber == 0)
                    {
                        OtherETA = ((v.IntersectionDistance / Vars.PPM) + (v.TurningRadius * Math.PI * .25)) / Methods.KmhToMs(othervelocity);
                        ThisETA = ((IntersectionDistance + Vars.IntersectionCurve) / Vars.PPM) / Methods.KmhToMs(thisvelocity);
                    }

                    if (ThisETA * Methods.KmhToMs(thisvelocity) < (Vehicle.Height / Vars.PPM) * .7 && OtherETA * Methods.KmhToMs(othervelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        if (ThisETA * Methods.KmhToMs(thisvelocity) < OtherETA * Methods.KmhToMs(othervelocity))
                        {
                            ThisETA = 0;
                            OtherETA = Vars.ETABuffer;
                        }
                        else
                        {
                            ThisETA = Vars.ETABuffer;
                            OtherETA = 0;
                        }
                       
                    }
                    else if (ThisETA * Methods.KmhToMs(thisvelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        ThisETA = 0;
                        OtherETA = Vars.ETABuffer;
                
                    }
                    else if (OtherETA * Methods.KmhToMs(othervelocity) < (Vehicle.Height / Vars.PPM) * .7)
                    {
                        ThisETA = Vars.ETABuffer;
                        OtherETA = 0;
                        
                    }
                    ETAs.Add(new double[] { v.ID, ThisETA, OtherETA });
                    //Methods.Print("Car" + ID + " vs Car " + v.ID + " : This ETA vs Other ETA: " + ThisETA + " vs " + OtherETA);
                }

                foreach (double[] ETAPair in ETAs)
                {
                    //Methods.Print("Car" + ID + " vs Car " + ETAPair[0] + this.DestinationNumber + " vs " + Vehicles.Find(x => x.ID == ETAPair[0]).DestinationNumber);
                    if (Vars.PrintETAs)
                        Methods.Print("Car" + ID + " vs Car " + ETAPair[0] + " : This ETA vs Other ETA: " + ETAPair[1] + " vs " + ETAPair[2]);
                    // if collision has not yet been avoided
                    if (Math.Sign(ETAPair[1] + (Vars.ETABuffer * .5)) == 1 && Math.Sign(ETAPair[2] + (Vars.ETABuffer * .5)) == 1)
                    {
                        // if positive then car is getting there first, if negative then car is getting there last
                        double ETADifference = ETAPair[2] - ETAPair[1];
                        if (Math.Abs(ETADifference) <= Vars.ETABuffer)
                        {

                            //Methods.Print("Car" + ID + " vs Car " + ETAPair[0] + " : This ETA vs Other ETA: " + ETAPair[1] + " vs " + ETAPair[2]);
                            ETATally += Math.Sign(ETADifference);

                            ETADiffTotal += (Vars.ETABuffer - Math.Abs(ETADifference)) * Math.Sign(ETADifference);
                            // normalized ETADIFFTOTAL
                            ThrottleModification += (((Vars.ETABuffer - Math.Abs(ETADifference)) * Math.Sign(ETADifference)) / Vars.ETABuffer)*Math.Pow(.5,(1+Math.Sign(ETADifference)*Convert.ToInt32(InIntersection)));
                            /*if (Math.Abs(ETAPair[1]) < .5 && ETADifference < .5)
                            {
                                ETATally += Math.Sign(ETADifference) * 100000;
                                ETADiffTotal += ((Vars.ETABuffer - ETADifference) * Math.Sign(ETADifference)) * 100000;
                            }*/
                            NumberOfETAs += 1;
                        }
                        // if nearly stopped & eta is wack
                        else if (this.Velocity[0] < 10 && ETAPair[2] < Vars.ETABuffer && this.IntersectionDistance < 2 * Vars.PPM)
                        {

                        }
                    }
                }

                //Methods.Print("(Car " +ID+ ") ETA Tally: " + ETATally);
                if (NumberOfETAs > 0)
                {
                    //ThrottleModification = ETADiffTotal / NumberOfETAs;
                    /*if (ETADiffTotal > 0)
                    {
                        ThrottleModification = Vars.ETABuffer - (ETADiffTotal / NumberOfETAs);
                    }
                    else
                    {
                        ThrottleModification = (ETADiffTotal / NumberOfETAs) - Vars.ETABuffer;
                    }*/
                }





                /////// instinct of car ////////
                if (!InIntersection && !LeavingIntersection)
                    FollowingDistance = (.5 * (Vehicle.Height / Vars.PPM)) + (this.Velocity[0] * .25); // target distance between cars based on velocity (in meters) outside of intersection
                else
                {
                    FollowingDistance = (.5 * (Vehicle.Height / Vars.PPM)) + (this.Velocity[0] * .1); // target distance between cars based on velocity (in meters) inside intersection
                    if (ThrottleModification > 0)
                        ThrottleModification = ThrottleModification *2;
                }
                //Methods.Print("ObjectAheadDistance: " + ObjectAheadDistance + " vs FollowingDistance: " + FollowingDistance);

                // braking if LIDAR detects car ahead (emergency/instinct of car)
                if (ObjectAheadDistance < FollowingDistance && ObjectAheadDistance > -2)
                {
                    // if slightly decelerating 
                    //Methods.Print("LiDAR");
                    // ThrottleCommand = delta v / max braking per second
                    //Methods.Print("car ahead distance: " + (this.IntersectionDistance-CarAhead.IntersectionDistance-Vehicle.Height)/Vars.PPM);
                    // if object detected ahead is a vehicle || has vehicle ahead
                    if (CarAhead.ID != 0 && !InIntersection)//&& Methods.AreSimilar((this.IntersectionDistance-CarAhead.IntersectionDistance + Vehicle.Height) /Vars.PPM, ObjectAheadDistance, (Vehicle.Height/Vars.PPM)))
                    {
                        //Methods.Print("slowing down for car ahead");
                        ThrottleCommand = (ObjectAheadDistance - FollowingDistance) / (FollowingDistance * .5);
                    }
                    else  // if object ahead is not a vehicle, i.e pedestrian // or if the car is in the intersection
                    {
                        ThrottleCommand = -1;
                        PedalTime = .25;
                    }
                    // if object ahead is vehicle, use car ahead state to get velocity, otherwise, assume stopped obstacle i.e. pedestrian 
                }
                else  // get acceleration / deceleration from AI / intercommunication 
                {
                    if (ThrottleModification == 0)  
                    {
                        if (LeavingIntersection)
                            ThrottleCommand = (Maps.SpeedLimit*1.5 - Velocity[0]) / 10.0;
                        else
                        {
                            if (DestinationNumber == 2 && InIntersection)
                                ThrottleCommand = (Maps.SpeedLimit*.75 - Velocity[0]) / 10.0;
                            else
                                ThrottleCommand = (Maps.SpeedLimit - Velocity[0]) / 10.0;
                        }   
                    }
                    else
                    {
                        if (DestinationNumber == 2 && Velocity[0] < Maps.SpeedLimit * .75 && InIntersection)
                            ThrottleCommand += ThrottleModification;
                        else if(DestinationNumber == 2 && Velocity[0] < Maps.SpeedLimit * 1 && !InIntersection)
                            ThrottleCommand += ThrottleModification;
                        else if (DestinationNumber != 2 && Velocity[0] < Maps.SpeedLimit * 1.5)
                            ThrottleCommand += ThrottleModification;
                        else
                            ThrottleCommand = 0;
                    }

                }


            }
            PedalTime = .5;
            // fundemental logic telling car to turn or continue straight once it has reached intersection // will call on the car's AI
            //// code specific to simulation // outside of AI - therefore running at 60Hz ////
            if (IntersectionDistance < -Vehicle.Height * .75)  //// if vehicle is in the intersection ////
            {
                if (Destination == "Straight" || Turned)
                {
                    Drive("Continuing Straight", ThrottleCommand, PedalTime);
                }
                else
                {
                    if (Destination == "Left")
                    {
                        Drive("TurningLeft", ThrottleCommand, PedalTime);
                    }
                    else
                    {
                        Drive("TurningRight", ThrottleCommand, PedalTime);
                    }
                }
            }
            else if (LeavingIntersection)  //// if leaving the intersection ////
            {
                Drive("Continuing Straight", ThrottleCommand, PedalTime);
            }
            else  //// if approaching the intersection ////
            {
                Drive("Straight", ThrottleCommand, PedalTime);
            }
            //Methods.Print(Position.X + ", " +Position.Y + " ; Direction: "+ Direction + "; velocity: " + Velocity[0]);
            // Methods.Print("Car " + ID + " intersection distance: " + IntersectionDistance);
        }
    }
}



