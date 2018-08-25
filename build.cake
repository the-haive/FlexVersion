#addin "nuget:?package=Cake.FileHelpers&version=2.0.0"
#addin "nuget:?package=Cake.Json"
#addin "nuget:?package=Newtonsoft.Json&version=9.0.1"
#tool "nuget:?package=NUnit.Extension.NUnitV2ResultWriter&version=3.6.0"
#tool "nuget:https://www.myget.org/F/nunit/api/v3/index.json?package=NUnit.ConsoleRunner&version=3.9.0-dev-04009"
#r "tools/addins/Cake.FlexVersion/Cake.FlexVersion.dll"

using System.Text.RegularExpressions;
using System.Xml;
using IntelliSearch.FlexVersion.Configuration;


/////////////////
// SETTINGS
/////////////////

// Default task
var target = Argument("Target", "Help");

// Default configuration
var configuration = Argument("configuration", "Debug");

// Default solution to act on
var solution = "./src/FlexVersion.sln";

// NuGet access
var nuGetSource = "https://int.nugettest.org"; // "https://api.nuget.org/v3/index.json";
var nuGetApiKey = EnvironmentVariable("NuGetApiKey"); // Must be set on the computer running the build in order to push packages to NuGet.org

// Which projects to post-process with `dotnet publish` in regards to include the libgit2 native dependencies
var projectsToPublish = new List<string> {
    "./src/FlexVersion.Console/FlexVersion.Console.csproj",
//    "./src/Cake.FlexVersion/Cake.FlexVersion.csproj",
};
var publishPath = "./artifacts";

// Which projects to make NuGet packages for.
var projectsToPackage = new List<string> {
    "./src/FlexVersion/FlexVersion.csproj",
    "./src/FlexVersion.Console/FlexVersion.Console.csproj",
    "./src/Cake.FlexVersion/Cake.FlexVersion.csproj",
};

// Which packages to deploy to NuGet.org
var packagesToDeployToNuGetOrg = new List<string> { 
    "./src/FlexVersion/**/*.nupkg",
    "./src/Cake.FlexVersion/**/*.nupkg"
};

// Which packages to deploy to Chocolatey
var packagesToDeployToChoco = new List<string> { 
    // TODO: Find out how to create Choco package and how to build binaries for the various platforms.
    // TODO: And, how to separate them. Same package, different packages?
    "./src/FlexVersion.Console/**/*.nupkg"
}; 

// Global variable for holding the version-info from FlexVersion, making it accessible in all tasks.
Dictionary<string,string> flexVersion = null;


/////////////////
// HELP
/////////////////

Task("Help")
    .Description("Shows the options and targets you can use.")
    .Does(() => {
        Information("USAGE: build --Target=<target> --Configuration=<configuration> --Verbosity=<verbosity> --DryRun --Debug");
        Information("");
        Information("Executes the <target> task on the <solutions> given using the <configuration> environment.");
        Information("");
        Information("Options:");
        Information("");
        Information("  --Target           The build-target to execute.");
        Information("                     (Default: Help)");
        Information("");
        Information("  --Configuration    The configuration to use for the build (passed to MSBuild).");
        Information("                     (Default: Debug)");
        Information("");
        Information("Options for diagnostics:");
        Information("");
        Information("  --Verbosity=value  Specifies the amount of information to be displayed (Quiet, Minimal, Normal, Verbose, Diagnostic).");
        Information("                     (Default: Normal)");
        Information("");
        Information("  --DryRun           Performs a dry run.");
        Information("");
        Information("  --Debug            To debug the build-script in VS:");
        Information("                     1. Open the build.cake file in Visual Studio and set a breakpoint on the 'line of interest'.");
        Information("                     2. Run build-script with --debug and note the pid listed.");
        Information("                     3. Attach Visual Studio to the process with the aforementioned pid.");
        Information("");
        Information("Available targets: ");
        Information("");
        foreach(var task in Tasks) {
            var deps = "";
            if (task.Dependencies.Count > 0) {
                deps = $" (depends on: {string.Join(", ", task.Dependencies.Select(i => i.Name))})";
            }
            Information($"* {task.Name}{deps}");
            Information($"{task.Description}");
            Information("");
        }

});


/////////////////
// CLEANING
/////////////////

Task("Clean")
    .Description("Cleans away binaries and NuGet packages.")
    .IsDependentOn("Clean-NuGet-Dependency-Packages")
    .IsDependentOn("Clean-Artifacts")
    .IsDependentOn("Clean-Binaries");

Task("Clean-NuGet-Dependency-Packages")
    .Description("Removes all NuGet dependency packages in the ./packages folder.")
    .Does(() =>
    {
        Information("*** Removing NuGet dependency packages...");
        CleanDirectories("./packages");
        Verbose($"*** Done removing NuGet dependency packages.");
    });

