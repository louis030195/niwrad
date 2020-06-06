using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal class ProtobufUnityCompiler : AssetPostprocessor
    {
        /// <summary>
        /// Path to the file of all protobuf files in your Unity folder.
        /// </summary>
        static string[] AllProtoFiles
        {
            get
            {
                string[] protoFiles = Directory.GetFiles(Application.dataPath, "*.proto", SearchOption.AllDirectories);
                return protoFiles;
            }
        }

        /// <summary>
        /// A parent folder of all protobuf files found in your Unity project collected together.
        /// This means all .proto files in Unity could import each other freely even if they are far apart.
        /// </summary>
        static string[] IncludePaths
        {
            get
            {
                string[] protoFiles = AllProtoFiles;

                string[] includePaths = new string[protoFiles.Length];
                for (int i = 0; i < protoFiles.Length; i++)
                {
                    string protoFolder = Path.GetDirectoryName(protoFiles[i]);
                    includePaths[i] = protoFolder;
                }
                return includePaths;
            }
        }

        static bool anyChanges = false;
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            anyChanges = false;
            if (ProtoPrefs.enabled == false)
            {
                return;
            }

            foreach (string str in importedAssets)
            {
                if (CompileProtobufAssetPath(str, IncludePaths) == true)
                {
                    anyChanges = true;
                }
            }

            /*
            for (int i = 0; i < movedAssets.Length; i++)
            {
                CompileProtobufAssetPath(movedAssets[i]);
            }
            */

            if (anyChanges)
            {
                UnityEngine.Debug.Log(nameof(ProtobufUnityCompiler));
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Called from Force Compilation button in the prefs.
        /// </summary>
        internal static void CompileAllInProject()
        {
            if (ProtoPrefs.logStandard)
            {
                UnityEngine.Debug.Log("Protobuf Unity : Compiling all .proto files in the project...");
            }

            foreach (string s in AllProtoFiles)
            {
                if (ProtoPrefs.logStandard)
                {
                    UnityEngine.Debug.Log("Protobuf Unity : Compiling " + s);
                }
                CompileProtobufSystemPath(s, IncludePaths);
            }
            UnityEngine.Debug.Log(nameof(ProtobufUnityCompiler));
            AssetDatabase.Refresh();
        }

        private static bool CompileProtobufAssetPath(string assetPath, string[] includePaths)
        {
            string protoFileSystemPath = Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + assetPath;
            return CompileProtobufSystemPath(protoFileSystemPath, includePaths);
        }

        private static bool CompileProtobufSystemPath(string protoFileSystemPath, string[] includePaths)
        {
            //Do not compile changes coming from UPM package.
            if (protoFileSystemPath.Contains("Packages/com.protobuf-unity")) return false;

            string options = $" -I {ProtoPrefs.protoPath} --csharp_out={ProtoPrefs.cSharpOut} ";
            options += $"{ProtoPrefs.protoFile} --grpc_out={ProtoPrefs.cSharpOut} --plugin=protoc-gen-grpc={ProtoPrefs.grpcPath}";

            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = ProtoPrefs.protocPath, Arguments = options };

            Process proc = new Process() { StartInfo = startInfo };
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (ProtoPrefs.logStandard)
            {
                if (output != "")
                {
                    UnityEngine.Debug.Log("Protobuf Unity : " + output);
                }
                UnityEngine.Debug.Log("Protobuf Unity : Compiled " + Path.GetFileName(protoFileSystemPath));
            }

            if (ProtoPrefs.logError && error != "")
            {
                UnityEngine.Debug.LogError("Protobuf Unity : " + error);
            }
            return true;
        }
    }
}
