using Microsoft.Gestures.UnitySdk;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class CameraManipulation : ObjectBase
    {
        /// <summary>
        /// The look at position is fixed to the world origin
        /// </summary>
        private Vector3 _lookAt = Vector3.zero;

        private bool _isRunning = false;
        private Skeleton _previousSkeleton;

        public Hand Hand = Hand.RightHand;
        public float TumbleSensitivity = 5;
        public float DollySensitivity = 5;

        public bool UseSmoothSkeleton = true;
        public float MinDistanceFromFocus = 1f;
        public float MaxDistanceFromFocus = 5f;

        public float GroundY = float.MinValue;

        public void Update()
        {
            if (GesturesManager.Instance && GesturesManager.Instance.IsSkeletonRegistered) HandleSkeleton();
        }

        private void HandleSkeleton()
        {
            if (!_isRunning) return;

            var skeleton = UseSmoothSkeleton ? GesturesManager.Instance.SmoothDefaultSkeleton : GesturesManager.Instance.LatestDefaultSkeleton;
    
            if (_previousSkeleton != null)
            {
                var deltaPos = skeleton.PalmPosition - _previousSkeleton.PalmPosition;
                Tumble(-deltaPos.x / (750 / TumbleSensitivity), deltaPos.y / (750 / TumbleSensitivity));
                Dolly(deltaPos.z / (900 / DollySensitivity));
            }
            _previousSkeleton = skeleton;
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

            if (!GesturesManager.Instance.IsSkeletonRegistered) GesturesManager.Instance.RegisterToSkeleton();

            if (GesturesManager.Instance.IsSkeletonRegistered)
            {
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