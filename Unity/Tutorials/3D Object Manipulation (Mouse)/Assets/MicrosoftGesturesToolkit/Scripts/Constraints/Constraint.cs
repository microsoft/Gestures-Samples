using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    public class Constraint : ObjectBase
    {
        public Transform Source;
        public bool MaintainOffset;
        public Vector3 Offset = Vector3.zero;

        public bool ConstainX;
        public bool ConstainY;
        public bool ConstainZ;
    }
}
