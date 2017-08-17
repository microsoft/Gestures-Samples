using UnityEngine;
using Microsoft.Gestures.UnitySdk;
using Microsoft.Gestures.Toolkit;

public class MouseCursor : MonoBehaviour
{
    private GameObject _hoveredGameObject;
    private bool _isGrabbing = false;

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
        // Step 1.5: Return mouse screen position.
        return Input.mousePosition;
    }

    private GameObject GetHoveredObject()
    {
        // Step 2.2 Cast a ray from camera towards the cursor.
        var cursorPosition = GetCursorScreenPosition();
        var ray = Camera.main.ScreenPointToRay(cursorPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance: 1000, layerMask: Mask.value))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private float GetCursorDepthDelta()
    {
        // Step 3.6: return mouse scroll delta
        return Input.mouseScrollDelta.y / 10;
    }

    public void StartGrab()
    {
        // Step 3.3:   Begin grab mode. 
        if (!_hoveredGameObject)
        {
            return;
        }

        _isGrabbing = true;
    }

    public void StopGrab()
    {
        // Step 3.3:   Stop grab mode.
        _isGrabbing = false;
    }

    private void Update()
    {
        // Step 2.2: Add highlight material to hovered object
        // Step 3.4: Do not change hovered object when grabbing
        if (HighlightMaterial && !_isGrabbing)
        {
            // Stop highlighting old hover object
            if (_hoveredGameObject)
                _hoveredGameObject.RemoveMaterial(HighlightMaterial);

            // Raycast and find object under cursor
            _hoveredGameObject = GetHoveredObject();

            // Add highlight material to hovered object
            if (_hoveredGameObject)
                _hoveredGameObject.AppendMaterial(HighlightMaterial);
        }

        // Start grabbing object when left mouse button is down
        if (Input.GetMouseButtonDown(0))
            StartGrab();

        // Stop grabbing object when left mouse button is up
        if (Input.GetMouseButtonUp(0))
            StopGrab();

        // Handle motion
        if (_isGrabbing)
        {
            var plane = new Plane(Camera.main.transform.forward, _hoveredGameObject.transform.position);
            var ray = Camera.main.ScreenPointToRay(GetCursorScreenPosition());
            float distanceFromCamera;
            plane.Raycast(ray, out distanceFromCamera);
            // Step 3.7: scale depth according to cursor's depth delta
            distanceFromCamera *= 1 + GetCursorDepthDelta();
            _hoveredGameObject.transform.position = ray.GetPoint(distanceFromCamera);
        }
    }

    private void OnGUI()
    {
        // Step 1.4: Draw cursor texture at the cursor's position on the screen.
        var cursorPosition = (Vector2)GetCursorScreenPosition();

        // Invert y direction
        cursorPosition.y = Screen.height - cursorPosition.y;

        // prepare bounds for cursor texture
        var bounds = new Rect(cursorPosition - 0.5f * CursorSize, CursorSize);

        // Change the tint color to match our cursor mode
        var originalColor = GUI.color;
        // Step 3.2: Add a condition when setting GUI.color
        GUI.color = _isGrabbing ? GrabCursorTint : CursorTint;
        GUI.DrawTexture(bounds, CursorImage);

        GUI.color = originalColor;
    }
}