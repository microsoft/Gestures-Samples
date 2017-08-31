using UnityEngine;
using Microsoft.Gestures.UnitySdk;
using Microsoft.Gestures.Toolkit;

public class Cursor : MonoBehaviour
{
    private GameObject _hoveredGameObject;
    private bool _isGrabbing = false;
    private float _lastObjectDistance;

    [Tooltip("The cursor image that will be displayed on the screen.")]
    public Texture2D CursorImage;

    [Tooltip("The size in pixels of the cursor icon.")]
    public Vector2 CursorSize = 24 * Vector2.one;

    [Tooltip("The color of the cursor in normal mode.")]
    public Color CursorTint = Color.red;

    [Tooltip("Material used to highlight hovered game objects.")]
    public Material HighlightMaterial;

    [Tooltip("A layer mask to filter hover-able game objects.")]
    public LayerMask Mask = -1; // -1 means "mask all layers in the scene"

    [Tooltip("The color of the cursor in grab mode.")]
    public Color GrabCursorTint = Color.green;

    private Vector3 GetCursorScreenPosition()
    {
        // Return mouse screen position.
        return Input.mousePosition;
    }

    private GameObject GetHoveredObject()
    {
        // Cast a ray from the camera towards the cursor.
        var cursorPosition = GetCursorScreenPosition();
        var ray = Camera.main.ScreenPointToRay(cursorPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance: 1000, layerMask: Mask.value))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private float GetCursorDistanceScalingFactor()
    {
        return 1 + Input.mouseScrollDelta.y / 10;
    }

    public void StartGrab()
    {
        // Start grab mode. 
        if (!_hoveredGameObject)
        {
            return;
        }

        _isGrabbing = true;
        _lastObjectDistance = Vector3.Distance(Camera.main.transform.position, _hoveredGameObject.transform.position);
    }

    public void StopGrab()
    {
        // Stop grab mode.
        _isGrabbing = false;
    }

    private void Update()
    {
        // Add highlighting material to hovered object
        // Do not change hovered object when grabbing
        if (HighlightMaterial && !_isGrabbing)
        {
            // Stop highlighting old hover object
            if (_hoveredGameObject)
            {
                _hoveredGameObject.RemoveMaterial(HighlightMaterial);
            }

            // Raycast and find object under cursor
            _hoveredGameObject = GetHoveredObject();

            // Add highlighting material to hovered object
            if (_hoveredGameObject)
            {
                _hoveredGameObject.AppendMaterial(HighlightMaterial);
            }
        }

        // Start grabbing object when left mouse button is down
        if (Input.GetMouseButtonDown(0))
        {
            StartGrab();
        }

        // Stop grabbing object when left mouse button is up
        if (Input.GetMouseButtonUp(0))
        {
            StopGrab();
        }

        // Handle motion
        if (_isGrabbing)
        {
            var ray = Camera.main.ScreenPointToRay(GetCursorScreenPosition());
            _lastObjectDistance *= GetCursorDistanceScalingFactor();
            _hoveredGameObject.transform.position = ray.GetPoint(_lastObjectDistance);
        }
    }

    private void OnGUI()
    {
        // Draw cursor texture at the cursor's position on the screen.
        var cursorPosition = (Vector2)GetCursorScreenPosition();

        // Invert y direction
        cursorPosition.y = Screen.height - cursorPosition.y;

        // prepare bounds for cursor texture
        var bounds = new Rect(cursorPosition - 0.5f * CursorSize, CursorSize);

        // Change the tint color to match our cursor mode
        var originalColor = GUI.color;
        // Add a condition when setting GUI.color
        GUI.color = _isGrabbing ? GrabCursorTint : CursorTint;
        GUI.DrawTexture(bounds, CursorImage);

        GUI.color = originalColor;
    }
}