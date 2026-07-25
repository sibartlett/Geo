using Geo.Geometries;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkt;

public class WktWriterTests
{
    [Fact]
    public void Empty_point_members_survive_a_write_then_read_round_trip()
    {
        var writer = new WktWriter();
        var reader = new WktReader();

        var multiPoint = new MultiPoint(new Point(), new Point(65.9, 0), new Point());
        var multiPointResult = (MultiPoint)reader.Read(writer.Write(multiPoint))!;
        Assert.Equal(3, multiPointResult.Geometries.Count);
        Assert.True(multiPointResult.Geometries[0].IsEmpty);
        Assert.True(multiPointResult.Geometries[2].IsEmpty);
        Assert.Equal(multiPoint, multiPointResult);

        var collection = new GeometryCollection(new Point(), new Point(65.9, 0));
        var collectionResult = (GeometryCollection)reader.Read(writer.Write(collection))!;
        Assert.Equal(2, collectionResult.Geometries.Count);
        Assert.True(collectionResult.Geometries[0].IsEmpty);
        Assert.Equal(collection, collectionResult);
    }

    [Fact]
    public void Point()
    {
        var writer = new WktWriter();

        var xy = writer.Write(new Point(65.9, 0));
        Assert.Equal("POINT (0 65.9)", xy);

        var xyz = writer.Write(new Point(65.9, 0, 5));
        Assert.Equal("POINT Z (0 65.9 5)", xyz);

        var xym = writer.Write(new Point(new CoordinateM(65.9, 0, 5)));
        Assert.Equal("POINT M (0 65.9 5)", xym);

        var xyzm = writer.Write(new Point(65.9, 0, 4, 5));
        Assert.Equal("POINT ZM (0 65.9 4 5)", xyzm);

        var empty = writer.Write(Geo.Geometries.Point.Empty);
        Assert.Equal("POINT EMPTY", empty);
    }

    [Fact]
    public void Point_measured_without_dimension_flag_round_trips()
    {
        var settings = new WktWriterSettings { DimensionFlag = false };
        var writer = new WktWriter(settings);

        // With no dimension flag to mark the measure, the Z slot is filled
        // with the null-ordinate placeholder so the M value keeps its position.
        var point = new Point(new CoordinateM(65.9, 0, 5));
        var xym = writer.Write(point);
        Assert.Equal("POINT (0 65.9 NaN 5)", xym);

        Assert.Equal(point, new WktReader().Read(xym));
    }

