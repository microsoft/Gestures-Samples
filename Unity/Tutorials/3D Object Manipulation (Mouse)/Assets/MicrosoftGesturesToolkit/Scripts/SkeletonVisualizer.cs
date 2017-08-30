using Microsoft.Gestures.UnitySdk;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class SkeletonVisualizer : ObjectBase
    {
        public const float mmToMeters = 0.001f;

        private Color _color;
        private GameObject _palm;
        private ValueByFinger<GameObject> _boxByFinger;

        public Vector3 UnitsScale = 0.001f * Vector3.one;
        public Vector3 UnitsOffset = Vector3.zero;
        public Vector3 CubeScale = Vector3.one;

        public Color SkeletonColor = Color.red;
        public bool ShowUI = true;
        public bool UseSmoothSkeleton = true;
        public bool IsVisible = true;

        void OnEnable()
        {
            if (GesturesManager.Instance == null) return;

            // Create primitive to represent the palm and the fingers.
            _palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _palm.transform.parent = transform;
            _palm.transform.name = "Palm";
            _palm.layer = gameObject.layer;

            _boxByFinger = new ValueByFinger<GameObject>();
            foreach (var finger in _boxByFinger.Keys)
            {
                _boxByFinger[finger] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _boxByFinger[finger].transform.parent = transform;
                _boxByFinger[finger].transform.name = finger.ToString();
                _boxByFinger[finger].layer = gameObject.layer;
            }

            if (!GesturesManager.Instance.IsSkeletonRegistered) GesturesManager.Instance.RegisterToSkeleton();
        }

        private void OnDisable()
        {
            if (_palm)
            {
                Destroy(_palm);
                _palm = null;
            }

            if (_boxByFinger != null)
            {
                foreach (var finger in _boxByFinger.Keys) Destroy(_boxByFinger[finger]);
                _boxByFinger = null;
            }
        }

        private void OnGUI()
        {
            if (ShowUI)
            {
                IsVisible = GUI.Toggle(new Rect(Screen.width - 150, Screen.height - 40, 140, 30), IsVisible, "Show Skeleton");
            }
        }

        private Vector3 TransformPoint(Vector3 pos)
        {
            var cam = Camera.main.transform;
            
            var viewScaledPos = Vector3.Scale(pos, UnitsScale) + UnitsOffset;
            var worldPos = cam.TransformPoint(viewScaledPos);
            return worldPos;
        }

        private Vector3 TransformVector(Vector3 dir)
        {
            var cam = Camera.main.transform;
            var signVector = new Vector3(Mathf.Sign(UnitsScale.x), Mathf.Sign(UnitsScale.y), Mathf.Sign(UnitsScale.z));
            var dirVector = Vector3.Scale(dir, signVector);

            return cam.TransformDirection(dirVector).normalized;
        }

        private void HandleSkeleton()
        {
            var skeleton = UseSmoothSkeleton ? GesturesManager.Instance.SmoothDefaultSkeleton : GesturesManager.Instance.LatestDefaultSkeleton;
            if (skeleton == null) return;

            var palmForward = TransformVector(skeleton.PalmOrientation);
            var palmUp = TransformVector(skeleton.PalmDirection);

            _palm.transform.position = TransformPoint(skeleton.PalmPosition);
            _palm.transform.LookAt(_palm.transform.position + palmForward, palmUp);

            foreach (var finger in _boxByFinger.Keys)
            {
                var cube = _boxByFinger[finger];

                var pos = TransformPoint(skeleton.FingerPositions[finger]);
                var dir = TransformVector(skeleton.FingerDirections[finger]);

                cube.transform.position = pos;
                cube.transform.LookAt(cube.transform.position + dir, Camera.main.transform.up);
            }
        }

        public void Update()
        {
            if (GesturesManager.Instance == null) return;

            if (GesturesManager.Instance.IsSkeletonRegistered) HandleSkeleton();

            var models = _boxByFinger.Values.Concat(new[] { _palm });
            foreach (var cube in models)
            {
                cube.transform.localScale = CubeScale;
                cube.GetComponent<Renderer>().enabled = IsVisible;
                if (_color != SkeletonColor)
                {
                    cube.GetComponent<Renderer>().material.color = SkeletonColor;
                }
                cube.GetComponent<Collider>().enabled = IsVisible;
            }
            _color = SkeletonColor;
            _palm.transform.localScale = CubeScale * 3;
        }
    } 
}