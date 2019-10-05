using System.Collections;
using UnityEngine;

public class LoadScreen : MonoBehaviour
{
    public Texture2D screenTexture;
    public static LoadScreen instance;
    public static AsyncOperation syncLevel;
    public static bool doneLoadingScene;
    public static string newLevel;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        gameObject.AddComponent<GUITexture>().enabled = false;
        GetComponent<GUITexture>().texture = screenTexture;
        transform.position = new Vector3(0.5f, 0.5f, 0.0f);
        DontDestroyOnLoad(this);
    }

    public static void Load(string name)
    {
        if (!instance) return;
        newLevel = name;
    }

    IEnumerator LoadCoroutine()
    {
        if (newLevel == null) yield break;
        syncLevel = Application.LoadLevelAsync(newLevel);
        yield return syncLevel;
    }


    void Update()
    {
        if (newLevel != null)
        {
            instance.GetComponent<GUITexture>().enabled = true;
            StartCoroutine(LoadCoroutine());
            doneLoadingScene = true;
            newLevel = null;
        }
        if (syncLevel != null && doneLoadingScene && syncLevel.isDone)
        {
            doneLoadingScene = false;
            instance.GetComponent<GUITexture>().enabled = false;
        }
    }

    void OnGUI()
    {
        if (doneLoadingScene) GUI.Box(new Rect(0, 0, Screen.width * syncLevel.progress, 100), syncLevel.progress * 100f + " %");
    }
}
