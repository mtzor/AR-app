using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    // Save a single module and append to the existing list of saved modules
    public static List<ModuleData> SaveModule(ModuleData module)
    {
        Debug.Log("CUSTOM NAME " + Application.persistentDataPath + "/"+LobbyManager.Instance.GetLobbbyName()+ "/Design.data");
        
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + LobbyManager.Instance.GetLobbbyName() + "/Design.data";

        // Create a file stream in open mode to either create a new file or open an existing one
        List<ModuleData> modules = new List<ModuleData>();

        if (File.Exists(path))
        {
            // Load existing modules if any
            modules = LoadAllModules(); // Load existing data
                                              
            if (modules == null) // Safety check
            {
                Debug.Log("Load all modules empty");
                modules = new List<ModuleData>();
            }
        }

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
}
