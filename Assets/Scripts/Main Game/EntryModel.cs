using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EntryModel {
    /// <summary>
    /// SCP name
    /// </summary>
    public string name;
    /// <summary>
    /// SCP Number, like 173
    /// </summary>
    public int code;
    /// <summary>
    /// SCP type, like evklid
    /// </summary>
    public string type;
    /// <summary>
    /// Description of SCP
    /// </summary>
    public string description;
    /// <summary>
    /// Conditions of detenion
    /// </summary>
    public string procedures;
    /// <summary>
    /// SCP Category
    /// </summary>
    public int scpcategory;
    /// <summary>
    /// Random text when finded
    /// </summary>
    public int randscpcat;
    /// <summary>
    /// Count of MOG to grab
    /// </summary>
    public int grabcoef;
    /// <summary>
    /// Escape probability
    /// </summary>
    public int probesc;
    /// <summary>
    /// Science addition
    /// </summary>
    public double addsci;
    /// <summary>
    /// Influence addition
    /// </summary>
    public double addinf;
    /// <summary>
    /// Class D to research
    /// </summary>
    public int classD;
    /// <summary>
    /// Money to research
    /// </summary>
    public int money;
    /// <summary>
    /// Initial description
    /// </summary>
    public string descriptionInit;
    /// <summary>
    /// Is on Base
    /// </summary>
    public bool isCatched = false;
    /// <summary>
    /// Is Researched
    /// </summary>
    public bool isResearched = false;

    public static EntryModel LoadFromFile(string filename) {
        var text = File.ReadAllText(EntryManager.AppendedSubPath(filename), System.Text.Encoding.UTF8);
        return JsonUtility.FromJson<EntryModel>(text);
    }
}
