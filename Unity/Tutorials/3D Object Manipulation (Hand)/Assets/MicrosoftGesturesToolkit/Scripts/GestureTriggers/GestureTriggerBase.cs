using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Gestures.UnitySdk;
using System;

namespace Microsoft.Gestures.Toolkit
{
    public abstract class GestureTriggerBase : ObjectBase
    {
        public bool IsCustomGesture = false;
        public StockGestures Gesture = StockGestures.Tap;
        public string GestureXaml;

        public virtual void OnEnable()
        {
            if (GesturesManager.Instance == null) return;

            if (!IsCustomGesture)
            {
                GesturesManager.Instance.Register(Gesture, OnGesturesManager_GestureReceived);
            }
            else if (!string.IsNullOrEmpty(GestureXaml))
            {
                GesturesManager.Instance.Register(GestureXaml, OnGesturesManager_GestureReceived);
            }
            else
            {
                Debug.LogError("Cannot register gesture. Gesture XAML is invalid.");
            }
        }

        public virtual string GetGestureName()
        {
            var gestureName = "None";

            if (!IsCustomGesture)
            {
                gestureName = Gesture.ToString();
            }
            else if (!string.IsNullOrEmpty(GestureXaml))
            {
                gestureName = GesturesManager.GetGestureName(GestureXaml);
            }

            return gestureName;
        }

        public virtual void OnDisable()
        {
            if (!GesturesManager.Instance) return;

            if (!IsCustomGesture)
            {
                GesturesManager.Instance.Unregister(Gesture, OnGesturesManager_GestureReceived);
            }
            else if (!string.IsNullOrEmpty(GestureXaml))
            {
                GesturesManager.Instance.Unregister(GestureXaml, OnGesturesManager_GestureReceived);
            }
            else
            {
                Debug.LogError("Cannot unregister gesture. Gesture XAML is invalid.");
            }
        }

        protected abstract void OnGesturesManager_GestureReceived(object sender, GestureEventArgs e);
    }
}