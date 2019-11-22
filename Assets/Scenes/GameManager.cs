using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    EntryManager entryManager;
    public MapManager mapManager;
    public ZonesManager zonesManager;
    // Start is called before the first frame update

    static GameManager _instance;
    public static GameManager instance {
        get {
            if (_instance == null) {
                _instance = GameObject.Find("GameManagerObject").GetComponent<GameManager>();
            }
            return _instance;
        }
    }

    void Start() {
        entryManager = new EntryManager();
#if UNITY_EDITOR
        if (StartGameSettings.mapSave == null) {
            mapManager.InitWithSave(MapSaveModel.NewGameSave("eeu"));
        } else
#endif
        mapManager.InitWithSave(StartGameSettings.mapSave);
        StartCoroutine(ShowEntryPopup());
    }

    IEnumerator ShowEntryPopup() {
        yield return new WaitForSeconds(15);
        var popup = EntryPopup.Instance();
        popup.OpenWithEntry(entryManager.GetRandomEntry());
    }
}
