using UnityEditor;
using UnityEngine;
using System.IO;

public class Build 
{
    [MenuItem("Build/Linux Standalone")]
    public static void BuildLinuxStandalone()
    {
        BuildProject(BuildTarget.StandaloneLinuxUniversal);
    }

    [MenuItem("Build/Windows Standalone")]
    public static void BuildWindowsStandalone()
    {
        BuildProject(BuildTarget.StandaloneWindows);
    }

    private static void BuildProject(BuildTarget target)
    {
        string path = "";
        string exeName = "Mez";
        string exeFileExtension = "";
        string[] levels = new string[] { "Assets/Maze.unity" };

        // Set target specific paths.
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
                path = "Build/Mez Windows Standalone/";
                exeFileExtension = ".exe";
                break;
            case BuildTarget.StandaloneLinuxUniversal:
                path = "Build/Mez Linux Standalone/";
                break;
            default:
                Debug.LogError("Invalid build target " + target.ToString());
                return;
        }

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + exeName + exeFileExtension, target, BuildOptions.None);

        // Copy the Themes folder to the data path of the build.
        CopyDirectory("Assets/Themes", path + exeName + "_Data/Themes", new string[] { ".png", ".json" });
    }

    // Copies a directory and all its contents, only accepting files with given filetypes.
    private static void CopyDirectory(string from, string to, string[] validFileTypes)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(from);

        if (!dir.Exists)
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + from);

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(to))
            Directory.CreateDirectory(to);
        
        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            // Check that the file has a valid filetype.
            bool validFile = false;
            foreach (string fileType in validFileTypes)
            {
                if (file.Extension == fileType)
                {
                    validFile = true;
                    break;
                }
            }

            if (validFile)
                file.CopyTo(Path.Combine(to, file.Name), false);
        }

        // Copy subdirectories and their contents to new location.
        foreach (DirectoryInfo subDir in dirs)
            CopyDirectory(subDir.FullName, Path.Combine(to, subDir.Name), validFileTypes);
    }
}