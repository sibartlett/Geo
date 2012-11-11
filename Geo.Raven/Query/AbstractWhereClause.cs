using Geo.Interfaces;
using Geo.Measure;
using Raven.Abstractions.Indexing;

namespace Geo.Raven.Query
{
    public abstract class AbstractWhereClause<T>
    {
        public abstract T RelatesToShape(IRavenIndexable geometry, SpatialRelation relation);
        public abstract T WithinRadiusOf(IPosition position, Distance distance);

        public T Within(IRavenIndexable geometry)
        {
            return RelatesToShape(geometry, SpatialRelation.Within);
        }

        public T Intersects(IRavenIndexable geometry)
        {
            return RelatesToShape(geometry, SpatialRelation.Intersects);
        }

        public T Contains(IRavenIndexable geometry)
        {
            return RelatesToShape(geometry, SpatialRelation.Contains);
        }

        public T WithinRadiusOf(IPosition position, double value, DistanceUnit unit = DistanceUnit.M)
        {
            return WithinRadiusOf(position, new Distance(value, unit));
        }
    }
}
