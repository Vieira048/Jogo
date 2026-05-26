using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildRunner
{
    private const string PackageName = "com.southbegonia.topdungeon";
    private const string ApkPath = "Builds/Android/TopDungeon.apk";

    public static void BuildAndRunConnectedDevice()
    {
        int exitCode = 0;
        string resultPath = Path.GetFullPath("TestResults/AndroidBuildResults.txt");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ApkPath)));
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes were found in EditorBuildSettings.");

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.AutoRunPlayer
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            File.WriteAllLines(resultPath, new[]
            {
                $"result={summary.result}",
                $"package={PackageName}",
                $"apk={Path.GetFullPath(ApkPath)}",
                $"totalSize={summary.totalSize}",
                $"totalTime={summary.totalTime}",
                $"errors={summary.totalErrors}",
                $"warnings={summary.totalWarnings}"
            });

            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Android build failed with result {summary.result}.");

            Debug.Log($"Android build succeeded and AutoRunPlayer was requested. APK: {Path.GetFullPath(ApkPath)}");
        }
        catch (Exception ex)
        {
            exitCode = 1;
            File.WriteAllText(resultPath, ex.ToString());
            Debug.LogError(ex);
        }
        finally
        {
            EditorApplication.Exit(exitCode);
        }
    }
}
