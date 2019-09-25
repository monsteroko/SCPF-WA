using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WPMF;

namespace WPMF.PolygonClipping {

	class Rectangle {
		public float minX, minY, width, height;

		public Rectangle(float minX, float minY, float width, float height) {
			this.minX = minX;
			this.minY = minY;
			this.width = width;
			this.height = height;
		}

		public float right {
			get {
				return minX + width;
			}
		}

		public float top {
			get {
				return minY + height;
			}
		}

		public Rectangle Union(Rectangle o) {
			float minX = this.minX < o.minX ? this.minX: o.minX;
			float maxX = this.right > o.right ? this.right: o.right;
			float minY = this.minY < o.minY ? this.minY: o.minY;
			float maxY = this.top > o.top ? this.top: o.top;
			return new Rectangle(minX, minY, maxX-minX, maxY-minY);
		}

		public bool Intersects(Rectangle o) {
			if (o.minX>right) return false;
			if (o.right<minX) return false;
			if (o.minY>top) return false;
			if (o.top<minY) return false;
			return true;
		}

		public bool Contains(Vector2 p) {
			if (minX>p.x) return false;
			if (minY>p.y) return false;
			if (right < p.x) return false;
			if (top < p.y) return false;
			return true;

		}
	}

}