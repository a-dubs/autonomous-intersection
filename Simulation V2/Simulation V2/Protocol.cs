using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation_V2
{
    class Protocol
    {
        /* ///// State ///// 
         * current intersection  ID,
         * intersection leaving ID,
         * road ID,
         * vehicle ID,
         * direction (number), 
         * destination (number), 
         * lane number, 
         * distance to intersection, 
         * speed, 
         * acceleration, 
         * distance to car ahead, 
         * distance to car behind
         * in front?
        */

        /*public static Vehicle DecodeState(double[] State)
        {
            Vehicle v = new Vehicle();
            v.CurrentIntersectionID = (int)State[0]; // 64 bits
            v.RoadID = (int)State[1];  // 64 bits
            v.ID = (int) State[2];  // 40 bits
            v.DirectionNumber = (int) State[3]; // 3 bits
            v.Direction = Vehicle.DirectionConverter(State[3]); 
            v.DestinationNumber = (int) State[4]; // 2 bits
            v.LaneNumber = (int) State[5]; // 4 bits
            v.IntersectionDistance = State[6]; // 32 bits
            v.Velocity[0] = State[7]; // 17 bits // 1 
            v.Acceleration = State[8]; // 16 bits 
            v.TurningRadius = State[9]; // 8 bits
            if (State[10] == 1) // 1 bit
                v.InFront = true;
            else
                v.InFront = false;
            if (State[11] == 1) // 1 bit
                v.InIntersection = true;
            else
                v.InIntersection = false;
            return v;
        }*/

        //// Decodes the vehicles state from bits (bytes) to a useable vehicle object
        public static Vehicle DecodeState(byte[][] bytearray)
        {
            Vehicle v = new Vehicle();
            v.CurrentIntersectionID = BitConverter.ToInt32(bytearray[0],0);
            v.RoadID = BitConverter.ToInt32(bytearray[1], 0);
            v.ID = BitConverter.ToInt32(bytearray[2], 0);
            v.DirectionNumber = BitConverter.ToInt32(bytearray[3], 0);
            v.DestinationNumber = BitConverter.ToInt32(bytearray[4], 0);
            v.LaneNumber = BitConverter.ToInt32(bytearray[5], 0);
            v.IntersectionDistance = BitConverter.ToInt32(bytearray[6],0);
            v.Velocity[0] = BitConverter.ToInt32(bytearray[7], 0)* BitConverter.ToInt32(bytearray[8], 0) *.01;
            v.Acceleration = BitConverter.ToInt32(bytearray[9], 0) * BitConverter.ToInt32(bytearray[10], 0) * .01;
            v.TurningRadius = BitConverter.ToDouble(bytearray[11], 0)/ 10.0;
            v.InFront = BitConverter.ToBoolean(bytearray[12], 0);
            v.InIntersection = BitConverter.ToBoolean(bytearray[13], 0);
            v.SpeedInaccuracy = BitConverter.ToDouble(bytearray[14], 0);
            return v;
        }

        

        public static byte[][] EncodeState(Vehicle v)
        {
            List<byte[]> bytes = new List<byte[]>();
            bytes.Add(BitConverter.GetBytes(v.CurrentIntersectionID)); 
            bytes.Add(BitConverter.GetBytes(v.RoadID));
            bytes.Add(BitConverter.GetBytes(v.ID));
            bytes.Add(BitConverter.GetBytes(v.DirectionNumber));
            bytes.Add(BitConverter.GetBytes(v.DestinationNumber));
            bytes.Add(BitConverter.GetBytes(v.LaneNumber));
            bytes.Add(BitConverter.GetBytes((int)v.IntersectionDistance));
            bytes.Add(BitConverter.GetBytes((int)Math.Floor(v.Velocity[0]*100))); // => 16 bit integer // save 2 decimal places
            bytes.Add(BitConverter.GetBytes(Math.Sign(v.Velocity[0])));
            bytes.Add(BitConverter.GetBytes((int)Math.Floor(v.Acceleration * 100)));// => 16 bit integer  + 1 bit // save 2 decimal places
            bytes.Add(BitConverter.GetBytes(Math.Sign(v.Acceleration))); 
            bytes.Add(BitConverter.GetBytes(Math.Floor(v.TurningRadius * 10))); // => 8 bit integer // save 1 decimal places
            bytes.Add(BitConverter.GetBytes(v.InFront));
            bytes.Add(BitConverter.GetBytes(v.InIntersection));
            bytes.Add(BitConverter.GetBytes(v.SpeedInaccuracy));
            return bytes.ToArray();
        }
        /*public static double[] EncodeState(Vehicle v)
        {
            int infront = 0;
            if (v.InFront)
                infront = 1;
            int inintersection = 0;
            if (v.InIntersection)
                inintersection = 1;
            return new double[] { v.CurrentIntersectionID, v.RoadID, v.ID, v.DirectionNumber, v.DestinationNumber, v.LaneNumber, v.IntersectionDistance, v.Velocity[0], v.Acceleration, v.TurningRadius, infront, inintersection }; ;
        }*/
        


    }
}
