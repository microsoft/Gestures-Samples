using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class HideWhenPaused : ObjectBase
    {
        [Tooltip("An array of game object that will be deactivated when the application is in pause mode.")]
        public GameObject[] ObjectsToHide;

        private void OnEnable() { AppPaused.Instance.IsPausedChanged.AddListener(OnIsPaused_Changed); }

        private void OnDisable() { AppPaused.Instance.IsPausedChanged.RemoveListener(OnIsPaused_Changed); }

        public void OnIsPaused_Changed() { foreach (var go in ObjectsToHide) go.SetActive(!AppPaused.Instance.IsPaused); }
    }
}