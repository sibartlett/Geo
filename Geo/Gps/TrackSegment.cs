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
            Fixes = new List<Fix>();
        }

        public List<Fix> Fixes { get; set; }

        public LineString ToLineString()
        {
            return new LineString(Fixes.Select(x => x.Coordinate));
        }

        public bool IsEmpty()
        {
            return Fixes.Count == 0;
        }

        public Fix GetFirstFix()
        {
            return IsEmpty() ? default(Fix) : Fixes[0];
        }

        public Fix GetLastFix()
        {
            return IsEmpty() ? default(Fix) : Fixes[Fixes.Count - 1];
        }

        public Speed GetAverageSpeed()
        {
            return new Speed(GetLength().SiValue, GetDuration());
        }

        public TimeSpan GetDuration()
        {
            return GetLastFix().TimeUtc - GetFirstFix().TimeUtc;
        }

        public Distance GetLength()
        {
            return ToLineString().GetLength();
        }

        public void Quantize(double seconds = 0)
        {
            var fixes = new List<Fix>();
            Fix lastFix = null;
            foreach (var fix in Fixes)
            {
                if (lastFix == null || Math.Abs((fix.TimeUtc - lastFix.TimeUtc).TotalSeconds) >= seconds)
                {
                    lastFix = fix;
                    fixes.Add(fix);
                }
            }
            Fixes = fixes;
        }
    }
}