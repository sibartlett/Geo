using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.IO.Wkt
{
    public class WktReader
    {
        private readonly WktTokenizer _wktTokenizer = new WktTokenizer();

        public IOgcGeometry Read(string wkt)
        {
            if (wkt == null)
                throw new ArgumentNullException("wkt");

            var tokens = new WktTokenQueue(_wktTokenizer.Tokenize(wkt));
            return ParseGeometry(tokens);
        }

        public IOgcGeometry Read(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (var reader = new StreamReader(stream))
            {
                var tokens = new WktTokenQueue(_wktTokenizer.Tokenize(reader));
                return ParseGeometry(tokens);
            }
        }

        private IOgcGeometry ParseGeometry(WktTokenQueue tokens)
        {
            if (tokens.Count == 0)
                return null;

            var token = tokens.Peek();

            if (token.Type == WktTokenType.String)
            {
                var value = token.Value.ToUpperInvariant();
                if (value == "POINT")
                    return ParsePoint(tokens);
                if (value == "LINESTRING")
                    return ParseLineString(tokens);
                if (value == "LINEARRING")
                    return ParseLinearRing(tokens);
                if (value == "POLYGON")
                    return ParsePolygon(tokens);
                if (value == "TRIANGLE")
                    return ParseTriangle(tokens);
                if (value == "MULTIPOINT")
                    return ParseMultiPoint(tokens);
                if (value == "MULTILINESTRING")
                    return ParseMultiLineString(tokens);
                if (value == "MULTIPOLYGON")
                    return ParseMultiPolygon(tokens);
                if (value == "GEOMETRYCOLLECTION")
                    return ParseGeometryCollection(tokens);
            }
            throw new SerializationException("WKT type '" + token.Value + "' not supported.");
        }

        private Point ParsePoint(WktTokenQueue tokens)
        {
            tokens.Dequeue("POINT");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return Point.Empty;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);
            var coordinate = ParseCoordinate(tokens, dimensions);
            tokens.Dequeue(WktTokenType.RightParenthesis);
            return new Point(coordinate);
        }

        private LineString ParseLineString(WktTokenQueue tokens)
        {
            tokens.Dequeue("LINESTRING");
            var dimensions = ParseDimensions(tokens);
            return ParseLineStringInner(tokens, dimensions);
        }

        private LineString ParseLineStringInner(WktTokenQueue tokens, WktDimensions dimensions)
        {
            var coords = ParseCoordinateSequence(tokens, dimensions);
            return coords == null ? LineString.Empty : new LineString(coords);
        }

        private LineString ParseLinearRing(WktTokenQueue tokens)
        {
            tokens.Dequeue("LINEARRING");
            var dimensions = ParseDimensions(tokens);
            var coords = ParseCoordinateSequence(tokens, dimensions);
            return coords == null ? LinearRing.Empty : new LinearRing(coords);
        }

        private Polygon ParsePolygon(WktTokenQueue tokens)
        {
            tokens.Dequeue("POLYGON");
            var dimensions = ParseDimensions(tokens);
            return ParsePolygonInner(tokens, dimensions);
        }

        private Polygon ParseTriangle(WktTokenQueue tokens)
        {
            tokens.Dequeue("TRIANGLE");
            var dimensions = ParseDimensions(tokens);
            return ParsePolygonInner(tokens, dimensions);
        }

        private Polygon ParsePolygonInner(WktTokenQueue tokens, WktDimensions dimensions)
        {
            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return Polygon.Empty;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);
            var linestrings = ParseLineStrings(tokens, dimensions);
            tokens.Dequeue(WktTokenType.RightParenthesis);
            return new Polygon(new LinearRing(linestrings.First().Coordinates), linestrings.Skip(1).Select(x => new LinearRing(x.Coordinates)));
        }

        private List<LineString> ParseLineStrings(WktTokenQueue tokens, WktDimensions dimensions)
        {
            var lineStrings = new List<LineString> { ParseLineStringInner(tokens, dimensions) };

            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                lineStrings.Add(ParseLineStringInner(tokens, dimensions));
            }

            return lineStrings;
        }

        private MultiPoint ParseMultiPoint(WktTokenQueue tokens)
        {
            tokens.Dequeue("MULTIPOINT");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return MultiPoint.Empty;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);


            var points = new List<Point> { ParseMultiPointCoordinate(tokens, dimensions) };
            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                points.Add(ParseMultiPointCoordinate(tokens, dimensions));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);

            return new MultiPoint(points);
        }

        private Point ParseMultiPointCoordinate(WktTokenQueue tokens, WktDimensions dimensions)
        {
            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return Point.Empty;
            }

            var parenthesis = false;

            if (tokens.NextTokenIs(WktTokenType.LeftParenthesis))
            {
                tokens.Dequeue(WktTokenType.LeftParenthesis);
                parenthesis = true;
            }
            var coordinate = ParseCoordinate(tokens, dimensions);
            if (parenthesis && tokens.NextTokenIs(WktTokenType.RightParenthesis))
                tokens.Dequeue(WktTokenType.RightParenthesis);
            return new Point(coordinate);
        }

        private MultiLineString ParseMultiLineString(WktTokenQueue tokens)
        {
            tokens.Dequeue("multilinestring");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return MultiLineString.Empty;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);
            var lineStrings = ParseLineStrings(tokens, dimensions);
            tokens.Dequeue(WktTokenType.RightParenthesis);

            return new MultiLineString(lineStrings);
        }

        private MultiPolygon ParseMultiPolygon(WktTokenQueue tokens)
        {
            tokens.Dequeue("MULTIPOLYGON");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return MultiPolygon.Empty;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);
            var polygons = new List<Polygon> { ParsePolygonInner(tokens, dimensions) };
            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                polygons.Add(ParsePolygonInner(tokens, dimensions));
            }
            tokens.Dequeue(WktTokenType.RightParenthesis);

            return new MultiPolygon(polygons);
        }

        private GeometryCollection ParseGeometryCollection(WktTokenQueue tokens)
        {
            tokens.Dequeue("GEOMETRYCOLLECTION");

            ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return GeometryCollection.Empty;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);

            var geometries = new List<IGeometry>();
            geometries.Add(ParseGeometry(tokens));

            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                geometries.Add(ParseGeometry(tokens));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);

            return new GeometryCollection(geometries);
        }

        private Coordinate ParseCoordinate(WktTokenQueue tokens, WktDimensions dimensions)
        {
            var token = tokens.Dequeue(WktTokenType.Number);
            var x = double.Parse(token.Value, CultureInfo.InvariantCulture);

            token = tokens.Dequeue(WktTokenType.Number);
            var y = double.Parse(token.Value, CultureInfo.InvariantCulture);

            var z = double.NaN;
            var m = double.NaN;

            var optional = ParseOptionalOrdinates(tokens);

            if (optional.Count > 0)
            {
                if (dimensions == WktDimensions.XYM)
                {
                    m = optional[0];
                }
                else
                {
                    z = optional[0];
                    if (optional.Count > 1)
                        m = optional[1];
                }
            }

            return new Coordinate(y, x, z, m);
        }

        private List<double> ParseOptionalOrdinates(WktTokenQueue tokens)
        {
            var attempt = true;
            var doubles = new List<double>();

            while (attempt)
            {
                if (tokens.NextTokenIs(WktTokenType.Number))
                {
                    var token = tokens.Dequeue(WktTokenType.Number);
                    doubles.Add(double.Parse(token.Value, CultureInfo.InvariantCulture));
                }
                else if (tokens.NextTokenIs(double.NaN.ToString(CultureInfo.InvariantCulture)))
                {
                    tokens.Dequeue(WktTokenType.String);
                    doubles.Add(double.NaN);
                }
                else
                {
                    attempt = false;
                }
            }
            return doubles;
        }

        private CoordinateSequence ParseCoordinateSequence(WktTokenQueue tokens, WktDimensions dimensions)
        {
            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return null;
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);

            var coordinates = new List<Coordinate> { ParseCoordinate(tokens, dimensions) };
            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                coordinates.Add(ParseCoordinate(tokens, dimensions));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);

            return new CoordinateSequence(coordinates);
        }

        private WktDimensions ParseDimensions(WktTokenQueue tokens)
        {
            var token = tokens.Peek();
            if (token.Type == WktTokenType.String)
            {
                var value = token.Value.ToUpperInvariant();
                if (value == "Z")
                {
                    tokens.Dequeue();
                    return WktDimensions.XYZ;
                }
                if (value == "M")
                {
                    tokens.Dequeue();
                    return WktDimensions.XYM;
                }
                if (value == "ZM")
                {
                    tokens.Dequeue();
                    return WktDimensions.XYZM;
                }
            }
            return WktDimensions.XY;
        }
    }
}
