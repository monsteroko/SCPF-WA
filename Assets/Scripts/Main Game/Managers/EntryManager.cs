using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manager of SCPs
/// </summary>
public class EntryManager {
    /// <summary>
    /// List of all SCPs
    /// </summary>
    private List<EntryModel> generalPool = new List<EntryModel>();

    public EntryManager() {
        LoadGeneralPool();
    }
    /// <summary>
    /// Load SCP data from file
    /// </summary>
    public void LoadGeneralPool() {
        generalPool.Clear();
        var info = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), AppendedSubPath("")));
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo) {
            if (file.Name.EndsWith(".meta")) {
                continue;
            } 
            var entry = EntryModel.LoadFromFile(file.Name);
            if (entry != null) {
                generalPool.Add(entry);
            }
        }
    }

    private static string SubPath = Path.Combine("Assets","Data","Entries");
    public static string AppendedSubPath(string filname) {
        return Path.Combine(SubPath, filname);
    }
    /// <summary>
    /// Get random SCP data
    /// </summary>
    /// <returns></returns>
    public EntryModel GetRandomEntry() {
        return generalPool[Random.Range(0, generalPool.Count)];
    }

    public EntryModel GetEntryByName(string name)
    {
        return generalPool.Find(x => x.name == name);
    }

    public void UpdateEntry(EntryModel model)
    {
        if (generalPool.Find(x => x.name == model.name) != null)
        {
            int index = generalPool.IndexOf(generalPool.Find(x => x.name == model.name));
            generalPool[index] = model;
        }
    }
}
