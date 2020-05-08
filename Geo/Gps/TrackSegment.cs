using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Gps
{
    public class TrackSegment : IHasLength
    {
        public TrackSegment()
        {
            Waypoints = new List<Waypoint>();
        }

        public List<Waypoint> Waypoints { get; set; }

        public LineString ToLineString()
        {
            return new LineString(Waypoints.Select(x => x.Coordinate));
        }

        public bool IsEmpty()
        {
            return Waypoints.Count == 0;
        }

        public Waypoint GetFirstWaypoint()
        {
            return IsEmpty() ? default(Waypoint) : Waypoints[0];
        }

        public Waypoint GetLastWaypoint()
        {
            return IsEmpty() ? default(Waypoint) : Waypoints[Waypoints.Count - 1];
        }

        public Speed GetAverageSpeed()
        {
            return new Speed(GetLength().SiValue, GetDuration());
        }

        public TimeSpan GetDuration()
        {
            if (GetFirstWaypoint().TimeUtc.HasValue && GetLastWaypoint().TimeUtc.HasValue)
                return GetLastWaypoint().TimeUtc.Value - GetFirstWaypoint().TimeUtc.Value;
            return TimeSpan.Zero;
        }

        public Distance GetLength()
        {
            return ToLineString().GetLength();
        }

        public void Quantize(double seconds = 0)
        {
            if (Waypoints.Any(x => !x.TimeUtc.HasValue)) {
                throw new NotSupportedException("All waypoints require a timestamp, for track segment to be quantized.");
            }

            var waypoints = new List<Waypoint>();
            Waypoint lastWaypoint = null;
            foreach (var waypoint in Waypoints)
            {
                if (lastWaypoint == null || Math.Abs((waypoint.TimeUtc.Value - lastWaypoint.TimeUtc.Value).TotalSeconds) >= seconds)
                {
                    lastWaypoint = waypoint;
                    waypoints.Add(waypoint);
                }
            }
            Waypoints = waypoints;
        }
    }
}