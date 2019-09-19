using UnityEngine;
using System.Collections;

namespace WPMF {

	public class MarkerBlinker : MonoBehaviour {

		public float duration = 4.0f;
		public float speed = 0.25f;

		public static void AddTo (GameObject marker, float duration, float speed) {
			MarkerBlinker mb = marker.AddComponent<MarkerBlinker> ();
			mb.duration = duration;
			mb.speed = speed;
		}

		float startTime, lapTime;
		Vector3 startingScale;
		bool phase;

		void Start () {
			startTime = Time.time;
			lapTime = startTime - speed;
			startingScale = transform.localScale;
		}


		// Update is called once per frame
		void Update () {
			float elapsed = Time.time - startTime;
			if (elapsed > duration) {
				// Restores material
				transform.localScale = startingScale;
				Destroy (this);
				return;
			}
			if (Time.time - lapTime > speed) {
				lapTime = Time.time;
				phase = !phase;
				if (phase) {
					transform.localScale = Misc.Vector3zero;
				} else {
					transform.localScale = startingScale;
				}
			}
		}
	}
}