namespace Geo.IO;

/// <summary>
/// Identifies the textual geospatial format detected by <see cref="GeoFormat" />.
/// </summary>
public enum GeoStringFormat
{
    /// <summary>The string did not match any supported format.</summary>
    Unknown = 0,

    /// <summary>A coordinate string, e.g. <c>42.29, -89.63</c> or <c>50°03'46"S 125°48'26"E</c>.</summary>
    Coordinate = 1,

    /// <summary>A Well-Known Text (WKT) geometry, e.g. <c>POINT (30 10)</c>.</summary>
    Wkt = 2,

    /// <summary>A GeoJSON object, e.g. <c>{ "type": "Point", "coordinates": [30, 10] }</c>.</summary>
    GeoJson = 3,
}
