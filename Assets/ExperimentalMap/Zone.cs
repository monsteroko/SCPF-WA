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
            this.position = position;
        }

        public void SetModelObject(GameObject zoneObj) {
            modelObject = zoneObj;
            modelObject.transform.localPosition = new Vector3(position.x, position.y, 0);
        }

    }
}
