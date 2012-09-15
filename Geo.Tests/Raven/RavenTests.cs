using System;
using System.Linq;
using Geo.Geometries;
using Geo.Raven;
using NUnit.Framework;
using Raven.Abstractions.Indexing;
using Raven.Json.Linq;
using Raven.Client;

namespace Geo.Tests.Raven
{
    [TestFixture]
    public class RavenTests : RavenTestFixtureBase
    {
        [Test]
        public void PointTest()
        {
            IndexPropertyTest(new Point(0, 0));
        }

        [Test]
        public void LineStringTest()
        {
            IndexPropertyTest(new LineString(new [] { new Point(0, 0), new Point(1, 0), new Point(0,2)  }));
        }

        [Test]
        public void LinearRingTest()
        {
            IndexPropertyTest(new LinearRing(new [] { new Point(0, 0), new Point(1, 0), new Point(0, 2)  }));
        }

        [Test]
        public void PolygonTest()
        {
            IndexPropertyTest(new Polygon(new LinearRing(new[] { new Point(0, 0), new Point(5, 0), new Point(0, 5) })));
        }

        private void IndexPropertyTest(IGeometry geometry)
        {
            InitRaven(new TestIndex());
            var doc =  new GeoDoc
                                  {
                                      Geometry = geometry
                                  };
            using (var session = Store.OpenSession())
            {
                session.Store(doc);
                session.SaveChanges();
            }
            using (var session = Store.OpenSession())
            {
                var json = session.Load<RavenJObject>(doc.Id);
                var result = json.Value<RavenJObject>("Geometry").ContainsKey(GeoContractResolver.IndexProperty);
                Assert.That(result, Is.True);
            }
            using (var session = Store.OpenSession())
            {
                Console.WriteLine(((IWktShape)geometry).ToWktString());
                var result = session.Query<RavenJObject, TestIndex>()
                    .Customize(x =>
                    {
                        x.RelatesToShape(geometry, SpatialRelation.Within);
                        x.WaitForNonStaleResults();
                    })
                    .Any();
                //Assert.That(result, Is.True);

                var result2 = session.Query<RavenJObject, TestIndex>()
                    .Customize(x =>
                    {
                        x.RelatesToShape(new Circle(0, 0, 600000), SpatialRelation.Within);
                        x.WaitForNonStaleResults();
                    })
                    .Any();
                Assert.That(result2, Is.True);

                var result3 = session.Query<RavenJObject, TestIndex>()
                    .Customize(x =>
                    {
                        x.RelatesToShape(new Circle(0, 160, 600000), SpatialRelation.Within);
                        x.WaitForNonStaleResults();
                    })
                    .Any();
                Assert.That(result3, Is.False);
            }
        }

        public class GeoDoc
        {
            public string Id { get; set; }
            public IGeometry Geometry { get; set; }
        }

        public class TestIndex : GeoIndexCreationTask<GeoDoc>
        {
            public TestIndex()
            {
                Map = docs => from doc in docs
                              select new
                                         {
                                             _ = GeoIndex(doc.Geometry)
                                         };
            }
        }
    }
}
