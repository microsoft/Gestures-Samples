using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.ImageEffects;

namespace Microsoft.Gestures.Toolkit
{
    public class AppPaused : Singleton<AppPaused>
    {
        private bool _isPaused = false;
        private AnimateCameraBlur _blur;
        private float _focusStartTime;

        /// <summary>
        /// Occurs when the IsPaaused property has changed.
        /// </summary>
        public UnityEvent IsPausedChanged = new UnityEvent();

        public Vector2 LabelSize = 100 * Vector2.one;
        public string PauseText = "Game Paused";
        public GUIStyle TextStyle = new GUIStyle();
        public bool MuteSoundWhenPaused = true;

        /// <summary>
        /// Gets a value indicating whether the application is paused.
        /// </summary>
        public bool IsPaused { get { return _isPaused; } }

        /// <summary>
        /// Gets the game time when the application lastly regain focus.
        /// If the application is not in focus, FocusStartTime will return -1.
        /// </summary>
        public float FocusStartTime { get { return _focusStartTime; } }

        /// <summary>
        /// Gets the game time elapsed time from when the application lastly regain focus.
        /// If the application is not in focus return -1.
        /// </summary>
        public float FocusDuration { get { return _isPaused  ? -1 : Time.time - _focusStartTime; } }

        private void Start()
        {
            Application.runInBackground = true;
            _blur = Camera.main.EnsureComponent<AnimateCameraBlur>();
        }

        private void Update()
        {
            if (_isPaused && !_blur.IsAnimating && Application.runInBackground) Application.runInBackground = false;
        }

        private void OnGUI()
        {
            if (!_isPaused) return;

            var pos = new Vector2(Screen.width, Screen.height) - LabelSize;                
            GUI.Label(new Rect(pos / 2, LabelSize), PauseText, TextStyle);
        }

        private void SetPause(bool isPaused)
        {
            _isPaused = isPaused;

            if (_blur) _blur.SetBlured(isPaused);

            if (MuteSoundWhenPaused && AudioListener.pause != _isPaused)
            {
                AudioListener.pause = _isPaused || (UIManager.Instance != null && UIManager.Instance.IsMute);
            }

            IsPausedChanged.Invoke();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _focusStartTime = hasFocus? Time.time : -1;
            SetPause(!hasFocus);
            Application.runInBackground = true;
        }

        private void OnApplicationPause(bool pauseStatus) { SetPause(pauseStatus); }
    } 
}