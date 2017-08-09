using Microsoft.Gestures.UnitySdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class HandCursor : Singleton<HandCursor>
    {
        private ParticleSystem _trail;

        [Tooltip("Choose which hand controls the cursor.")]
        public Hand Hand = Hand.RightHand;

        [Tooltip("The cursor image that would be displayed on screen.")]
        public Texture2D CursorIcon;

        [Tooltip("The size in pixels of the cursor icon.")]
        public Vector2 CursorSize = 50 * Vector2.one;

        [Tooltip("Check this for a more smooth hand cursor motion.")]
        public bool UseStabalizer = false;

        public Vector3 UnitsScale = new Vector3(.1f, .1f, -.1f);

        public Vector3 UnitsOffset = new Vector3(0f, 0f, 55f);

        [Tooltip("Check to view the hand cursor.")]
        public bool ShowIcon = true;

        public Vector2 CursorScreenPosition { get; private set; }
        
        public Vector3 CursorViewportPosition { get; private set; }

        public Vector3 CursorWorldPosition { get; private set; }
        
        public ParticleSystem Trail;

        public float TrailZ = 1f;

        private void OnEnable()
        {
            if(GesturesManager.Instance == null)
            {
                Debug.LogError("Gestures Manager does not exist. Hand cursor cannot be displayed.");
                enabled = false;
                return;
            }

            if (!GesturesManager.Instance.IsSkeletonRegistered) GesturesManager.Instance.RegisterToSkeleton();

            CursorScreenPosition = -100 * Vector3.one;//set to off-screen

            if (!_trail) _trail = Instantiate(Trail);
        }
        
        private void Update()
        {
            var skeleton = UseStabalizer ? GesturesManager.Instance.StableSkeletons[Hand]: GesturesManager.Instance.Skeletons[Hand];

            if (skeleton == null) return;

            var pos = Vector3.Scale(skeleton.PalmPosition, UnitsScale) + UnitsOffset;
            var worldPos = Camera.main.transform.TransformPoint(pos);
            CursorWorldPosition = worldPos;
            CursorViewportPosition = pos;
            CursorScreenPosition = Camera.main.WorldToScreenPoint(worldPos);

            if (_trail) _trail.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(CursorScreenPosition.x, CursorScreenPosition.y, TrailZ));
        }

        private void OnGUI()
        {
            if (!ShowIcon) return;

            var pos = CursorScreenPosition;
            pos.y = Screen.height - pos.y;
            var bounds = new Rect(pos - 0.5f * CursorSize, CursorSize);
            GUI.DrawTexture(bounds, CursorIcon);
        }
    }
}