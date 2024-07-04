using UnityEngine;
using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public class AutoDefineSymbols
{
    static AutoDefineSymbols()
    {
        SetScriptingDefineSymbols();
    }

    private static void SetScriptingDefineSymbols()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';').ToList();

        bool isUsingXRToolkit = IsPackageInstalled("com.unity.xr.interaction.toolkit");
        bool isUsingOVR = IsPackageInstalled("com.meta.xr.sdk.all");

        if (isUsingXRToolkit && !defines.Contains("USE_XR_TOOLKIT"))
        {
            defines.Add("USE_XR_TOOLKIT");
        }

        if (isUsingOVR && !defines.Contains("USE_OVR"))
        {
            defines.Add("USE_OVR");
        }

        if (!isUsingXRToolkit && defines.Contains("USE_XR_TOOLKIT"))
        {
            defines.Remove("USE_XR_TOOLKIT");
        }

        if (!isUsingOVR && defines.Contains("USE_OVR"))
        {
            defines.Remove("USE_OVR");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defines));
    }

    private static bool IsPackageInstalled(string packageName)
    {
        var packageListRequest = UnityEditor.PackageManager.Client.List(true);
        while (packageListRequest.Status == UnityEditor.PackageManager.StatusCode.InProgress) { }
        if (packageListRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
        {
            Debug.LogError("Failed to list packages: " + packageListRequest.Error.message);
            return false;
        }
        return packageListRequest.Result.Any(package => package.name == packageName);
    }
}
