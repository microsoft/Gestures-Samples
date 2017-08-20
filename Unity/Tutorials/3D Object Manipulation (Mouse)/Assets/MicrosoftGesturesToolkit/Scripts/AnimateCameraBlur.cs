using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Microsoft.Gestures.Toolkit
{
    public class AnimateCameraBlur : ObjectBase
    {
        private bool _isBlured = false;
        private bool _isAnimating;
        private float _blurSizeTarget;
        private float _blurIterationTarget;
        private float _currentBlurIterations;
        private BlurOptimized _blur;

        /// <summary>
        /// A value that controls the speed in which the camera blur is transitioned.
        /// </summary>
        [Tooltip("A value that controls the speed in which the camera blur is transitioned.")]
        public float Speed = 2;

        /// <summary>
        /// Gets a value indicating whether this instance is animating the camera blur post porcess.
        /// </summary>
        public bool IsAnimating { get { return _isAnimating; } }

        private void Start()
        {
            _blur = Camera.main.EnsureComponent<BlurOptimized>();
            _blurSizeTarget = _blur.blurSize;
            _blurIterationTarget = _blur.blurIterations;
            _blur.blurSize = 0;
            _blur.blurIterations = 0;
            _blur.enabled = false;
        }

        private void Update()
        {
            const float ReachedTargetThreshold = 0.01f;

            if (!_isAnimating) return;

            var targetIter = _isBlured ? _blurIterationTarget : 0f;
            var targetSize = _isBlured ? _blurSizeTarget : 0f;

            _blur.blurSize += (targetSize - _blur.blurSize) / Speed;
            _currentBlurIterations += (targetIter - _currentBlurIterations) / Speed;
            _blur.blurIterations = Mathf.RoundToInt(_currentBlurIterations);

            if (Mathf.Abs(_blur.blurSize - targetSize) < ReachedTargetThreshold)
            {
                _blur.blurSize = targetSize;
                _blur.blurIterations = Mathf.RoundToInt(targetIter);
                _isAnimating = false;

                if (!_isBlured) _blur.enabled = false;
            }
        }

        public void SetBlured(bool isBlured)
        {
            _isBlured = isBlured;
            _isAnimating = true;

            if (_blur) _blur.enabled = true;
        }

        public void ApplyBlur() { SetBlured(true); }

        public void RemoveBlur() { SetBlured(false); }        
    }
}