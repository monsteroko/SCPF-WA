using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntryModel {
    string name;
    string code;
    int type;
    string description;
    string procedures;
    bool isStatic;

    public static EntryModel LoadFromFile(string filename) {
        var text = File.ReadAllText(EntryManager.AppendedSubPath(filename));
        return JsonConvert.DeserializeObject<EntryModel>(text);
    }
}
