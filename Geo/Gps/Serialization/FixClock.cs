#nullable enable
using System;

namespace Geo.Gps.Serialization;

/// <summary>
/// Turns the bare time of day carried by a track fix into a full timestamp, rolling the
/// date forward every time the clock wraps past midnight.
/// </summary>
/// <remarks>
/// IGC B-records and NMEA sentences date their fixes with a time of day and nothing else:
/// a flight or a drive that runs past midnight UTC simply restarts at 00:00:00. Stamping
/// every fix onto the same day therefore sent the track twenty-four hours backwards
/// mid-recording, which left <see cref="TrackSegment.GetDuration" /> negative and
/// <see cref="TrackSegment.GetAverageSpeed" /> with it. Fixes arrive in order, so a time
/// of day earlier than the one before it can only mean the date has moved on.
/// </remarks>
internal sealed class FixClock
{
    private TimeSpan _previous = TimeSpan.MinValue;
    private int _days;

    /// <summary>
    /// The timestamp for a fix recorded at <paramref name="timeOfDay" /> on the day that
    /// began at <paramref name="date" />, or on a later one if the clock has wrapped since
    /// the first fix.
    /// </summary>
    public DateTime Resolve(DateTime date, TimeSpan timeOfDay)
    {
        if (timeOfDay < _previous)
            _days++;

        _previous = timeOfDay;
        return date.AddDays(_days) + timeOfDay;
    }
}
