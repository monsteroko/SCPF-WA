using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentalMap {
    public class Zone {

        public readonly string name;
        public readonly Area area;
        public readonly Vector2 position;
        public bool isBuilt;
        public GameObject modelObject { get; private set; }

        public Zone(string name, Area area, Vector2 position, bool isBuilt = false) {
            this.name = name;
            this.area = area;
            this.position = position;
            this.isBuilt = isBuilt;
        }

        public void SetModelObject(GameObject zoneObj) {
            if (modelObject != null) {
                UnityEngine.Object.Destroy(modelObject);
            }
            modelObject = zoneObj;
            modelObject.transform.localPosition = new Vector3(position.x, position.y, 0);
        }

    }
}
