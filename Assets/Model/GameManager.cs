using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    EntryManager entryManager;
    MapManager mapManager;
    // Start is called before the first frame update
    void Start() {
        entryManager = new EntryManager();
        mapManager = new MapManager(StartGameSettings.mapSave);
        StartCoroutine(ShowEntryPopup());
    }

    IEnumerator ShowEntryPopup() {
        yield return new WaitForSeconds(15);
        var popup = EntryPopup.Instance();
        popup.OpenWithEntry(entryManager.GetRandomEntry());
    }
}
