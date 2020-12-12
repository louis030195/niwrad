using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Builds
    {
        private static readonly string[] Scenes = {
            "Assets/Scenes/SingleClient.unity",
        };
        private static readonly List<string> Secrets = new List<string>
            {"androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass"};

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
                var flagHasValue = next < args.Length && !args[next].StartsWith("-");
                var value = flagHasValue ? args[next].TrimStart('-') : "";
                var secret = Secrets.Contains(flag);
                // Useless anyway, Github hides it somehow, but a good safety
                var displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

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

            return validatedOptions;
        }

        // Public main entry point of all builds, called directly
        [UsedImplicitly]
        public static void BuildOptions()
        {
            // Gather values from args
            var options = GetValidatedOptions();

            // Set version for this build
            options.TryGetValue("buildVersion", out var buildVersion);
            PlayerSettings.bundleVersion = buildVersion;
            PlayerSettings.macOS.buildNumber = buildVersion;

            options.TryGetValue("androidVersionCode", out var androidVersionCode);
            PlayerSettings.Android.bundleVersionCode = int.Parse(androidVersionCode ?? "1");

            options.TryGetValue("customBuildPath", out var customBuildPath);

            // Apply build target
            var buildTarget = (BuildTarget) Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    if (customBuildPath != null)
                        EditorUserBuildSettings.buildAppBundle = customBuildPath.EndsWith(".aab");
                    if (options.TryGetValue("androidKeystoreName", out var keystoreName) &&
                        !string.IsNullOrEmpty(keystoreName))
                        PlayerSettings.Android.keystoreName = keystoreName;
                    if (options.TryGetValue("androidKeystorePass", out var keystorePass) &&
                        !string.IsNullOrEmpty(keystorePass))
                        PlayerSettings.Android.keystorePass = keystorePass;
                    if (options.TryGetValue("androidKeyaliasName", out var keyaliasName) &&
                        !string.IsNullOrEmpty(keyaliasName))
                        PlayerSettings.Android.keyaliasName = keyaliasName;
                    if (options.TryGetValue("androidKeyaliasPass", out var keyaliasPass) &&
                        !string.IsNullOrEmpty(keyaliasPass))
                        PlayerSettings.Android.keyaliasPass = keyaliasPass;
                    // IL2CPP 
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    // Google Play want all CPU especially arm64
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                    break;
                case BuildTarget.StandaloneOSX:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    break;
            }

            options.TryGetValue("niwradMode", out var mode);
            Build(buildTarget, customBuildPath != string.Empty ? customBuildPath : "build" , mode);
        }

        // Internal main entry point of all builds
        private static void Build(BuildTarget buildTarget, string filePath, string niwradMode) 
        {
            BuildOptions options = 0;
            if (niwradMode == "executor") {
                options = UnityEditor.BuildOptions.EnableHeadlessMode;
            }
            
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = filePath,
                target = buildTarget,
                options = options
            };
            var buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
            Console.WriteLine($"Build summary: {buildSummary}");
        }
    }
}
