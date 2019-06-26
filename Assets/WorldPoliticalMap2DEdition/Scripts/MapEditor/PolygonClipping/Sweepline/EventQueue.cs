using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WPMF;

namespace WPMF.PolygonClipping {
	class EventQueue {
		List<SweepEvent>elements;
		bool sorted;
		
		public EventQueue () {
			elements = new List<SweepEvent> (); 
			sorted = false;
		}
		
		public void Enqueue (SweepEvent obj) {
			if (!sorted) {
				elements.Add (obj);
				return;
			}
			// If already sorted use insertionSort on the inserted item.
			int count = elements.Count;
			if (count == 0) {
				elements.Add (obj);
				return;
			}
			elements.Add (null); // Expand the Vector by one.
			int i = count - 1;
			while (i >= 0 && CompareSweepEvent(obj, elements[i]) == -1) {
				elements [i + 1] = elements [i];
				i--;
			}
			elements [i + 1] = obj;
		}
		
		// The ordering is reversed because push and pop are faster.
		int CompareSweepEvent (SweepEvent e1, SweepEvent e2) {
			if (e1.Equals(e2)) return 0; 

			if (e1.p.x > e2.p.x) // Different x coordinate
				return -1;
			
			if (e1.p.x < e2.p.x) // Different x coordinate
				return 1;

			if (e1.p != e2.p) // Different points, but same x coordinate. The event with lower y coordinate is processed first
				return (e1.p.y > e2.p.y) ? -1 : 1;
			
			if (e1.isLeft != e2.isLeft) // Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first
				return (e1.isLeft) ? -1 : 1;

			// Same point, both events are left endpoints or both are right endpoints. The event associate to the bottom segment is processed first
			return e1.isAbove (e2.otherSE.p) ? -1 : 1;
		}
		
		public SweepEvent Dequeue () {
			if (!sorted) {
				elements.Sort (CompareSweepEvent);
				sorted = true;
			}
			SweepEvent e = elements [elements.Count - 1];
			elements.RemoveAt (elements.Count - 1);
			return e;
		}
		
		public bool isEmpty { get {
				return elements.Count == 0;
			} }
		
		public int length { get {
				return elements.Count;
			} }
		
	}

}