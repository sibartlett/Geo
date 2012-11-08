using System;
using Geo.Geometries;
using NUnit.Framework;
using Raven.Abstractions.Indexing;

namespace Geo.Tests.Raven
{
    [TestFixture]
    public class RavenTests : RavenTestFixtureBase
    {
        [Test]
        public void PointTests()
        {
            var point = new Point(0, 0);
            AssertThatIndexPropertyIsGenerated(point);
            AssertTrue(point, SpatialRelation.Within, new Circle(0, 0, 1));
            AssertFalse(point, SpatialRelation.Within, new Circle(0, 160, 600000));
            //AssertTrue(point, SpatialRelation.Disjoint, new Point(1, 1));
        }

        [Test]
        public void CircleTests()
        {
            var circle = new Circle(0, 0, 110000);
            AssertThatIndexPropertyIsGenerated(circle);
            AssertTrue(circle, SpatialRelation.Within, new Circle(0, 0, 220000));
            AssertFalse(circle, SpatialRelation.Within, new Circle(0, 160, 600000));
            AssertTrue(circle, SpatialRelation.Intersects, new Circle(1, 0, 110000));
            AssertFalse(circle, SpatialRelation.Intersects, new Circle(2, 0, 110000));
            //AssertTrue(point, SpatialRelation.Disjoint, new Point(1, 1));
        }

        [Test]
        public void LineStringTests()
        {
            var lineString = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(1, 2), new Coordinate(0, 0) });
            AssertThatIndexPropertyIsGenerated(lineString);
            AssertTrue(lineString, SpatialRelation.Within, new Circle(0, 0, 600000));
            AssertFalse(lineString, SpatialRelation.Within, new Circle(0, 160, 600000));
            //AssertTrue(lineString, SpatialRelation.Disjoint, new Circle(0, 160, 600000));
            AssertTrue(lineString, SpatialRelation.Intersects, new LineString(new[] { new Coordinate(1, 0), new Coordinate(0, 1) }));
            AssertTrue(lineString, SpatialRelation.Intersects, new LineString(new[] { new Coordinate(0, 1), new Coordinate(1, 1), new Coordinate(2, 1) }));
        }

        [Test]
        public void LinearRingTests()
        {
            var linearRing = new LinearRing(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 2), new Coordinate(0, 0) });
            AssertThatIndexPropertyIsGenerated(linearRing);
            AssertTrue(linearRing, SpatialRelation.Within, new Circle(0, 0, 600000));
            AssertFalse(linearRing, SpatialRelation.Within, new Circle(0, 160, 600000));
        }

        [Test]
        public void PolygonTests()
        {
            var polygon = new Polygon(new LinearRing(new[] { new Coordinate(0, 0), new Coordinate(5, 0), new Coordinate(0, 5), new Coordinate(0, 0) }));
            Console.WriteLine(polygon.ToWktString());
            AssertThatIndexPropertyIsGenerated(polygon);
            AssertTrue(polygon, SpatialRelation.Within, new Circle(0, 0, 600000));
            AssertFalse(polygon, SpatialRelation.Within, new Circle(0, 160, 600000));
            AssertTrue(polygon, SpatialRelation.Intersects, new Circle(0, 0, 110000));
            AssertFalse(polygon, SpatialRelation.Intersects, new Circle(0, 160, 110000));
        }
    }
}
