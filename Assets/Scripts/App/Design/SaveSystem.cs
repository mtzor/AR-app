using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string AllFilesListPath = Application.persistentDataPath + "/saved_design_files.txt"; // Path for the file storing all saved design file paths

    private static string currentFile;

    // Save a single module and append to the existing list of saved modules
    public static List<ModuleData> SaveModule(ModuleData module)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string fileName = LobbyManager.Instance.GetLobbbyName();
        string path = Application.persistentDataPath + "/" + fileName + "/Design.data";

        currentFile = path;

        // Create the directory if it doesn't exist
        Directory.CreateDirectory(Application.persistentDataPath + "/" + fileName);

        // Load existing modules if any
        List<ModuleData> modules = new List<ModuleData>();

        if (File.Exists(path))
        {
            Debug.Log("Design File "+path +"already exists! Loading saved modules to add at the end of the list.");
            modules = LoadAllModules(); // Load existing data
            if (modules == null) // Safety check
            {
                Debug.Log("Load all modules empty");
                modules = new List<ModuleData>();
            }
        }

        // Add the new module to the list and save it
        modules.Add(module);

        // Now serialize the updated list back to the file
        FileStream stream = new FileStream(path, FileMode.Create); // Overwrite with new data
        formatter.Serialize(stream, modules); // Save the updated list
        stream.Close();

        return modules;
    }

    // Load all modules
    public static List<ModuleData> LoadAllModules()
    {
        string path = Application.persistentDataPath + "/" + LobbyManager.Instance.GetLobbbyName() + "/Design.data";
        Debug.Log("Loading Module list from :"+path);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            List<ModuleData> modules = formatter.Deserialize(stream) as List<ModuleData>;
            stream.Close();

            return modules;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    // Delete a saved file
    public static void DeleteSaveFile()
    {
        string path = Application.persistentDataPath + "/" + LobbyManager.Instance.GetLobbbyName() + "/Design.data";

        // Check if the file exists
        if (File.Exists(path))
        {
            // Delete the file
            File.Delete(path);
            Debug.Log("Save file deleted successfully.");
        }
        else
        {
            Debug.LogError("Save file not found at: " + path);
        }
    }
    public static void SaveDesignFile()
    {
        // Update the global file that tracks all saved files
        AddFileNameToAllFilesList(currentFile);
    }
    // Add file name to the global list of all saved files
    private static void AddFileNameToAllFilesList(string fileName)
    {
        if (!File.Exists(AllFilesListPath))
        {
            // Create the file if it doesn't exist
            File.Create(AllFilesListPath).Close();
            Debug.Log("Creating the file containing all the design files!");
        }

        // Check if the file name is already in the list
        List<string> allFiles = LoadAllSavedFileNames();
        if (!allFiles.Contains(fileName))
        {
            // Add the new file name to the list
            File.AppendAllText(AllFilesListPath, fileName + "\n");
            Debug.Log("Appending file: "+fileName+" at the end of "+AllFilesListPath);
        }
    }

    // Load the list of all saved file names (or paths)
    public static List<string> LoadAllSavedFileNames()
    {
        List<string> allFiles = new List<string>();

        if (File.Exists(AllFilesListPath))
        {
            string[] lines = File.ReadAllLines(AllFilesListPath);
            allFiles.AddRange(lines);
        }
        else
        {
            Debug.Log("No saved files found.");
        }

        return allFiles;
    }
}
