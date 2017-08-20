using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    /// <summary>
    /// Base class for Microsoft.Gestures.Toolkit scripts.
    /// </summary>
    public abstract class ObjectBase : MonoBehaviour
    {
        /// <summary>
        /// Invokes the specified method in time seconds.
        /// </summary>
        public void Invoke(Action method, float time) { Invoke(Utils.nameof(method), time); }

        /// <summary>
        /// Cancels all invoke calls with the same name as the specified action name on this behavior instance.
        /// </summary>
        public void CancelInvoke(Action method) { CancelInvoke(Utils.nameof(method)); }
    }
}