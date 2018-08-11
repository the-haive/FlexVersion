using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IntelliSearch.GitSemVer.Configuration;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace IntelliSearch.GitSemVer
{
    // https://regex101.com/r/ysJPjT/2/

    /// <summary>
    /// GitSemVer is a class that facilitates creating SemVer versions for your git repo, based on provided settings.
    /// </summary>
    public class GitSemVer
    {
        private readonly string _repoPath;
        private readonly GitSemVerConfiguration _gitSemVerConfiguration;

        /// <summary>
        /// Initialize GitSemVer with repoPath and given configuration-object.
        /// 
        /// </summary>
        /// <param name="gitSemVerConfiguration">The config-object to use.</param>
        /// <param name="repoPath">If null the current working directory is assumed.</param>
        public GitSemVer(GitSemVerConfiguration gitSemVerConfiguration, string repoPath = null)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            _gitSemVerConfiguration = gitSemVerConfiguration;
        }

        /// <summary>
        /// Initialize the repo with configuration from a Yaml-file.
        /// </summary>
        /// <param name="configurationAsYamlFilePath">If null it defaults to 'gitsemver.yml'</param>
        /// <param name="repoPath">If null current working directory is assumed.</param>
        public GitSemVer(string configurationAsYamlFilePath = null, string repoPath = null)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            configurationAsYamlFilePath = configurationAsYamlFilePath ?? @".\gitsemver.yml";

            if (!File.Exists(configurationAsYamlFilePath)) throw new ArgumentException($"The configuration-file cannot be found at '{configurationAsYamlFilePath}'");

            var deserializer = new Deserializer();
            var parser = new MergingParser(new Parser(File.OpenText(configurationAsYamlFilePath)));
            //var result = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);
            _gitSemVerConfiguration = deserializer.Deserialize<GitSemVerConfiguration>(parser);
        }

        /// <summary>
        /// Analyzes the branch using the configuration given to identify versioning information as well as providing some git repo information.
        /// </summary>
        /// <returns>A result object with various GitSemVer-relevant versioning information.</returns>
        public Result Analyze()
        {
            return null;
        }
    }

    public class Test
    {
        public Item Base { get; set; }
        public Item Foo { get; set; }
        public Item Bar { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}