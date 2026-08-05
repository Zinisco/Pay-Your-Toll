using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameplayCamera;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 0.08f;
    [SerializeField] private bool scalePanWithZoom = true;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 35f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private float minPitch = 20f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Input")]
    [SerializeField] private bool ignoreInputOverUI = true;

    private float currentYaw;
    private float currentPitch;
    private float currentDistance;

    private void Awake()
    {
        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "CameraController could not find a gameplay camera.",
                this
            );

            enabled = false;
            return;
        }

        Vector3 cameraEuler = gameplayCamera.transform.eulerAngles;

        currentYaw = cameraEuler.y;
        currentPitch = NormalizeAngle(cameraEuler.x);

        currentPitch = Mathf.Clamp(
            currentPitch,
            minPitch,
            maxPitch
        );

        currentDistance =
            Vector3.Distance(
                transform.position,
                gameplayCamera.transform.position
            );

        currentDistance = Mathf.Clamp(
            currentDistance,
            minDistance,
            maxDistance
        );

        ApplyCameraTransform();
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        bool pointerOverUI =
            ignoreInputOverUI &&
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();

        HandlePan(pointerOverUI);
        HandleRotation(pointerOverUI);
        HandleZoom(pointerOverUI);
    }

    private void HandlePan(bool pointerOverUI)
    {
        if (pointerOverUI)
            return;

        if (!Mouse.current.middleButton.isPressed)
            return;

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        Vector3 cameraRight =
            gameplayCamera.transform.right;

        Vector3 cameraForward =
            gameplayCamera.transform.forward;

        cameraRight.y = 0f;
        cameraForward.y = 0f;

        cameraRight.Normalize();
        cameraForward.Normalize();

        float adjustedPanSpeed = panSpeed;

        if (scalePanWithZoom)
        {
            adjustedPanSpeed *=
                Mathf.Lerp(
                    0.6f,
                    2f,
                    Mathf.InverseLerp(
                        minDistance,
                        maxDistance,
                        currentDistance
                    )
                );
        }

        Vector3 movement =
            -cameraRight * mouseDelta.x +
            -cameraForward * mouseDelta.y;

        transform.position +=
            movement * adjustedPanSpeed;
    }

    private void HandleZoom(bool pointerOverUI)
    {
        if (pointerOverUI)
            return;

        float scroll =
            Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(scroll, 0f))
            return;

        float scrollDirection = Mathf.Sign(scroll);

        currentDistance -=
            scrollDirection * zoomSpeed;

        currentDistance = Mathf.Clamp(
            currentDistance,
            minDistance,
            maxDistance
        );

        ApplyCameraTransform();
    }

    private void HandleRotation(bool pointerOverUI)
    {
        if (pointerOverUI)
            return;

        if (!Mouse.current.rightButton.isPressed)
            return;

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        currentYaw +=
            mouseDelta.x * rotationSpeed;

        currentPitch -=
            mouseDelta.y * rotationSpeed;

        currentPitch = Mathf.Clamp(
            currentPitch,
            minPitch,
            maxPitch
        );

        ApplyCameraTransform();
    }

    private void ApplyCameraTransform()
    {
        Quaternion rotation =
            Quaternion.Euler(
                currentPitch,
                currentYaw,
                0f
            );

        Vector3 cameraOffset =
            rotation * Vector3.back * currentDistance;

        gameplayCamera.transform.position =
            transform.position + cameraOffset;

        gameplayCamera.transform.rotation =
            rotation;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }
}