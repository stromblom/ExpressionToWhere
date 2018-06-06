var solutionPath = "./src/Stromblom.ExpressionToWhere.sln";
var artifactPath = Directory("./nuget-packages");
DotNetCoreMSBuildSettings msBuildSettings = null;

Task("Clean")
    .Does(() => {
        if(DirectoryExists(artifactPath))
        {
            DeleteDirectory(artifactPath, new DeleteDirectorySettings { Recursive = true, Force = true });
        }

        DeleteFiles("./*.nupkg");
    });

Task("Restore")
    .Does(() => {
        Information("Restoring NuGet Packages for dotnet core...");
        DotNetCoreRestore(solutionPath);
    });

Task("Version")
    .Does(() => {
        var packageVersion = "0.1.0-local";
        var informationalVersion = packageVersion;

        if (AppVeyor.IsRunningOnAppVeyor)
        {
            packageVersion = $"0.1.0.{AppVeyor.Environment.Build.Number}";
            var commitSha = AppVeyor.Environment.Repository.Commit.Id;
            informationalVersion = $"{packageVersion}-{commitSha}";

            if(AppVeyor.Environment.Repository.Branch != "master")
            {
                packageVersion = $"{packageVersion}-{AppVeyor.Environment.Repository.Branch}";
            }
        }

        msBuildSettings = new DotNetCoreMSBuildSettings()
                            .WithProperty("Version", packageVersion)
                            .WithProperty("FileVersion", packageVersion)
                            .WithProperty("InformationalVersion", informationalVersion);
    });

 Task("Build")
    .Does(() => {
        var settings = new DotNetCoreBuildSettings {
            Configuration = "Release",
            MSBuildSettings = msBuildSettings
        };

        DotNetCoreBuild(solutionPath, settings);
    });

Task("Package")
    .Does(() => {
        CreateDirectory(artifactPath​);
        
        var dotNetCorePackSettings = new DotNetCorePackSettings {
            IncludeSymbols = true,
            Configuration = "Release",
            MSBuildSettings = msBuildSettings,
            OutputDirectory = artifactPath,
            NoBuild = true
        };

        DotNetCorePack("./src/Stromblom.ExpressionToWhere/Stromblom.ExpressionToWhere.csproj", dotNetCorePackSettings);
    });

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Version")
    .IsDependentOn("Build")
    .IsDependentOn("Package");
    
var target = Argument("target", "Default");
RunTarget(target);