using UnityEngine;

public class CarVisualRandomizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualRoot;

    [Header("Car Models")]
    [SerializeField] private GameObject[] carVisualPrefabs;

    [Header("Variation")]
    [SerializeField] private bool randomizeRotation;
    [SerializeField] private Vector3 rotationOffset;

    private GameObject spawnedVisual;

    private void Awake()
    {
        SpawnRandomVisual();
    }

    private void SpawnRandomVisual()
    {
        if (visualRoot == null)
        {
            Debug.LogError(
                "CarVisualRandomizer requires a Visual Root.",
                this
            );

            return;
        }

        if (carVisualPrefabs == null || carVisualPrefabs.Length == 0)
        {
            Debug.LogError(
                "No car visual prefabs have been assigned.",
                this
            );

            return;
        }

        int randomIndex = Random.Range(0, carVisualPrefabs.Length);
        GameObject selectedPrefab = carVisualPrefabs[randomIndex];

        if (selectedPrefab == null)
            return;

        spawnedVisual = Instantiate(
            selectedPrefab,
            visualRoot
        );

        spawnedVisual.transform.localPosition = Vector3.zero;
        spawnedVisual.transform.localRotation =
            Quaternion.Euler(rotationOffset);

        if (randomizeRotation)
        {
            spawnedVisual.transform.localRotation *=
                Quaternion.Euler(0f, 180f, 0f);
        }
    }
}