using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Gestures.UnitySdk;
using UnityEngine;

namespace Microsoft.Gestures.Toolkit
{
    [RequireComponent(typeof(HandCursor))]
    public class Selection : ObjectBase
    {
        private const string HighlightShaderName = "Outlined/Silhouette Only";
        private const string OutlineParamName = "_Outline";
        private const string OutlineColorParamName = "_OutlineColor";

        private HandCursor _cursor;
        private GameObject _isHovered;
        private GameObject _selectedGameObject;

        private Material _hoveredMaterial;
        private Material _selectedMaterial;

        public bool IsHoverHighlighted = true;
        public Color HoveredColor = Color.white;
        public Color SelectedColor = Color.red;
        public float HighlightWidth = 0.3f;

        public Material OutlineMaterial;
        
        public LayerMask Mask = 31;

        public GameObject IsHovered { get { return _isHovered; } }

        public GameObject SelectedGameObject { get { return _selectedGameObject; } }
        
        public void Start()
        {
            _cursor = GetComponent<HandCursor>();
            _hoveredMaterial = new Material(OutlineMaterial);
            _selectedMaterial = new Material(OutlineMaterial);
        }

        private void Translate(Vector3 delta) { _selectedGameObject.transform.position += delta; }

        private void Update()
        {
            // Update materials' color on every frame
            _selectedMaterial.SetColor(OutlineColorParamName, SelectedColor);
            _selectedMaterial.SetFloat(OutlineParamName, HighlightWidth);

            _hoveredMaterial.SetColor(OutlineColorParamName, HoveredColor);
            _hoveredMaterial.SetFloat(OutlineParamName, HighlightWidth);
            
            // Fire a ray from camera through hand screen location to find the hover object.
            var ray = Camera.main.ScreenPointToRay(_cursor.CursorScreenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, Mask.value))
            {
                Highlight(hit.collider.gameObject);
            }
            else
            {
                UnHighlight();
            }
        }
        

        private void Highlight(GameObject go)
        {
            if (go == _isHovered) return;

            UnHighlight();

            if(go && go.GetComponent<Renderer>())
            {
                _isHovered = go;
                _isHovered.AppendMaterial(_hoveredMaterial);
            }
        }

        private void UnHighlight()
        {
            if (_isHovered == null) return;

            _isHovered.RemoveMaterial(_hoveredMaterial);
            _isHovered = null;
        }

        public void PerformSelection()
        {
            if (_selectedGameObject == _isHovered) return;
            
            if (_selectedGameObject) _selectedGameObject.RemoveMaterial(_selectedMaterial);

            _selectedGameObject = _isHovered;

            if (_selectedGameObject) _selectedGameObject.AppendMaterial(_selectedMaterial);
        }
    }
}