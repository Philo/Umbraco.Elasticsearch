// #tool "xunit.runner.console"
#tool "GitVersion.CommandLine"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target                  = Argument("target", "Default");
var configuration           = Argument("configuration", "Release");
var solutionPath            = MakeAbsolute(File(Argument("solutionPath", "./Umbraco.Elasticsearch.sln")));
var nugetProjects            = Argument("nugetProjects", "Umbraco.Elasticsearch");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var testAssemblies          = new [] { 
                                "./tests/**/bin/" +configuration +"/*.UnitTests.dll" 
                            };

var artifacts               = MakeAbsolute(Directory(Argument("artifactPath", "./artifacts")));
var buildOutput             = MakeAbsolute(Directory(artifacts +"/build/"));
var testResultsPath         = MakeAbsolute(Directory(artifacts + "./test-results"));
var versionAssemblyInfo     = MakeAbsolute(File(Argument("versionAssemblyInfo", "VersionAssemblyInfo.cs")));

IEnumerable<FilePath> nugetProjectPaths     = null;
SolutionParserResult solution               = null;
GitVersion versionInfo                      = null;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(() => {
    if(!FileExists(solutionPath)) throw new Exception(string.Format("Solution file not found - {0}", solutionPath.ToString()));
    solution = ParseSolution(solutionPath.ToString());

    var projects = solution.Projects.Where(x => nugetProjects.Contains(x.Name));
    if(projects == null || !projects.Any()) throw new Exception(string.Format("Unable to find projects '{0}' in solution '{1}'", nugetProjects, solutionPath.GetFilenameWithoutExtension()));
    nugetProjectPaths = projects.Select(p => p.Path);
    
    // if(!FileExists(nugetProjectPath)) throw new Exception("project path not found");
    Information("[Setup] Using Solution '{0}'", solutionPath.ToString());
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(artifacts.ToString());
    CreateDirectory(artifacts);
    CreateDirectory(buildOutput);
    
    var binDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\bin");
    var objDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\obj");
    CleanDirectories(binDirs);
    CleanDirectories(objDirs);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath, new NuGetRestoreSettings());
});

Task("Update-Version-Info")
    .IsDependentOn("CreateVersionAssemblyInfo")
    .Does(() => 
{
        versionInfo = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true,
            UpdateAssemblyInfoFilePath = versionAssemblyInfo
        });

    if(versionInfo != null) {
        Information("Version: {0}", versionInfo.FullSemVer);
    } else {
        throw new Exception("Unable to determine version");
    }
});

