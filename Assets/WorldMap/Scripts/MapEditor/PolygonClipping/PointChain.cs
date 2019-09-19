using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WPMF;

namespace WPMF.PolygonClipping {
	class PointChain {
		public bool closed;
		public List<Vector3> pointList;
		
		public PointChain(Segment s) {
			pointList = new List<Vector3>();
			pointList.Add(s.start);
			pointList.Add(s.end);
			closed = false;
		}
		
		// Links a segment to the pointChain
		public bool LinkSegment(Segment s) {
			Vector3 front = pointList[0];
			Vector3 back = pointList[pointList.Count - 1];
			
			if (Misc.PointEquals(s.start, front)) {
				if (Misc.PointEquals(s.end, back))
					closed = true;
				else
					pointList.Insert(0,s.end);
				return true;
			} else if (Misc.PointEquals(s.end, back)) {
				if (Misc.PointEquals(s.start, front))
					closed = true;
				else
					pointList.Add(s.start);
				return true;
			} else if (Misc.PointEquals(s.end, front)) {
				if (Misc.PointEquals(s.start, back))
					closed = true;
				else
					pointList.Insert (0,s.start);
				return true;
			} else if (Misc.PointEquals(s.start, back)) {
				if (Misc.PointEquals(s.end, front))
					closed = true;
				else
					pointList.Add(s.end);
				
				return true;
			}
			
			return false;
		}
		
		// Links another pointChain onto this point chain.
		public bool LinkPointChain(PointChain chain) {
			Vector3 firstPoint = pointList[0];
			Vector3 lastPoint = pointList[pointList.Count - 1];
			
			Vector3 chainFront = chain.pointList[0];
			Vector3 chainBack = chain.pointList[chain.pointList.Count - 1];
			
			if (Misc.PointEquals(chainFront, lastPoint)) {
				List<Vector3>temp = new List<Vector3>(chain.pointList);
				temp.RemoveAt(0);
				pointList.AddRange(temp);
				chain.pointList.Clear();
				return true;
			}
			
			if (Misc.PointEquals(chainBack, firstPoint)) {
				pointList.RemoveAt (0); // Remove the first element, and join this list to chain.pointList.
				List<Vector3>temp = new List<Vector3>(chain.pointList);
				temp.AddRange(pointList);
				pointList = temp;
				chain.pointList.Clear();
				return true;
			}
			
			if (Misc.PointEquals(chainFront, firstPoint)) {
				pointList.RemoveAt (0); // Remove the first element, and join to reversed chain.pointList
				List<Vector3>temp = new List<Vector3>(chain.pointList);
				temp.Reverse();
				temp.AddRange(pointList);
				pointList = temp;
				chain.pointList.Clear();
				return true;
			}
			
			if (Misc.PointEquals(chainBack, lastPoint)) {
				pointList.RemoveAt (pointList.Count-1);
				List<Vector3>temp = new List<Vector3>(chain.pointList);
				temp.Reverse();
				pointList.AddRange(temp);
				chain.pointList.Clear();
				return true;
			}
			return false;
		}
		
	}

}