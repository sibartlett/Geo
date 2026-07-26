#nullable enable
namespace Geo.Abstractions.Interfaces;

public interface ISpatialEquatable
{
    bool Equals(object? obj, SpatialEqualityOptions options);
    bool Equals2D(object? obj);
    bool Equals3D(object? obj);

    /// <summary>
    /// A hash consistent with <see cref="Equals(object, SpatialEqualityOptions)" /> under
    /// the same <paramref name="options" />: values that compare equal under them must
    /// hash alike, though unequal ones may still collide.
    /// </summary>
    /// <remarks>
    /// Only <see cref="SpatialEqualityOptions.UseElevation" /> and
    /// <see cref="SpatialEqualityOptions.UseM" /> bear on the result.
    /// <see cref="SpatialEqualityOptions.PoleCoordiantesAreEqual" /> and
    /// <see cref="SpatialEqualityOptions.AntiMeridianCoordinatesAreEqual" /> do not,
    /// because the longitudes they govern are collapsed together whichever way those
    /// options are set - a hash may put unequal values in one bucket, so collapsing them
    /// always is correct under either setting and keeps the result the same under both.
    /// <para>
    /// This is what a composite geometry passes down to the geometries it holds, and what
    /// <see cref="Linq.Spatial2DComparer{TSource}" /> and
    /// <see cref="Linq.Spatial3DComparer{TSource}" /> hash through, so an implementation
    /// has to honour the two ordinate options rather than deferring to
    /// <see cref="object.GetHashCode()" />.
    /// </para>
    /// </remarks>
    int GetHashCode(SpatialEqualityOptions options);
}
