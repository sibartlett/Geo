using System;
using System.Reflection;
using Geo.Geometries;
using Geo.Interfaces;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Geo.Raven
{
    public class GeoContractResolver : DefaultContractResolver
    {
        private readonly Assembly _assembly = typeof(Coordinate).Assembly;
        public const string IndexProperty = "__geo";

        protected override JsonProperty CreateProperty(MemberInfo member, global::Raven.Imports.Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if(member.DeclaringType !=null)
            {
                if (member.DeclaringType.Assembly == _assembly)
                {
                    if (!prop.Writable)
                    {
                        var property = member as PropertyInfo;
                        if (property != null)
                        {
                            prop.Writable = property.GetSetMethod(true) != null;
                        }
                    }  
                }
            }

            return prop;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (typeof(IRavenIndexable).IsAssignableFrom(objectType))
            {
                contract.Properties.Add(new JsonProperty
                {
                    Readable = true,
                    ShouldSerialize = value => true,
                    PropertyName = IndexProperty,
                    PropertyType = typeof(string),
                    Converter = ResolveContractConverter(typeof(string)),
                    ValueProvider = new GeoValueProvider()
                });
            }

            return contract;
        }
    }
}
