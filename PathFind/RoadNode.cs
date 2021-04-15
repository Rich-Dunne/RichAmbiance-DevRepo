using Rage;
using RichAmbiance.Utils;
using System;

namespace RichAmbiance.PathFind
{
    [Flags]
    internal enum RoadNodeFlags
    {
        Unknown = 0,
        Road = 2,
        SlowStopIndicator = 4,
        MinorRoad = 8,
        Alley = 10,
        Alley2 = 14,
        Unknown2 = 16,
        Unknown3 = 32,
        ParkingLot = 42,
        Freeway = 64,
        Intersection = 128,
        Stop = 256,
        StopBeforeIntersection = 512
    };

    internal class RoadNode
    {
        internal Vector3 Position { get; private set; }
        internal float Heading { get; private set; }
        internal Vector3 RoadSidePosition { get; private set; }
        internal bool IsAlley { get; private set; }
        internal RoadNode PreviousNode { get; set; }
        internal RoadNode NextNode { get; set; }

        internal RoadNode(Vector3 position, float heading, bool isAlley = false)
        {
            Position = position;
            Heading = heading;
            NativeWrappers.GetRoadsidePointWithHeading(position, heading, out Vector3 roadSidePoint);
            RoadSidePosition = roadSidePoint;
            IsAlley = isAlley;
        }
    }
}
