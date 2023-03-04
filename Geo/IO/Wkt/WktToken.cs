namespace Geo.IO.Wkt;

internal struct WktToken
{
    public WktToken(WktTokenType type)
        : this()
    {
        Type = type;
        Value = default;
    }

    public WktToken(WktTokenType type, string value)
        : this()
    {
        Type = type;
        Value = value;
    }

    public WktTokenType Type { get; }
    public string Value { get; }
}