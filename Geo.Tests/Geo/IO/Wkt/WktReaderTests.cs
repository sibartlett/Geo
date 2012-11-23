using System.IO;
using NUnit.Framework;
using Geo.IO.Wkt;
using Geo.Geometries;

namespace Geo.Tests.Geo.IO.Wkt
{
    [TestFixture]
    public class WktReaderTests
    {
        [Test]
        [ExpectedException]
        public void Invalid_geometry_type()
        {
            var reader = new WktReader();
            reader.Read("SOMETHING EMPTY");
        }

        [Test]
        [ExpectedException]
        public void Null_input_string_throws_argument_exception()
        {
            var reader = new WktReader();
            reader.Read((string) null);
        }

        [Test]
        [ExpectedException]
        public void Null_input_stream_throws_argument_exception()
        {
            var reader = new WktReader();
            reader.Read((Stream)null);
        }

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
            Assert.AreEqual(Geometries.Point.Empty, empty);
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
        public void LinearRing()
        {
            var reader = new WktReader();

            var xy = reader.Read("LINEARRING (0.0 65.9, -34.5 9, 5.0 65.9, 0.0 65.9)");
            Assert.AreEqual(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(65.9, 5), new Coordinate(65.9, 0)), xy);

            var empty = reader.Read("LINEARRING ZM EMPTY");
            Assert.AreEqual(new LinearRing(), empty);
        }

        [Test]
        public void Polygon()
        {
            var reader = new WktReader();

            var xy = reader.Read("POLYGON ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
            Assert.AreEqual(new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0))), xy);

            var empty = reader.Read("POLYGON ZM EMPTY");
            Assert.AreEqual(Geometries.Polygon.Empty, empty);
        }

        [Test]
        public void Triangle()
        {
            var reader = new WktReader();

            var xy = reader.Read("TRIANGLE ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
            Assert.AreEqual(new Triangle(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0))), xy);

            var empty = reader.Read("Triangle ZM EMPTY");
            Assert.AreEqual(Geometries.Triangle.Empty, empty);
        }

        [Test]
        public void GeometryCollection()
        {
            var reader = new WktReader();

            var points = reader.Read("GEOMETRYCOLLECTION (POINT (0.0 65.9), POINT (-34.5 9), POINT  (-20 40), POINT (0 65.9))");
            Assert.AreEqual(new GeometryCollection(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)), points);

            var empty = reader.Read("GEOMETRYCOLLECTION ZM EMPTY");
            Assert.AreEqual(new GeometryCollection(), empty);
        }

        [Test]
        public void MultiPoint()
        {
            var reader = new WktReader();

            var none = reader.Read("MULTIPOINT (0.0 65.9, -34.5 9, -20 40, 0 65.9)");
            Assert.AreEqual(new MultiPoint(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)), none);
            
            var brackets = reader.Read("MULTIPOINT (EMPTY, (0.0 65.9), (-34.5 9), (-20 40), (0 65.9))");
            Assert.AreEqual(new MultiPoint(new Point(), new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)), brackets);

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
