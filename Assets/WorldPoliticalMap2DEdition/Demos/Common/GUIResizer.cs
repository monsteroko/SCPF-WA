using UnityEngine;
using System.Collections;

namespace WPMF {
	public static class GUIResizer {

		public static int authoredScreenWidth, authoredScreenHeight;
		static Vector2 resizeRatio;

		public static void Init (int screenWidth, int screenHeight) {
			if (Application.isEditor) {
				authoredScreenWidth = Screen.width;
				authoredScreenHeight = Screen.height;
			} else {
				authoredScreenWidth = screenWidth;
				authoredScreenHeight = screenHeight;
			}
		
			resizeRatio = new Vector2 ((float)Screen.width / authoredScreenWidth, (float)Screen.height / authoredScreenHeight);
		}

		public static void AutoResize () {
			if (resizeRatio.x!=0)
				GUI.matrix = Matrix4x4.TRS (Misc.Vector3zero, Quaternion.identity, new Vector3 (resizeRatio.x, resizeRatio.y, 1.0f));
		}
	
		public static Rect GetRect (float left, float top, float width, float height) {
			Vector2 position = GUI.matrix.MultiplyVector (new Vector2 (left, top));
			Vector2 size = GUI.matrix.MultiplyVector (new Vector2 (width, height));
		
			return new Rect (position.x, position.y, size.x, size.y);
		}

	}

}