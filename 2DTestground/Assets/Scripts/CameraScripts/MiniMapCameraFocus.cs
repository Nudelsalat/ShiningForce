using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CameraScripts {
    class MiniMapCameraFocus : MonoBehaviour {

        private float cameraDistance = 1.0f;

        void Update() {
            GameObject go = GameObject.Find("World/Tiles/Boarder");
            if (go != null) {
                var collider = go.GetComponent<Collider2D>();
                Vector3 objectSizes = collider.bounds.max - collider.bounds.min;
                float objectSize = Mathf.Max(objectSizes.x, objectSizes.y * 1.5f);
                float distance = cameraDistance * objectSize;
                transform.position = collider.bounds.center - distance * transform.forward; //middle
            }
        }
    }
}
