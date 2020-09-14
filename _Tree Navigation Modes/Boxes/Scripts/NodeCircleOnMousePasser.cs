using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeNotes_Visual
{
    public class NodeCircleOnMousePasser : MonoBehaviour
    {
        public Camera raycastCamera;
        [SerializeField] private LayerMask raycastLayer;

        void Reset()
        {
            if (!raycastCamera)
                raycastCamera = GetComponent<Camera>();
        }

        void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, maxDistance: 10000, layerMask: raycastLayer.value))
                {
                    Transform objectHit = hit.transform;

                    var cmp = objectHit.GetComponent<OnMouseOverProxy>();

                    if (cmp)
                        cmp.OnMouseOver();
                    // Do something with the object that was hit by the raycast.
                }
            }
        }
    }
}