Task("Clean-Artifacts")
    .Description($"Removes all publish-artifacts in the {publishPath}.")
    .Does(() =>
    {
        Information("*** Removing publish-artifacts...");
        CleanDirectories(publishPath);
        Verbose($"*** Done removing publish-artifacts.");
    });

Task("Clean-Binaries")
    .Description("Removes all binaries in the 'bin' and 'obj' folders, and even also calls Clean using MSBuild for the respective solutions.")
    .Does(() =>
    {
        Information("*** Removing existing binaries...");
        CleanDirectories($"./**/obj/{configuration}");
        CleanDirectories($"./**/bin/{configuration}");

        DotNetCoreClean(solution);
    });


/////////////////
// PREPARE
/////////////////

Task("Prepare")
    .Description("Prepares for building.")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Create-Generated.Build.Props");

Task("Restore-NuGet-Packages")
    .Description("Fetches all dependent NuGet packages for the solution, before even starting the build for the solution.")
    .Does(() =>
    {
        DotNetCoreRestore(solution);
    });

Task("Create-Generated.Build.Props")
    .Description("Generates a common assemblyInfo that is linked in for all built projects in the solutions, after getting the version-info from the repo.")
    .IsDependentOn("Version")
    .Does(() =>
    {
        Information("*** Creating ./src/Generated.Build.props...");

        var yearFrom = 2018;
        var yearTo = DateTime.Now.Year;
        var yearSpec = yearTo == yearFrom ? $"{yearFrom}" : $"{yearFrom}-{yearTo}";

        XmlWriter xmlWriter = XmlWriter.Create(@".\src\Generated.Build.props", new XmlWriterSettings {
            OmitXmlDeclaration = true,
            Indent = true,
        });
        xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Project");
                xmlWriter.WriteStartElement("PropertyGroup");
                    xmlWriter.WriteElementString("Copyright", $"All rights reserved © {yearSpec} IntelliSearch Software AS");
                    xmlWriter.WriteElementString("Version", flexVersion["SemVerNuGetV2"]);
                    xmlWriter.WriteElementString("AssemblyVersion", $"{flexVersion["Major"]}.0.0.0");
                    xmlWriter.WriteElementString("FileVersion", $"{flexVersion["Version"]}.0");
                xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();

        Verbose("*** Done creating ./src/Generated.Build.props.");
    });

Task("Version")
    .Description("Gets the version of the repo, as deducted by FlexVersion.")
    .Does(() => {
        var result = FlexVersion("./src/flexVersion.yml", null, $"Configuration={configuration}");
        flexVersion = result.Output;
        Information($"FlexVersion[SemVerFull] = {flexVersion["SemVerFull"]}");
        Verbose(SerializeJsonPretty(flexVersion));
        //packageVersion = configuration == "Debug" ? flexVersion["SemVerNuGetV2"] : flexVersion["Version"];
});

Task("Clean-Prepare")
    .Description("Prepares for building, after cleaning.")
    .IsDependentOn("Clean")
    .IsDependentOn("Prepare");


/////////////////
// BUILDING
/////////////////

Task("Build")
    .Description("Builds the solutions, after prepare.")
    .IsDependentOn("Prepare")
    .IsDependentOn("Build-Execute");

Task("Build-Execute")
    .Description("Builds the solutions, without cleaning. Depends on the NuGetPackages (and the Generated.Build.props-file to exist.")
    .Does(() =>
    {
        DotNetCoreBuild(solution, new DotNetCoreBuildSettings
                {
                    Configuration = configuration,
                    NoRestore = true
                });
    });

Task("Clean-Build")
    .Description("Builds, after cleaning and prepare.")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");


/////////////////
// TESTING
/////////////////

Task("Test")
    .Description("Executes unit-tests, after building. Picks up any assembly where the name ends with 'Tests.dll' or 'Tests.dll', but limits according to the --TestFilter setting, if used.")
    .IsDependentOn("Build")
    .IsDependentOn("Test-Execute");

Task("Test-Execute")
    .Description("Executes unit-tests, without cleaning or building. Depends on the test-binaries to already exist.")
    .Does(() =>
    {
        var projects = GetFiles($"./src/**/bin/{configuration}/**/*Tests.csproj");

        foreach(var project in projects)
        {
            DotNetCoreTest(
              project.FullPath,
              new DotNetCoreTestSettings {
                    Configuration = configuration,
                    NoRestore = true,
                    NoBuild = true
            });
        }
    });

Task("Clean-Test")
    .Description("Executes unit-tests, after cleaning and building.")
    .IsDependentOn("Clean")
    .IsDependentOn("Test");


