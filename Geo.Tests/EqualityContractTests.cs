using System.Collections.Generic;
using Geo.Geodesy;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests;

/// <summary>
/// Sweeps every equatable type against every other and holds them to the two rules
/// <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode" /> are obliged
/// to keep, whatever a type decides to compare.
/// </summary>
/// <remarks>
/// Written after a base type and its subclass were found disagreeing about whether they
/// were equal - each type's own tests passed, because neither was asked about the other.
/// Only a sweep across the pairs catches that.
/// </remarks>
public class EqualityContractTests
{
    private static readonly Coordinate[] Ring =
    {
        new Coordinate(0, 0),
        new Coordinate(0, 1),
        new Coordinate(1, 1),
        new Coordinate(0, 0),
    };

    public static IEnumerable<object[]> Instances()
    {
        foreach (var instance in All())
            yield return new[] { instance };
    }

    private static IEnumerable<object> All()
    {
        yield return new Coordinate(1, 2);
        yield return new CoordinateZ(1, 2, 3);
        yield return new CoordinateM(1, 2, 3);
        yield return new CoordinateZM(1, 2, 3, 4);
        yield return new Point(1, 2);
        yield return new Point(1, 2, 3);
        yield return Point.Empty;
        yield return new LineString(Ring);
        yield return new LinearRing(Ring);
        yield return new Polygon(new LinearRing(Ring));
        yield return new Triangle(new Coordinate(0, 0), new Coordinate(0, 1), new Coordinate(1, 1));
        yield return new GeometryCollection(new Point(1, 2));
        yield return new MultiPoint(new Point(1, 2));
        yield return new MultiLineString(new LineString(Ring));
        yield return new MultiPolygon(new Polygon(new LinearRing(Ring)));
        yield return new LineSegment(new Coordinate(0, 0), new Coordinate(1, 1));
        yield return new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 5, 10, 20);
        yield return new Circle(new Coordinate(0, 0), 100);
        yield return new CoordinateSequence(Ring);
        yield return new Envelope(0, 0, 1, 1);
        yield return MultiPoint.Empty;
        yield return MultiPolygon.Empty;
        yield return GeometryCollection.Empty;
        yield return Polygon.Empty;
        yield return Triangle.Empty;
        yield return LineString.Empty;
        yield return LinearRing.Empty;
        yield return Circle.Empty;
    }

    [Fact]
    public void Equality_is_symmetric_across_every_pair_of_types()
    {
        var failures = new List<string>();

        foreach (var left in All())
        foreach (var right in All())
            if (left.Equals(right) != right.Equals(left))
                failures.Add(
                    $"{left.GetType().Name}.Equals({right.GetType().Name}) = {left.Equals(right)} "
                        + $"but {right.GetType().Name}.Equals({left.GetType().Name}) = {right.Equals(left)}"
                );

        Assert.Empty(failures);
    }

    [Fact]
    public void Values_that_compare_equal_share_a_hash_code()
    {
        var failures = new List<string>();

        foreach (var left in All())
        foreach (var right in All())
            if (left.Equals(right) && left.GetHashCode() != right.GetHashCode())
                failures.Add($"{left.GetType().Name} / {right.GetType().Name}");

        Assert.Empty(failures);
    }

    [Theory]
    [MemberData(nameof(Instances))]
    public void Equality_is_reflexive_and_rejects_null_and_foreign_types(object instance)
    {
        Assert.True(instance.Equals(instance));
        Assert.Equal(instance.GetHashCode(), instance.GetHashCode());
        Assert.False(instance.Equals(null));
        Assert.False(instance.Equals("not a geometry"));
    }
}
