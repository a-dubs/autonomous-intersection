// import XNA libraries
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// standard imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation_V2
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>

    public class Simulation : Microsoft.Xna.Framework.Game
    {
        // variables for the visual engine to use
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Create array of all possible vehicles, starting all as defaults
        //Vehicle[] Vehicles = new Vehicle[Vars.MaxNumberOfVehicles];
        public static List<Vehicle> Vehicles = new List<Vehicle>();

        // array of bounding (collision) boxes of all objects present in the intersection: vehicles & pedestrians
        public static List<Vector2[]> Objects = new List<Vector2[]>();

        private Texture2D VehicleSprite;
        private SpriteFont font; // font variable for displaying on screen text
        Texture2D pixel;

        //// variables for the simulation to use ////
        // this array will be used to simulate peer to peer broadcasting of each car's states
        public static List<byte[][]> Airwaves = new List<byte[][]>();
        private KeyboardState oldState;
        private double OldDeltaTime;
        private bool Cleared;
        public static bool Paused;
        private Vector2 CollisionPoint;
        private bool Collision;
        private int Hidden = 0;
        private double CSI_ET = 0;
        private double PSI_ET = 0;
        MouseState oldMouse = Mouse.GetState();
        private Pedestrian pedestrian = new Pedestrian();
        int LastDirectionSpawn = 0;
        private bool[,] SpawningSlots = new bool[Maps.Directions.Length,Vars.NumberOfLanes];  // true means slot is open
        private int[,] RecentlySpawnedVehicleIDs = new int[Maps.Directions.Length, Vars.NumberOfLanes];
        double PSI_rand;

        public Simulation()
        {
            // start up graphics engine
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Vars.WindowWidth;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = Vars.WindowHeight;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
           
            System.Random rand = new System.Random();
            PSI_rand = (rand.Next(0, (int)((60.00 / Vars.PedestrianFrequency) * .5 * 10))) * .1;
            for (int i = 0; i < Maps.Directions.Length; i++)
            {
               // RecentlySpawnedVehicleIDs[i] = new int[Vars.NumberOfLanes];
                //SpawningSlots[i] = new bool[Vars.NumberOfLanes];
                for (int j = 0; j< Vars.NumberOfLanes; j++)
                {
                    SpawningSlots[i,j] = true;
                    RecentlySpawnedVehicleIDs[i,j] = 0;
                }
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            for(int i=0;i<Vehicle.ModelSpriteFileNames.Length;i++)
            {
                Vehicle.ModelSprites[i] = Content.Load<Texture2D>("images/"+Vehicle.ModelSpriteFileNames[i]);
            }
            //Vehicle.Sprite = Content.Load<Texture2D>(Vehicle.SpriteFileName);

            pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });
            // create a 2d texture object of a single 1x1 pixel for manipulation & use later

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            Vars.ElapsedGameTime += Vars.DeltaTime; // add to total elapsed time the simulation has been running
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {  // if "esc" key is pressed, leave simulation
                Exit();
            }

            Cleared = false;

            //////////////////////////////// SELECTING VEHICLE CODE ////////////////////////////////////
            double dist = 9999999; // set overly large number to start
            MouseState mouse = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            // finding which car the mouse is closest to, allowing us to hover over cars and therefore selecting them
            if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < Vehicles.Count; i++)
                {
                    //System.Diagnostics.Debug.WriteLine("Mouse Checking");
                    if (Math.Abs(Methods.Distance(mousePos, Vehicles[i].Center)) < dist && Math.Abs(Methods.Distance(mousePos, Vehicles[i].Center)) < Vehicle.Height * 3)
                    {
                        dist = Math.Abs(Methods.Distance(new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Vehicles[i].Center));
                        Vars.SelectedVehicleID = Vehicles[i].ID;
                    }
                }

            }
            oldMouse = Mouse.GetState();
            //Vector2 oldMousePos = new Vector2(oldMouse.X, oldMouse.Y);
            KeyboardState newState = Keyboard.GetState(); // get current state of keyboard

            // if H is pressed, cycle 3 modes of diagnostic visibility
            if (oldState.IsKeyUp(Keys.H) && newState.IsKeyDown(Keys.H)) // compare current to old state of keyboard
            {
                Hidden += 1;
                Hidden = Hidden % 3; // reset H value
                System.Diagnostics.Debug.WriteLine(Hidden);
            }
            // speed time up
            if (oldState.IsKeyUp(Keys.Up) && newState.IsKeyDown(Keys.Up)) // compare current to old state of keyboard
            {
                if (Paused)
                {
                    OldDeltaTime += (double).0167 / 4;
                }
                else
                {
                    Vars.DeltaTime += (double).0167 / 4;
                }

            }
            if (oldState.IsKeyUp(Keys.Delete) && newState.IsKeyDown(Keys.Delete))
            {
                Simulation.Airwaves.Remove(Simulation.Airwaves.Find(z => Protocol.DecodeState(z).ID == Vars.SelectedVehicleID));  // finds its own state in the airwave list by finding it's id and then removes this element from the list 
                Vehicles.Remove(Vehicles.Find(x => x.ID == Vars.SelectedVehicleID));
                Vars.SelectedVehicleID = 0;
                Vars.NumberOfActiveVehicles--;
            }
            /*if(oldState.IsKeyUp(Keys.O) && newState.IsKeyDown(Keys.O))
            {
                Vars.PPM += 1;
                Methods.Print("out");
                Vars.UpdateForPPM();
            }
            else if (oldState.IsKeyUp(Keys.I) && newState.IsKeyDown(Keys.I))
            {
                Vars.PPM -= 1;
                Vars.UpdateForPPM();
            }*/
            // slow time down
            if (oldState.IsKeyUp(Keys.Down) && newState.IsKeyDown(Keys.Down)) // compare current to old state of keyboard
            {
                if (Paused)
                {
                    OldDeltaTime -= (double).0167 / 4;
                }
                else
                {
                    Vars.DeltaTime -= (double).0167 / 4;
                }
            }
            // pausing the game
            if (oldState.IsKeyUp(Keys.Space) && newState.IsKeyDown(Keys.Space)) // compare current to old state of keyboard
            {
                if (Paused)  // if game is paused:
                {  // then unpause
                    Paused = false;
                    Vars.DeltaTime = OldDeltaTime;
                }
                else  // if game is not paused:
                {  // then pause
                    Paused = true;
                    OldDeltaTime = Vars.DeltaTime;
                    Vars.DeltaTime = 0;

                }
            }

            //// Set Lane Number For Next Car Spawned In ////
            if (newState.IsKeyDown(Keys.D1))
            {
                Vars.LaneNumber = 1;
            }
            if (newState.IsKeyDown(Keys.D2))
            {
                Vars.LaneNumber = 2;
            }
            else if (newState.IsKeyDown(Keys.D3))
            {
                Vars.LaneNumber = 3;
            }
            else if (newState.IsKeyDown(Keys.D4))
            {
                Vars.LaneNumber = 4;
            }
            else if (newState.IsKeyDown(Keys.D5))
            {
                Vars.LaneNumber = 5;
            }
            else if (newState.IsKeyDown(Keys.D6))
            {
                Vars.LaneNumber = 6;
            }

            //// change destination of selected vehicle ////
            if (newState.IsKeyDown(Keys.NumPad4) && Vehicles.Find(v => v.ID == Vars.SelectedVehicleID).LaneNumber == 1)
            {
                (Vehicles.Find(v => v.ID == Vars.SelectedVehicleID)).UpdateDestination(0);
            }
            else if (newState.IsKeyDown(Keys.NumPad8))
            {
                (Vehicles.Find(v => v.ID == Vars.SelectedVehicleID)).UpdateDestination(1);
            }
            else if (newState.IsKeyDown(Keys.NumPad6) && Vehicles.Find(v => v.ID == Vars.SelectedVehicleID).LaneNumber == Vars.NumberOfLanes)
            {
                (Vehicles.Find(v => v.ID == Vars.SelectedVehicleID)).UpdateDestination(2);
            }


            // generating cars going specific directions, but with varying/random characteristics
            if (oldState.IsKeyUp(Keys.S) && newState.IsKeyDown(Keys.S))
            {
                int i = Vehicles.Count;
                Vehicles.Add(new Vehicle(Vars.VehicleNumber, 1, Vars.LaneNumber));
                Vars.VehicleNumber += 1;
                System.Diagnostics.Debug.WriteLine("Vehicle Spawned Manually. Lane Number: " + Vars.LaneNumber);
                //Methods.Print(Vars.VehicleNumber);

                Vars.DirectionNumber = Vars.DirectionNumber % 4 + 1;
                Vars.NumberOfActiveVehicles += 1;
                Vars.LaneNumber = (Vars.LaneNumber % Vars.NumberOfLanes) + 1;
            }
            if (oldState.IsKeyUp(Keys.A) && newState.IsKeyDown(Keys.A))
            {
                int i = Vehicles.Count;
                Vehicles.Add(new Vehicle(Vars.VehicleNumber, 2, Vars.LaneNumber));
                Vars.VehicleNumber += 1;
                System.Diagnostics.Debug.WriteLine("Vehicle Spawned Manually");
                //Methods.Print(Vars.VehicleNumber);

                Vars.DirectionNumber = Vars.DirectionNumber % 4 + 1;
                Vars.NumberOfActiveVehicles += 1;
                Vars.LaneNumber = (Vars.LaneNumber % Vars.NumberOfLanes) + 1;
            }
            if (oldState.IsKeyUp(Keys.W) && newState.IsKeyDown(Keys.W))
            {
                int i = Vehicles.Count;
                Vehicles.Add(new Vehicle(Vars.VehicleNumber, 3, Vars.LaneNumber));
                Vars.VehicleNumber += 1;
                System.Diagnostics.Debug.WriteLine("Vehicle Spawned Manually");
                //Methods.Print(Vars.VehicleNumber);

                Vars.DirectionNumber = Vars.DirectionNumber % 4 + 1;
                Vars.NumberOfActiveVehicles += 1;
                Vars.LaneNumber = (Vars.LaneNumber % Vars.NumberOfLanes) + 1;
            }
            if (oldState.IsKeyUp(Keys.D) && newState.IsKeyDown(Keys.D))
            {
                int i = Vehicles.Count;
                Vehicles.Add(new Vehicle(Vars.VehicleNumber, 4, Vars.LaneNumber));
                Vars.VehicleNumber += 1;
                System.Diagnostics.Debug.WriteLine("Vehicle Spawned Manually");
                //Methods.Print(Vars.VehicleNumber);

                Vars.DirectionNumber = Vars.DirectionNumber % 4 + 1;
                Vars.NumberOfActiveVehicles += 1;
                Vars.LaneNumber = (Vars.LaneNumber % Vars.NumberOfLanes) + 1;
            }



            //reseting Delta Time Variable to Normal : Left and Right Arrow Keys
            if (newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Right)) // compare current to old state of keyboard
            {
                Vars.DeltaTime = (Double)(1.0 / 60.0);
                System.Diagnostics.Debug.WriteLine(Vars.DeltaTime);

            }

            // toggle visibility of ETAs for debugging in output console
            if (oldState.IsKeyUp(Keys.E) && newState.IsKeyDown(Keys.E)) // compare current to old state of keyboard
            {
                Vars.PrintETAs = !Vars.PrintETAs;
            }

            // reset/restart simulation
            if (oldState.IsKeyUp(Keys.R) && newState.IsKeyDown(Keys.R)) // compare current to old state of keyboard
            {
                Vars.VehicleNumber = 1;
                Vars.ElapsedGameTime = 0;
                Vars.LaneNumber = 0;
                Vars.SelectedVehicleID = 0;
                Vars.SecondElapsed = false;
                Vars.NumberOfActiveVehicles = 0;
                Vars.DirectionNumber = 0;
                Vars.Paused = false;
                Vars.VehiclesThrough = 0;
                Vehicles.Clear();
                Cleared = true;
                Airwaves.Clear();
            }

            // clear cars from screen :  C key
            if (oldState.IsKeyUp(Keys.C) && newState.IsKeyDown(Keys.C)) // compare current to old state of keyboard
            {
                Vehicles.Clear();
                System.Diagnostics.Debug.WriteLine("Vehicles Cleared");
                Cleared = true;
                Airwaves.Clear();
                Vars.NumberOfActiveVehicles = 0;
                Vars.VehicleNumber = 1; // reset to starting vehicle number of 0
                Vars.SelectedVehicleID = 0;
                for (int i = 0; i < Maps.Directions.Length; i++)
                {
                    for (int j = 0; j < Vars.NumberOfLanes; j++)
                    {
                        SpawningSlots[i, j] = true;
                        RecentlySpawnedVehicleIDs[i, j] = 0;
                    }
                }
            }


            oldState = newState;  // set the new state of the keyboard as the old state for next time

            if (!Paused)
            {
                // Detect if whole second has elapsed
                if (Vars.ElapsedGameTime % 1 <= Vars.DeltaTime)
                {
                    Vars.SecondElapsed = true;
                    System.Diagnostics.Debug.WriteLine("Second Elapsed");
                }
                else
                {
                    Vars.SecondElapsed = false;
                    CSI_ET += Vars.DeltaTime;
                    PSI_ET += Vars.DeltaTime;
                }
                for (int i = 0; i < Maps.Directions.Length; i++)
                {
                    for (int j = 0; j < Vars.NumberOfLanes; j++)
                    {
                        if (RecentlySpawnedVehicleIDs[i,j] != 0)
                        {
                            Vehicle v = Vehicles.Find(x => x.ID == RecentlySpawnedVehicleIDs[i,j]);
                            // if no car in the way
                            if (v.InIntersection || v.LeavingIntersection || (v.DistanceTravelled > Vars.PPM * 15))
                            {
                                SpawningSlots[i,j] = true;
                            }
                            else  // car in the way
                            {
                                SpawningSlots[i,j] = false;
                            }
                        }
                        else
                        {
                            SpawningSlots[i,j] = true;
                        }
                    }
                }
                //// Instantiate vehicles //// 
                double CSI = 60.00 / Vars.CarFrequency; // time between car spawns (Car Spawn Interval)                            
                // Create new Vehicle class randomly
                System.Random rand = new System.Random();
                if (Vars.CarFrequency != 0 && Vars.MaxNumberOfVehicles > 0 && Vars.DeltaTime > 0 && CSI_ET >= CSI && Vehicles.Count < Vars.MaxNumberOfVehicles)
                {
                    List<int[]> AvailableSlots = new List<int[]>();
                    for (int i = 0; i < Maps.Directions.Length; i++)
                    {
                        for (int j = 0; j < Vars.NumberOfLanes; j++)
                        {
                            if(SpawningSlots[i,j]==true)
                                AvailableSlots.Add(new int[2] { i + 1, j + 1 });
                        }
                    }
                    if (AvailableSlots.Count > 0)
                    {
                        int[] selectedslot = AvailableSlots[rand.Next(0, AvailableSlots.Count)];
                        CSI_ET = 0;


                        int directionnumber = selectedslot[0];
                        int lanenumber = selectedslot[1];
                        //LastDirectionSpawn = directionnumber;

                        if (RecentlySpawnedVehicleIDs[directionnumber - 1, lanenumber - 1] == 0)
                        {
                            Vehicles.Add(new Vehicle(Vars.VehicleNumber, dn: directionnumber, lane: lanenumber));
                            Methods.Print("Vehicle randomly spawned in empty lane");
                        }
                        else
                        {
                            Methods.Print("Vehicle randomly spawned in busy lane");
                            Vehicles.Add(new Vehicle(Vars.VehicleNumber, directionnumber, lane: lanenumber, speed: Vehicles.Find(x => x.ID == RecentlySpawnedVehicleIDs[directionnumber - 1, lanenumber - 1]).Velocity[0]));
                        }
                        RecentlySpawnedVehicleIDs[directionnumber - 1,Vehicles.Find(x => x.ID == Vars.VehicleNumber).LaneNumber - 1] = Vars.VehicleNumber;
                        Vars.VehicleNumber += 1;
                        System.Diagnostics.Debug.WriteLine("Vehicle Spawned Randomly");
                        //Methods.Print(Vars.VehicleNumber);
                        Vars.DirectionNumber += 1;
                        Vars.NumberOfActiveVehicles += 1;
                        Vars.LaneNumber = (Vars.LaneNumber % Vars.NumberOfLanes) + 1;
                    }
                }
                //// Instantiate Pedestrians //// 
                double PSI = 60.00 / Vars.PedestrianFrequency; // time between car spawns (Car Spawn Interval)  
                
                // Create new Vehicle class randomly
                rand = new System.Random();
                if (Vars.DeltaTime > 0 && PSI_ET >= PSI-PSI_rand)
                {
                    Methods.Print("pedestrian spawned in");
                    PSI_ET = 0;
                    pedestrian = new Pedestrian(rand.Next(1, 5));
                }


                Objects.Clear();
                //Objects.Add(new Vector2[] { new Vector2((float)(Vars.OriginSE.X - (Vars.LaneWidth * .5 + Vars.IntersectionCurve) * Vars.PPM), (float)Vars.OriginSE.Y) });
                if (pedestrian.Exists)
                {
                    pedestrian.Move();
                    Objects.Add(pedestrian.GetBoundingBox());
                    Methods.Print(pedestrian.GetBoundingBox().Length);
                }

                //// iterate through collection of all vehicles on screen ////
                for (int x = 0; x < Vehicles.Count; x++)
                {
                    Vehicle v = Vehicles[x];

                    // if car reaches edge of simulation's screen
                    if (Math.Abs(v.Position.X - Vars.Origin.X) - (Vehicle.Height) > Vars.Origin.X || Math.Abs(v.Position.Y - Vars.Origin.Y) - (Vehicle.Height) > Vars.Origin.Y)
                    {
                        // delete car
                        if (Vars.SelectedVehicleID == v.ID)
                        {
                            Vars.SelectedVehicleID = 0;
                        }
                        for (int i = 0; i < Maps.Directions.Length; i++)
                        {
                            for (int j = 0; j < Vars.NumberOfLanes; j++)
                            {
                                if(RecentlySpawnedVehicleIDs[i,j] == v.ID)
                                {
                                    RecentlySpawnedVehicleIDs[i, j] = 0;
                                }
                            }
                        }
                                Simulation.Airwaves.Remove(Simulation.Airwaves.Find(z => Protocol.DecodeState(z).ID == v.ID));  // finds its own state in the airwave list by finding it's id and then removes this element from the list 
                        Vars.NumberOfActiveVehicles -= 1;
                        Vars.VehiclesThrough += 1;
                        Vehicles.Remove(v);
                    }
                    // run all cars' AI


                    // add vehicle's bounding box to Objects list
                    /*Vector2[] objectboundingbox = new Vector2[v.GetBoundingBox().Length];  // temporary array to aid in multiplying bounding boxes by LiDAR uncertainty
                    for (int i=0;i<objectboundingbox.Length;i++)
                    {
                        objectboundingbox[i] = new Vector2((float)(v.GetBoundingBox()[i].X * (1+Vars.LiDARUncertainty)), (float)(v.GetBoundingBox()[i].Y * (1 + Vars.LiDARUncertainty)));
                    }
                    Objects.Add(objectboundingbox);*/
                    // add vehicle's modified bounding box
                    //Objects.Add(v.GetBoundingBox());
                }
                for (int x = 0; x < Vehicles.Count; x++)
                {
                    Vehicle v = Vehicles[x];

                    v.LiDAR = new List<Vector2[]>(Objects);
                    v.Update(gameTime);
                }
                
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.ForestGreen); // set background color 

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            font = Content.Load<SpriteFont>("Output");

            // drawing roads
            {
                // N Road
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginNW.X, 0, (int)(Vars.InnerOriginNE.X - Vars.InnerOriginNW.X), (int)Vars.InnerOriginNW.Y), Color.Black);
                // E Road
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginNE.X, (int)Vars.InnerOriginNE.Y, (int)Vars.InnerOriginNW.X, (int)(Vars.InnerOriginSE.Y - Vars.InnerOriginNE.Y)), Color.Black);
                // S Road
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginSW.X, (int)Vars.InnerOriginSW.Y, (int)(Vars.StreetWidthPixels * 2), (int)Vars.InnerOriginNW.Y), Color.Black);
                // W Road
                spriteBatch.Draw(pixel, new Rectangle(0, (int)Vars.InnerOriginNW.Y, (int)Vars.InnerOriginNW.X, (int)(Vars.StreetWidthPixels * 2)), Color.Black);
                // inner square (intersection)
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginNW.X, (int)Vars.InnerOriginNW.Y, (int)(Vars.StreetWidthPixels * 2) + 1, (int)(Vars.StreetWidthPixels * 2) + 1), Color.Black);

                // NW Corner Curve
                for (int x = 0; x < Vars.IntersectionCurve * Vars.PPM; x++)
                {
                    int y = (int)(-Math.Sqrt(Math.Pow(Vars.IntersectionCurve * Vars.PPM, 2) - Math.Pow(x, 2)) + Vars.IntersectionCurve * Vars.PPM);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vars.OriginNW.X + x, (int)Vars.InnerOriginNW.Y - y, 1, y), Color.Black);
                }
                // NE Corner Curve
                for (int x = 0; x < Vars.IntersectionCurve * Vars.PPM; x++)
                {
                    int y = (int)(-Math.Sqrt(Math.Pow(Vars.IntersectionCurve * Vars.PPM, 2) - Math.Pow(x - Vars.IntersectionCurve * Vars.PPM, 2)) + Vars.IntersectionCurve * Vars.PPM);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginNE.X + x, (int)Vars.InnerOriginNE.Y - y, 1, y), Color.Black);
                }
                // SW Corner Curve
                for (int x = 0; x < Vars.IntersectionCurve * Vars.PPM; x++)
                {
                    int y = (int)(-Math.Sqrt(Math.Pow(Vars.IntersectionCurve * Vars.PPM, 2) - Math.Pow(x, 2)) + Vars.IntersectionCurve * Vars.PPM);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vars.OriginSW.X + x, (int)Vars.InnerOriginSW.Y - 1, 1, y), Color.Black);
                }
                // SE Corner Curve
                for (int x = 0; x < Vars.IntersectionCurve * Vars.PPM; x++)
                {
                    int y = (int)(-Math.Sqrt(Math.Pow(Vars.IntersectionCurve * Vars.PPM, 2) - Math.Pow(x - Vars.IntersectionCurve * Vars.PPM, 2)) + Vars.IntersectionCurve * Vars.PPM);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginSE.X + x - 1, (int)Vars.InnerOriginSE.Y, 1, y), Color.Black);
                }
                // east road lines5
                for (int i = 1; i < Vars.NumberOfLanes * 2; i++)
                {
                    int y = (int)(Vars.InnerOriginNW.Y + (i * Vars.PPM * Vars.LaneWidth));

                    if (i == Vars.NumberOfLanes)
                    {
                        spriteBatch.Draw(pixel, new Rectangle((int)(Vars.OriginNE.X), (int)(y - (2 * Vars.DashedLineThickness)), (int)Vars.InnerOriginNW.X, (int)(Vars.DashedLineThickness)), Color.Yellow);
                        spriteBatch.Draw(pixel, new Rectangle((int)(Vars.OriginNE.X), (int)(y + (2 * Vars.DashedLineThickness)), (int)Vars.InnerOriginNW.X, (int)(Vars.DashedLineThickness)), Color.Yellow);
                    }
                    else
                    {
                        for (int i2 = 0; i2 < Vars.OriginNW.X / ((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM); i2++)
                        {
                            int dist = (int)(((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM * i2) + Vars.OriginNE.X);
                            spriteBatch.Draw(pixel, new Rectangle(dist, (int)(y - (Vars.DashedLineThickness / 2)), (int)(Vars.DashedLineLength * Vars.PPM), (int)Vars.DashedLineThickness), Color.White);
                        }
                    }
                }

                // south road lines
                for (int i = 1; i < Vars.NumberOfLanes * 2; i++)
                {
                    int x = (int)(Vars.InnerOriginSW.X + (i * Vars.PPM * Vars.LaneWidth));

                    if (i == Vars.NumberOfLanes)  // draw double yellow line
                    {
                        spriteBatch.Draw(pixel, new Rectangle((int)(x - (2 * Vars.DashedLineThickness)), (int)(Vars.OriginSW.Y), (int)Vars.DashedLineThickness, (int)Vars.OriginNW.Y), Color.Yellow);
                        spriteBatch.Draw(pixel, new Rectangle((int)(x + (2 * Vars.DashedLineThickness)), (int)(Vars.OriginSW.Y), (int)Vars.DashedLineThickness, (int)Vars.OriginNW.Y), Color.Yellow);
                    }
                    else  // draw dashed white lines
                    {
                        for (int i2 = 0; i2 < Vars.OriginNW.Y / ((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM); i2++)
                        {
                            int dist = (int)(((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM * i2) + Vars.OriginSW.Y);
                            spriteBatch.Draw(pixel, new Rectangle((int)(x - (Vars.DashedLineThickness / 2)), dist, (int)Vars.DashedLineThickness, (int)(Vars.DashedLineLength * Vars.PPM)), Color.White);
                        }
                    }
                }
                // west road lines            
                for (int i = 1; i < Vars.NumberOfLanes * 2; i++)
                {
                    int y = (int)(Vars.InnerOriginNW.Y + (i * Vars.PPM * Vars.LaneWidth));

                    if (i == Vars.NumberOfLanes)
                    {
                        spriteBatch.Draw(pixel, new Rectangle(0, (int)(y - (2 * Vars.DashedLineThickness)), (int)Vars.OriginNW.X, (int)(Vars.DashedLineThickness)), Color.Yellow);
                        spriteBatch.Draw(pixel, new Rectangle(0, (int)(y + (2 * Vars.DashedLineThickness)), (int)Vars.OriginNW.X, (int)(Vars.DashedLineThickness)), Color.Yellow);
                    }
                    else
                    {
                        for (int i2 = 0; i2 < Vars.OriginNW.X / ((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM); i2++)
                        {
                            int dist = (int)(((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM * i2));
                            spriteBatch.Draw(pixel, new Rectangle(dist, (int)(y - (Vars.DashedLineThickness / 2)), (int)(Vars.DashedLineLength * Vars.PPM), (int)Vars.DashedLineThickness), Color.White);
                        }
                    }
                }
                ////// north road lines //////
                for (int i = 1; i < Vars.NumberOfLanes * 2; i++)
                {
                    int x = (int)(Vars.InnerOriginSW.X + (i * Vars.PPM * Vars.LaneWidth));

                    if (i == Vars.NumberOfLanes)
                    {
                        spriteBatch.Draw(pixel, new Rectangle((int)(x - (2 * Vars.DashedLineThickness)), 0, (int)Vars.DashedLineThickness, (int)Vars.OriginNW.Y), Color.Yellow);
                        spriteBatch.Draw(pixel, new Rectangle((int)(x + (2 * Vars.DashedLineThickness)), 0, (int)Vars.DashedLineThickness, (int)Vars.OriginNW.Y), Color.Yellow);
                    }
                    else
                    {
                        for (int i2 = 0; i2 < Vars.OriginNW.Y / ((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM); i2++)
                        {
                            int dist = (int)(((Vars.DashedLineLength + Vars.DashedLineSpacing) * Vars.PPM * i2));
                            spriteBatch.Draw(pixel, new Rectangle((int)(x - (Vars.DashedLineThickness / 2)), dist, (int)Vars.DashedLineThickness, (int)(Vars.DashedLineLength * Vars.PPM)), Color.White);
                        }
                    }
                }
                // intersection stopping line
                //spriteBatch.Draw(pixel, new Rectangle((int)(Vars.OriginNW.X + Vars.IntersectionCurve * Vars.PPM), (int)(Vars.OriginNW.Y), (int)Vars.StreetWidthPixels, (int)Vars.DashedLineThickness), Color.White);
            }


            // mark origins
            {/*
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.OriginNW.X-2, (int)Vars.OriginNW.Y - 2, 5, 5), Color.Red);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.OriginNE.X - 2, (int)Vars.OriginNE.Y - 2, 5, 5), Color.Red);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.OriginSE.X - 2, (int)Vars.OriginSE.Y - 2, 5, 5), Color.Red);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.OriginSW.X - 2, (int)Vars.OriginSW.Y - 2, 5, 5), Color.Red);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.Origin.X - 2, (int)Vars.Origin.Y - 2, 5, 5), Color.Purple);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginNW.X - 2, (int)Vars.InnerOriginNW.Y - 2, 5, 5), Color.Blue);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginNE.X - 2, (int)Vars.InnerOriginNE.Y - 2, 5, 5), Color.Blue);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginSE.X - 2, (int)Vars.InnerOriginSE.Y - 2, 5, 5), Color.Blue);
                spriteBatch.Draw(pixel, new Rectangle((int)Vars.InnerOriginSW.X - 2, (int)Vars.InnerOriginSW.Y - 2, 5, 5), Color.Blue);*/
            }


            // draw debug text
            if (Vars.ElapsedGameTime > 0)
            {
                // Debugging Text // 
                if (Hidden == 1)
                {
                    int height = 20;
                    if (Vehicles.Count > 0 && Vars.SelectedVehicleID != 0)
                    {

                        Vehicle HoveredVehicle = Vehicles.Find(v => v.ID == Vars.SelectedVehicleID);
                        spriteBatch.DrawString(font, "ID: " + HoveredVehicle.ID + " ; Model Number: " + HoveredVehicle.ModelNumber, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Destination: " + HoveredVehicle.Destination + " #: " + HoveredVehicle.DestinationNumber, new Vector2(20, height), Color.Red); height += 50;
                        //spriteBatch.DrawString(font, "Direction: " + HoveredVehicle.DirectionNumber + ": "+ HoveredVehicle.Direction, new Vector2(20, height), Color.Red); height += 50;
                        //spriteBatch.DrawString(font, "LaneNumber: " + HoveredVehicle.LaneNumber, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Throttle: " + Math.Round(HoveredVehicle.ThrottleTarget, 3) + "  Applied Throttle: " + Math.Round(HoveredVehicle.AppliedThrottle, 3), new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Velocity: " + Math.Round(HoveredVehicle.Velocity[0], 1) + " ; " + Methods.RadToDeg(HoveredVehicle.Velocity[1]) + " degrees", new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Acceleration: " + Math.Round(HoveredVehicle.Acceleration, 1), new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Interesction Distance: " + Math.Round(HoveredVehicle.IntersectionDistance) + "Distance Traveled: "+HoveredVehicle.DistanceTravelled, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "In Intersection?: " + HoveredVehicle.InIntersection, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Leaving Intersection?: " + HoveredVehicle.LeavingIntersection, new Vector2(20, height), Color.Red); height += 50;
                        //spriteBatch.DrawString(font, "Vehicle Position (x,y): " + HoveredVehicle.Position, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "ObjectAheadDistance: " + Math.Round(HoveredVehicle.ObjectAheadDistance, 2) + " ; FollowingDistance: " + Math.Round(HoveredVehicle.FollowingDistance, 2), new Vector2(20, height), Color.Red); height += 50;
                        //spriteBatch.DrawString(font, "Car Ahead ID: " + HoveredVehicle.CarAhead.ID, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "In Front?: " + HoveredVehicle.InFront, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, HoveredVehicle.PotentialCollisions, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "ETATally: " + HoveredVehicle.ETATally + "; ETADiffTotal: " + HoveredVehicle.ETADiffTotal, new Vector2(20, height), Color.Red); height += 50;
                        spriteBatch.DrawString(font, "Number of ETAs: " + HoveredVehicle.NumberOfETAs, new Vector2(20, height), Color.Red); height += 50;
                    }
                    //spriteBatch.DrawString(font, ": " + , new Vector2(20, height), Color.Red); height += 50;
                    height = 670;
                    spriteBatch.DrawString(font, "Seconds Elapsed: " + Math.Round(Vars.ElapsedGameTime, 2), new Vector2(Vars.WindowWidth - 500, height), Color.Red); height += 50;
                    spriteBatch.DrawString(font, "Time Factor: " + Math.Round(Vars.DeltaTime / .0166667, 2) + "x", new Vector2(Vars.WindowWidth - 500, height), Color.Red); height += 50;
                    spriteBatch.DrawString(font, "Intersection ID: " + Maps.IntersectionID, new Vector2(Vars.WindowWidth - 500, height), Color.Red); height += 50;
                    spriteBatch.DrawString(font, "Number of Active Vehicles: " + Vars.NumberOfActiveVehicles, new Vector2(Vars.WindowWidth - 500, height), Color.Red); height += 50;
                    spriteBatch.DrawString(font, "Vehicles Through: " + Vars.VehiclesThrough, new Vector2(Vars.WindowWidth - 500, height), Color.Red); height += 50;
                    spriteBatch.DrawString(font, "Vehicles Through Per Second: " + Math.Round(Vars.VehicleNumber / Vars.ElapsedGameTime, 2), new Vector2(Vars.WindowWidth - 500, height), Color.Red); height += 50;
                    //spriteBatch.DrawString(font, "LaneNumber: " + Vars.LaneNumber, new Vector2(Vars.WindowWidth - 500, height), Color.Black); height += 50;
                    //spriteBatch.DrawString(font, ": " + , new Vector2(Vars.WindowWidth - 500, height), Color.Black); height += 50; }

                }
                // Scoreboard Text //
                if (Hidden == 2)
                {
                    spriteBatch.DrawString(font, "Time Elapsed: " + Math.Floor(Vars.ElapsedGameTime / 60.0) + ":" + Math.Floor(Vars.ElapsedGameTime % 60) + ":" + (int)((Math.Round(Vars.ElapsedGameTime, 2) - Math.Floor(Vars.ElapsedGameTime)) * 100), new Vector2(Vars.OriginSE.X, Vars.OriginSE.Y), Color.Black);
                    spriteBatch.DrawString(font, "Vehicles Through: " + Vars.VehiclesThrough, new Vector2(Vars.OriginSE.X, Vars.OriginSE.Y + 40), Color.Black);
                    spriteBatch.DrawString(font, "Vehicles Per Second: " + Math.Round(Vars.VehicleNumber / Vars.ElapsedGameTime, 2), new Vector2(Vars.OriginSE.X, Vars.OriginSE.Y + 80), Color.Black);
                }

            }



#pragma warning disable CS0618 // Type or member is obsolete

            if (pedestrian.Exists)
                spriteBatch.Draw(pixel, new Rectangle((int)Objects[0][0].X - (int)(Vars.PedestrianThiccness * Vars.PPM * .5), (int)Objects[0][0].Y-(int)(Vars.PedestrianThiccness*Vars.PPM*.5), (int)(Vars.PedestrianThiccness * Vars.PPM), (int)(Vars.PedestrianThiccness * Vars.PPM)), Color.HotPink);


            // drawing the vehicle sprites
            if (Vars.MaxNumberOfVehicles > 0)
            {
                for (int i = 0; i < Vehicles.Count; i++)
                {
                    if (i == Vehicles.FindIndex(x => x.ID == Vars.SelectedVehicleID))
                    {
                        spriteBatch.Draw(Vehicles[i].Sprite, Vehicles[i].Position, null, null, Vehicle.Origin, (float)-Vehicles[i].Velocity[1], new Vector2(Vehicle.Xscale, Vehicle.Yscale), Color.DarkSlateGray);
                    }
                    else
                    {
                        spriteBatch.Draw(Vehicles[i].Sprite, Vehicles[i].Position, null, null, Vehicle.Origin, (float)-Vehicles[i].Velocity[1], new Vector2(Vehicle.Xscale, Vehicle.Yscale), Vehicle.ModelColors[Vehicles[i].ModelNumber]);
                    }
                    if (Vars.ElapsedGameTime % 1 < .5)
                    {
                        if (Vehicles[i].DestinationNumber == 0)
                        {
                            spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].TopLeftCorner.X, (int)Vehicles[i].TopLeftCorner.Y, 4, 4), Color.Yellow);
                        }
                        else if (Vehicles[i].DestinationNumber == 2)
                        {
                            spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].TopRightCorner.X, (int)Vehicles[i].TopRightCorner.Y, 4, 4), Color.Yellow);
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                    /*
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].RightLowerCenter.X - 1, (int)Vehicles[i].RightLowerCenter.Y - 1, 2, 2), Color.Purple);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].RightUpperCenter.X - 1, (int)Vehicles[i].RightUpperCenter.Y - 1, 2, 2), Color.Purple);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].LeftLowerCenter.X - 1, (int)Vehicles[i].LeftLowerCenter.Y - 1, 2, 2), Color.Purple);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].LeftUpperCenter.X - 1, (int)Vehicles[i].LeftUpperCenter.Y - 1, 2, 2), Color.Purple);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].BottomLeftCorner.X - 1, (int)Vehicles[i].BottomLeftCorner.Y - 1, 2, 2), Color.Green);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].BottomRightCorner.X - 1, (int)Vehicles[i].BottomRightCorner.Y - 1, 2, 2), Color.Green);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].TopCenter.X - 1, (int)Vehicles[i].TopCenter.Y - 1, 2, 2), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].BottomCenter.X - 1, (int)Vehicles[i].BottomCenter.Y - 1, 2, 2), Color.Yellow);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].LeftCenter.X - 1, (int)Vehicles[i].LeftCenter.Y - 1, 2, 2), Color.Red);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].RightCenter.X - 1, (int)Vehicles[i].RightCenter.Y - 1, 2, 2), Color.Red);
                    spriteBatch.Draw(pixel, new Rectangle((int)Vehicles[i].Center.X - 1, (int)Vehicles[i].Center.Y - 1, 2, 2), Color.White);*/
                }
            }
            spriteBatch.Draw(pixel, new Rectangle((int)Mouse.GetState().X, (int)Mouse.GetState().Y, 4, 4), Color.LightCyan);
            //if (Collision) { spriteBatch.Draw(Explosion, CollisionPoint, null, null, new Vector2(498 / 2, 452 / 2), 0, new Vector2(Vehicle.Height / 498, Vehicle.Height / 452), Color.White); }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }


    public class Pedestrian
    {
        public double Speed;
        public bool Exists = false;
        public int DirectionNumber = 0;
        public Vector2 Position = new Vector2();
        public int StreetSide = -1;  // -1 = left, 1 = right
        public double Dist = 0;

        public Pedestrian(int dn)
        {
            Exists = true;
            System.Random rand = new System.Random();
            DirectionNumber = dn;
            Speed = Vars.AvgPedestrianSpeed + rand.Next(-1, 1);
            if (DirectionNumber == 1)  // norf
            {
                Position = new Vector2((float)(Vars.Origin.X - StreetSide * (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)), (float)(Vars.Origin.Y - (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)));
            }
            else if (DirectionNumber == 2)  // east
            {
                Position = new Vector2((float)(Vars.Origin.X - (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)), (float)(Vars.Origin.Y + StreetSide * (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)));
            }
            else if (DirectionNumber == 3)  // south
            {
                Position = new Vector2((float)(Vars.Origin.X + StreetSide * (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)), (float)(Vars.Origin.Y + (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)));
            }
            else  // west
            {
                Position = new Vector2((float)(Vars.Origin.X + (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)), (float)(Vars.Origin.Y - StreetSide * (Vars.StreetWidthPixels + Vars.IntersectionCurve * Vars.PPM)));

            }
        }
        public Pedestrian()
        {
            Exists = false;
        }
        public Vector2[] GetBoundingBox()
        {
            Vector2[] array = {
                Position,
                new Vector2((float)(Position.X - Vars.PedestrianThiccness * Vars.PPM), (float)(Position.Y - Vars.PedestrianThiccness * Vars.PPM)),
                new Vector2((float)(Position.X + Vars.PedestrianThiccness * Vars.PPM), (float)(Position.Y - Vars.PedestrianThiccness * Vars.PPM)),
                new Vector2((float)(Position.X + Vars.PedestrianThiccness * Vars.PPM), (float)(Position.Y + Vars.PedestrianThiccness * Vars.PPM)),
                new Vector2((float)(Position.X - Vars.PedestrianThiccness * Vars.PPM), (float)(Position.Y + Vars.PedestrianThiccness * Vars.PPM)),
                new Vector2((float)(Position.X), (float)(Position.Y - Vars.PedestrianThiccness * Vars.PPM)),
                new Vector2((float)(Position.X + Vars.PedestrianThiccness * Vars.PPM), (float)(Position.Y)),
                new Vector2((float)(Position.X), (float)(Position.Y + Vars.PedestrianThiccness * Vars.PPM)),
                new Vector2((float)(Position.X - Vars.PedestrianThiccness * Vars.PPM), (float)(Position.Y)) };
            return array;
        }
        public void Move()
        {
            if (Dist > 2*(Vars.StreetWidthPixels+Vars.IntersectionCurve*Vars.PPM))
            {
                Exists = false;
            }
            else
            {
                if (DirectionNumber == 1)  // norf
                {
                    float dx = (float)(StreetSide * Methods.KmhToMs(Speed) * Vars.PPM * Vars.DeltaTime);
                    Dist += Math.Abs(dx);
                    Position.X += dx;
                }
                else if (DirectionNumber == 2)  // east
                {
                    float dy = (float)(StreetSide * Methods.KmhToMs(Speed) * Vars.PPM * Vars.DeltaTime);
                    Dist += Math.Abs(dy);
                    Position.Y -= dy;
                }
                else if (DirectionNumber == 3)  // south
                {
                    float dx = (float)(StreetSide * Methods.KmhToMs(Speed) * Vars.PPM * Vars.DeltaTime);
                    Dist += Math.Abs(dx);
                    Position.X -= dx;
                }
                else  // west
                {
                    float dy = (float)(StreetSide * Methods.KmhToMs(Speed) * Vars.PPM * Vars.DeltaTime);
                    Dist += Math.Abs(dy);
                    Position.Y += dy;
                }
            }
        }
    }

}
