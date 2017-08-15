using UnityEngine;
using Microsoft.Gestures.UnitySdk;
using Microsoft.Gestures.Toolkit;

public class HandCursor : MonoBehaviour
{
    private bool _isGrabbing = false;
    private GameObject _hoveredGameObject;
    private float _lastPalmDepth;

    [Tooltip("When true, cursor will track the mouse position. When false, cursor will track the palm position.")]
    public bool IsMouseMode = true;

    [Tooltip("The cursor image that will be displayed on the screen.")]
    public Texture2D CursorImage;

    [Tooltip("The size in pixels of the cursor icon.")]
    public Vector2 CursorSize = 24 * Vector2.one;

    [Tooltip("The color of the cursor in normal mode.")]
    public Color CursorTint = Color.red;

    [Tooltip("Scales the palm position vector to camera space.")]
    public Vector3 PalmUnitsScale = new Vector3(-.1f, .1f, -.1f);

    [Tooltip("Offsets the palm position vector in camera space.")]
    public Vector3 PalmUnitsOffset = new Vector3(0f, 0f, 55f);

    [Tooltip("Material used to highlight hovered game objects.")]
    public Material HighlightMaterial;

    [Tooltip("A layer mask to filter hover-able game objects.")]
    public LayerMask Mask = -1; // -1 means "mask all layers in the scene"

    [Tooltip("The color of the cursor in grab mode.")]
    public Color GrabCursorTint = Color.green;

    private Vector3 GetPalmCameraPosition()
    {
        // Step 1.9: Convert palm position from depth-camera space to Main-Camera space
        var skeleton = GesturesManager.Instance.StableSkeletons[Hand.RightHand];
        if (skeleton == null)
        {
            return Vector3.zero;
        }
        return Vector3.Scale(skeleton.PalmPosition, PalmUnitsScale) + PalmUnitsOffset;
    }

    private Vector3 GetCursorScreenPosition()
    {
        if (IsMouseMode)
        {
            // Step 1.5: Return mouse screen position.
            return Input.mousePosition;
        }
        else
        {
            // Step 1.9: Replace mouse position with palm position.
            var palmCameraPosition = GetPalmCameraPosition();
            var palmWorldPosition = Camera.main.transform.TransformPoint(palmCameraPosition);
            var palmScreenPosition = (Vector2)Camera.main.WorldToScreenPoint(palmWorldPosition);
            return palmScreenPosition;
        }
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
        float delta;
        if (IsMouseMode)
        {
            // Step 5.1: return mouse scroll delta
            delta = Input.mouseScrollDelta.y / 10;
        }
        else
        {
            // Step 5.1: return palm depth delta
            var currentDepth = GetPalmCameraPosition().z;
            delta = (currentDepth - _lastPalmDepth) / 10;
            _lastPalmDepth = currentDepth;
        }

        return Mathf.Max(Mathf.Min(delta, 1), -1);
    }

    public void StartGrab()
    {
        // Step 3.3:   Begin grab mode. 
        if (!_hoveredGameObject)
        {
            return;
        }

        _isGrabbing = true;
        // step 5.1: save last value of hand depth
        _lastPalmDepth = GetPalmCameraPosition().z;
    }

    public void StopGrab()
    {
        // Step 3.3:   Stop grab mode.
        _isGrabbing = false;
    }

    private void OnEnable()
    {
        // Step 1.8: Register to skeleton events
        GesturesManager.Instance.RegisterToSkeleton();
    }

    private void OnDisable()
    {
        // Step 1.8: Unregister from skeleton events
        GesturesManager.Instance.UnregisterFromSkeleton();
    }

    private void Update()
    {
        // Step 2.2: Add highlight material to hovered object
        // Step 3:   Do not change hover object when grabbing
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

        // Step 3: Handle Grabbing
        if (IsMouseMode)
        {
            // Start grabbing object when left mouse button is down
            if (Input.GetMouseButtonDown(0))
                StartGrab();

            // Stop grabbing object when left mouse button is up
            if (Input.GetMouseButtonUp(0))
                StopGrab();
        }

        if (_isGrabbing)
        {
            var plane = new Plane(Camera.main.transform.forward, _hoveredGameObject.transform.position);
            var ray = Camera.main.ScreenPointToRay(GetCursorScreenPosition());
            float distanceFromCamera;
            plane.Raycast(ray, out distanceFromCamera);
            // Step 5.1: scale depth according to cursor's depth delta
            distanceFromCamera *= 1 + GetCursorDepthDelta();
            _hoveredGameObject.transform.position = ray.GetPoint(distanceFromCamera);
        }
    }

    private void OnGUI()
    {
        // Step 1.5: Draw cursor texture at the cursor's position on the screen.
        var cursorPosition = (Vector2)GetCursorScreenPosition();

        // Invert y direction
        cursorPosition.y = Screen.height - cursorPosition.y;

        // prepare bounds for cursor texture
        var bounds = new Rect(cursorPosition - 0.5f * CursorSize, CursorSize);

        // Change the tint color to match our cursor mode
        var originalColor = GUI.color;

        // Step 3. Change cursor color when grab mode is on.
        // Add a condition when setting GUI.color
        GUI.color = _isGrabbing ? GrabCursorTint : CursorTint;
        GUI.DrawTexture(bounds, CursorImage);
        GUI.color = originalColor;
    }
}