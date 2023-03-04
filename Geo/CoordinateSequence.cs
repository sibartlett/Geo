using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions;

namespace Geo;

public class CoordinateSequence : SpatialReadOnlyCollection<Coordinate>
{
    public CoordinateSequence() : base(new List<Coordinate>())
    {
    }

    public CoordinateSequence(IEnumerable<Coordinate> coordinates) : base(coordinates.ToList())
    {
    }

    public CoordinateSequence(params Coordinate[] coordinates) : base(coordinates.ToList())
    {
    }

    public bool HasElevation
    {
        get { return this.Any(x => x.Is3D); }
    }

    public bool HasM
    {
        get { return this.Any(x => x.IsMeasured); }
    }

    public bool IsClosed => Count > 2 && this[0].Equals(this[Count - 1]);

    public Envelope GetBounds()
    {
        return IsEmpty
            ? null
            : new Envelope(this.Min(x => x.Latitude), this.Min(x => x.Longitude), this.Max(x => x.Latitude),
                this.Max(x => x.Longitude));
    }

    public IEnumerable<LineSegment> ToLineSegments()
    {
        Coordinate last = null;
        foreach (var coordinate in this)
        {
            if (last != null)
                yield return new LineSegment(last, coordinate);
            last = coordinate;
        }
    }

    #region Equality methods

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(CoordinateSequence left, CoordinateSequence right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(CoordinateSequence left, CoordinateSequence right)
    {
        return !(left == right);
    }

    #endregion
}