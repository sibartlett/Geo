using Geo.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Tests.Geometries {
    [TestFixture]
    public class PolygonTests {
        [Test]
        public void PointInsidePolygon() {
            var bermudaTriangle = new Polygon(
                new Coordinate( 25.774, -80.190 ),
                new Coordinate( 18.446, -66.118 ),
                new Coordinate( 32.321, -64.757 ),
                new Coordinate( 25.774, -80.190 )
                );

            var point = new Coordinate( 25.123574, -76.149055 );

            Assert.IsTrue( bermudaTriangle.Contains( point ) );
        }

        [Test]
        public void PointOutsidePolygon() {
            var bermudaTriangle = new Polygon(
                new Coordinate( 25.774, -80.190 ),
                new Coordinate( 18.446, -66.118 ),
                new Coordinate( 32.321, -64.757 ),
                new Coordinate( 25.774, -80.190 )
                );

            var point = new Coordinate( 25.282622, -80.433723 );

            Assert.IsFalse( bermudaTriangle.Contains( point ) );
        }

        private Polygon GetCityOfPlovdiv() {
            return new Polygon(
                new Coordinate( 42.154709, 24.739262 ),
                new Coordinate( 42.154655, 24.754635 ),
                new Coordinate( 42.148127, 24.764811 ),
                new Coordinate( 42.143639, 24.754609 ),
                new Coordinate( 42.137317, 24.751387 ),
                new Coordinate( 42.138554, 24.745020 ),
                new Coordinate( 42.140354, 24.742744 ),
                new Coordinate( 42.144382, 24.745839 ),
                new Coordinate( 42.146361, 24.736663 ),
                new Coordinate( 42.154709, 24.739262 )
                );
        }

        [Test]
        public void PointNotInsideComplexPolygon() {
            var cityOfPlovdivCenter = GetCityOfPlovdiv();

            var memorialOfAliosha = new Coordinate( 42.143670, 24.738352 );

            Assert.IsFalse( cityOfPlovdivCenter.Contains( memorialOfAliosha ) );
        }
        
        [Test]
        public void PointInsideComplexPolygon() {
            var cityOfPlovdivCenter = GetCityOfPlovdiv();

            var danovHill = new Coordinate( 42.145138, 24.747093 );

            Assert.IsTrue( cityOfPlovdivCenter.Contains( danovHill ) );
        }

        [Test]
        public void PointInsideComplexPolygon2() {
            var cityOfPlovdivCenter = GetCityOfPlovdiv();

            var oldTown = new Coordinate( 42.148477, 24.751817 );

            Assert.IsTrue( cityOfPlovdivCenter.Contains( oldTown ) );
        }

        [Test]
        public void PointOutsideComplexPolygon2() {
            var cityOfPlovdivCenter = GetCityOfPlovdiv();

            var centralStation = new Coordinate( 42.134773, 24.741196 );

            Assert.IsFalse( cityOfPlovdivCenter.Contains( centralStation ) );
        }
    }
}
