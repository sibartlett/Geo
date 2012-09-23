using System;
using System.Linq;
using Geo.Geometries;
using Geo.Interfaces;
using Geo.Raven;
using NUnit.Framework;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Json.Linq;

namespace Geo.Tests.Raven
{
    public abstract class RavenTestFixtureBase : IDisposable
    {
        public IDocumentStore Store { get; private set; }

        public void InitRaven(params AbstractIndexCreationTask[] indexes)
        {
            if (Store != null)
                Store.Dispose();
            Store = new EmbeddableDocumentStore { RunInMemory = true }.ApplyGeoConventions().Initialize();
            foreach (var index in indexes)
                Store.ExecuteIndex(index);
        }

        public void Dispose()
        {
            if (Store != null)
                Store.Dispose();
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

        public void AssertThatIndexPropertyIsGenerated(IGeometry geometry)
        {
            InitRaven(new TestIndex());
            var doc = new GeoDoc
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
        }

        public void AssertTrue(IGeometry geometry, SpatialRelation relation, IGeometry geometry2)
        {
            AssertThat(geometry, relation, geometry2, true);
        }

        public void AssertFalse(IGeometry geometry, SpatialRelation relation, IGeometry geometry2)
        {
            AssertThat(geometry, relation, geometry2, false);
        }

        private void AssertThat(IGeometry geometry, SpatialRelation relation, IGeometry geometry2, bool expected)
        {
            InitRaven(new TestIndex());
            var doc = new GeoDoc
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
                var result = session.Query<RavenJObject, TestIndex>()
                    .Customize(x =>
                    {
                        x.RelatesToShape(geometry2, relation);
                        x.WaitForNonStaleResults();
                    })
                    .Any();

                var relationString = relation.ToString().ToLowerInvariant();
                bool s = false;
                if(relationString.EndsWith("s"))
                {
                    s = true;
                    relationString = relationString.Substring(0, relationString.Length - 1);
                }

                var msg = string.Format("Geometry {0}{1} {2} {3}", s? "does" : "is", result? "" : " not", relationString, new GeoValueProvider().GetValue(geometry2));

                Console.WriteLine(msg);


                Assert.That(result, Is.EqualTo(expected), msg);
            }
        }
    }
}
