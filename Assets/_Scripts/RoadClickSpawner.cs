using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RoadClickSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameplayCamera;

    [Header("Road Detection")]
    [SerializeField] private LayerMask roadLayerMask;

    private void Awake()
    {
        if (gameplayCamera == null)
            gameplayCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (PointerIsOverUI())
            return;

        TryClickRoad();
    }

    private void TryClickRoad()
    {
        if (gameplayCamera == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            gameplayCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                Mathf.Infinity,
                roadLayerMask))
        {
            return;
        }

        RoadLane clickedLane =
            hit.collider.GetComponentInParent<RoadLane>();

        clickedLane?.TrySpawnCar();
    }

    private bool PointerIsOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
}