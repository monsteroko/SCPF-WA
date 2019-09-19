using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WPMF;

namespace WPMF.PolygonClipping {
	class Polygon {
		public List<Contour> contours;
		public Rectangle bounds;
		
		public Polygon () {
			contours = new List<Contour> ();
			bounds = null;
		}
		
		public int numVertices {
			get {
				int verticesCount = 0;
				foreach (Contour c in contours) {
					verticesCount += c.points.Count;
				}
				return verticesCount;
			}
		}
		
		public List<Vector3>vertices { 
			get {
				List<Vector3> allVertices = new List<Vector3> ();
				foreach (Contour c in contours) {
					allVertices.AddRange (c.points);
				}
				return allVertices;
			}
		}

		public Rectangle boundingBox {
			get {
				if (bounds != null)
					return bounds;
			
				Rectangle bb = null;
				foreach (Contour c in contours) {
					Rectangle cBB = c.boundingBox;
					if (bb == null)
						bb = cBB;
					else
						bb = bb.Union (cBB);
				}
				bounds = bb;
				return bounds;
			}
		}
		
		public void AddContour (Contour c) {
			contours.Add (c);
		}
		
		public Polygon Clone () {
			Polygon poly = new Polygon ();
			foreach (Contour cont in this.contours) {
				Contour c = new Contour ();
				c.AddRange (cont.points);
				poly.AddContour (c);
			}
			return poly;
		}
		
	}

}