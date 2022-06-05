using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    EntryManager entryManager;
    public MapManager mapManager;
    public ZonesManager zonesManager;
    public TechManager techManager;
    static GameManager _instance;


    public static int ClassD = 0;
    public static double Science = 0;
    public static double Money = 20000000;
    public static double Secrecy = 100;

    public static GameManager instance {
        get {
            if (_instance == null) {
                _instance = GameObject.Find("GameManager").GetComponent<GameManager>();
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
        techManager = new TechManager();
        StartCoroutine(ShowEntryPopup());
    }

    IEnumerator ShowEntryPopup() {
        yield return new WaitForSeconds(30);
        var popup = EntryPopup.Instance();
        popup.OpenWithEntry(entryManager.GetRandomEntry());
    }
}
