using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel
{
    private List<EntryModel> _listofSCPS = new List<EntryModel>();
    public string name { get; set; }

    public int amountofMog = 10;

    public List<EntryModel> listofSCPs { get => _listofSCPS; }

    public bool isUpdated = false;
}
