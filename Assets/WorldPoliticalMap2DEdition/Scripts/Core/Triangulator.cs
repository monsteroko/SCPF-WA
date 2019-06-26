using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {
	public class Triangulator {
		Vector3[] m_points;
		int m_numPoints;

		public static int[]  GetPoints (Vector3[] points) {
			Triangulator triangulator = new Triangulator (points);
			return triangulator.Triangulate ();
		}
	
		public Triangulator (Vector3[] points) {
			this.m_points = points;
			m_numPoints = points.Length;
		}
	
		public int[] Triangulate () {
			int n = m_numPoints;
			if (n < 3)
				return new int[0];
			List<int> indices = new List<int> (n*3);
			int[] V = new int[n];
			if (Area () > 0) {
				for (int v = 0; v < n; v++)
					V [v] = v;
			} else {
				for (int v = 0; v < n; v++)
					V [v] = (n - 1) - v;
			}
			int nv = n;
			int count = 2 * nv;
			int sizeofInt = sizeof(int);

			for (int v = nv - 1; nv > 2;) {
				if ((count--) <= 0)
					return indices.ToArray ();
			
				int u = v;
				if (nv <= u)
					u = 0;
				v = u + 1;
				if (nv <= v)
					v = 0;
				int w = v + 1;
				if (nv <= w)
					w = 0;
			
				if (Snip (u, v, w, nv, V)) {
					int a, b, c;
					a = V [u];
					b = V [v];
					c = V [w];
					indices.Add (a);
					indices.Add (b);
					indices.Add (c);
					Buffer.BlockCopy(V, (v+1) * sizeofInt, V, v*sizeofInt, (nv-v-1)*sizeofInt); // fast shift array to the left one position
					nv--;
					count = 2 * nv;
				}
			}
			indices.Reverse ();
			return indices.ToArray ();
		}

		private bool Snip (int u, int v, int w, int n, int[] V) {
			Vector3 A = m_points [V [u]];
			Vector3 B = m_points [V [v]];
			Vector3 C = m_points [V [w]];
			if (Mathf.Epsilon > (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x))
				return false;
			for (int p = 0; p < n; p++) {
				if (InsideTriangle (A, B, C, m_points [V [p]])) {
					if ((p == u) || (p == v) || (p == w))
						continue;
					return false;
				}
			}
			return true;
		}

		private bool InsideTriangle (Vector3 A, Vector3 B, Vector3 C, Vector3 P) {
			float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
			float cCROSSap, bCROSScp, aCROSSbp;
			
			ax = C.x - B.x;
			bpy = P.y - B.y;
			ay = C.y - B.y;
			bpx = P.x - B.x;
			aCROSSbp = ax * bpy - ay * bpx;
			if (aCROSSbp < 0.0f) return false;
			
			bx = A.x - C.x;
			by = A.y - C.y;
			cpx = P.x - C.x;
			cpy = P.y - C.y;
			bCROSScp = bx * cpy - by * cpx;
			if (bCROSScp < 0.0f) return false;
			
			cx = B.x - A.x;
			cy = B.y - A.y;
			apx = P.x - A.x;
			apy = P.y - A.y;
			cCROSSap = cx * apy - cy * apx;
			return (cCROSSap >= 0.0f);
		}
	
		private float Area () {
			int n = m_numPoints;
			float A = 0.0f;
			for (int p = n - 1, q = 0; q < n; p = q++) {
				Vector2 pval = m_points [p];
				Vector2 qval = m_points [q];
				A += pval.x * qval.y - qval.x * pval.y;
			}
			return (A * 0.5f);
		}
	

	}


}