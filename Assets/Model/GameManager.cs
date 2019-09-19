using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    EntryManager entryManager;
    // Start is called before the first frame update
    void Start() {
        entryManager = new EntryManager();
        StartCoroutine(ShowEntryPopup());
    }

    IEnumerator ShowEntryPopup() {
        yield return new WaitForSeconds(15);
        var popup = EntryPopup.Instance();
        popup.OpenWithEntry(entryManager.GetRandomEntry());
    }
}
