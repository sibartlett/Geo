using System;
using System.Collections.Generic;

namespace Geo.Gps.Metadata;

public abstract class Metadata<TKeys> : Dictionary<string, string>
{
    private readonly TKeys _metadataKeys;

    protected Metadata(TKeys metadataKeys)
    {
        _metadataKeys = metadataKeys;
    }

    public string Attribute(Func<TKeys, string> attribute)
    {
        var key = attribute(_metadataKeys);
        string result;
        TryGetValue(key, out result);
        return result;
    }

    public void Attribute(Func<TKeys, string> attribute, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var key = attribute(_metadataKeys);
            this[key] = value.Trim();
        }
    }

    public void RemoveAttribute(Func<TKeys, string> attribute)
    {
        var key = attribute(_metadataKeys);
        Remove(key);
    }
}
