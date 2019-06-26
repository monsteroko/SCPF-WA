using UnityEngine;
using System.Collections;

namespace WPMF
{
	public static class Misc
	{
		public static Vector4 Vector4back = new Vector4 (0f, 0f, -1f, 0f);
		public static Vector3 Vector3one = new Vector3(1f,1f,1f); 
		public static Vector3 Vector3zero = new Vector3(0f,0f,0f);
		public static Vector3 Vector3back = new Vector3 (0f, 0f, -1f);
		public static Vector3 Vector3right = new Vector3 (1f, 0f, 0f);
		public static Vector3 Vector3left = new Vector3 (-1f, 0f, 0f);
		public static Vector3 Vector3up = new Vector3 (0f, 1f, 0f);
		public static Vector3 Vector3down = new Vector3 (0f, -1f, 0f);
		public static Vector2 Vector2one = new Vector2 (1f, 1f);
		public static Vector2 Vector2zero = new Vector2 (0f, 0f);
		public static Vector3 Vector2down = new Vector3 (0f, -1f);
		public static Vector3 ViewportCenter = new Vector3 (0.5f, 0.5f, 0.0f);
	}
}