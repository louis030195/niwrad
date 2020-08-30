using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Builds
    {
        private const string BasePath = "Builds/";
        private const string ArtifactName = "niwrad";
        private static readonly string[] GameLevels = {
	        "Assets/Scenes/LoginMenu.unity",
	        "Assets/Scenes/SecondMenu.unity",
            "Assets/Scenes/Online.unity"
        };

        [MenuItem("Builds/Windows %#W")]
        public static void BuildWindows()
        {
            PlayerSettings.runInBackground = true;
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Windows/{ArtifactName}.exe",
                BuildTarget.StandaloneWindows64,
                BuildOptions.None);

            if (message)
                Debug.Log($"Windows build complete");
            else
                Debug.LogError($"Error building Windows { message }");
        }

        [MenuItem("Builds/Linux/Client %#L")]
         public static void BuildLinuxClient()
        {
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Linux/Client/{ArtifactName}.x86_64",
                BuildTarget.StandaloneLinux64,
                BuildOptions.None);

            if (message) Debug.Log($"Linux client build complete");
            else Debug.LogError($"Error building Linux client { message }");
        }

        [MenuItem("Builds/Linux/Server %#H")]
        public static void BuildLinuxServer()
        {
            PlayerSettings.runInBackground = true;
            var message = BuildPipeline.BuildPlayer(
	            new[] {GameLevels[2]},
	            $"{BasePath}Linux/Server/{ArtifactName}.x86_64",
	            BuildTarget.StandaloneLinux64,
	            BuildOptions.EnableHeadlessMode);

            if (message) Debug.Log($"Linux server build complete");
            else Debug.LogError($"Error building Linux server { message }");
        }

        [MenuItem("Builds/Web/Online")]
        public static void BuildWebOnline()
        {
            PlayerSettings.runInBackground = true;
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Web/",
                BuildTarget.WebGL,
                BuildOptions.None);

            if (message) Debug.Log($"WebGL build complete");
            else Debug.LogError($"Error building WebGL { message }");
        }
        
        [MenuItem("Builds/Web/Offline")]
        public static void BuildWebOffline()
        {
            PlayerSettings.runInBackground = true;
            var message = BuildPipeline.BuildPlayer(
                new[] {"Assets/Scenes/Offline.unity"},
                $"{BasePath}Web/",
                BuildTarget.WebGL,
                BuildOptions.None);

            if (message) Debug.Log($"WebGL build complete");
            else Debug.LogError($"Error building WebGL { message }");
        }

        [MenuItem("Builds/Android %#A")]
        public static void BuildAndroid()
        {
            PlayerSettings.runInBackground = true;
            EditorPrefs.SetString("AndroidSdkRoot", System.Environment.GetEnvironmentVariable("ANDROID_HOME"));
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Android/{ArtifactName}.apk",
                BuildTarget.Android,
                BuildOptions.None);

            if (message)
                Debug.Log($"Android build complete");
            else
                Debug.LogError($"Error building Android { message }");
        }

        [MenuItem("Builds/Clients")]
        public static void BuildAllPc() {
            BuildWindows();
            BuildLinuxClient();
            BuildAndroid();
            BuildWebOnline();
        }
    }
}
