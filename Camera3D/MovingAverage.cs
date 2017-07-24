using Microsoft.Gestures.Skeleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Gestures.Samples.Camera3D
{
    public delegate void AverageChangedHandler(Vector3 delta);

    public class MovingAverage
    {
        const int DefaultWindowSize = 5; // [samples]
        const float DefaultJumpThreshold = 50; // [mm]        

        private readonly int _windowSize;
        private readonly float _jumpThreshold;

        private Queue<Vector3> _window;

        public event AverageChangedHandler AverageChanged;

        public MovingAverage(int windowSize = DefaultWindowSize, float jumpThreshold = DefaultJumpThreshold)
        {            
            _window = new Queue<Vector3>(windowSize);
            _windowSize = windowSize;
            _jumpThreshold = jumpThreshold;
        }

        public void AddNewSample(Vector3 value)
        {
            var previousAverage = CurrentAverage();

            if ((value - previousAverage).TwoNorm() > _jumpThreshold)
            {
                // filter jump - flush the queue and start over averaging
                _window.Clear();
                _window.Enqueue(value);
                return;
            }

            _window.Enqueue(value);            
            if (_window.Count > _windowSize)
            {
                _window.Dequeue();
            }

            var currentAverage = CurrentAverage();

            AverageChanged?.Invoke(currentAverage - previousAverage);
        }

        private Vector3 CurrentAverage() => _window.Any() ? _window.Aggregate((v1, v2) => v1 + v2) * (1f / _window.Count) : new Vector3(0, 0, 0);
    }
}

