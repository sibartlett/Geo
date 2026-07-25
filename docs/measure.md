# Measure — distances, areas, and speeds

Calculations in Geo return strongly-typed measurements from the `Geo.Measure`
namespace rather than bare `double`s, so the unit is always explicit. Each type
stores its value internally in **S.I. units** and converts on demand.

## Distance

`Distance` stores metres internally (`SiValue`) and converts to other units with
`ConvertTo`.

```csharp
using Geo.Measure;

var distance = new Distance(5000);              // 5000 metres
double km = distance.ConvertTo(DistanceUnit.Km).Value;   // 5
double miles = distance.ConvertTo(DistanceUnit.Mile).Value;

// Construct in a specific unit:
var nauticalMiles = new Distance(3, DistanceUnit.Nm);
double metres = nauticalMiles.SiValue;          // 5556
```

`DistanceUnit` values: `M` (metres), `Nm` (nautical miles), `Km` (kilometres),
`Mile` (statute miles), `Ft` (feet).

`ToString()` formats with the measurement's unit; `ToString(unit)` converts
first:

```csharp
distance.ToString();                 // e.g. "5000 m"
distance.ToString(DistanceUnit.Km);  // e.g. "5 km"
```

`Distance` values support arithmetic (`+`, `-`) and comparison operators
(`<`, `>`, `<=`, `>=`, `==`, `!=`), all evaluated on the underlying S.I. value.

## Speed

`Speed` stores metres per second internally.

```csharp
using System;
using Geo.Measure;

var speed = new Speed(10);                        // 10 m/s
double kph = speed.ConvertTo(SpeedUnit.Kph).Value; // 36

// From a distance covered over a time span:
var average = new Speed(1000 /* metres */, TimeSpan.FromMinutes(5));
double mph = average.ConvertTo(SpeedUnit.Mph).Value;
```

`SpeedUnit` values: `Ms` (metres/second), `Knots`, `Kph` (km/h), `Mph`
(miles/hour).

## Area

`Area` stores square metres internally (`SiValue`) and is what area calculations
return (for example `Envelope.GetArea()` and
`IGeodeticCalculator.CalculateArea`). It converts to other units with
`ConvertTo`, using `AreaUnit` (not `DistanceUnit`) so the conversion scales by
the **square** of the linear factor.

```csharp
using Geo.Measure;

Area area = envelope.GetArea();
double squareMetres = area.SiValue;
double squareKm = area.ConvertTo(AreaUnit.Km).Value;

// Construct in a specific unit (2 km² = 2,000,000 m²):
var field = new Area(2, AreaUnit.Km);
double m2 = field.SiValue;            // 2000000
```

`AreaUnit` values: `M` (m²), `Nm` (nautical miles²), `Km` (km²), `Mile` (statute
miles²), `Ft` (ft²).

`ToString()` formats with the measurement's unit; `ToString(unit)` converts
first:

```csharp
area.ToString();               // e.g. "1000000 m²"
area.ToString(AreaUnit.Km);    // e.g. "1 km²"
```

Like `Distance`, `Area` values support arithmetic (`+`, `-`) and comparison
operators, all evaluated on the underlying square-metre value.

## Converting bare doubles

The unit enums also provide extension methods for converting plain `double`
values, which is handy when you are not holding a measurement type:

```csharp
using Geo.Measure;

double km = 5000d.ConvertTo(DistanceUnit.Km);   // 5
```

See also: [Geodesy](geodesy.md), whose distance/length/area calculations return
these types.
