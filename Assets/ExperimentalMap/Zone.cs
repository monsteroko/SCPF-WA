using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentalMap {
    public class Zone {

        public readonly string name;
        public readonly Area area;
        public readonly Vector2 position;
        public GameObject modelObject { get; private set; }

        public Zone(string name, Area area, Vector2 position) {
            this.name = name;
            this.area = area;
            //this.position = MapUtility.PointFromFloatData(position);
            this.position = position;
        }

        public void SetModelObject(GameObject zoneObj, bool isPure = true) {
            modelObject = zoneObj;
            if (isPure) {
                modelObject.transform.rotation = Quaternion.Euler(90.0f, 0, 0);
            }
        }

    }
}
