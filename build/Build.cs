using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

[GitHubActions(
    "ci",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[]
    {
        nameof(CheckForUncommittedChanges),
        nameof(Test),
        nameof(AotSmokeTest),
    }
)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild
        ? Configuration.Debug
        : Configuration.Release;

    [Solution]
    readonly Solution Solution;

    [PathVariable]
    readonly Tool Git;

    Target CheckForUncommittedChanges =>
        _ =>
            _.Executes(() =>
            {
                DotNetTasks.DotNet("husky run --group verify");
            });

    Target Clean =>
        _ =>
            _.Before(Restore)
                .Executes(() =>
                {
                    DotNetTasks.DotNetClean(_ =>
                        _.SetProject(Solution).SetConfiguration(Configuration)
                    );
                });

    Target Restore =>
        _ =>
            _.Executes(() =>
            {
                DotNetTasks.DotNetRestore(_ => _.SetProjectFile(Solution));
            });

    Target Compile =>
        _ =>
            _.DependsOn(Restore)
                .Executes(() =>
                {
                    DotNetTasks.DotNetBuild(_ =>
                        _.SetProjectFile(Solution)
                            .SetNoRestore(InvokedTargets.Contains(Restore))
                            .SetConfiguration(Configuration)
                    );
                });

    Target Test =>
        _ =>
            _.DependsOn(Compile)
                .Executes(() =>
                {
                    var projects = Solution.GetAllProjects("*.Tests");
                    foreach (var project in projects)
                    {
                        DotNetTasks.DotNetTest(_ =>
                            _.SetProjectFile(project.Path)
                                .SetConfiguration(Configuration)
                                .EnableNoBuild()
                        );
                    }
                });

    // Publishes Geo.AotSmokeTest natively and runs it. The library is meant to be
    // usable from a NativeAOT application, and nothing else in the build proves that:
    // the trimming and AOT analysers catch what they can see, but reflection-based
    // serialization only fails once the published binary runs.
    Target AotSmokeTest =>
        _ =>
            _.DependsOn(Compile)
                .Executes(() =>
                {
                    var project = Solution.GetProject("Geo.AotSmokeTest");
                    var output = RootDirectory / "artifacts" / "aot-smoke-test";

                    // No runtime identifier: PublishAot infers the host's, so this
                    // target runs wherever the build does.
                    DotNetTasks.DotNetPublish(_ =>
                        _.SetProject(project.Path).SetConfiguration("Release").SetOutput(output)
                    );

                    var executable =
                        output
                        / (EnvironmentInfo.IsWin ? "Geo.AotSmokeTest.exe" : "Geo.AotSmokeTest");
                    ProcessTasks.StartProcess(executable).AssertZeroExitCode();
                });

    // Packs the library for release. This is `pack`, not `publish`: the release
    // artifact of a library is its NuGet package, where `dotnet publish` lays out
    // the assemblies an application runs from — which nothing consumes here, and
    // which the SDK will not produce for Geo at all now that it multi-targets
    // (NETSDK1129: publish needs a single framework). Packing the one project also
    // keeps the test and smoke-test projects out of it; the solution-wide publish
    // swept those in.
    Target Publish =>
        _ =>
            _.Executes(() =>
            {
                var project = Solution.GetProject("Geo").Path;
                var output = RootDirectory / "artifacts" / "package";

                // Built first because Geo sets GeneratePackageOnBuild, which makes
                // pack imply --no-build: left to itself it would look for Release
                // assemblies nothing had produced (NU5026).
                DotNetTasks.DotNetBuild(_ => _.SetProjectFile(project).SetConfiguration("Release"));

                DotNetTasks.DotNetPack(_ =>
                    _.SetProject(project)
                        .SetConfiguration("Release")
                        .EnableNoBuild()
                        .SetOutputDirectory(output)
                );
            });
}
