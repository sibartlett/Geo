using System.Reflection;
using Geo.Geometries;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Geo.Raven
{
    public class GeoContractResolver : DefaultContractResolver
    {
        private readonly Assembly _assembly = typeof (ILatLngCoordinate).Assembly;

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
    }
}
