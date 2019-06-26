using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPMF {
	public static class Drawing {

		/// <summary>
		/// Rotates one point around another
		/// </summary>
		/// <param name="pointToRotate">The point to rotate.</param>
		/// <param name="centerPoint">The centre point of rotation.</param>
		/// <param name="angleInDegrees">The rotation angle in degrees.</param>
		/// <returns>Rotated point</returns>
		static Vector2 RotatePoint (Vector2 pointToRotate, Vector2 centerPoint, float angleInDegrees) {
			float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
			float cosTheta = Mathf.Cos (angleInRadians);
			float sinTheta = Mathf.Sin (angleInRadians);
			return new Vector2 (cosTheta * (pointToRotate.x - centerPoint.x) - sinTheta * (pointToRotate.y - centerPoint.y) + centerPoint.x,
			                   sinTheta * (pointToRotate.x - centerPoint.x) + cosTheta * (pointToRotate.y - centerPoint.y) + centerPoint.y);
		}

		public static GameObject CreateSurface (string name, Vector3[] surfPoints, int[] indices, Material material) {
			Rect dummyRect = new Rect ();
			return CreateSurface (name, surfPoints, indices, material, dummyRect, Misc.Vector2one, Misc.Vector2zero, 0);
		}

		public static GameObject CreateSurface (string name, Vector3[] points, int[] indices, Material material, Rect rect, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
			
			GameObject hexa = new GameObject (name, typeof(MeshRenderer), typeof(MeshFilter));
			hexa.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			Mesh mesh = new Mesh ();
			mesh.hideFlags = HideFlags.DontSave;
			mesh.vertices = points;
			mesh.triangles = indices;
			// uv mapping
			if (material.mainTexture != null) {
				Vector2[] uv = new Vector2[points.Length];
				for (int k=0; k<uv.Length; k++) {
					Vector2 coor = points [k];
					coor.x /= textureScale.x;
					coor.y /= textureScale.y;
					if (textureRotation != 0) 
						coor = RotatePoint (coor, Misc.Vector2zero, textureRotation);
					coor += textureOffset;
					Vector2 normCoor = new Vector2 ((coor.x - rect.xMin) / rect.width, (coor.y - rect.yMax) / rect.height);
					uv [k] = normCoor;
				}
				mesh.uv = uv;
			}
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			#if !UNITY_5_5_OR_NEWER
			mesh.Optimize ();
			#endif
			
			MeshFilter meshFilter = hexa.GetComponent<MeshFilter> ();
			meshFilter.mesh = mesh;
			
			hexa.GetComponent<Renderer> ().sharedMaterial = material;
			return hexa;
			
		}

		public static GameObject CreateText (string text, GameObject parent, Vector2 center, Font labelFont, Color textColor, bool showShadow, Material shadowMaterial, Color shadowColor) {
			// create base text
			GameObject textObj = new GameObject (text);
			textObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			if (parent != null) {
				textObj.transform.SetParent (parent.transform, false);
			}
			textObj.transform.localPosition = new Vector3 (center.x, center.y, 0);
			TextMesh tm = textObj.AddComponent<TextMesh> ();
			tm.font = labelFont;
			textObj.GetComponent<Renderer> ().sharedMaterial = tm.font.material;
			tm.alignment = TextAlignment.Center;
			tm.anchor = TextAnchor.MiddleCenter;
			tm.color = textColor;
			tm.text = text;

			// add shadow
			if (showShadow) {
				GameObject shadow = GameObject.Instantiate (textObj);
				shadow.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				shadow.name = "shadow";
				shadow.transform.SetParent (textObj.transform, false);
				shadow.transform.localScale = Misc.Vector3one;
				shadow.transform.localPosition = new Vector3 (Mathf.Max (center.x / 100.0f, 1), Mathf.Min (center.y / 100.0f, -1), 0);
				shadow.GetComponent<Renderer> ().sharedMaterial = shadowMaterial;
				shadow.GetComponent<TextMesh> ().color = shadowColor;
			}
			return textObj;
		}
	}


}



