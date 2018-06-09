///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

// To pass parameters to build.ps1:
// (https://stackoverflow.com/questions/40046752/how-do-i-pass-my-own-custom-arguments-to-build-ps1)
// .\build.ps1 -ScriptArgs "-Configuration=Debug","-SolutionFilename=MoreTesting.sln"

var _target = Argument("Target", "Default");
var _configuration = Argument("Configuration", "Release");
var _solutionFilename = Argument("SolutionFilename", "MoreTesting.sln");
var _sourceDir = Directory(Argument("SourceDir", "./src/"));
var _testDir = Directory(Argument("TestDir", "./tests/"));
var _outDir = Directory(Argument("OutDir", "./out/"));  // for storing tests results/artifacts/etc

///////////////////////////////////////////////////////////////////////////////
// TOOLS / ADDINS
///////////////////////////////////////////////////////////////////////////////

#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=NUnit.ConsoleRunner"

#addin "nuget:?package=Cake.Incubator"

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var _solutionPath = _sourceDir.Path.CombineWithFilePath(_solutionFilename);
var _solution = ParseSolution(_solutionPath);
var _projects = _solution.GetProjects();
var _projectDirs = _projects.Select(p => p.Path.GetDirectory());

var _testResultsOutDir = _outDir.Path.Combine("TestResults");
var _binOutDir = _outDir.Path.Combine("Binaries");
var _artifactsOutDir = _outDir.Path.Combine("Artifacts");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        foreach (var projectDir in _projectDirs)
        {
            CleanDirectories(GetProjectBinPathPattern(projectDir, _configuration));
            CleanDirectories(GetProjectObjPathPattern(projectDir, _configuration));
        }
        CleanDirectory(_outDir);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => 
    {
        NuGetRestore(_solutionPath);
    });


var _nugetVersion = "0.0.0";

Task("Update-Version")
    .Does(() =>
    {
        var result = GitVersion(new GitVersionSettings()
        {
            UpdateAssemblyInfo = true
        });

        Information(result.Dump());
        _nugetVersion = result.NuGetVersion;

    });

Task("Update-AppVeyor-Version")
    .IsDependentOn("Update-Version")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() => 
    {
        AppVeyor.UpdateBuildVersion(_nugetVersion);
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Update-Version")
    .Does(() => 
    {
        MSBuild(_solutionPath, new MSBuildSettings()
            .SetConfiguration(_configuration)
            .SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
    .IsDependentOn("Build")
    .DeferOnError() // should not continue if tests failed
    .Does(() => 
    {
        if (!DirectoryExists(_testResultsOutDir))
        {
            CreateDirectory(_testResultsOutDir);
        }

        var tests = GetTestsAssembliesPathPattern();
        NUnit3(tests, new NUnit3Settings(){
            // how to exclude certain category: https://stackoverflow.com/questions/47131112/nunit3-console-exclude-specific-test-category
            // NUnit test selection language: https://github.com/nunit/docs/wiki/Test-Selection-Language
            Where = "cat != IntegrationTests", 
            Work = _testResultsOutDir,
        });

    });

Task("Upload-Test-Results-To-AppVeyor")
    .IsDependentOn("Test")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() => 
    {
        var testResult = _testResultsOutDir.CombineWithFilePath("TestResult.xml");
        AppVeyor.UploadTestResults(testResult, AppVeyorTestResultsType.NUnit3);
    });

// projects that need to be distributed
var _projectNames = new[]
{
    "MoreTesting.Wpf",
    "MoreTesting.Console"
};
var _selectedProjects = _projects.Where(p => p.Name.IsIn(_projectNames));

// Copy built binaries for packaging/uploading/etc
Task("Copy-Binaries")
    .IsDependentOn("Update-Version")
    .IsDependentOn("Build")
    .Does(() => 
    {
        foreach (var project in _selectedProjects)
        {
            var binDir = GetProjectBinPath(project.Path.GetDirectory(), _configuration);
            // ! assume project assembly version is the nuget version
            var outDir = _binOutDir.Combine($"{project.Name}-{_nugetVersion}");
            CreateDirectory(outDir);
            CopyDirectory(binDir, outDir);  
        }
    });

// Create zip packages of each binaries
// and copy to Artifacts folder
Task("Zip-Binaries-Then-Copy")
    .IsDependentOn("Copy-Binaries")
    .Does(() => 
    {
        CreateDirectory(_artifactsOutDir);
        var binDirs = GetSubDirectories(_binOutDir);
        foreach (var binDir in binDirs)
        {
            Zip(binDir, $"{_artifactsOutDir.CombineWithFilePath(binDir.GetDirectoryName())}.zip");
        }
    });

Task("Upload-Artifacts-To-AppVeyor")
    .IsDependentOn("Zip-Binaries-Then-Copy")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() => 
    {
        var artifacts = GetFiles(_artifactsOutDir.FullPath);
        foreach (var artifact in artifacts)
        {
            Information(artifact.FullPath);
            AppVeyor.UploadArtifact(artifact.FullPath);
        }
    });

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Update-Version")
    .IsDependentOn("Update-AppVeyor-Version")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Upload-Test-Results-To-AppVeyor")
    .IsDependentOn("Copy-Binaries")
    .IsDependentOn("Zip-Binaries-Then-Copy")
    .IsDependentOn("Upload-Artifacts-To-AppVeyor");

///////////////////////////////////////////////////////////////////////////////
// HELPERS
///////////////////////////////////////////////////////////////////////////////

string GetProjectBinPathPattern(DirectoryPath projectDirectory, string configuration)
{
    return $"{projectDirectory}/**/bin/{configuration}";
}

string GetProjectBinPath(DirectoryPath projectDirectory, string configuration)
{
    return $"{projectDirectory}/bin/{configuration}";
}

string GetProjectObjPathPattern(DirectoryPath projectDirectory, string configuration)
{
    return $"{projectDirectory}/**/obj/{configuration}";
}

string GetTestsAssembliesPathPattern()
{
    return $"{_testDir.Path}/**/bin/{_configuration}/*.Tests.dll";
}

RunTarget(_target);
