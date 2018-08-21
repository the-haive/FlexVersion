using System;
using System.Collections.Generic;
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
    /// </summary>
    public static class Addin
    {
        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, FlexVersionConfiguration configuration, params string[] arguments)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configuration, null, arguments).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, FlexVersionConfiguration configuration, string repoPath, params string[] arguments)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configuration, repoPath, arguments).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, string configFilePath, params string[] arguments)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configFilePath, null, arguments).Analyze();
            }
            catch (Exception ex)
            {
                ShowError(context, ex);
                return null;
            }
        }

        [CakeMethodAlias]
        public static Results FlexVersion(this ICakeContext context, string configFilePath, string repoPath, params string[] arguments)
        {
            try
            {
                return new IntelliSearch.FlexVersion.FlexVersion(configFilePath, repoPath, arguments).Analyze();
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