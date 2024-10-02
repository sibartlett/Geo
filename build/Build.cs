using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

[GitHubActions(
    "ci",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(CheckForUncommittedChanges), nameof(Test) }
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

                if (Git($"status --porcelain").Count > 0)
                {
                    Assert.Fail("Uncommitted changes - run csharpier and prettier");
                }
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

    Target Publish =>
        _ =>
            _.Executes(() =>
            {
                DotNetTasks.DotNetPublish(_ => _.SetConfiguration("Release"));
            });
}
