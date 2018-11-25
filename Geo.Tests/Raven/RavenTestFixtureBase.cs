using System;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Raven;
using Geo.Raven.Indexes;
using Geo.Raven.Json;
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
        private readonly RavenIndexStringWriter _writer = new RavenIndexStringWriter();
        public IDocumentStore Store { get; private set; }

        public void InitRaven(params AbstractIndexCreationTask[] indexes)
        {
            if (Store != null)
                Store.Dispose();

            EmbeddableDocumentStore store = new EmbeddableDocumentStore { RunInMemory = true };
            store.Configuration.Storage.Voron.AllowOn32Bits = true;

            Store = store.ApplyGeoConventions().Initialize();
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
            public string Data1 { get; set; }
            public string Data2 { get; set; }
            public string Data3 { get; set; }
        }

        public class TestIndex : GeoIndexCreationTask<GeoDoc>
        {
            public TestIndex()
            {
                Map = docs => from doc in docs
                              select new
                              {
                                  doc.Data1,
                                  doc.Data2,
                                  doc.Data3,
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
                var result = json.Value<RavenJObject>("Geometry").ContainsKey("__spatial");
                Console.WriteLine(json.Value<RavenJObject>("Geometry").Value<string>("__spatial"));
                Assert.That(result, Is.True);
            }
        }

        public void AssertThatEntityDeserializes(IGeometry geometry)
        {
            InitRaven(new TestIndex());
            string docId;
            using (var session = Store.OpenSession())
            {
                var doc = new GeoDoc
                {
                    Geometry = geometry
                };
                session.Store(doc);
                session.SaveChanges();
                docId = doc.Id;
            }
            using (var session = Store.OpenSession())
            {
                var doc = session.Load<GeoDoc>(docId);
                Assert.AreEqual(geometry, doc.Geometry);
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

        private void AssertThat(IGeometry geometry, SpatialRelation relation, IRavenIndexable geometry2, bool expected)
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
                var result = session.Query<GeoDoc, TestIndex>()
                    .Geo(x => x.Geometry, x => x.RelatesToShape(geometry2, relation))
                    .Customize(x => x.WaitForNonStaleResults())
                    .Any();

                var relationString = relation.ToString().ToLowerInvariant();
                bool s = false;
                if(relationString.EndsWith("s"))
                {
                    s = true;
                    relationString = relationString.Substring(0, relationString.Length - 1);
                }

                var msg = string.Format("Geometry {0}{1} {2} {3}", s ? "does" : "is", result ? "" : " not", relationString, _writer.Write(geometry2.GetSpatial4nShape()));

                Assert.That(result, Is.EqualTo(expected), msg);
            }
        }
    }
}
