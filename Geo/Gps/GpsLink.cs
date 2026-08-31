#nullable enable
using System;

namespace Geo.Gps;

/// <summary>
/// A link to something describing the thing that holds it - a web page for a
/// waypoint, the source of a track, and so on.
/// </summary>
/// <remarks>
/// GPX 1.1 carries these as &lt;link&gt;, and allows any number of them on the file,
/// a waypoint, a route and a track. GPX 1.0 has no such element: it holds a single
/// &lt;url&gt; and &lt;urlname&gt; in the same places, which map onto
/// <see cref="Href" /> and <see cref="Text" />. A document written as 1.0 therefore
/// keeps only the first link, and never <see cref="Type" />, because the version has
/// nowhere to put either.
/// </remarks>
public class GpsLink
{
    /// <param name="href">Where the link points. GPX requires this of every link.</param>
    public GpsLink(string href)
    {
        Href = href ?? throw new ArgumentNullException(nameof(href));
    }

    public GpsLink(string href, string? text, string? type)
        : this(href)
    {
        Text = text;
        Type = type;
    }

    /// <summary>
    /// Where the link points. Required: the GPX schemas declare the attribute
    /// <c>use="required"</c>, so a link without one cannot be written.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// How the link should read - the text of the hyperlink.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// The media type of whatever is linked to, such as <c>image/jpeg</c>. GPX 1.1
    /// only; 1.0 has no equivalent.
    /// </summary>
    public string? Type { get; set; }

    public override string ToString()
    {
        return Text == null ? Href : Text + " (" + Href + ")";
    }
}
