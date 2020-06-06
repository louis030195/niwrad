using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Builds
    {
        private const string BasePath = "Builds/";
        private const string ArtifactName = "Niwrad";
        private static readonly string[] GameLevels = {
            "Assets/Scenes/SampleScene.unity"
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

        [MenuItem("Builds/Linux %#L")]
         public static void BuildLinux()
        {
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Linux/{ArtifactName}.x86_64",
                BuildTarget.StandaloneLinux64,
                BuildOptions.None);

            if (message)
                Debug.Log($"Linux build complete");
            else
                Debug.LogError($"Error building Linux { message }");
        }

        [MenuItem("Builds/LinuxHeadless %#H")]
        public static void BuildLinuxHeadless()
        {
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Linux/{ArtifactName}.x86_64",
                BuildTarget.StandaloneLinux64,
                BuildOptions.EnableHeadlessMode);

            if (message)
                Debug.Log($"Linux build complete");
            else
                Debug.LogError($"Error building Linux { message }");
        }

        [MenuItem("Builds/Web")]
        public static void BuildWeb()
        {
            PlayerSettings.runInBackground = true;
            var message = BuildPipeline.BuildPlayer(
                GameLevels,
                $"{BasePath}Web/",
                BuildTarget.WebGL,
                BuildOptions.None);

            if (message)
                Debug.Log($"WebGL build complete");
            else
                Debug.LogError($"Error building WebGL { message }");
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

        // Seems to be runnable from bash
        [MenuItem("Builds/PC All Platforms")]
        public static void BuildAllPc() {
            BuildWindows();
            BuildLinux();
        }
    }
}
