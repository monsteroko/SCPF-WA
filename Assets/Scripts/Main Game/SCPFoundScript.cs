using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPFoundScript : MonoBehaviour
{
    private EntryModel model;

    void OnMouseDown()
    {
        model = EntryPopup.Instance().GetEntryModel();
        MogSCPFoundPanel.Instance().OpenWithEntry(model);
    }
}
