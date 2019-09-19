using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WPMF;

namespace WPMF.PolygonClipping {

	class SweepEvent {

		public Vector3 p;
		public bool isLeft; 		// Is the point the left endpoint of the segment (p, other->p)?
		public PolygonType polygonType; 	// PolygonType to which this event belongs to: either PolygonClipper.SUBJECT, or PolygonClipper.CLIPPING
		public SweepEvent otherSE; 	// Event associated to the other endpoint of the segment
		
		/* Does the segment (p, other->p) represent an inside-outside transition
	 * in the polygon for a vertical ray from (p.x, -infinite) that crosses the segment? 
	 */
		public bool inOut;
		public EdgeType edgeType; 		// The EdgeType. @see EdgeType.as
		
		public bool inside; 		// Only used in "left" events. Is the segment (p, other->p) inside the other polygon?

		public SweepEvent(Vector3 p, bool isLeft, PolygonType polygonType): this(p,isLeft,polygonType,null, EdgeType.NORMAL) {
		}

		public SweepEvent(Vector3 p, bool isLeft, PolygonType polygonType, SweepEvent otherSweepEvent): this(p,isLeft,polygonType,otherSweepEvent, EdgeType.NORMAL) {
		}

		public SweepEvent(Vector3 p, bool isLeft, PolygonType polygonType, SweepEvent otherSweepEvent, EdgeType edgeType) {
			this.p = p;
			this.isLeft = isLeft;
			this.polygonType = polygonType;
			this.otherSE = otherSweepEvent;
			this.edgeType = edgeType;
		}
		
		public Segment segment {
			get {
				return new Segment(p, otherSE.p);
			}
		}

		float signedArea(Vector3 p0, Vector3 p1, Vector3 p2) {
			return (p0.x - p2.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p0.y - p2.y);
		}


		// Checks if this sweep event is below point p.
		public bool isBelow(Vector3 x) {
			return (isLeft) ? (signedArea(p, otherSE.p, x) > 0) : (signedArea(otherSE.p, p, x) > 0);		
		}
		
		public bool isAbove(Vector3 x) {
			return !isBelow(x);
		}

		public bool Equals(SweepEvent e2) {
			bool equal = Misc.PointEquals(p, e2.p) && isLeft == e2.isLeft && polygonType == e2.polygonType &&
				inOut == e2.inOut && edgeType == e2.edgeType &&	inside == e2.inside;
			if (!equal) return false;

			return Misc.PointEquals(otherSE.p, e2.otherSE.p) && otherSE.isLeft == e2.otherSE.isLeft && otherSE.polygonType == e2.otherSE.polygonType &&
				otherSE.inOut == e2.otherSE.inOut && otherSE.edgeType == e2.otherSE.edgeType &&	otherSE.inside == e2.otherSE.inside;
		}
		
	}

}