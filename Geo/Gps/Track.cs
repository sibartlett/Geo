using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Metadata;
using Geo.Measure;

namespace Geo.Gps
{
    public class Track : IHasLength
    {
        public Track()
        {
            Metadata = new TrackMetadata();
            Segments = new List<TrackSegment>();
        }

        public TrackMetadata Metadata { get; private set; }
        public List<TrackSegment> Segments { get; set; }

        public LineString ToLineString()
        {
            return new LineString(Segments.SelectMany(x=>x.Waypoints).Select(x => x.Coordinate));
        }

        public TrackSegment GetFirstSegment()
        {
            return Segments.Count == 0 ? default(TrackSegment) : Segments[0];
        }

        public TrackSegment GetLastSegment()
        {
            return Segments.Count == 0 ? default(TrackSegment) : Segments[Segments.Count - 1];
        }

        public IEnumerable<Waypoint> GetAllFixes()
        {
            return Segments.SelectMany(x => x.Waypoints);
        }

        public Waypoint GetFirstWaypoint()
        {
            var segment = GetFirstSegment();
            return segment == null ? default(Waypoint) : segment.GetFirstWaypoint();
        }

        public Waypoint GetLastWaypoint()
        {
            var segment = GetLastSegment();
            return segment == null ? default(Waypoint) : segment.GetLastWaypoint();
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

        public void Quantize(double seconds = 0)
        {
            foreach (var segment in Segments)
                segment.Quantize(seconds);
        }

        public Distance GetLength()
        {
            return ToLineString().GetLength();
        }
    }
}