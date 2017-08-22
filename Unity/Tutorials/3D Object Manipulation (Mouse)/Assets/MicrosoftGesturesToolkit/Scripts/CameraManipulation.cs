using Microsoft.Gestures.UnitySdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Microsoft.Gestures.Toolkit
{
    public class CameraManipulation : ObjectBase
    {
        private Vector3 _lookAt;
        private bool _isRunning = false;
        private Skeleton _previousSkeleton;

        public Hand Hand = Hand.RightHand;
        public float TumbleSensitivity = 5;
        public float DollySensitivity = 5;

        public bool UseStabilizer = true;
        public float MinDistanceFromFocus = 1f;
        public float MaxDistanceFromFocus = 5f;

        public float GroundY = float.MinValue;

        private void RegisterToSkeleton()
        {
            if (GesturesManager.Instance && !GesturesManager.Instance.IsSkeletonRegistered)
            {
                GesturesManager.Instance.RegisterToSkeleton();
            }
        }


        public void Update() { RegisterToSkeleton(); }

        private void OnGesturesManager_SkeletonReady(object sender, SkeletonEventArgs e)
        {
            var manager = GesturesManager.Instance;
    
            if (_previousSkeleton != null)
            {
                var deltaPos = e.Skeleton.PalmPosition - _previousSkeleton.PalmPosition;
                Tumble(deltaPos.x / (750 / TumbleSensitivity), deltaPos.y / (750 / TumbleSensitivity));
                Dolly(deltaPos.z / (900 / DollySensitivity));
            }
            _previousSkeleton = e.Skeleton;
        }

        public void StartManipulating()
        {
            if(_isRunning)
            {
                Debug.LogWarning("Camera manipulation is already running.");
                return;
            }

            if (!GesturesManager.Instance)
            {
                Debug.LogError("Couldn't start camera manipulation. " + typeof(GesturesManager).Name + " is null!");
                return;
            }

            RegisterToSkeleton();

            if (GesturesManager.Instance.IsSkeletonRegistered)
            {
                GesturesManager.Instance.SkeletonReady += OnGesturesManager_SkeletonReady;
                _previousSkeleton = GesturesManager.Instance.SmoothDefaultSkeleton;
                _isRunning = true;
                Debug.Log("Gesture camera manipulation started successfully.");
            }
            else
            {
                Debug.LogError("Failed to StartManipulating. Skeleton is not registered.");
            }
        }

        public void StopManipulating()
        {
            if (!_isRunning) return;

            GesturesManager.Instance.SkeletonReady -= OnGesturesManager_SkeletonReady;
            _isRunning = false;
            Debug.Log("Gesture camera manipulation stopped successfully.");
        }
    
        /// <summary>
        /// Rotate the camera along the world Y axis and camera's left axis using the deltaX and deltaY, respectively.
        /// </summary>
        /// <param name="deltaX">The amount to rotate the camera along the world y axis.</param>
        /// <param name="deltaY">The amount to rotate the camera along the camera's left axis.</param>
        public void Tumble(float deltaX, float deltaY)
        {
            var cam = Camera.main;
            var norm = -cam.transform.right;

            var ry = Quaternion.Euler(0, deltaX, 0);
            var rAxis = Quaternion.AngleAxis(deltaY, norm);
            var final = Matrix4x4.TRS(Vector3.zero, rAxis, Vector3.one) * Matrix4x4.TRS(Vector3.zero, ry, Vector3.one);

            var up = cam.transform.up;
            var eye = cam.transform.position - _lookAt;
            up = ry * rAxis * up;
            eye = ry * rAxis * eye + _lookAt;

            if (eye.y >= GroundY)
            {
                cam.transform.up = up.normalized;
                cam.transform.forward = (_lookAt - eye).normalized;
                cam.transform.position = eye;
            }
        }

        /// <summary>
        /// Dollies/Moves the camera in the look at direction using the specified amount value.
        /// </summary>
        public void Dolly(float amount)
        {
            var cam = Camera.main;
            var delta = (amount) * 2 * cam.transform.forward;

            var pos = cam.transform.position + delta;
            var length = (pos - _lookAt).magnitude;
            if (MinDistanceFromFocus <= length && length <= MaxDistanceFromFocus && pos.y >= GroundY)
            {
                cam.transform.position = pos;
            }
        }
    }
}