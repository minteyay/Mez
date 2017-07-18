using UnityEditor;
using System.IO;

public class Build 
{
    [MenuItem("Build/Linux Standalone")]
    public static void BuildLinuxStandalone()
    {
        string path = "Build/Mez Linux Standalone/";
        string exeName = "Mez";
        string[] levels = new string[] { "Assets/Maze.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + exeName, BuildTarget.StandaloneLinuxUniversal, BuildOptions.None);

        // Copy the Themes folder to the data path of the build.
        CopyDirectory("Assets/Themes", path + exeName + "_Data/Themes", new string[] { ".png", ".mez" });
    }

    [MenuItem("Build/Windows Standalone")]
    public static void BuildWindowsStandalone()
    {
        string path = "Build/Mez Windows Standalone/";
        string exeName = "Mez";
        string[] levels = new string[] { "Assets/Maze.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + exeName + ".exe", BuildTarget.StandaloneWindows, BuildOptions.None);

        // Copy the Themes folder to the data path of the build.
        CopyDirectory("Assets/Themes", path + exeName + "_Data/Themes", new string[] { ".png", ".mez" });
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