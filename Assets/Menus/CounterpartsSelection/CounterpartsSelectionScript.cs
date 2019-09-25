using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CounterpartsSelectionScript : MonoBehaviour {

    public Button naButton;
    public Button saButton;
    public Button eeuButton;
    public Image naImage;
    public Image saImage;
    public Image eeuImage;
    public Button proceedButton;

    private Dictionary<string, Image> counterpartImages = new Dictionary<string, Image>();

    void Start() {
        counterpartImages["na"] = naImage;
        counterpartImages["sa"] = saImage;
        counterpartImages["eeu"] = eeuImage;
        proceedButton.interactable = false;
        proceedButton.SetNormalBackgroundColor(Color.black);
        int i = 0;
        foreach (var button in new Button[] { naButton, saButton, eeuButton }) {
            var key = counterpartImages.Keys.ToArray()[i];
            counterpartImages[key].SetOpacity(0);
            button.SetHighlightedBackgroundColor(Color.gray);
            button.onClick.AddListener(() => {
                button.Select();
                SetCounterpartHighlighted(key);
                proceedButton.SetNormalBackgroundColor(Color.white);
                proceedButton.interactable = true;
            });
            i++;
        }
        proceedButton.onClick.AddListener(Proceed);
    }

    private string selectedCounterpart;
    private void SetCounterpartHighlighted(string newCounterpart) {
        if (selectedCounterpart != null) {
            counterpartImages[selectedCounterpart].SetOpacity(0);
        }
        counterpartImages[newCounterpart].SetOpacity(1);
        selectedCounterpart = newCounterpart;
        Debug.Log(newCounterpart);
    }

    private void Proceed() {
        StartGameSettings.mapSave = MapSaveModel.NewGameSave(selectedCounterpart);
        SceneManager.LoadScene("Game");
    }

}

public static class ImageExtensions {
    public static void SetOpacity(this Image image, float opacity) {
        var tempColor = image.color;
        tempColor.a = opacity;
        image.color = tempColor;
    }
}

public static class ButtonExtenstions {
    public static void SetNormalBackgroundColor(this Button button, Color color) {
        var colors = button.colors;
        colors.normalColor = color;
        button.colors = colors;
    }

    public static void SetHighlightedBackgroundColor(this Button button, Color color) {
        var colors = button.colors;
        colors.highlightedColor = color;
        button.colors = colors;
    }
}
