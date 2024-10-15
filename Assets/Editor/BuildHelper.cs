using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildHelper : MonoBehaviour
{
    // [MenuItem("VR Build/Open XR Device Simulator Settings...", priority=0)]
    // static void OpenSimSettings()
    // {
    //     EditorWindow window = SettingsService.OpenProjectSettings("Project/XR Plug-in Management/XR Interaction Toolkit");
    // }

    [MenuItem("VR Build/Open Project Validation...", priority=0)]
    static void OpenProjectValidation()
    {
        EditorWindow window = SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Project Validation");
    }

    [MenuItem("VR Build/Open Build Settings...", priority=0)]
    static void OpenBuildSettings()
    {
        EditorApplication.ExecuteMenuItem("File/Build Settings...");
    }

    [MenuItem("VR Build/Build and Run...", priority=12)]
    static void BuildAndRun()
    {
        BuildInternal(true);
    }

    [MenuItem("VR Build/Build APK Only...", priority=12)]
    static void BuildApk()
    {
        BuildInternal(false);
    }

    [MenuItem("VR Build/Deploy APK to Headset...", priority=13)]
    static void RunOnHeadset()
    {
        if (!VerifySdk()) return;

        string apkPath = EditorUtility.OpenFilePanel("Select .apk file to deploy", "", "apk");
        if (apkPath.Length > 0)
        {
            var output = RunAdbCommand(AndroidExternalToolsSettings.sdkRootPath, $"install \"{apkPath}\"");
            if (output.Item2.Length > 0)
            {
                EditorUtility.DisplayDialog("Error!", output.Item2, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Apk Installed!", output.Item1, "OK");
            }
        }
    }
    
    [MenuItem("VR Build/View Connected Devices", priority=24)]
    static void ViewDevices()
    {
        if (!VerifySdk()) return;

        var output = RunAdbCommand(AndroidExternalToolsSettings.sdkRootPath, $"devices");
        if (output.Item2.Length > 0)
        {
            EditorUtility.DisplayDialog("Error!", output.Item2, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Connected Devices", output.Item1, "OK");
        }
    }

    [MenuItem("VR Build/Open ADB Location", priority=25)]
    static void OpenAdbLocation()
    {
        if (!VerifySdk()) return;
        EditorUtility.RevealInFinder(AndroidExternalToolsSettings.sdkRootPath + "/platform-tools/adb");
    }

    private static void BuildInternal(bool runAfterBuild = false)
    {
        BuildPlayerOptions defaultOptions = new BuildPlayerOptions();
        try {
        defaultOptions = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(defaultOptions);
        } catch (BuildPlayerWindow.BuildMethodException) {
            return;
        }
        
        if (defaultOptions.target != BuildTarget.Android)
        {
            UnityEngine.Debug.LogError("Error! Build target must be set to Android.");
            // Open the Build Settings editor window
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
            return;
        }

        if (defaultOptions.locationPathName.Length > 0)
        {
            if (runAfterBuild)
            {
                defaultOptions.options |= BuildOptions.AutoRunPlayer;
            }
            else
            {
                defaultOptions.options &= ~BuildOptions.AutoRunPlayer;
            }

            EditorUtility.DisplayProgressBar("Build APK", "Building apk...", 0.5f);
            var report = BuildPipeline.BuildPlayer(defaultOptions);
            EditorUtility.DisplayProgressBar("Build APK", "Finishing up...", 1);
            EditorUtility.ClearProgressBar();
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("Build APK", $"Build succeeded: {report.summary.totalSize} bytes", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Build APK", $"Build failed: {report.summary.totalSize} bytes", "OK");
            }
        }
    }

    private static bool VerifySdk()
    {
        if (AndroidExternalToolsSettings.sdkRootPath.Length == 0)
        {
            UnityEngine.Debug.LogError("Error! Android SDK path not set. Make sure you have installed the Android SDK with Unity.");
            return false;
        }
        return true;
    }

    private static Tuple<string, string> RunAdbCommand(string sdkPath, string args)
    {
        try
        {
            string title = $"Running adb command: {args}";

            EditorUtility.DisplayProgressBar(title, "Running command...", 1);

            string adbFullPath = sdkPath + "/platform-tools/" + "adb";

            ProcessStartInfo procStartInfo = new ProcessStartInfo(adbFullPath, args)
            {
                WorkingDirectory = sdkPath + "/platform-tools/",
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            Process p = Process.Start(procStartInfo);

            while (!p.WaitForExit(100))
            {
                if (EditorUtility.DisplayCancelableProgressBar(title, "Running command...", 1))
                {
                    p.Kill();
                    break;
                }
            }
            EditorUtility.DisplayProgressBar(title, "Finishing up...", 1);

            string output = p.StandardOutput.ReadToEnd();
            if (output.Length > 0)
            {
                UnityEngine.Debug.Log($"adb output:\n{output}");
            }
            
            string error = p.StandardError.ReadToEnd();
            if (error.Length > 0)
            {
                UnityEngine.Debug.LogError($"adb error:\n{error}");
            }

            return new Tuple<string, string>(output, error);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(ex);
            return new Tuple<string, string>("", ex.ToString());
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
