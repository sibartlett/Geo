using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Geo.Gps.Serialization.Xml;

// Reading helpers shared by the XML based GPS deserializers.
//
// Every accessor answers null for content that is absent, and also for content
// that is present but cannot be converted. That tolerance is deliberate: the
// XmlSerializer these replaced skipped members it could not bind, and the
// reference corpus contains real-world files that depend on it - a waypoint
// carrying an unparseable <ele> still yields a waypoint, it just has no
// elevation. Throwing instead would turn files that parse today into failures.
//
// Conversions go through XmlConvert rather than the BCL's Parse/ToString so that
// they stay culture-invariant and keep the exact lexical forms XmlSerializer
// produced and accepted.
internal static class XmlExtensions
{
    public static IEnumerable<XElement> ElementsOrEmpty(this XElement? element, XName name)
    {
        return element == null ? Enumerable.Empty<XElement>() : element.Elements(name);
    }

    public static string? ElementValue(this XElement? element, XName name)
    {
        return element?.Element(name)?.Value;
    }

    public static string? AttributeValue(this XElement? element, XName name)
    {
        return element?.Attribute(name)?.Value;
    }

    public static decimal? DecimalElement(this XElement? element, XName name)
    {
        return ToDecimal(element.ElementValue(name));
    }

    public static decimal? DecimalAttribute(this XElement? element, XName name)
    {
        return ToDecimal(element.AttributeValue(name));
    }

    public static double? DoubleElement(this XElement? element, XName name)
    {
        return ToDouble(element.ElementValue(name));
    }

    public static DateTime? DateTimeElement(this XElement? element, XName name)
    {
        return ToDateTime(element.ElementValue(name));
    }

    public static decimal? ToDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return XmlConvert.ToDecimal(value!.Trim());
        }
        catch (FormatException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    public static double? ToDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return XmlConvert.ToDouble(value!.Trim());
        }
        catch (FormatException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    // RoundtripKind, matching what XmlSerializer used for a DateTime member. The
    // reference files carry three shapes - "2009-01-01T10:00:00", the same with a
    // trailing Z, and one with fractional seconds - and only this mode preserves
    // each one's DateTimeKind, which the GPX round-trip tests compare.
    public static DateTime? ToDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return XmlConvert.ToDateTime(value!.Trim(), XmlDateTimeSerializationMode.RoundtripKind);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public static string ToString(decimal value)
    {
        return XmlConvert.ToString(value);
    }

    public static string ToString(DateTime value)
    {
        return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
    }

    /// <summary>
    /// An element named <paramref name="name" /> holding <paramref name="value" />, or
    /// <c>null</c> when there is no value to write.
    /// </summary>
    /// <remarks>
    /// Returning null rather than an empty element lets a document be built by passing
    /// every optional child to <see cref="XElement" />'s constructor at once - it ignores
    /// null content - instead of testing each one before adding it.
    /// <para>
    /// Blank content decides only whether the element is written, never what it holds:
    /// the value goes out as it came in. Trimming it here cost a waypoint in the
    /// reference files its trailing space, so a name that survived being read no longer
    /// survived being written back.
    /// </para>
    /// </remarks>
    public static XElement? OptionalElement(XName name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new XElement(name, value!);
    }
}
