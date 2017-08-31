using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Gestures.Toolkit
{
    public static class Utils
    {
        public static string nameof(Action action) { return action == null ? "null" : action.Method.Name; }

        public static string nameof<T>(Action<T> action) { return action == null ? "null" : action.Method.Name; }
    }
}
