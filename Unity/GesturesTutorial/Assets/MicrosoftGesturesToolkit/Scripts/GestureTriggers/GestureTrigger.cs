using Microsoft.Gestures.UnitySdk;
using System;
using UnityEngine.Events;

namespace Microsoft.Gestures.Toolkit
{
    [Serializable]
    public class GestureTrigger : GestureTriggerBase
    {
        [Serializable]
        public class SegmentTrigger
        {
            public string Segment;
            public UnityEvent OnTrigger = new UnityEvent();
        }

        public UnityEvent OnTrigger = new UnityEvent();
        public SegmentTrigger[] SegmentTriggers = new SegmentTrigger[0];

        protected override void OnGesturesManager_GestureReceived(object sender, GestureEventArgs e)
        {
            // Whole gesture is triggered when there are no segments in the event
            if (e.IsWholeGesture) OnTrigger.Invoke();

            // Fire segment events
            foreach (var trigger in SegmentTriggers)
            {
                if(e.ContainsSegment(trigger.Segment)) trigger.OnTrigger.Invoke();
            }
        }
    }
}
