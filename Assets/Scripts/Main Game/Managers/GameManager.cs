using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public EntryManager entryManager;
    public MapManager mapManager;
    public ZonesManager zonesManager;
    public TechManager techManager;
    public TimeManager timeManager;
    public EventsManager eventsManager;
    public ResourcesManager resourcesManager;
    public ZonesResourcesManager zonesResourcesManager;
    static GameManager _instance;

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
    }

}
