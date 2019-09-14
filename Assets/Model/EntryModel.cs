using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EntryModel {
    public string name;
    public string code;
    public string type;
    public string description;
    public string procedures;

    public static EntryModel LoadFromFile(string filename) {
        var text = File.ReadAllText(EntryManager.AppendedSubPath(filename), System.Text.Encoding.UTF8);
        return JsonUtility.FromJson<EntryModel>(text);
    }
}
