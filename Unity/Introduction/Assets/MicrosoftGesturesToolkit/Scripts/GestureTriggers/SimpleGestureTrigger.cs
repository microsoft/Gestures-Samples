using Microsoft.Gestures.UnitySdk;

namespace Microsoft.Gestures.Toolkit
{
    public abstract class SimpleGestureTrigger : GestureTriggerBase
    {
        public bool UseGestureSegment = false;
        public string GestureSegment = string.Empty;

        protected abstract void OnGestureDetected(GestureEventArgs e);

        protected override void OnGesturesManager_GestureReceived(object sender, GestureEventArgs e)
        {
            if (UseGestureSegment)
            {
                if(e.ContainsSegment(GestureSegment)) OnGestureDetected(e);
            }
            else if (e.GestureSegments.Length == 0)
            {
                OnGestureDetected(e);
            }
        }
    } 
}