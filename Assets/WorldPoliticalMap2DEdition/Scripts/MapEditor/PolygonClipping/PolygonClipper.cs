using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace WPMF.PolygonClipping {

	public enum PolygonOp {
		UNION,
		INTERSECTION,
		DIFFERENCE,
		XOR
	}

	
	public enum PolygonType {
		SUBJECT,
		CLIPPING
	}
	
	public enum EdgeType {
		NORMAL,
		NON_CONTRIBUTING,
		SAME_TRANSITION,
		DIFFERENT_TRANSITION,
	}

	public class Segment {
		public Vector3 start, end;
		
		public Segment(Vector3 start, Vector3 end) {
			this.start = start;
			this.end = end;
		}
	}

	public class IntersectResult {
		public int max;
		public Vector3 point1;
		public Vector3 point2;
		
		public IntersectResult(int max, Vector3 point1, Vector3 point2) {
			this.max = max;
			this.point1 = point1;
			this.point2 = point2;
		}
	}

	public class PolygonClipper {

		Polygon subject, clipping;
		EventQueue eventQueue;
		WPMF.Region regionSubject;
		const float PRECISION = 1000; //000000000.0f;

		public PolygonClipper(WPMF.Region regionSubject, WPMF.Region regionClipping) {
			// Setup subject and clipping polygons
			this.regionSubject = regionSubject;
			subject = new Polygon();
			Contour scont = new Contour();
			scont.AddRange(regionSubject.points);
			for (int k=0;k<scont.points.Count;k++) scont.points[k] *= PRECISION;
			subject.AddContour(scont);
			clipping = new Polygon();
			Contour ccont = new Contour();
			ccont.AddRange(regionClipping.points);
			for (int k=0;k<ccont.points.Count;k++) ccont.points[k] *= PRECISION;
			clipping.AddContour(ccont);

			// Init event queue
			eventQueue = new EventQueue();
		}

		public bool OverlapsSubjectAndClipping() {

			if (!subject.boundingBox.Intersects(clipping.boundingBox)) return false;

			for (int k=0;k<clipping.contours[0].points.Count;k++) {
				Vector3 p = clipping.contours[0].points[k];
				for (int j=0;j<subject.contours[0].points.Count;j++) {
					if (Misc.PointEquals(subject.contours[0].points[j], p)) {
						return true;
					}
				}
			}

			bool outside = false, inside = false;
			for (int k=0;k<clipping.contours[0].points.Count;k++) {
				Vector3 p = clipping.contours[0].points[k];
				if (subject.contours[0].ContainsPoint(p)) {
					inside = true;
					if (outside) return true;
				} else {
					outside = true;
					if (inside) return true;
				}
			}
			return false;
		}


		public void Compute(PolygonOp operation) {
			Polygon polygon = ComputeInternal (operation);
			if (polygon==null) return;

			for (int k=0;k<polygon.contours.Count;k++) {
				Contour cont = polygon.contours[k];
				for(int j=0;j<cont.points.Count;j++) {
					cont.points[j] /= PRECISION;
				}
			}
			// Returns the contour
			regionSubject.points = polygon.contours[0].points.ToArray();
		}

		void ProcessSegment(Segment segment, PolygonType polygonType) {
			if (Misc.PointEquals(segment.start, segment.end)) return;
			SweepEvent e1 = new SweepEvent(segment.start, true, polygonType);
			SweepEvent e2 = new SweepEvent(segment.end, true, polygonType, e1);
			e1.otherSE = e2;

			if (e1.p.x < e2.p.x) {
				e2.isLeft = false;
			} else if (e1.p.x > e2.p.x) {
				e1.isLeft = false;
			} else if (e1.p.y < e2.p.y) { // the segment isLeft vertical. The bottom endpoint isLeft the isLeft endpoint 
				e2.isLeft = false;
			} else {
				e1.isLeft = false;
			}
			
			// Pushing it so the que is sorted from left to right, with object on the left
			// having the highest priority.
			eventQueue.Enqueue(e1);
			eventQueue.Enqueue(e2);
		}


		Polygon ComputeInternal (PolygonOp operation) {
			Polygon result = null;

			// Test 1 for trivial result case
			if (subject.contours.Count * clipping.contours.Count == 0) {
				if (operation == PolygonOp.DIFFERENCE)
					result = subject;
				else if (operation == PolygonOp.UNION || operation == PolygonOp.XOR)
					result = (subject.contours.Count == 0) ? clipping : subject;
				return result;
			}

			// Test 2 for trivial result case
			Rectangle subjectBB = subject.boundingBox;
			Rectangle clippingBB = clipping.boundingBox;
			if (!subjectBB.Intersects(clippingBB)) {
				if (operation == PolygonOp.DIFFERENCE)
					result = subject;
				if (operation == PolygonOp.UNION || operation == PolygonOp.XOR) {
					result = subject;
					foreach(Contour c in clipping.contours)
						result.AddContour(c);
				}
				
				return result;
			}
			
			// Add each segment to the eventQueue, sorted from left to right.
			foreach(Contour sCont in subject.contours)
				for (int pParse1=0;pParse1<sCont.points.Count;pParse1++)
					ProcessSegment(sCont.GetSegment(pParse1), PolygonType.SUBJECT);
			
			foreach(Contour cCont in clipping.contours)
				for (int pParse2=0;pParse2<cCont.points.Count;pParse2++)
					ProcessSegment(cCont.GetSegment(pParse2), PolygonType.CLIPPING);
			
			Connector connector = new Connector();
			
			// This is the SweepLine. That is, we go through all the polygon edges
			// by sweeping from left to right.
			SweepEventSet S = new SweepEventSet();
			
			float MINMAX_X = Mathf.Min(subjectBB.right, clippingBB.right);
			
			SweepEvent prev, next;

			int panicCounter = 0; 

			while (!eventQueue.isEmpty)
			{
				if (panicCounter++>100000) break;
				prev = null;
				next = null;
				
				SweepEvent e = eventQueue.Dequeue();
				
				if ((operation == PolygonOp.INTERSECTION && e.p.x > MINMAX_X) || (operation == PolygonOp.DIFFERENCE && e.p.x > subjectBB.right)) 
					return connector.ToPolygonFromLargestLineStrip();
				
				if (operation == PolygonOp.UNION && e.p.x > MINMAX_X) {
					// add all the non-processed line segments to the result
					if (!e.isLeft)
						connector.Add(e.segment);
					
					while (!eventQueue.isEmpty) {
						e = eventQueue.Dequeue();
						if (!e.isLeft)
							connector.Add(e.segment);
					}
					return connector.ToPolygonFromLargestLineStrip();
				}
				
				if (e.isLeft) {  // the line segment must be inserted into S
					int pos = S.Insert(e);
					
					prev = (pos > 0) ? S.eventSet[pos - 1] : null;
					next = (pos < S.eventSet.Count - 1) ? S.eventSet[pos + 1] : null;				
					
					if (prev == null) {
						e.inside = e.inOut = false;
					} else if (prev.edgeType != EdgeType.NORMAL) {
						if (pos - 2 < 0) { // e overlaps with prev
							// Not sure how to handle the case when pos - 2 < 0, but judging
							// from the C++ implementation this looks like how it should be handled.
							e.inside = e.inOut = false;
							if (prev.polygonType != e.polygonType)
								e.inside = true;
							else
								e.inOut = true;
						} else {
							SweepEvent prevTwo = S.eventSet[pos - 2];						
							if (prev.polygonType == e.polygonType) {
								e.inOut = !prev.inOut;
								e.inside = !prevTwo.inOut;
							} else {
								e.inOut = !prevTwo.inOut;
								e.inside = !prev.inOut;
							}
						}
					} else if (e.polygonType == prev.polygonType) {
						e.inside = prev.inside;
						e.inOut = !prev.inOut;
					} else {
						e.inside = !prev.inOut;
						e.inOut = prev.inside;
					}

					// Process a possible intersection between "e" and its next neighbor in S
					if (next != null)
						PossibleIntersection(e, next);

					// Process a possible intersection between "e" and its previous neighbor in S
					if (prev != null)
						PossibleIntersection(prev, e);
				} else { // the line segment must be removed from S

					// Get the next and previous line segments to "e" in S
					int otherPos = -1;
					for (int evt=0;evt<S.eventSet.Count;evt++) {
						if (e.otherSE.Equals(S.eventSet[evt])) {
							otherPos = evt;
							break;
						}
					}
					if (otherPos != -1) {
						prev = (otherPos > 0) ? S.eventSet[otherPos - 1] : null;
						next = (otherPos < S.eventSet.Count - 1) ? S.eventSet[otherPos + 1] : null;
					}
					
					switch (e.edgeType) {
					case EdgeType.NORMAL:
						switch (operation) {
						case PolygonOp.INTERSECTION:
							if (e.otherSE.inside)
								connector.Add(e.segment);
							break;
						case PolygonOp.UNION:
							if (!e.otherSE.inside)
								connector.Add(e.segment);
							break;
						case PolygonOp.DIFFERENCE:
							if ((e.polygonType == PolygonType.SUBJECT && !e.otherSE.inside) || (e.polygonType == PolygonType.CLIPPING && e.otherSE.inside))
								connector.Add(e.segment);
							break;
						case PolygonOp.XOR:
							connector.Add (e.segment);
							break;
						}
						break;
					case EdgeType.SAME_TRANSITION:
						if (operation == PolygonOp.INTERSECTION || operation == PolygonOp.UNION)
							connector.Add(e.segment);
						break;
					case EdgeType.DIFFERENT_TRANSITION:
						if (operation == PolygonOp.DIFFERENCE)
							connector.Add(e.segment);
						break;
					}
					
					if (otherPos != -1)
						S.Remove(S.eventSet[otherPos]);
					
					if (next != null && prev != null)
						PossibleIntersection(prev, next);				
				}
			}
			
			return connector.ToPolygonFromLargestLineStrip();
		}


		IntersectResult FindIntersection(Segment seg0, Segment seg1) {
			Vector3 pi0 = WPMF.Misc.Vector3zero;
			Vector3 pi1 = WPMF.Misc.Vector3zero;
			
			Vector3 p0 = seg0.start;
			Vector3 d0 = new Vector3(seg0.end.x - p0.x, seg0.end.y - p0.y);
			Vector3 p1 = seg1.start;
			Vector3 d1 = new Vector3(seg1.end.x - p1.x, seg1.end.y - p1.y);
			float sqrEpsilon = 0.0000001f; // Antes 0.001
			Vector3 E = new Vector3(p1.x - p0.x, p1.y - p0.y);
			float kross = d0.x * d1.y - d0.y * d1.x;
			float sqrKross = kross * kross;
			float sqrLen0 = d0.magnitude;
			float sqrLen1 = d1.magnitude;
			
			if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1) {
				// lines of the segments are not parallel
				float s = (E.x * d1.y - E.y * d1.x) / kross;
				if (s < 0 || s > 1) {
					return new IntersectResult (max: 0, point1: pi0, point2: pi1);
				}
				float t = (E.x * d0.y - E.y * d0.x) / kross;
				if (t < 0 || t > 1) {
					return new IntersectResult (max: 0, point1: pi0, point2: pi1);
				}
				// intersection of lines is a point an each segment
				pi0.x = p0.x + s * d0.x;
				pi0.y = p0.y + s * d0.y;
				
				// The following will prevent precision errors
//				if (Vector3.Distance(pi0,seg0.start) < 0.00000001f) pi0 = seg0.start;
//				if (Vector3.Distance(pi0,seg0.end) < 0.00000001f) pi0 = seg0.end;
//				if (Vector3.Distance(pi0,seg1.start) < 0.00000001f) pi0 = seg1.start;
//				if (Vector3.Distance(pi0,seg1.end) < 0.00000001f) pi0 = seg1.end;
				return new IntersectResult ( max: 1, point1: pi0, point2: pi1 );
			}
			
			// lines of the segments are parallel
			float sqrLenE = E.magnitude;
			kross = E.x * d0.y - E.y * d0.x;
			sqrKross = kross * kross;
			if (sqrKross > sqrEpsilon * sqrLen0 * sqrLenE) {
				// lines of the segment are different
				return new IntersectResult ( max: 0, point1: pi0, point2: pi1 );
			}
			
			// Lines of the segments are the same. Need to test for overlap of segments.
			float s0 = (d0.x * E.x + d0.y * E.y) / sqrLen0;  // so = Dot (D0, E) * sqrLen0
			float s1 = s0 + (d0.x * d1.x + d0.y * d1.y) / sqrLen0;  // s1 = s0 + Dot (D0, D1) * sqrLen0
			float smin = Mathf.Min(s0, s1);
			float smax = Mathf.Max(s0, s1);
			float[] w = new float[2];
			int imax = FindIntersection2(0.0f, 1.0f, smin, smax, w);
			
			if (imax > 0) {
				pi0.x = p0.x + w[0] * d0.x;
				pi0.y = p0.y + w[0] * d0.y;
//				if (Vector3.Distance(pi0,seg0.start) < 0.00000001) pi0 = seg0.start;
//				if (Vector3.Distance(pi0,seg0.end) < 0.00000001) pi0 = seg0.end;
//				if (Vector3.Distance(pi0,seg1.start) < 0.00000001) pi0 = seg1.start;
//				if (Vector3.Distance(pi0,seg1.end) < 0.00000001) pi0 = seg1.end;
				if (imax > 1) {
					pi1.x = p0.x + w[1] * d0.x;
					pi1.y = p0.y + w[1] * d0.y;
				}
			}
			return new IntersectResult (max: imax, point1: pi0, point2: pi1);
		}

		int FindIntersection2(float u0, float u1, float v0, float v1, float[] w) {
			if (u1 < v0 || u0 > v1)
				return 0;
			if (u1 > v0) {
				if (u0 < v1) {
					w[0] = (u0 < v0) ? v0 : u0;
					w[1] = (u1 > v1) ? v1 : u1;
					return 2;
				} else {
					// u0 == v1
					w[0] = u0;
					return 1;
				}
			} 

			// u1 == v0
			w[0] = u1;
			return 1;
		}


		void PossibleIntersection(SweepEvent e1, SweepEvent e2) {
			//	if ((e1->pl == e2->pl) ) // Uncomment these two lines if self-intersecting polygons are not allowed
			//		return false;
			
			IntersectResult intData = FindIntersection(e1.segment, e2.segment);
			int numIntersections = intData.max;
			Vector3 ip1 = intData.point1;

			if (numIntersections == 0)
				return;
			
			if (numIntersections == 1 && (Misc.PointEquals(e1.p, e2.p) || Misc.PointEquals(e1.otherSE.p, e2.otherSE.p)))
				return; // the line segments intersect at an endpoint of both line segments
			
			if (numIntersections == 2 && e1.polygonType==e2.polygonType)
				return;  // the line segments overlap, but they belong to the same polygon

			// The line segments associated to e1 and e2 intersect
			if (numIntersections == 1) {
				if (!Misc.PointEquals(e1.p,ip1) && !Misc.PointEquals(e1.otherSE.p,ip1))
					DivideSegment (e1, ip1); // if ip1 is not an endpoint of the line segment associated to e1 then divide "e1"
				if (!Misc.PointEquals(e2.p, ip1) && !Misc.PointEquals(e2.otherSE.p,ip1))
					DivideSegment (e2, ip1); // if ip1 is not an endpoint of the line segment associated to e2 then divide "e2"
				return;
			}

			// The line segments overlap
			List<SweepEvent>sortedEvents = new List<SweepEvent>();
			if (Misc.PointEquals(e1.p,e2.p)) {
				sortedEvents.Add(null);
			} else if (Sec(e1, e2)) {
				sortedEvents.Add(e2);
				sortedEvents.Add(e1);
			} else {
				sortedEvents.Add(e1);
				sortedEvents.Add(e2);
			}
			
			if (Misc.PointEquals(e1.otherSE.p,e2.otherSE.p)) {
				sortedEvents.Add(null);
			} else if (Sec(e1.otherSE, e2.otherSE)) {
				sortedEvents.Add(e2.otherSE);
				sortedEvents.Add(e1.otherSE);
			} else {
				sortedEvents.Add(e1.otherSE);
				sortedEvents.Add(e2.otherSE);
			}
			
			if (sortedEvents.Count == 2) { // are both line segments equal?
				e1.edgeType = e1.otherSE.edgeType = EdgeType.NON_CONTRIBUTING;
				e2.edgeType = e2.otherSE.edgeType = ((e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION);
				return;
			}
			
			if (sortedEvents.Count == 3) {  // the line segments share an endpoint
				sortedEvents[1].edgeType = sortedEvents[1].otherSE.edgeType = EdgeType.NON_CONTRIBUTING;
				if (sortedEvents[0] != null)         // is the right endpoint the shared point?
					sortedEvents[0].otherSE.edgeType = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
				else 								// the shared point is the left endpoint
					sortedEvents[2].otherSE.edgeType = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
				DivideSegment (sortedEvents[0] != null ? sortedEvents[0] : sortedEvents[2].otherSE, sortedEvents[1].p);
				return;
			}
			
			if (!sortedEvents[0].Equals(sortedEvents[3].otherSE))
			{ // no segment includes totally the otherSE one
				sortedEvents[1].edgeType = EdgeType.NON_CONTRIBUTING;
				sortedEvents[2].edgeType = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
				DivideSegment (sortedEvents[0], sortedEvents[1].p);
				DivideSegment (sortedEvents[1], sortedEvents[2].p);
				return;
			}

			// one line segment includes the other one
			sortedEvents[1].edgeType = sortedEvents[1].otherSE.edgeType = EdgeType.NON_CONTRIBUTING;
			DivideSegment (sortedEvents[0], sortedEvents[1].p);
			sortedEvents[3].otherSE.edgeType = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
			DivideSegment (sortedEvents[3].otherSE, sortedEvents[2].p);
		}

		
		bool Sec(SweepEvent e1, SweepEvent e2) {
			// Different x coordinate
			if (e1.p.x != e2.p.x) {
				return e1.p.x > e2.p.x;
			}
			
			// Same x coordinate. The event with lower y coordinate is processed first
			if (e1.p.y != e2.p.y) {
				return e1.p.y > e2.p.y;
			}
			
			// Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first
			if (e1.isLeft != e2.isLeft) {
				return e1.isLeft;
			}
			
			// Same point, both events are left endpoints or both are right endpoints. The event associate to the bottom segment is processed first
			return e1.isAbove(e2.otherSE.p);
		}

		void DivideSegment(SweepEvent e, Vector3 p) {
			// "Right event" of the "left line segment" resulting from dividing e (the line segment associated to e)
			SweepEvent r = new SweepEvent(p, false, e.polygonType, e, e.edgeType);
			// "Left event" of the "right line segment" resulting from dividing e (the line segment associated to e)
			SweepEvent l =  new SweepEvent(p, true, e.polygonType, e.otherSE, e.otherSE.edgeType);
			
			if (Sec(l, e.otherSE)) { // avoid a rounding error. The left event would be processed after the right event
				e.otherSE.isLeft = true;
				e.isLeft = false;
			}
			
			e.otherSE.otherSE = l;
			e.otherSE = r;
			
			eventQueue.Enqueue(l);
			eventQueue.Enqueue(r);
		}
	}

}