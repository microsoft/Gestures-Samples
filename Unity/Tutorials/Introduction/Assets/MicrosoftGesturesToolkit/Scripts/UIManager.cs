using Microsoft.Gestures.UnitySdk;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.Gestures.Toolkit
{
    public sealed class UIManager : Singleton<UIManager>
    {
        public UnityEvent OnResetButtonClick = new UnityEvent();

        private SkeletonVisualizer _skeletonVisualizer;
        private string _currentText = string.Empty;
        private string _prevMessage;
        private bool _isMute = false;

        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isShowOptions = false;
        private bool _isShowStatus = true;
        private bool _isShowDebug = false;

        public int DebugMaxCharacters = 30000;
        public bool IsAllowShowSkeleton = false;
        public Texture2D ConnectedTexture;
        public Texture2D DisconnectedTexture;
        public float IconSize = 30f;
        public Rect OptionsButtonBounds = new Rect(84, 10, 100, 24);
        public Rect OptionsWindowBounds = new Rect(10, 44, 174, 300);

        public bool IsMute { get { return _isMute; } }

        public bool IsShowOptions { get { return _isShowOptions; } }
        public bool ShowRestartButton = true;
        public bool ShowExitButton = true;
        public string RestartButtonText = "Restart";
        public Rect RestartButtonBounds = new Rect(10, 40, 20, 50);

        // Use this for initialization
        private void Awake()
        {
            _currentText = string.Empty;
            Application.logMessageReceived += LogMessage;
        }

        private void Start()
        {
            _skeletonVisualizer = GameObject.FindObjectOfType<SkeletonVisualizer>();
        }

        private void OnDestroy() { Application.logMessageReceived -= LogMessage; }

        public void LogMessage(string message, string stackTrace, LogType type)
        {
            if (message == null) return;

            _currentText = string.Format("{0:HH:mm:ss} {1}:\t{2}\n{3}", DateTime.Now, type, message, _currentText);
            if (_currentText.Length > DebugMaxCharacters) _currentText = _currentText.Substring(0, DebugMaxCharacters);
        }

        public bool IsMouseOverOptions()
        {
            // Reversing the y component of the mouse position to fit GUI coordinates which has the (0,0) match the top left corner 
            // while mouse positions (0,0) is bottom left corner
            var topLeftMousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            return OptionsButtonBounds.Contains(topLeftMousePosition) || // options button
                   OptionsWindowBounds.Contains(topLeftMousePosition) || // options dialog
                   RestartButtonBounds.Contains(topLeftMousePosition);   // reset button
        }

        private void OnGUI()
        {
            var size = new Vector2(100, 24);
            var padding = new Vector2(10, 10);
            var screenSize = new Vector2(Screen.width, Screen.height);
            if (ShowRestartButton && GUI.Button(RestartButtonBounds, RestartButtonText)) OnResetButtonClick.Invoke();

            if (GUI.Button(OptionsButtonBounds, "Options")) _isShowOptions = !_isShowOptions;

            var exitButtonBounds = new Rect(Screen.width - 90, 10, 80, 24);
            if (ShowExitButton && GUI.Button(exitButtonBounds, "Close")) Application.Quit();

            if (_isShowOptions)
            {
                GUI.BeginGroup(OptionsWindowBounds);
                GUI.Box(new Rect(Vector2.zero, OptionsWindowBounds.size), string.Empty);
                GUI.BeginGroup(new Rect(padding, OptionsWindowBounds.size - 2 * padding));

                _isShowStatus = GUILayout.Toggle(_isShowStatus, "Show Status");


                if (IsAllowShowSkeleton && _skeletonVisualizer)
                {
                    _skeletonVisualizer.IsVisible = GUILayout.Toggle(_skeletonVisualizer.IsVisible, "Show Skeleton");
                }

                _isShowDebug = GUILayout.Toggle(_isShowDebug, "Show Debug");
                _isMute = GUILayout.Toggle(_isMute, "Mute Sound");
                AudioListener.pause = _isMute || (AppPaused.Instance && AppPaused.Instance.IsPaused);
                GUILayout.Label("Gestures:");

                var gestures = GameObject.FindObjectsOfType<GestureTrigger>().OrderBy(g => g.GetGestureName());
                var gesturesGrouped = gestures.GroupBy(g => g.GetGestureName(), g => g);
                foreach (var gestureGroup in gesturesGrouped)
                {
                    var gestureText = Regex.Replace(gestureGroup.Key.Replace("Gesture", string.Empty), "(\\B[A-Z])", " $1");
                    var enabled = GUILayout.Toggle(gestureGroup.First().enabled, gestureText);
                    foreach (var gesture in gestureGroup) gesture.enabled = enabled;
                }
                
                GUI.EndGroup();
                GUI.EndGroup();
            }

            if (_isShowDebug)
            {
                var debugPos = new Vector2(OptionsWindowBounds.xMax + padding.x, OptionsButtonBounds.yMin);
                var rec = new Rect(debugPos, screenSize - debugPos - padding - new Vector2(padding.x, 0));
                GUI.TextArea(rec, _currentText);
            }

            if(_isShowStatus)
            {
                var isConnected = GesturesManager.Instance && GesturesManager.Instance.IsConnected;
                var statusTexture = isConnected ? ConnectedTexture : DisconnectedTexture;

                if (statusTexture)
                {
                    var w = IconSize;
                    var h = statusTexture.height / statusTexture.width * w;
                    GUI.DrawTexture(new Rect(Screen.width - 116 - w, Screen.height - 10 - h, w, h), statusTexture);
                    GUI.color = Color.black;
                    GUI.Label(new Rect(Screen.width - 110, Screen.height - 6 - h, 100, h), isConnected ? "Connected" : "Disconnected");
                }
            }
        }
    } 
}