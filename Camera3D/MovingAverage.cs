using Microsoft.Gestures.Skeleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera3D
{
    public class MovingAverage
    {
        private Queue<Vector3> _window;
        private int _windowSize;
        private Vector3 _currentSum = new Vector3(0, 0, 0);

        public MovingAverage(int windowSize)
        {            
            _window = new Queue<Vector3>(windowSize);
            _windowSize = windowSize;
        }

        public void AddNewSample(Vector3 value)
        {
            _window.Enqueue(value);
            _currentSum += value;
            if (_window.Count > _windowSize)
            {
                _currentSum -= _window.Dequeue();
            }
        }

        public Vector3 CurrentAverage => _currentSum * (1 / (float)Math.Max(_window.Count, 1));
    }
}

