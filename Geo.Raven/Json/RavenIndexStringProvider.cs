using System;
using Geo.Abstractions.Interfaces;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Geo.Raven.Json
{
    public class RavenIndexStringProvider : IValueProvider
    {
        private readonly RavenIndexStringWriter _writer = new RavenIndexStringWriter();

        public void SetValue(object target, object value)
        {
            throw new NotSupportedException();
        }

        public object GetValue(object target)
        {
            var obj = target as IRavenIndexable;
            return obj == null ? default(string) : _writer.Write(obj.GetSpatial4nShape());
        }
    }
}