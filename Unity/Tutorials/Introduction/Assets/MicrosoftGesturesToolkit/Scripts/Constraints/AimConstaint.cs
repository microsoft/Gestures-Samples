using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class AimConstaint : Constraint
    {
        private Matrix4x4 _inverseRotationTrans;
        private Quaternion _offset;

        public Vector3 Up = Vector3.up;
        public Vector3 Aim = Vector3.left;
        public Transform Target;

        private void Start()
        {
            if (!Source)
            {
                Debug.LogError("Failed " + GetType().Name + ". Source not set.");
                enabled = false;
                return;
            }

            _offset = MaintainOffset ? transform.localRotation : Quaternion.identity;

            transform.localRotation = Quaternion.identity;

            var x = transform.right;
            var y = transform.up;
            var z = transform.forward;

            var cm = Matrix4x4.identity;
            cm.SetColumn(0, x);
            cm.SetColumn(1, y);
            cm.SetColumn(2, z);
            _inverseRotationTrans = cm.inverse;
        }

        private void Update()
        {
            if (!Source) return;

            var localPos = transform.localPosition;
            var localScale = transform.localScale;
            var parent = transform.parent;
            transform.parent = null;
            var dir = (Source.position - transform.position).normalized;

            transform.right  = Vector3.Cross(Aim, Up).normalized;
            transform.up = Vector3.Cross(transform.right, Aim).normalized;
            transform.forward = Aim.normalized;

            transform.localRotation = Quaternion.LookRotation(dir) * transform.localRotation;
            transform.parent = parent;
            transform.localPosition = localPos;
            transform.localScale = localScale;
        }
    } 
}
