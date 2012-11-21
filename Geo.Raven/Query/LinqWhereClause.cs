using System;
using System.Linq.Expressions;
using Geo.Abstractions.Interfaces;
using Geo.Measure;
using Raven.Abstractions.Indexing;
using Raven.Client.Linq;

namespace Geo.Raven.Query
{
    public class LinqWhereClause<T> : AbstractWhereClause<IRavenQueryable<T>> 
    {
        private readonly IRavenQueryable<T> _source;
        private readonly Expression<Func<T, IRavenIndexable>> _property;

        public LinqWhereClause(IRavenQueryable<T> source, Expression<Func<T, IRavenIndexable>> property)
        {
            _source = source;
            _property = property;
        }

        public override IRavenQueryable<T> RelatesToShape(IRavenIndexable geometry, SpatialRelation relation)
        {
            return _source.Customize(x => x.RelatesToShape(SpatialField.NameFor(_property), geometry.GetIndexString(), relation));
        }

        public override IRavenQueryable<T> WithinRadiusOf(IPosition position, Distance distance)
        {
            return _source.Customize(x => x.WithinRadiusOf(SpatialField.NameFor(_property), distance.ConvertTo(DistanceUnit.Km).Value, position.GetCoordinate().Latitude, position.GetCoordinate().Longitude));
        }
    }
}