using System;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Geo.Raven.Json
{
    public class GeoValueProvider<T, TResult> : IValueProvider where T : class
    {
        private readonly Func<T, object> _func;

        public GeoValueProvider(Func<T, object> func)
        {
            _func = func;
        }

        public void SetValue(object target, object value)
        {
            throw new NotSupportedException();
        }

        public object GetValue(object target)
        {
            var obj = target as T;
            return obj == null ? default(TResult) : _func(obj);
        }
    }
}