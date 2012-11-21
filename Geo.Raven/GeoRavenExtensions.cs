using System;
using System.Linq.Expressions;
using Geo.Abstractions.Interfaces;
using Geo.Raven.Json;
using Geo.Raven.Query;
using Raven.Client;
using Raven.Client.Linq;

namespace Geo.Raven
{
    public static class GeoRavenExtensions
    {
        public static IDocumentStore ApplyGeoConventions(this IDocumentStore store)
        {
            store.Conventions.JsonContractResolver = new GeoContractResolver();
            return store;
        }

        public static IRavenQueryable<T> Geo<T>(this IRavenQueryable<T> source, Expression<Func<T, IRavenIndexable>> property, Func<LinqWhereClause<T>, IRavenQueryable<T>> clause)
        {
            return clause(new LinqWhereClause<T>(source, property));
        }

        public static IDocumentQuery<T> Geo<T>(this IDocumentQuery<T> query, Expression<Func<T, IRavenIndexable>> propertySelector, Func<LuceneWhereClause<T>, IDocumentQuery<T>> clause)
        {
            return clause(new LuceneWhereClause<T>(query, propertySelector));
        }
    }
}
