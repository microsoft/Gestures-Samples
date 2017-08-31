using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public abstract class Singleton<T> : ObjectBase where T : Singleton<T>
    {
        private static T _instrance;

        public static T Instance { get { return _instrance ?? (_instrance = FindObjectOfType<T>()); } }
    }
}