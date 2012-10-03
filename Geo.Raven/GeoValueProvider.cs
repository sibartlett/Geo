using System;
using Geo.Interfaces;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Geo.Raven
{
    public class GeoValueProvider : IValueProvider
    {
        public void SetValue(object target, object value)
        {
            throw new NotSupportedException();
        }

        public object GetValue(object target)
        {
            return GetValue(target as IRavenIndexable);
        }

        public string GetValue(IRavenIndexable target)
        {
            return target == null ? null : target.GetIndexString();
        }
    }
}