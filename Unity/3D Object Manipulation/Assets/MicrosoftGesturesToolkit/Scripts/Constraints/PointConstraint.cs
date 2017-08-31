using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class PointConstraint : Constraint
    {
        private void Awake()
        {
            if(!Source)
            {
                Debug.LogError("Failed " + GetType().Name + ". Source not set.");
                enabled = false;
                return;
            }

            if (MaintainOffset) Offset = transform.position - Source.position;
        }

        private void Update()
        {
            var pos = transform.position;
            var targetPos = Source.position + Offset;

            if (ConstainX) pos[0] = targetPos[0];
            if (ConstainY) pos[1] = targetPos[1];
            if (ConstainZ) pos[2] = targetPos[2];

            transform.position = pos;
        }
    } 
}
