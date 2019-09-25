using UnityEngine;
using System.Collections;

namespace WPMF {
	public class SurfaceBlinker : MonoBehaviour {

		public float duration;
		public Color color1, color2;
		public float speed;
		public Material blinkMaterial;
		public bool disableAtEnd;
		public Region customizableSurface;
		Material oldMaterial;
		float startTime, lapTime;
		bool whichColor, oldActiveState;

		void Start () {
			oldMaterial = GetComponent<Renderer> ().sharedMaterial;
			GenerateMaterial ();
			startTime = Time.time;
			lapTime = startTime - speed;
		}
	
		// Update is called once per frame
		void Update () {
			float elapsed = Time.time - startTime;
			if (elapsed > duration) {
				// Restores material
				Material goodMat;
				if (customizableSurface.customMaterial!=null) {
					goodMat = customizableSurface.customMaterial;
				} else {
					goodMat = oldMaterial;
				}
				GetComponent<Renderer> ().sharedMaterial = goodMat;
				// Hide surface?
				if (disableAtEnd)
					gameObject.SetActive (false);
				Destroy (this);
				return;
			}
			if (Time.time - lapTime > speed) {
				lapTime = Time.time;
				Material mat = GetComponent<Renderer> ().sharedMaterial;
				if (mat != blinkMaterial)
					GenerateMaterial ();
				whichColor = !whichColor;
				if (whichColor) {
					blinkMaterial.color = color1;
				} else {
					blinkMaterial.color = color2;
				}
			}
		}

		void GenerateMaterial () {
			blinkMaterial = Instantiate (blinkMaterial);
			blinkMaterial.hideFlags = HideFlags.DontSave;
			GetComponent<Renderer> ().sharedMaterial = blinkMaterial;
		}
	}

}