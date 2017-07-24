using Microsoft.Gestures.Skeleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Camera3D
{
    public class SphericalCamera
    {
        const float TumbleCoefficient = 1 / 100f;
        const float DolllyCoefficient = 1 / 8f;

        private Dispatcher _dispatcher;
        private PerspectiveCamera _camera;

        private double _r; 
        private double _horizontalAngle; // [radians], _horizontalAngle = 0 ↔ positive x-axis, _horizontalAngle = π ↔ negative x-axis
        private double _verticalAngle;   // [radians], _verticalAngle = 0 ↔ positive z-axis, _verticalAngle = π ↔ negative x-axis

        public SphericalCamera(PerspectiveCamera camera, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _camera = camera;

            var p = (Vector3D)camera.Position;
            _r = p.Length;
            _horizontalAngle = Math.Atan2(p.Y, p.X);
            _verticalAngle = Math.Atan2(Math.Sqrt(p.X * p.X + p.Y * p.Y), p.Z);
        }

        public void UpdateCamera(Vector3 diff)
        {
            Tumble(diff.X, -diff.Y);
            Dolly(diff.Z);
            UpdateCamera();
        }

        private void Tumble(float dX, float dY)
        {
            _horizontalAngle += dX * TumbleCoefficient;
            _verticalAngle += dY * TumbleCoefficient;
        }

        private void Dolly(float dZ)
        {
            _r -= dZ * DolllyCoefficient;
        }

        private void UpdateCamera()
        {
            _dispatcher.InvokeAsync(() =>
            {
                var cartesian = ToCartesian();
                _camera.Position = (Point3D)cartesian;
                _camera.LookDirection = -cartesian;
            });
        }

        private Vector3D ToCartesian()
        {
            var sineOfVertical = Math.Sin(_verticalAngle);
            return new Vector3D(_r * sineOfVertical * Math.Cos(_horizontalAngle), _r * sineOfVertical * Math.Sin(_horizontalAngle), _r * Math.Cos(_verticalAngle));
        }
    }
}
