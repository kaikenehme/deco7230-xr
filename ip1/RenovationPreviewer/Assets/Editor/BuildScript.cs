using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        var report = BuildPipeline.BuildPlayer(
            new[] { "Assets/Scenes/Room.unity" },
            "Builds/ip1.apk",
            BuildTarget.Android,
            BuildOptions.None);

        Debug.Log($"Build result: {report.summary.result}, size {report.summary.totalSize}, " +
                  $"errors {report.summary.totalErrors}, time {report.summary.totalTime}");

        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
