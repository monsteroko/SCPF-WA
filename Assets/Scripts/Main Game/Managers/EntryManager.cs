﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntryManager {

    private List<EntryModel> generalPool = new List<EntryModel>();

    public EntryManager() {
        LoadGeneralPool();
    }

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

    public EntryModel GetRandomEntry() {
        return generalPool[Random.Range(0, generalPool.Count)];
    }
}
