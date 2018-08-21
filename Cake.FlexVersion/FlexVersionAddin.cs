using System;
using Cake.Core;
using Cake.Core.Annotations;
using IntelliSearch.FlexVersion;
using IntelliSearch.FlexVersion.Configuration;
using LogLevel = Cake.Core.Diagnostics.LogLevel;
using Verbosity = Cake.Core.Diagnostics.Verbosity;

namespace Cake.FlexVersion
{
    /// <summary>
    /// TODO: Add liblog wrapper, that consumes log-entries and makes them cake-log-messages.
    /// TODO: Add support for argument variables.
    /// </summary>
    public static class Addin
    {
        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, FlexVersionConfiguration configuration)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configuration).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, FlexVersionConfiguration configuration, string repoPath)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configuration, repoPath).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, string configFilePath)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configFilePath).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, string configFilePath, string repoPath)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configFilePath, repoPath).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        private static void ShowError(ICakeContext context, Exception ex)
        {
            var msg = context.Log.Verbosity == Verbosity.Diagnostic
                ? ex.ToString()
                : ex.Message;
            context.Log.Write(Verbosity.Normal, LogLevel.Fatal, msg);
        }
    }
}