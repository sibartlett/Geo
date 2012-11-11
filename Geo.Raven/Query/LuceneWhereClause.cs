using System;
using System.Linq.Expressions;
using Geo.Interfaces;
using Geo.Measure;
using Raven.Abstractions.Indexing;
using Raven.Client;

namespace Geo.Raven.Query
{
    public class LuceneWhereClause<T> : AbstractWhereClause<IDocumentQuery<T>>
    {
        private readonly IDocumentQuery<T> _query;
        private readonly Expression<Func<T, IRavenIndexable>> _propertySelector;

        public LuceneWhereClause(IDocumentQuery<T> query, Expression<Func<T, IRavenIndexable>> propertySelector)
        {
            _query = query;
            _propertySelector = propertySelector;
        }

        public override IDocumentQuery<T> RelatesToShape(IRavenIndexable geometry, SpatialRelation relation)
        {
            return _query.RelatesToShape(SpatialField.NameFor(_propertySelector), geometry.GetIndexString(), relation);
        }

        public override IDocumentQuery<T> WithinRadiusOf(IPosition position, Distance distance)
        {
            return _query.WithinRadiusOf(SpatialField.NameFor(_propertySelector), distance.ConvertTo(DistanceUnit.Km).Value, position.GetCoordinate().Latitude, position.GetCoordinate().Longitude);
        }
    }
}