    [Fact]
    public void LineString()
    {
        var writer = new WktWriter();

        var xy = writer.Write(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5)));
        Assert.Equal("LINESTRING (0 65.9, -34.5 9)", xy);

        var empty = writer.Write(Geo.Geometries.LineString.Empty);
        Assert.Equal("LINESTRING EMPTY", empty);
    }

    [Fact]
    public void LinearRing()
    {
        var writer = new WktWriter();

        var lineString = writer.Write(
            new LinearRing(
                new Coordinate(65.9, 0),
                new Coordinate(9, -34.5),
                new Coordinate(50, 0),
                new Coordinate(65.9, 0)
            )
        );
        Assert.Equal("LINESTRING (0 65.9, -34.5 9, 0 50, 0 65.9)", lineString);

        var writer2 = new WktWriter(new WktWriterSettings { LinearRing = true });

        var linearRing = writer2.Write(
            new LinearRing(
                new Coordinate(65.9, 0),
                new Coordinate(9, -34.5),
                new Coordinate(50, 0),
                new Coordinate(65.9, 0)
            )
        );
        Assert.Equal("LINEARRING (0 65.9, -34.5 9, 0 50, 0 65.9)", linearRing);

        var empty = writer2.Write(Geo.Geometries.LinearRing.Empty);
        Assert.Equal("LINEARRING EMPTY", empty);
    }

    [Fact]
    public void Polygon()
    {
        var writer = new WktWriter();

        var xy = writer.Write(
            new Polygon(
                new LinearRing(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            )
        );
        Assert.Equal("POLYGON ((0 65.9, -34.5 9, -20 40, 0 65.9))", xy);

        var empty = writer.Write(Geo.Geometries.Polygon.Empty);
        Assert.Equal("POLYGON EMPTY", empty);
    }

    [Fact]
    public void Triangle()
    {
        var writer = new WktWriter();

        var polygon = writer.Write(
            new Triangle(
                new LinearRing(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            )
        );
        Assert.Equal("POLYGON ((0 65.9, -34.5 9, -20 40, 0 65.9))", polygon);

        var writer2 = new WktWriter(new WktWriterSettings { Triangle = true });

        var triangle = writer2.Write(
            new Triangle(
                new LinearRing(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            )
        );
        Assert.Equal("TRIANGLE ((0 65.9, -34.5 9, -20 40, 0 65.9))", triangle);

        var empty = writer2.Write(Geo.Geometries.Triangle.Empty);
        Assert.Equal("TRIANGLE EMPTY", empty);
    }

    [Fact]
    public void GeometryCollection()
    {
        var writer = new WktWriter();

        var brackets = writer.Write(
            new GeometryCollection(
                new Point(65.9, 0),
                new Point(9, -34.5),
                new Point(40, -20),
                new Point(65.9, 0)
            )
        );
        Assert.Equal(
            "GEOMETRYCOLLECTION (POINT (0 65.9), POINT (-34.5 9), POINT (-20 40), POINT (0 65.9))",
            brackets
        );

        var empty = writer.Write(new GeometryCollection());
        Assert.Equal("GEOMETRYCOLLECTION EMPTY", empty);
    }

    [Fact]
    public void MultiPoint()
    {
        var writer = new WktWriter();

        var brackets = writer.Write(
            new MultiPoint(
                new Point(65.9, 0),
                new Point(9, -34.5),
                new Point(40, -20),
                new Point(65.9, 0)
            )
        );
        Assert.Equal("MULTIPOINT ((0 65.9), (-34.5 9), (-20 40), (0 65.9))", brackets);

        var empty = writer.Write(new MultiPoint());
        Assert.Equal("MULTIPOINT EMPTY", empty);
    }

    [Fact]
    public void MultiLineString()
    {
        var writer = new WktWriter();

        var one = writer.Write(
            new MultiLineString(
                new LineString(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            )
        );
        Assert.Equal("MULTILINESTRING ((0 65.9, -34.5 9, -20 40, 0 65.9))", one);

        var two = writer.Write(
            new MultiLineString(
                new LineString(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                ),
                new LineString(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            )
        );
        Assert.Equal(
            "MULTILINESTRING ((0 65.9, -34.5 9, -20 40, 0 65.9), (0 65.9, -34.5 9, -20 40, 0 65.9))",
            two
        );

        var empty = writer.Write(new MultiLineString());
        Assert.Equal("MULTILINESTRING EMPTY", empty);
    }

    [Fact]
    public void MultiPolygon()
    {
        var writer = new WktWriter();

        var one = writer.Write(
            new MultiPolygon(
                new Polygon(
                    new LinearRing(
                        new Coordinate(65.9, 0),
                        new Coordinate(9, -34.5),
                        new Coordinate(40, -20),
                        new Coordinate(65.9, 0)
                    )
                )
            )
        );
        Assert.Equal("MULTIPOLYGON (((0 65.9, -34.5 9, -20 40, 0 65.9)))", one);

        var two = writer.Write(
            new MultiPolygon(
                new Polygon(
                    new LinearRing(
                        new Coordinate(65.9, 0),
                        new Coordinate(9, -34.5),
                        new Coordinate(40, -20),
                        new Coordinate(65.9, 0)
                    )
                ),
                new Polygon(
                    new LinearRing(
                        new Coordinate(65.9, 0),
                        new Coordinate(9, -34.5),
                        new Coordinate(40, -20),
                        new Coordinate(65.9, 0)
                    )
                )
            )
        );
        Assert.Equal(
            "MULTIPOLYGON (((0 65.9, -34.5 9, -20 40, 0 65.9)), ((0 65.9, -34.5 9, -20 40, 0 65.9)))",
            two
        );

        var empty = writer.Write(new MultiPolygon());
        Assert.Equal("MULTIPOLYGON EMPTY", empty);
    }

    // ---- Mixed dimensionality ----------------------------------------------

    [Fact]
    public void Coordinates_are_padded_to_the_declared_dimensions()
    {
        // Regression: the dimension tag comes from the geometry (Is3D is an "any
        // coordinate" test) while the ordinates used to be written per coordinate, so
        // the tag and the points disagreed and the WKT was invalid.
        var writer = new WktWriter();

        Assert.Equal(
            "LINESTRING Z (0 0 10, 1 1 NaN)",
            writer.Write(new LineString(new CoordinateZ(0, 0, 10), new Coordinate(1, 1)))
        );
        Assert.Equal(
            "LINESTRING ZM (0 0 10 NaN, 1 1 NaN 5)",
            writer.Write(new LineString(new CoordinateZ(0, 0, 10), new CoordinateM(1, 1, 5)))
        );
    }

    [Fact]
    public void Polygon_tag_covers_its_holes_as_well_as_its_shell()
    {
        // Polygon.Is3D reports only the shell, so a polygon whose elevations live in a
        // hole used to be tagged two-dimensional while writing three ordinates for
        // every hole coordinate.
        var flat = new LinearRing(
            new Coordinate(0, 0),
            new Coordinate(0, 3),
            new Coordinate(3, 3),
            new Coordinate(0, 0)
        );
        var raised = new LinearRing(
            new CoordinateZ(1, 1, 7),
            new CoordinateZ(1, 2, 7),
            new CoordinateZ(2, 2, 7),
            new CoordinateZ(1, 1, 7)
        );

        Assert.Equal(
            "POLYGON Z ((0 0 NaN, 3 0 NaN, 3 3 NaN, 0 0 NaN), (1 1 7, 2 1 7, 2 2 7, 1 1 7))",
            new WktWriter().Write(new Polygon(flat, raised))
        );
    }

    [Fact]
    public void Multi_geometry_tag_covers_every_member()
    {
        // One tag covers all the members, so they must all write its ordinates.
        var writer = new WktWriter();

        Assert.Equal(
            "MULTIPOINT Z ((0 0 5), (1 1 NaN))",
            writer.Write(new MultiPoint(new Point(0, 0, 5), new Point(1, 1)))
        );
        Assert.Equal(
            "MULTILINESTRING Z ((0 0 5, 1 1 5), (2 2 NaN, 3 3 NaN))",
            writer.Write(
                new MultiLineString(
                    new LineString(new CoordinateZ(0, 0, 5), new CoordinateZ(1, 1, 5)),
                    new LineString(new Coordinate(2, 2), new Coordinate(3, 3))
                )
            )
        );
    }

    [Fact]
    public void Mixed_dimension_geometries_round_trip()
    {
        var reader = new WktReader();
        var writer = new WktWriter();
        var ntsWriter = new WktWriter(WktWriterSettings.NtsCompatible);

        var geometries = new Geo.Abstractions.Interfaces.IGeometry[]
        {
            new LineString(new CoordinateZ(0, 0, 10), new Coordinate(1, 1)),
            new LineString(new CoordinateM(0, 0, 5), new Coordinate(1, 1)),
            new LineString(new CoordinateZM(0, 0, 1, 2), new CoordinateZ(1, 1, 3)),
        };

        foreach (var geometry in geometries)
        {
            Assert.Equal(geometry, reader.Read(writer.Write(geometry)));
            Assert.Equal(geometry, reader.Read(ntsWriter.Write(geometry)));
        }
    }

    [Fact]
    public void Measured_geometry_beyond_max_dimensions_writes_no_tag()
    {
        // MaxDimesions = 3 drops the measure, so there is no dimension left to tag and
        // no separator to write for it either.
        var writer = new WktWriter(new WktWriterSettings { MaxDimesions = 3 });

        Assert.Equal("POINT (0 65.9)", writer.Write(new Point(new CoordinateM(65.9, 0, 5))));
    }
}
