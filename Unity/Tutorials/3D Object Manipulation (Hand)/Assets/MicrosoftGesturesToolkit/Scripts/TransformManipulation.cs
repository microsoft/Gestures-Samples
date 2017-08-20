using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    [RequireComponent(typeof(HandCursor))]
    public class TransformManipulation : ObjectBase
    {
        public enum TransformationMode
        {
            None,
            Translate,
            Rotate,
            Scale,
        }

        private HandCursor _cursor;
        private Selection _selection;

        private TransformationMode _mode = TransformationMode.None;
        private Vector3 _lastCursorWorldPos;
        private Vector3 _initCursorWorldPos;
        private Vector3 _initCursorViewPos;
        private Vector2 _initCursorScreenOffset;
        private bool _initalSelectedIsKinematic;
        private Vector3 _initSelectedWorldPos;
        private Queue<Vector3> _averageVelocity = new Queue<Vector3>();
        private bool _isTranslateCrossedThreshold = false;

        public Vector3 TranslationSensitivity = new Vector3(.3f, .2f, .1f);
        public Vector3 InertiaScale = Vector3.one * 30;
        public float TranslateOnGrabThresholdCm = 0.015f;

        private void Translate()
        {
            if (!_isTranslateCrossedThreshold && (_cursor.CursorWorldPosition - _initCursorWorldPos).magnitude < TranslateOnGrabThresholdCm) return;

            _isTranslateCrossedThreshold = true;
            var delta = _cursor.CursorWorldPosition - _lastCursorWorldPos;
            var z0 = Camera.main.transform.InverseTransformPoint(_initSelectedWorldPos).z;
            var zt = z0 * _cursor.CursorViewportPosition.z / _initCursorViewPos.z;
            var rayOffseted = Camera.main.ScreenPointToRay(_cursor.CursorScreenPosition + _initCursorScreenOffset);
            var newPos = rayOffseted.GetPoint(zt);
            var veolcity = newPos - _selection.SelectedGameObject.transform.position;
            _selection.SelectedGameObject.transform.position = newPos;
            if (_averageVelocity != null)
            {
                _averageVelocity.Enqueue(veolcity);
                if (_averageVelocity.Count > 4)
                    _averageVelocity.Dequeue();
            }
        }

        private void Rotate() { }

        private void Scale() { }

        public void Start()
        {
            _cursor = GetComponent<HandCursor>();
            _selection = GetComponent<Selection>();
        }

        public void FixedUpdate()
        {
            if (_selection.SelectedGameObject == null) _mode = TransformationMode.None;

            switch (_mode)
            {
                case TransformationMode.Translate:
                    Translate();
                    break;
                case TransformationMode.Rotate:
                    Rotate();
                    break;
                case TransformationMode.Scale:
                    Scale();
                    break;
            }

            _lastCursorWorldPos = _cursor.CursorWorldPosition;
        }

        public void StartTranslation() { StartTransform(TransformationMode.Translate); }

        public void StartRotate() { StartTransform(TransformationMode.Rotate); }

        public void StartScale() { StartTransform(TransformationMode.Scale); }

        public void StartTransform(TransformationMode mode)
        {
            _selection.PerformSelection();

            if (!_selection.SelectedGameObject) return;

            _mode = mode;
            _isTranslateCrossedThreshold = false;
            _initCursorWorldPos = _cursor.CursorWorldPosition;
            _initCursorViewPos = _cursor.CursorViewportPosition;
            _initSelectedWorldPos = _selection.SelectedGameObject.transform.position;
            _initCursorScreenOffset = (Vector2)Camera.main.WorldToScreenPoint(_initSelectedWorldPos) - _cursor.CursorScreenPosition;

            var rb = _selection.SelectedGameObject.GetComponent<Rigidbody>();

            if (rb)
            {
                _initalSelectedIsKinematic = rb.isKinematic;
                rb.isKinematic = true;
            }
        }

        public void StopTransform()
        {
            if (_mode == TransformationMode.None) return;

            _mode = TransformationMode.None;

            if (_selection.SelectedGameObject && _selection.SelectedGameObject.GetComponent<Rigidbody>())
            {
                var rb = _selection.SelectedGameObject.GetComponent<Rigidbody>();
                rb.isKinematic = _initalSelectedIsKinematic;
                if (!_initalSelectedIsKinematic && _averageVelocity.Count > 2)
                {
                    var avg = Vector3.zero;
                    var count = _averageVelocity.Count;
                    while (_averageVelocity.Count > 0)
                    {
                        avg += _averageVelocity.Dequeue();
                    }
                    var velocity = Vector3.Scale(InertiaScale, avg / count * 60);
                    rb.AddForce(velocity);
                    Debug.Log("Set velocity to :" + velocity);
                }
            }

            if (_averageVelocity != null) _averageVelocity.Clear();
        }
    }
}