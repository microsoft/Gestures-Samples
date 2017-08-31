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
        private bool _isMute = false;

        private bool _isShowOptions = false;
        private bool _isShowStatus = true;
        private bool _isShowDebug = false;

        public int DebugMaxCharacters = 30000;
        public bool IsAllowShowSkeleton = false;
        public bool IsAllowMute = false;
        public Texture2D StatusTexture;
        public Texture2D StatusBackTexutre;
        public Color StatusBackColor = new Color(0x54 / 255f, 0x54 / 255f, 0x54 / 255f, 0.55f);
        public Color ConnectedStatusColor = new Color(0x55 / 255f, 0x99 / 255f, 0xff / 255f); 
        public Color DisconnectedStatusColor = new Color(0xcc / 255f, 0xcc / 255f, 0xcc / 255f);
        public Color DetectingStatusColor = Color.white;

        public Font StatusFont;
        public Rect OptionsButtonBounds = new Rect(84, 10, 100, 24);
        public Rect OptionsWindowBounds = new Rect(10, 44, 175, 155);

        public bool IsMute { get { return _isMute; } }

        public bool IsShowOptions { get { return _isShowOptions; } }
        public bool ShowRestartButton = false;
        public bool ShowExitButton = false;
        public string RestartButtonText = "Restart";
        public Rect RestartButtonBounds = new Rect(12, 10, 60, 24);

        [HideInInspector]
        public bool IsAppPaused = false;

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
                if(IsAllowMute) _isMute = GUILayout.Toggle(_isMute, "Mute Sound");
                AudioListener.pause = _isMute || IsAppPaused;
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
                var statusColor= DisconnectedStatusColor;
                var gesturesServiceStatus = EndpointStatus.Disconnected.ToString();

                if (GesturesManager.Instance && GesturesManager.Instance.IsConnected)
                {
                    gesturesServiceStatus = GesturesManager.Instance.Status.ToString();
                    statusColor = GesturesManager.Instance.Status == EndpointStatus.Detecting ? DetectingStatusColor : ConnectedStatusColor;
                }

                if (StatusBackTexutre && StatusTexture)
                {
                    var size = 92;
                    var statusBackPadding = 10;
                    var statusBackIconBounds = new Rect(Screen.width - size - statusBackPadding, Screen.height - size - statusBackPadding, size, size);
                    GUI.color = StatusBackColor;
                    GUI.DrawTexture(statusBackIconBounds, StatusBackTexutre);

                    size = 52;
                    var statusPadding  = (statusBackIconBounds.width - size) / 2;
                    var statusIconBounds = new Rect(statusBackIconBounds.x + 0.8f * statusPadding, statusBackIconBounds.y + 0.5f * statusPadding, size, size);
                    GUI.color = statusColor;
                    GUI.DrawTexture(statusIconBounds, StatusTexture);
                    var myStyle = new GUIStyle();
                    GUI.color = Color.white;
                    myStyle.font = StatusFont;
                    myStyle.fontStyle = FontStyle.Bold;
                    myStyle.normal.textColor = Color.white;
                    myStyle.fontSize = 10;
                    myStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(statusBackIconBounds.x, statusBackIconBounds.yMax - 31, statusBackIconBounds.width, 20), gesturesServiceStatus, myStyle);
                }
            }
        }
    } 
}