using System;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

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
    }
}
