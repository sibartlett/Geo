using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Geo.Tests.Documentation;

/// <summary>
/// Compiles every C# code block in the docs/ folder so the examples cannot drift
/// away from the public API (which is exactly what happened to the old wiki).
///
/// The snippets are illustrative fragments, so they are wrapped in methods over a
/// shared set of example variables (see <see cref="Preamble"/>) before compiling.
/// </summary>
public class DocumentationExamplesTests
{
    private static readonly Regex CodeBlock = new(
        "```csharp\\r?\\n(?<code>.*?)```",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    // A `using X;` directive, as opposed to a `using var x = ...;` / `using (...)` statement.
    private static readonly Regex UsingDirective = new(
        "^\\s*using\\s+[A-Za-z_][A-Za-z0-9_.]*\\s*;\\s*$",
        RegexOptions.Compiled
    );

    // Shared example variables the fragments refer to without declaring.
    private const string Preamble =
        @"
        private readonly Coordinate london = new Coordinate(51.5074, -0.1278);
        private readonly Coordinate newYork = new Coordinate(40.7128, -74.0060);
        private readonly Distance distance = new Distance(5000);
        private readonly LinearRing outerRing = new LinearRing(
            new Coordinate(0, 0), new Coordinate(0, 10), new Coordinate(10, 10), new Coordinate(0, 0));
        private readonly LinearRing innerRing = new LinearRing(
            new Coordinate(2, 2), new Coordinate(2, 4), new Coordinate(4, 4), new Coordinate(2, 2));
        private readonly LineString line = new LineString(new Coordinate(0, 0), new Coordinate(0, 10));
        private readonly LineString line1 = new LineString(new Coordinate(0, 0), new Coordinate(0, 10));
        private readonly LineString line2 = new LineString(new Coordinate(1, 0), new Coordinate(1, 10));
        private readonly Point point = new Point(0, 0);
        private readonly Polygon polygon = new Polygon(
            new Coordinate(0, 0), new Coordinate(0, 10), new Coordinate(10, 10), new Coordinate(0, 0));
        private readonly Polygon polygon1 = new Polygon(
            new Coordinate(0, 0), new Coordinate(0, 10), new Coordinate(10, 10), new Coordinate(0, 0));
        private readonly Polygon polygon2 = new Polygon(
            new Coordinate(5, 5), new Coordinate(5, 15), new Coordinate(15, 15), new Coordinate(5, 5));
        private readonly Circle circle = new Circle(0, 0, 1000);
        private readonly Envelope envelope = new Envelope(0, 0, 10, 10);
        private readonly GpsData gpsData = new GpsData();
        private readonly GpsData data = new GpsData();
        private readonly Stream myStream = Stream.Null;
        ";

    [Fact]
    public void All_csharp_examples_in_the_docs_compile()
    {
        var docsDirectory = FindDocsDirectory();
        var snippets = Directory
            .EnumerateFiles(docsDirectory, "*.md")
            .OrderBy(x => x)
            .SelectMany(ExtractSnippets)
            .ToList();

        Assert.NotEmpty(snippets);

        var source = BuildProgram(snippets);
        var errors = Compile(source).Where(x => x.Severity == DiagnosticSeverity.Error).ToList();

        Assert.True(
            errors.Count == 0,
            "Documentation examples failed to compile:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, errors)
        );
    }

    private static IEnumerable<(string File, string Code)> ExtractSnippets(string file)
    {
        var text = File.ReadAllText(file);
        foreach (Match match in CodeBlock.Matches(text))
            yield return (Path.GetFileName(file), match.Groups["code"].Value);
    }

    private static string BuildProgram(IReadOnlyList<(string File, string Code)> snippets)
    {
        var usings = new SortedSet<string>(StringComparer.Ordinal)
        {
            "using System;",
            "using System.Collections.Generic;",
            "using System.IO;",
            "using System.Linq;",
            "using Geo;",
            "using Geo.Abstractions.Interfaces;",
            "using Geo.Geodesy;",
            "using Geo.Geomagnetism;",
            "using Geo.Geometries;",
            "using Geo.Gps;",
            "using Geo.Gps.Serialization;",
            "using Geo.IO.GeoJson;",
            "using Geo.IO.Wkt;",
            "using Geo.IO.Wkb;",
            "using Geo.Measure;",
            "using Xunit;",
        };

        var methods = new StringBuilder();
        for (var i = 0; i < snippets.Count; i++)
        {
            var body = new StringBuilder();
            foreach (var line in snippets[i].Code.Split('\n'))
            {
                if (UsingDirective.IsMatch(line))
                    usings.Add(line.Trim());
                else
                    body.Append(line).Append('\n');
            }

            methods
                .Append("    // ")
                .Append(snippets[i].File)
                .Append('\n')
                .Append("    void Snippet_")
                .Append(i)
                .Append("()\n    {\n")
                .Append(body)
                .Append("\n    }\n\n");
        }

        return string.Join("\n", usings)
            + "\n\nclass DocExamples\n{\n"
            + Preamble
            + "\n"
            + methods
            + "}\n";
    }

    private static IEnumerable<Diagnostic> Compile(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
            .ToList();

        var compilation = CSharpCompilation.Create(
            "DocExamples",
            new[] { CSharpSyntaxTree.ParseText(source) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation.GetDiagnostics();
    }

    private static string FindDocsDirectory()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath)
        );

        while (directory != null)
        {
            var docs = Path.Combine(directory.FullName, "docs");
            if (Directory.Exists(docs))
                return docs;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the docs/ directory.");
    }
}