Task("CreateVersionAssemblyInfo")
    .WithCriteria(() => !FileExists(versionAssemblyInfo))
    .Does(() =>
{
    Information("Creating version assembly info");
    CreateAssemblyInfo(versionAssemblyInfo, new AssemblyInfoSettings {
        Version = "0.0.0.0",
        FileVersion = "0.0.0.0",
        InformationalVersion = "",
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version-Info")
    .Does(() =>
{
    MSBuild(solutionPath, settings => settings
        .WithProperty("TreatWarningsAsErrors","true")
        .WithProperty("UseSharedCompilation", "false")
        .WithProperty("AutoParameterizationWebConfigConnectionStrings", "false")
        .SetVerbosity(Verbosity.Quiet)
        .SetConfiguration(configuration)
        .WithTarget("Rebuild")
    );
});

Task("Copy-Files-Umbraco-Elasticsearch")
    .IsDependentOn("Build")
    .Does(() => 
{
    EnsureDirectoryExists(buildOutput +"/Umbraco.Elasticsearch");
    CopyFile("./src/Umbraco.Elasticsearch/bin/" +configuration +"/Umbraco.Elasticsearch.dll", buildOutput +"/Umbraco.Elasticsearch/Umbraco.Elasticsearch.dll");
    CopyDirectory("./src/Umbraco.Elasticsearch/content", buildOutput +"/Umbraco.Elasticsearch/content");
});

Task("Copy-Files-Umbraco-Elasticsearch-Core")
    .IsDependentOn("Build")
    .Does(() => 
{
    EnsureDirectoryExists(buildOutput +"/Umbraco.Elasticsearch.Core");
    CopyFile("./src/Umbraco.Elasticsearch.Core/bin/" +configuration +"/Umbraco.Elasticsearch.Core.dll", buildOutput +"/Umbraco.Elasticsearch.Core/Umbraco.Elasticsearch.Core.dll");
});

Task("Package-Umbraco-Elasticsearch-Core")
    .IsDependentOn("Build")
    .IsDependentOn("Copy-Files-Umbraco-Elasticsearch-Core")
    .Does(() => 
{
        var settings = new NuGetPackSettings {
            BasePath = buildOutput +"/Umbraco.Elasticsearch.Core",
            Id = "Umbraco.Elasticsearch.Core",
            Authors = new [] { "Phil Oyston" },
            Owners = new [] {"Phil Oyston", "Storm ID" },
            Description = "Provides integration between Umbraco content and media, and Elasticsearch as a search platform",
            LicenseUrl = new Uri("https://raw.githubusercontent.com/Philo/Umbraco.Elasticsearch/master/LICENSE"),
            ProjectUrl = new Uri("https://github.com/Philo/Umbraco.Elasticsearch"),
            IconUrl = new Uri("https://raw.githubusercontent.com/Philo/Umbraco.Elasticsearch/master/lib/icons/umbraco-es.png"),
            RequireLicenseAcceptance = false,
            Properties = new Dictionary<string, string> { { "Configuration", configuration }},
            Symbols = false,
            NoPackageAnalysis = true,
            Version = versionInfo.NuGetVersionV2,
            OutputDirectory = artifacts,
            IncludeReferencedProjects = true,
            Tags = new[] { "Umbraco", "Elasticsearch" },
            Files = new[] {
                new NuSpecContent { Source = "Umbraco.Elasticsearch.Core.dll", Target = "lib/net452" },
            },
            Dependencies = new [] {
                new NuSpecDependency { Id = "Nest.Indexify", Version = "0.3.1" },
                new NuSpecDependency { Id = "Nest.Searchify", Version = "0.9.1" },
                new NuSpecDependency { Id = "UmbracoCms.Core", Version = "7.2.0" }
            }
        };
        NuGetPack("./src/Umbraco.Elasticsearch.Core/Umbraco.Elasticsearch.Core.nuspec", settings);                     
});

Task("Package-Umbraco-Elasticsearch")
    .IsDependentOn("Build")
    .IsDependentOn("Copy-Files-Umbraco-Elasticsearch")
    .Does(() => 
{
        var settings = new NuGetPackSettings {
            BasePath = buildOutput +"/Umbraco.Elasticsearch",
            Id = "Umbraco.Elasticsearch",
            Authors = new [] { "Phil Oyston" },
            Owners = new [] {"Phil Oyston", "Storm ID" },
            Description = "Provides integration between Umbraco content and media, and Elasticsearch as a search platform",
            LicenseUrl = new Uri("https://raw.githubusercontent.com/Philo/Umbraco.Elasticsearch/master/LICENSE"),
            ProjectUrl = new Uri("https://github.com/Philo/Umbraco.Elasticsearch"),
            IconUrl = new Uri("https://raw.githubusercontent.com/Philo/Umbraco.Elasticsearch/master/lib/icons/umbraco-es.png"),
            RequireLicenseAcceptance = false,
            Properties = new Dictionary<string, string> { { "Configuration", configuration }},
            Symbols = false,
            NoPackageAnalysis = true,
            Version = versionInfo.NuGetVersionV2,
            OutputDirectory = artifacts,
            IncludeReferencedProjects = true,
            Tags = new[] { "Umbraco", "Elasticsearch" },
            Files = new[] {
                new NuSpecContent { Source = "Umbraco.Elasticsearch.dll", Target = "lib/net452" },

                new NuSpecContent { Source = "content/web.config.install.xdt", Target = "content" },
                new NuSpecContent { Source = "content/web.config.uninstall.xdt", Target = "content" },
                new NuSpecContent { Source = "content/dashboard.config.install.xdt", Target = "content/config" },
                new NuSpecContent { Source = "content/dashboard.config.uninstall.xdt", Target = "content/config" },

                new NuSpecContent { Source = "content/UmbracoElasticsearchStartup.cs.pp", Target = "content" },

                new NuSpecContent { Source = "content/App_Plugins/umbElasticsearch/**/*", Target = "" }
            },
            Dependencies = new [] {
                new NuSpecDependency { Id = "Nest.Indexify", Version = "0.3.1" },
                new NuSpecDependency { Id = "Nest.Searchify", Version = "0.9.1" },
                new NuSpecDependency { Id = "UmbracoCms.Core", Version = "7.2.0" },
                new NuSpecDependency { Id = "Umbraco.Elasticsearch.Core", Version = "[" +versionInfo.NuGetVersionV2 +"]" }
            }
        };
        NuGetPack("./src/Umbraco.Elasticsearch/Umbraco.Elasticsearch.nuspec", settings);                     
});

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("Package-Umbraco-Elasticsearch")
    .IsDependentOn("Package-Umbraco-Elasticsearch-Core")
    .Does(() => { });

// Task("Package")
//     .IsDependentOn("Build")
//     .Does(() => 
// {
//     foreach(var nugetProjectPath in nugetProjectPaths) {
//         var settings = new NuGetPackSettings {
//             Properties = new Dictionary<string, string> { { "Configuration", configuration }},
//             Symbols = true,
//             NoPackageAnalysis = true,
//             Version = versionInfo.NuGetVersionV2,
//             OutputDirectory = artifacts,
//             IncludeReferencedProjects = true
//         };
//         NuGetPack(nugetProjectPath, settings);                     
//     }
// });

/*
 * TODO : erm, unit tests
 *
Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    CreateDirectory(testResultsPath);

    var settings = new XUnit2Settings {
        XmlReportV1 = true,
        NoAppDomain = true,
        OutputDirectory = testResultsPath,
    };
    settings.ExcludeTrait("Category", "Integration");
    
    XUnit2(testAssemblies, settings);
}); */

Task("Update-AppVeyor-Build-Number")
    .IsDependentOn("Update-Version-Info")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UpdateBuildVersion(versionInfo.FullSemVer +" | " +AppVeyor.Environment.Build.Number);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Update-Version-Info")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    ;

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
