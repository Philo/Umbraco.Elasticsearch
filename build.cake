// #tool "xunit.runner.console"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target                  = Argument("target", "Default");
var configuration           = Argument("configuration", "Release");
var solutionPath            = MakeAbsolute(File(Argument("solutionPath", "./Umbraco.Elasticsearch.sln")));
var nugetProject            = Argument("nugetProject", "Umbraco.Elasticsearch");


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

FilePath nugetProjectPath            = null;
SolutionParserResult solution        = null;
GitVersion versionInfo               = null;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(() => {
    CreateDirectory(artifacts);
    
    if(!FileExists(solutionPath)) throw new Exception(string.Format("Solution file not found - {0}", solutionPath.ToString()));
    solution = ParseSolution(solutionPath.ToString());

    var project = solution.Projects.FirstOrDefault(x => x.Name == nugetProject);
    if(project == null) throw new Exception(string.Format("Unable to find project '{0}' in solution '{1}'", nugetProject, solutionPath.GetFilenameWithoutExtension()));
    nugetProjectPath = project.Path;
    
    if(!FileExists(nugetProjectPath)) throw new Exception("project path not found");
    Information("[Setup] Using Solution '{0}' with project '{1}'", solutionPath.ToString(), project.Name);    
        
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifacts);
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
        .WithProperty("OutputPath", buildOutput.ToString())
        .SetVerbosity(Verbosity.Quiet)
        .SetConfiguration(configuration)
        .WithTarget("Rebuild")
    );
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() => 
{
    var nuGetPackSettings   = new NuGetPackSettings {
                                 Id                      = "Umbraco.Elasticsearch",
                                 Version                 = versionInfo.NuGetVersionV2,
                                 Title                   = "Umbraco.Elasticsearch",
                                 Authors                 = new[] {"Phil Oyston"},
                                 Owners                  = new[] {"Phil Oyston", "Storm ID"},
                                 Description             = "Integration of Elasticsearch into Umbraco for front end search",
                                 Summary                 = "Integration of Elasticsearch into Umbraco for front end search",
                                 ProjectUrl              = new Uri("https://github.com/Philo/Umbraco.Elasticsearch"),
                                 //IconUrl                 = new Uri("http:cdn.rawgit.com/SomeUser/TestNuget/master/icons/testnuget.png"),
                                 LicenseUrl              = new Uri("https://raw.githubusercontent.com/Philo/Umbraco.Elasticsearch/master/LICENSE"),
                                 Copyright               = "2016",
                                 // ReleaseNotes            = new [] {"Bug fixes", "Issue fixes", "Typos"},
                                 Tags                    = new [] {"Elasticsearch", "Umbraco", "Nest"},
                                 RequireLicenseAcceptance= false,
                                 Symbols                 = true,
                                 NoPackageAnalysis       = true,
                                 Properties              = new Dictionary<string, string> { { "Configuration", configuration }},
                                 Files                   = new [] {
                                                                      new NuSpecContent {Source = buildOutput + "/Umbraco.Elasticsearch.dll", Target = "lib/net45"},
                                                                      new NuSpecContent {Source = buildOutput + "/Umbraco.Elasticsearch.pdb", Target = "lib/net45"},
                                                                      new NuSpecContent {Source = buildOutput + "/App_Plugins/**/*", Target = "content"},
                                                                   }, 
                                 BasePath                = buildOutput,
                                 OutputDirectory         = artifacts
                             };
                             
    NuGetPack(nugetProject +".nuspec", nuGetPackSettings);                             
});

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
    AppVeyor.UpdateBuildVersion(versionInfo.FullSemVer +" (Build: " +AppVeyor.Environment.Build.Number +")");
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
