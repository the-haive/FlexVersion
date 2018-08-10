# GitSemVer
Calculates SemVer version based on information from the git-repo (commits, merges and tags) as well as your GitSemVer configuration.

This library/tool uses the git repo as a source for calculating which SemVer version your project is currently at. It is inspired by
GitVersion, but uses a different algorithm and solution to find versioning information. See at the bottom for reasons I did this instead of using GitVersion.

## Concepts
A central concept is the version-source. The version-source can be a tag, a merge or a fallback value. Each commit (or merge-commit) after
the version-source can (depending on your settings bump the major, minor, patch or pre-count of the version.

Another central part is that this system allows for a prerelease-count to be automatically calculated. The idea is that each commit in the 
repo, will have an identifyable *pre-release* version, by adding a label and a pre-count as sem-ver prerelease information.

The project has a central library, GitSemVer (.NetStandard 2.0), that can be used as a dll in your .Net project.

## Console app
There is also an executable that allows you to run this in a console, and which outputs the results in a JSON format.

Since the console project is created using .Net Core it can run on Linux, Mac and Windows.

The console app is bare bones in its set up. No help output (yet). It optionally takes two parameters:
* The path to the settings-file (.\gitsemver.yml if omitted)
* The path to the repo (current directory if omitted)

## Settings
The settings-file is formatted via YAML. 

Here is a sample (very loosely geared towards the GitFlow pattern):
```yaml
Branches:
  # This is used as a default. Any branch that does not match the defined branch-regexes will use this.
  # Also, any defined branch will use these values as a starting point and will override only values defined.
  "*": 
    # This means that it follows the first parent for merges when iterating. This prevents pollution from other branches.
    IterateFirstParentOnly: True 
    MaxCommitsToAnalyze: -1 # In the unlikely event that analyzing takes too long, restrict the number of commits to iterate. 
    MergeSourceBranchPattern: Merged?\s+(?>(?>remote-tracking)|(?>branch)\s+)?(?<from>.+)\s+into\s+.* # How the 'from'-branch is detected via regex. 
    TagPattern: ^v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+).*$ # Only tags that matches are considered (has major, minor and patch)
    AnnotatedTagsOnly: False # Tags in git are annotated or lightweight.
    OnCommit: BumpPre # Bump pre-count on every commit after version-source.
    OnMerge:
      "*": None # Default merge action is nothing.
    Label: 0.wip # Default label
  Master:
    Regex: master
    OnCommit: BumpPatch # Every commit on master should bump the patch number.
    OnMerge:
      Release: BumpMajor # Every merge from the release-branch to master should bump master (this may not be what you want, for illustration purposes only)
      Feature: BumpMinor # Every merge from feature-branch to master should bump minor.
      Hotfix: BumpPatch # Every merge from hotfix to master should bump the patch-number.
    Label: # No label for the master releases
  Support:
    Regex: support[/-]
    OnCommit: BumpPatch
    OnMerge:
      Release: BumpMajor
      Feature: BumpMinor
      Hotfix: BumpPatch
    Label:
  Release:
    Regex: release[/-]
    Label: beta
  Develop:
    Regex: develop
    Label: alpha
  Feature:
    Regex: (feature|issue|.+-/d+)[/-] # Also handle Jira-branches without feature or issue-prefix
  Hotfix:
    Regex: hotfix[/-]
```
## Disclaimer
This project is in its early days (alpha state), and although it "works for me" it might not "work for you". Use at your own risk. If you find any issues or have any suggestions plese create an issue on the issue page. Or, if you want to contribute then that is super-cool too :).

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

