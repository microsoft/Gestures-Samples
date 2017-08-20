using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.Gestures.Toolkit
{
    [RequireComponent(typeof(BoxCollider))]
    public class SceneBounds : Singleton<SceneBounds>
    {
        private BoxCollider _bounds;

        public float RepelForce = 10;
        public bool MaintainWithinBounds = true;

        private void Start()
        {
            _bounds = GetComponent<BoxCollider>();
            _bounds.isTrigger = true;
        }

        public bool Contains(Bounds bounds)
        {
            return _bounds.bounds.Contains(bounds.min) && _bounds.bounds.Contains(bounds.max);
        }

        private void FixedUpdate()
        {
            if (!_bounds || !MaintainWithinBounds) return;

            var targetBounds = _bounds.bounds;
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var rb in go.GetComponentsInChildren<Rigidbody>())
                {
                    var collider = rb.GetComponent<Collider>();

                    if (rb.isKinematic || !collider || Contains(collider.bounds)) continue;

                    var colliderBounds = collider.bounds;
                    var deltaPos = Vector3.zero;

                    //bound X
                    if (colliderBounds.min.x < targetBounds.min.x)
                        deltaPos.x = targetBounds.min.x - colliderBounds.min.x;
                    else if (colliderBounds.max.x > targetBounds.max.x)
                        deltaPos.x = targetBounds.max.x - colliderBounds.max.x;

                    //bound Y
                    if (colliderBounds.min.y < targetBounds.min.y)
                        deltaPos.y = targetBounds.min.y - colliderBounds.min.y;
                    else if (colliderBounds.max.y > targetBounds.max.y)
                        deltaPos.y = targetBounds.max.y - colliderBounds.max.y;

                    //bound X
                    if (colliderBounds.min.z < targetBounds.min.z)
                        deltaPos.z = targetBounds.min.z - colliderBounds.min.z;
                    else if (colliderBounds.max.z > targetBounds.max.z)
                        deltaPos.z = targetBounds.max.z - colliderBounds.max.z;

                    rb.AddForce(RepelForce * deltaPos);
                    rb.transform.position += deltaPos;
                }
            }
        }
    }
}