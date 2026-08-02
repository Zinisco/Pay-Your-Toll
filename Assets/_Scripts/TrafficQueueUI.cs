using TMPro;
using UnityEngine;

public class TrafficQueueUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private TextMeshPro queueText;

    private int lastDisplayedTotal = -1;
    private int lastDisplayedMaximum = -1;

    private void Awake()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (carSpawner == null)
            return;

        if (lastDisplayedTotal != carSpawner.TotalRequestedCars ||
            lastDisplayedMaximum != carSpawner.MaximumTotalCars)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (carSpawner == null || queueText == null)
            return;

        lastDisplayedTotal = carSpawner.TotalRequestedCars;
        lastDisplayedMaximum = carSpawner.MaximumTotalCars;

        queueText.text =
            $"{lastDisplayedTotal} / {lastDisplayedMaximum}";
    }
}