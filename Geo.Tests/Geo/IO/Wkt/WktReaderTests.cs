using NUnit.Framework;
using Geo.IO.Wkt;
using Geo.Geometries;

namespace Geo.Tests.Geo.IO.Wkt
{
    [TestFixture]
    public class WktReaderTests
    {
        [Test]
        public void Null()
        {
            var reader = new WktReader();

            var nothing = reader.Read("");
            Assert.AreEqual(null, nothing);
        }

        [Test]
        public void Point()
        {
            var reader = new WktReader();

            var xy = reader.Read("POINT (0.0 65.9)");
            Assert.AreEqual(new Point(65.9, 0), xy);

            var xyz = reader.Read("POINT Z (0.0 65.9 5)");
            Assert.AreEqual(new Point(65.9, 0, 5), xyz);

            var xyz2 = reader.Read("POINT (0.0 65.9 5)");
            Assert.AreEqual(new Point(65.9, 0, 5), xyz2);

            var xym = reader.Read("POINT M (0.0 65.9 5)");
            Assert.AreEqual(new Point(65.9, 0, double.NaN, 5), xym);

            var xyzm = reader.Read("POINT ZM (0.0 65.9 4 5)");
            Assert.AreEqual(new Point(65.9, 0, 4, 5), xyzm);

            var xyzm2 = reader.Read("POINT (0.0 65.9 4 5)");
            Assert.AreEqual(new Point(65.9, 0, 4, 5), xyzm2);

            var empty = reader.Read("POINT ZM EMPTY");
            Assert.AreEqual(global::Geo.Geometries.Point.Empty, empty);
        }

        [Test]
        public void LineString()
        {
            var reader = new WktReader();

            var xy = reader.Read("LINESTRING (0.0 65.9, -34.5 9)");
            Assert.AreEqual(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5)), xy);

            var empty = reader.Read("LINESTRING ZM EMPTY");
            Assert.AreEqual(new LineString(), empty);
        }

        [Test]
        public void Polygon()
        {
            var reader = new WktReader();

            var xy = reader.Read("POLYGON ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
            Assert.AreEqual(new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0))), xy);

            var empty = reader.Read("POLYGON ZM EMPTY");
            Assert.AreEqual(global::Geo.Geometries.Polygon.Empty, empty);
        }

        [Test]
        public void MultiPoint()
        {
            var reader = new WktReader();

            var none = reader.Read("MULTIPOINT (0.0 65.9, -34.5 9, -20 40, 0 65.9)");
            Assert.AreEqual(new MultiPoint(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)), none);
            
            var brackets = reader.Read("MULTIPOINT ((0.0 65.9), (-34.5 9), (-20 40), (0 65.9))");
            Assert.AreEqual(new MultiPoint(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)), brackets);

            var empty = reader.Read("MULTIPOINT ZM EMPTY");
            Assert.AreEqual(new MultiPoint(), empty);
        }

        [Test]
        public void MultiLineString()
        {
            var reader = new WktReader();

            var one = reader.Read("MULTILINESTRING ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
            Assert.AreEqual(new MultiLineString(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0))), one);


            var two = reader.Read("MULTILINESTRING ((0.0 65.9, -34.5 9, -20 40, 0 65.9), (0.0 65.9, -34.5 9, -20 40, 0 65.9))");
            Assert.AreEqual(new MultiLineString(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0)), new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0))), two);

            var empty = reader.Read("MULTILINESTRING ZM EMPTY");
            Assert.AreEqual(new MultiLineString(), empty);
        }

        [Test]
        public void MultiPolygon()
        {
            var reader = new WktReader();

            var one = reader.Read("MULTIPOLYGON (((0.0 65.9, -34.5 9, -20 40, 0 65.9)))");
            Assert.AreEqual(new MultiPolygon(new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0)))), one);


            var two = reader.Read("MULTIPOLYGON (((0.0 65.9, -34.5 9, -20 40, 0 65.9)),((0.0 65.9, -34.5 9, -20 40, 0 65.9)))");
            Assert.AreEqual(new MultiPolygon(new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0))), new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0)))), two);

            var empty = reader.Read("MULTIPOLYGON ZM EMPTY");
            Assert.AreEqual(new MultiPolygon(), empty);
        }
    }
}
