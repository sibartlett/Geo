# Geometries

Geo's geometry types mirror the OGC simple-feature geometries, plus a `Circle`.

## Point

Similar to the OGC Point.

## LineString

Similar to the OGC LineString.

## LinearRing

Similar to the OGC LinearRing.

A special type of `LineString` where the start and end coordinates are equal,
forming a closed ring. A `LineString` may also have equal start and end
coordinates, but it is not necessarily semantically closed.

## Polygon

Similar to the OGC Polygon.

Though it is not enforced, it is advised that:

- the exterior ring/shell is CCW (counter-clockwise/anti-clockwise);
- the interior rings/holes are CW (clockwise).

## Triangle

Similar to the OGC Triangle.

The same as a `Polygon`, but the outer ring/shell is limited to 4 sets of
coordinates (3 distinct points).

## GeometryCollection

Similar to the OGC GeometryCollection.

## MultiPoint

Similar to the OGC MultiPoint.

## MultiLineString

Similar to the OGC MultiLineString.

## MultiPolygon

Similar to the OGC MultiPolygon.

## Circle

A geometry type representing a circle, consisting of a centre and a radius in
metres. It does not map to an OGC-defined shape.
