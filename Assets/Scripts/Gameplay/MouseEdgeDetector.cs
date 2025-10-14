using UnityEngine;
using UnityEngine.InputSystem;

public class MouseEdgeDetector : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    public float edgeMargin = 20f; // The distance in pixels from the edge to trigger detection

    void Update()
    {
        // Check if a mouse is connected
        if (Mouse.current == null)
        {
            return;
        }

        // Read the current mouse position
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Check if the mouse is at the left edge
        if (mousePos.x <= edgeMargin)
        {
            _cameraTransform.Translate(new Vector3(-1 * Time.deltaTime, 0, 0), Space.World);
        }

        // Check if the mouse is at the right edge
        if (mousePos.x >= Screen.width - edgeMargin)
        {
            _cameraTransform.Translate(new Vector3(1 * Time.deltaTime, 0,0 ), Space.World);
        }

        // Check if the mouse is at the bottom edge
        if (mousePos.y <= edgeMargin)
        {
            _cameraTransform.Translate(new Vector3(0, 0, -1 * Time.deltaTime), Space.World);
        }

        // Check if the mouse is at the top edge
        if (mousePos.y >= Screen.height - edgeMargin)
        {
            _cameraTransform.Translate(new Vector3(0, 0, 1 * Time.deltaTime), Space.World);
        }
    }
}
