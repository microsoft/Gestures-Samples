using UnityEngine;
using Microsoft.Gestures.UnitySdk;
using Microsoft.Gestures.Toolkit;

public class HandCursor : MonoBehaviour
{
    private GameObject _hoveredGO;
    private bool _isGrabbing = false;
    private Vector3 _lastCursorWorldPos;
    private float _initDistanceFromCamera;

    [Tooltip("Set this to true if you wish to use the mouse input instead of the hand skeleton input.")]
    public bool IsMouseMode = true;

    [Tooltip("The cursor image that will be displayed on the screen.")]
    public Texture2D CursorImage;

    [Tooltip("The size in pixels of the cursor icon.")]
    public Vector2 CursorSize = 24 * Vector2.one;

    [Tooltip("The color of the cursor in normal mode.")]
    public Color CursorTint = Color.red;

    [Tooltip("The color of the cursor in grab mode.")]
    public Color GrabCursorTint = Color.green;

    [Tooltip("Scales the palm position vector to camera space.")]
    public Vector3 PalmUnitsScale = new Vector3(.1f, .1f, -.1f);

    [Tooltip("Offsets the palm position vector in camera space.")]
    public Vector3 PalmUnitsOffset = new Vector3(0f, 0f, 55f);

    [Tooltip("Material used to highlight hovered game objects.")]
    public Material HighlightMaterial;

    [Tooltip("A layer mask to filter hover-able game objects")]
    public LayerMask Mask = -1;

    private Vector3 GetPalmWorldPosition()
    {
        // Chapter 1.b Replace mouse screen position with Microsoft.Gestures palm position.
        var skeleton = GesturesManager.Instance.StableSkeletons[Hand.RightHand];
        if (skeleton == null)
            return Vector2.zero;

        // Convert PalmPosition to screen space
        var palmCamPos = Vector3.Scale(skeleton.PalmPosition, PalmUnitsScale) + PalmUnitsOffset;
        return Camera.main.transform.TransformPoint(palmCamPos);
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
            var skeleton = GesturesManager.Instance.StableSkeletons[Hand.RightHand];
            if (skeleton == null)
            {
                return Vector2.zero;
            }

            // Convert PalmPosition to screen space
            var palmCameraPosition = Vector3.Scale(skeleton.PalmPosition, PalmUnitsScale) + PalmUnitsOffset;
            var palmWorldPosition = Camera.main.transform.TransformPoint(palmCameraPosition);
            var palmScreenPosition = (Vector2)Camera.main.WorldToScreenPoint(palmWorldPosition);
            return palmScreenPosition;
        }
    }

    private GameObject GetHoverObject()
    {
        // Chapter 2.a Raycast a ray from camera to object under cursor.
        var cursorPosition = GetCursorScreenPosition();
        var ray = Camera.main.ScreenPointToRay(cursorPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance: 1000, layerMask: Mask.value))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    public void StartGrab()
    {
        // Chapter 3.   Begin grab mode. 
        //              Setup initial values needed for translation manipulation 
        //              that occur on the Update method.
        if (!_hoveredGO)
            return;

        _isGrabbing = true;

        _initDistanceFromCamera = Camera.main.transform.InverseTransformPoint(_hoveredGO.transform.position).z;
        var ray = Camera.main.ScreenPointToRay(GetCursorScreenPosition());
        _lastCursorWorldPos = ray.GetPoint(_initDistanceFromCamera);
    }

    public void StopGrab()
    {
        // Chapter 3.   Stop grab mode.
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
        // Chapter 2.a Add highlight material to hovered object
        // Chapter 3. Do not change hover object when grabbing
        if (HighlightMaterial && !_isGrabbing)
        {
            // Stop highlighting old hover object
            if (_hoveredGO)
                _hoveredGO.RemoveMaterial(HighlightMaterial);

            // Raycast and find object under cursor
            _hoveredGO = GetHoverObject();

            // Add highlight material to hovered object
            if (_hoveredGO)
                _hoveredGO.AppendMaterial(HighlightMaterial);
        }

        // Chapter 3. Handle Grabbing
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
            var ray = Camera.main.ScreenPointToRay(GetCursorScreenPosition());
            var currentCursorWorldPos = ray.GetPoint(_initDistanceFromCamera);
            _hoveredGO.transform.position += currentCursorWorldPos - _lastCursorWorldPos;
            _lastCursorWorldPos = currentCursorWorldPos;
        }
    }

    private void OnGUI()
    {
        // Chapter 1.a Draw cursor texture in cursor screen position.
        var cursorPos = (Vector2)GetCursorScreenPosition();

        // Invert y direction
        cursorPos.y = Screen.height - cursorPos.y;

        // Draw hand cursor
        var bounds = new Rect(cursorPos - 0.5f * CursorSize, CursorSize);

        // Change the tint color to match our cursor mode.
        var orgColor = GUI.color;

        // Chapter 3. Change cursor color when grab mode is on.
        // Add a condition when setting GUI.color
        GUI.color = _isGrabbing ? GrabCursorTint : CursorTint;
        GUI.DrawTexture(bounds, CursorImage);
        GUI.color = orgColor;
    }
}