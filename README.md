# FlexVersion

This library/tool makes it easy to calculate very flexible version-strings (++) based on the repository commits and tags.

## Concepts

* Version Source

  The version source is the commit that identifies the repo in regards to the state it is in. The tag name or the merge commit message can contain identifiers that help identify this, like i.e. 1.0.0-beta.12. The typical (and default config) use case is that tags and merges with that message will be used as a version-source. 

* Actions

  When a version-source has been identified, the commits are played from oldest to newest and for each message the configuration allows you to perform increments (or decrements) on any of the detected parts from the version-source. Or you can create new parts that are counted. For instance, the default config adds "CommitCount", and then increments this on each commit/merge.

* Output

  At the end you usually want to create various output strings. These can be freely setup using any text, the above parts from the version-source, various other variables and functions.

* Very flexible configration

  No configuration options are default, they are all provided in the flexversion.yml file.
  The configuration system has all configuration options in separate branches, but all branches by default are setup to inherit from it. If you don't want to inherit then you can manipulate which parts you want to copy and not.
  The configuration file is written in YAML, which makes it very human friendly, and also host a lot of options for reusing config sections within the config-file.

## Console app

The FlexVersion.exe (TODO) allows you to run this in a console. It can take additional input parameters to control which confguration-file to use, which repo to connect to and additional variables that you want to pass to the generated output strings. Run with help as option to see the usage.

Since the console project is created using .Net Core it can run on Linux, Mac and Windows [full specification](https://github.com/dotnet/core/blob/master/release-notes/2.0/2.0-supported-os.md).

## Library

The project core library, IntelliSearch.FlexVersion targets .NetStandard 2.0. This library can be used as a dll in your .Net project. This means that it can be used by a any .Net implementation that supports .Net Standard 2.0. At the time of writing this:

* .Net Core 2.0+
* .Net Framework 4.6.1+
* Mono 5.4+ 
* Xamarin.iOS 10.14+
* Xamarin.Mac 3.8+
* Xamarin.Android 8.0+
* Universal Windows Platform 10.0.16299+

Ref: [.NET Standard Implementation support](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

## Getting started

At the moment you will have to clone the project, build it and then use the assemblies created. It is planned however to make this available on NuGet, Chocolatey and as a Cake addin.

## Configuration

The settings-file is formatted via YAML and all configuration is added as items in the **Branches** section. The `"*"` entry is special, as this is the configuration that is used when a branch has no specific matches.

The suggested strategy (as used in the default config-file) is to add an entry per branch-type you use. I.e. master, support, release, feature, hotfix etc. The branches that has not been specified will use the `"*"` branch as fallback.

Have a look at the [default configuration-file](IntelliSearch.FlexVersion.Console/flexversion.yml) to get more details on the options available. It may look daunting at first sight, but it is fairly well documented and most of it are defaults that you may not want to change anyway.

## Roadmap

* Set up CI  build and push to NuGet
* Complete Cake addin
* Handle pre-release label overrides via tag (and branch?). Specifically handy for release-candidates in GitFlow where the `beta` label is to be replaced with i.e. `RC` or `RC1`.
* Write tests.

## Disclaimer

This project is in beta state, and although it "works for me" it might not "work for you". If you find any issues or have any suggestions plese create an issue on the issue page. Or, if you want to contribute then that is super-cool too :).

## Why not just use GitVersion?

I did this project instead of GitVersion because:

* I could not get GitVersion to play the way I wanted it to.
* Response on the project was slow.
* It seems to be buggy for both the 3.x stable version as well as the 4.x beta. The CommitsSinceVersionSource counter seems to not do what I would have thought that it did.
* I could not find out how to add increment for the pre-counter for every commit for pre-release info.
* It is dead-slow, even when you are runing it on a repo where the current commit has a tag that dictates the version. It sometimes takes many minutes to figure out the version.
* It sometimes fails and crashes, without me having been able to figure out the issues.
* Response on the project on GitHub seems slow. Not sure if it is being maintained.

Now, all the issues I was having could be me doing something wrong in the configuration. While looking into things I got my own idea on how to approach this and that is what triggered this project.

## License

Copyright (c) 2018 IntelliSearch Software AS.

FlexVersion is provided as-is under the MIT license. For more information see LICENSE.
