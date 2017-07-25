using Microsoft.Gestures.Skeleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Gestures.Samples.Camera3D
{
    public class SmoothedPositionChangeEventArgs : EventArgs
    {
        /// <summary> Position of palm after jitter was smoothed. </summary>
        public Vector3 SmoothedPosition { get; set; }
        /// <summary> Difference between current smoothed position and previous one. </summary>
        public Vector3 SmoothedPositionDelta { get; set; }
    }

    public class PalmSmoother
    {
        const int DefaultWindowSize = 5; // [samples]
        const float DefaultJumpThreshold = 50; // [mm]        

        private readonly int _windowSize;
        private readonly float _jumpThreshold;

        private Queue<Vector3> _window;
        private Vector3 CurrentAverage => _window.Any() ? _window.Aggregate((v1, v2) => v1 + v2) * (1f / _window.Count) : new Vector3(0, 0, 0);

        public event EventHandler<SmoothedPositionChangeEventArgs> SmoothedPositionChanged;

        public PalmSmoother(int windowSize = DefaultWindowSize, float jumpThreshold = DefaultJumpThreshold)
        {            
            _window = new Queue<Vector3>(windowSize);
            _windowSize = windowSize;
            _jumpThreshold = jumpThreshold;
        }

        public void Smooth(IHandSkeleton skeleton)
        {
            var palmPosition = skeleton.PalmPosition;
            var previousAverage = CurrentAverage;

            if ((palmPosition - previousAverage).TwoNorm() > _jumpThreshold)
            {
                // filter jump - flush the queue and start over averaging
                _window.Clear();
                _window.Enqueue(palmPosition);
                return;
            }

            _window.Enqueue(palmPosition);            
            if (_window.Count > _windowSize)
            {
                _window.Dequeue();
            }

            var currentAverage = CurrentAverage;

            SmoothedPositionChanged?.Invoke(this, new SmoothedPositionChangeEventArgs() { SmoothedPosition = currentAverage,
                                                                                          SmoothedPositionDelta = currentAverage - previousAverage } );
        }

    }
}

