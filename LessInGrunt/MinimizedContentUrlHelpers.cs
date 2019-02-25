using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace LessInGrunt
{
    public static class MinimizedContentUrlHelpers
    {
        /*
         * Usage:
         * Reference scripts, both application own and external, with Url.Script
         * In Application_Start
         * (1) Specify the current application version
         *   Application[MinimizedContentUrlHelpers.appVersionKey] = Assembly.Load("YourAppAssemblyName").GetName().Version.ToString();
         *   You may want to set version in AssemblyInfo to major.minor.* to increment version on each build.
         * (2) Optionally switch to debug scripts globally
         *   MinimizedContentUrlHelpers.SetGlobalScriptDebug();
         *   This can be done conditionally, e.g. based on Properties.Settings.useDebugScripts
         * 
         * To load non-minimized scripts locally, add ?_debugScripts=1 to the request url - for this single page only.
         */

        private const string minScriptPattern = @".min.js$";
        private const string minStylesheetPattern = @".min.css$";
        public const string debugScriptKey = "_debugScripts";
        public const string appVersionKey = "appVersion";

        /// <summary>
        /// Generates an application-absolute path to the specified script, appending the version and optionally switching to the non-minified version.
        /// </summary>
        /// <param name="url">UrlHelper taken from method invocation.</param>
        /// <param name="contentPath">The application relative path to the minified script, like "~/scripts/scriptName.min.js"; it is safe to pass non-minimized script here without .min</param>
        /// <param name="version">The actual script version, used for a package scripts, like jQuery, "1.9.1"</param>
        /// <param name="noDebugVersion">Specify <c>true</c> when this minified script has no full version deployed.</param>
        /// <returns>Complete url to script, with version appended as a query string and optionally switched to non-minified</returns>
        /// <remarks>
        /// This helper has two goals:
        /// (1) Prevents script caching upon application upgrade by appending application version to the script name.
        /// (2) Switches scripts to non-minified ones upon local or global setting.
        /// When used with a package scripts (3-rd party scripts) you can specify actual script version explicitly.
        /// If you don't, the current application version will be used, which still gives correct results after upgrade.
        /// </remarks>
        public static string MkoScript(this UrlHelper url, string contentPath, string version = null, bool noDebugVersion = false)
        {
            contentPath = url.Content(contentPath).Trim();

            if (!noDebugVersion && IsDebug())
            {
                contentPath = Regex.Replace(contentPath, minScriptPattern, ".js", RegexOptions.IgnoreCase);
            }

            if (version == null)
            {
                version = (string)HttpContext.Current.Application[appVersionKey];
            }

            if (version != null)
            {
                contentPath += "?v=" + version;
            }

            return contentPath;
        }

        /// <summary>
        /// The same as MkoScript, but for stylesheets
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentPath"></param>
        /// <param name="version"></param>
        /// <param name="noDebugVersion"></param>
        /// <returns></returns>
        public static string MkoStylesheet(this UrlHelper url, string contentPath, string version = null, bool noDebugVersion = false)
        {
            contentPath = url.Content(contentPath).Trim();

            if (!noDebugVersion && IsDebug())
            {
                contentPath = Regex.Replace(contentPath, minStylesheetPattern, ".css", RegexOptions.IgnoreCase);
            }

            if (version == null)
            {
                version = (string)HttpContext.Current.Application[appVersionKey];
            }

            if (version != null)
            {
                contentPath += "?v=" + version;
            }

            return contentPath;
        }

        /// <summary>
        /// Switches to non-minimized scripts for the whole application.
        /// </summary>
        /// <param name="disableDebug"></param>
        public static void SetGlobalScriptDebug(bool disableDebug = false)
        {
            HttpContext.Current.Application[MinimizedContentUrlHelpers.debugScriptKey] = disableDebug ? "0" : "1";
        }


        private static bool IsDebug()
        {
            bool debug = ((string)HttpContext.Current.Application[debugScriptKey]) == "1";
            debug |= HttpContext.Current.Request.QueryString[debugScriptKey] == "1";
            return debug;
        }
    }

}