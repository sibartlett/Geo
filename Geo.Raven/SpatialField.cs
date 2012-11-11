using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Geo.Interfaces;

namespace Geo.Raven
{
    internal static class SpatialField
    {
        public const string Name = "__spatial";

        public static string NameFor<T>(Expression<Func<T, IRavenIndexable>> propertySelector)
        {
            var members = new List<string>();
            var expression = propertySelector.Body;
            while (expression != null)
            {
                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = (MemberExpression)expression;
                    members.Add(memberExpression.Member.Name);
                    expression = memberExpression.Expression;
                }
                else
                    break;
            }

            if (members.Count <= 0)
                return null;

            members.Add(Name);

            return string.Join("_", members);
        }
    }
}