/////////////////
// PUBLISHING
/////////////////

Task("Publish")
    .Description("Publishes the projects, after testing.")
    .IsDependentOn("Test")
    .IsDependentOn("Publish-Execute");

Task("Publish-Execute")
    .Description("Publishes projects (without any task-dependencies).")
    .Does(() =>
    {
        DotNetCorePublish(solution, new DotNetCorePublishSettings
        {
            Configuration = configuration,
            //NoRestore = true,
            //Runtime = "win7-x64",
            //ArgumentCustomization = args => args.Append("--no-build"),
            //TODO: NoBuild = true,
            //OutputDirectory = System.IO.Path.Combine(publishPath),
            //DiagnosticOutput = true,
            //Verbosity = DotNetCoreVerbosity.Detailed
        });
        //foreach(var project in projectsToPublish)
        //{
        //    var path = project.Split('/').Last();
        //    path = path.Substring(0, path.LastIndexOf('.'));
        //    DotNetCorePublish(project, new DotNetCorePublishSettings
        //        {
        //            Configuration = configuration,
        //            NoRestore = true,
        //            Runtime = "win7-x64",
        //            ArgumentCustomization = args => args.Append("--no-build"),
        //            //TODO: NoBuild = true,
        //            OutputDirectory = System.IO.Path.Combine(publishPath, path),
        //            //DiagnosticOutput = true,
        //            //Verbosity = DotNetCoreVerbosity.Detailed
        //        });
        //}
    });

Task("Clean-Publish")
    .Description("Publishes projects, after cleaning and testing.")
    .IsDependentOn("Clean")
    .IsDependentOn("Publish");


/////////////////
// PACKAGING
/////////////////

Task("Package")
    .Description("Packages NuGet packages, after publishing.")
    .IsDependentOn("Publish")
    .IsDependentOn("Package-Execute");

Task("Package-Execute")
    .Description("Packages NuGet packages (without any task-dependencies).")
    .Does(() =>
    {
        foreach(var project in projectsToPackage)
        {
            DotNetCorePack(
              project,
              new DotNetCorePackSettings {
                Configuration = configuration,
                NoRestore = true,
                NoBuild = true
            });
        }
    });

Task("Clean-Package")
    .Description("Packages NuGet packages, after cleaning and publishing.")
    .IsDependentOn("Clean")
    .IsDependentOn("Package");


/////////////////
// DEPLOYING
/////////////////

Task("Deploy")
    .Description("Deploying packages, after packaging. The deployment is only performed if running on the build-server.")
    .IsDependentOn("Package")
    .IsDependentOn("Deploy-NuGetOrg-Execute")
    .IsDependentOn("Deploy-Choco-Execute");

Task("Deploy-NuGetOrg-Execute")
    .Description("Deploying packages (without any task-dependencies). The deployment is only performed if running on the build-server.")
    .Does(() =>
    {
        if (configuration != "Release")
        {
            throw new Exception("Only deploy packages for Configuration=Release. No packages deployed.");
        }

        foreach(var package in packagesToDeployToNuGetOrg)
        {
            var nuGetPackages = GetFiles(package);
            string pkgToPublish = null;
            if (nuGetPackages.Count > 1)
            {
                DateTime newest = DateTime.MinValue;
                foreach(var pkg in nuGetPackages)
                {
                    if (System.IO.File.GetCreationTime(pkg.FullPath) > newest)
                    {
                        pkgToPublish = pkg.FullPath;
                    }
                }
            }
            else if (nuGetPackages.Count == 1)
            {
                pkgToPublish = nuGetPackages.First().FullPath;
            }

            if (pkgToPublish == null) return;

            Information($"Deploying {pkgToPublish} to {nuGetSource}...");
            DotNetCoreNuGetPush(
              pkgToPublish,
              new DotNetCoreNuGetPushSettings {
                Source = nuGetSource,
                ApiKey = nuGetApiKey
            });
        }
    });

Task("Deploy-Choco-Execute")
    .Description("Deploying packages (without any task-dependencies). The deployment is only performed if running on the build-server.")
    .Does(() =>
    {
        if (configuration != "Release")
        {
            throw new Exception("Only deploy packages for Configuration=Release. No packages deployed.");
        }

        Information("Deploy to Choco is not implemented yet.");
        foreach(var package in packagesToDeployToChoco)
        {
    		// TODO: Deploy on Choco
        }
    });

Task("Clean-Deploy")
    .Description("Deploying packages, after cleaning and packaging. The deployment is only performed if running on the build-server.")
    .IsDependentOn("Clean")
    .IsDependentOn("Deploy");


RunTarget(target);
