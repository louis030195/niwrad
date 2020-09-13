using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Builds
    {
        private static readonly string[] OnlineScenes = {
	        "Assets/Scenes/LoginMenu.unity",
	        "Assets/Scenes/SecondMenu.unity",
            "Assets/Scenes/Online.unity",
        };

        private static readonly string[] ExecutorScenes = {
            "Assets/Scenes/Online.unity",
        };

        private static readonly string[] OfflineScenes = {
            "Assets/Scenes/Offline.unity",
        };

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            var args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                "\n" +
                "###########################\n" +
                "#    Parsing settings     #\n" +
                "###########################\n" +
                "\n"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                var isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                var flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
                // bool secret = Secrets.Contains(flag);
                bool secret = false;
                string displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

                // Assign
                Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
                providedArguments.Add(flag, value);
            }
        }
        
        private static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out var validatedOptions);

            if (!validatedOptions.ContainsKey("projectPath"))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("buildTarget", out var buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                EditorApplication.Exit(121);
            }

            // if (!validatedOptions.ContainsKey("customBuildPath"))
            // {
            //     Console.WriteLine("Missing argument -customBuildPath");
            //     EditorApplication.Exit(130);
            // }

            if (!validatedOptions.ContainsKey("niwradMode"))
            {
                Console.WriteLine("Missing argument -niwradMode");
                EditorApplication.Exit(130);
            }

            return validatedOptions;
        }

        // Public main entry point of all builds, called directly
        [UsedImplicitly]
        public static void BuildOptions()
        {
            // Gather values from args
            var options = GetValidatedOptions();

            // Set version for this build
            PlayerSettings.bundleVersion = options["buildVersion"];
            // PlayerSettings.macOS.buildNumber = options["buildVersion"];
            // PlayerSettings.Android.bundleVersionCode = int.Parse(options["androidVersionCode"]);

            // Apply build target
            var buildTarget = (BuildTarget) Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
            if (buildTarget == BuildTarget.Android)
            {
                // EditorUserBuildSettings.buildAppBundle = options["customBuildPath"].EndsWith(".aab");
                // if (options.TryGetValue("androidKeystoreName", out string keystoreName) &&
                //     !string.IsNullOrEmpty(keystoreName))
                //     PlayerSettings.Android.keystoreName = keystoreName;
                // if (options.TryGetValue("androidKeystorePass", out string keystorePass) &&
                //     !string.IsNullOrEmpty(keystorePass))
                //     PlayerSettings.Android.keystorePass = keystorePass;
                // if (options.TryGetValue("androidKeyaliasName", out string keyaliasName) &&
                //     !string.IsNullOrEmpty(keyaliasName))
                //     PlayerSettings.Android.keyaliasName = keyaliasName;
                // if (options.TryGetValue("androidKeyaliasPass", out string keyaliasPass) &&
                //     !string.IsNullOrEmpty(keyaliasPass))
                //     PlayerSettings.Android.keyaliasPass = keyaliasPass;
            }
            else if (buildTarget != BuildTarget.iOS)
            {
                // PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            }

            // Custom build
            Build(buildTarget, options["customBuildPath"], options["niwradMode"]);
        }

        // Internal main entry point of all builds
        private static void Build(BuildTarget buildTarget, string filePath, string niwradMode) 
        {
            string[] scenes = {""};
            BuildOptions options = 0;
            if (niwradMode == "online") {
                scenes = OnlineScenes;
            } else if (niwradMode == "offline") {
                scenes = OfflineScenes;
            } else if (niwradMode == "executor") {
                scenes = ExecutorScenes;
                options = UnityEditor.BuildOptions.EnableHeadlessMode;
            } else {
                Console.WriteLine("Invalid argument -niwradMode");
                EditorApplication.Exit(130);
            }

            // if (buildTarget == BuildTarget.WebGL)
            // {
            //     options = options & UnityEditor.BuildOptions.Development; // Tentative to disable code stripping which breaks everything
            // }
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = filePath,
                target = buildTarget,
                options = options
            };
            var buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
            Console.WriteLine($"Build summary: {buildSummary}");
        }
    }
}
