using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TollBooth : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Transform boothStopPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Queue")]
    [SerializeField] private Vector3 queueDirection = Vector3.back;
    [SerializeField] private float queueSpacing = 2.5f;

    [Header("Processing")]
    [SerializeField] private float processingTime = 3f;
    [SerializeField] private int moneyPerCar = 1;

    [Header("Processing Limits")]
    [SerializeField] private float minimumProcessingTime = 0.5f;

    private readonly List<CarController> queuedCars = new();

    private Coroutine processingRoutine;

    public int QueueCount => queuedCars.Count;
    public float ProcessingTime => processingTime;
    public float MinimumProcessingTime => minimumProcessingTime;

    public int MoneyPerCar => moneyPerCar;

    public void JoinQueue(CarController car)
    {
        if (car == null || queuedCars.Contains(car))
            return;

        queuedCars.Add(car);

        RefreshQueuePositions();

        if (processingRoutine == null)
            processingRoutine = StartCoroutine(ProcessCars());
    }

    public bool ReduceProcessingTime(float amount)
    {
        if (amount <= 0f)
            return false;

        if (processingTime <= minimumProcessingTime)
            return false;

        processingTime = Mathf.Max(
            minimumProcessingTime,
            processingTime - amount
        );

        return true;
    }

    private IEnumerator ProcessCars()
    {
        while (queuedCars.Count > 0)
        {
            CarController frontCar = queuedCars[0];

            if (frontCar == null)
            {
                queuedCars.RemoveAt(0);
                RefreshQueuePositions();
                continue;
            }

            while (!frontCar.HasReachedTarget)
                yield return null;

            yield return new WaitForSeconds(processingTime);

            MoneyPopupManager.Instance?.ShowPopup(
    frontCar.transform.position + Vector3.up * 2f,
    moneyPerCar);

            moneyManager.AddMoney(moneyPerCar);

            queuedCars.RemoveAt(0);

            frontCar.Leave(exitPoint.position);

            RefreshQueuePositions();
        }

        processingRoutine = null;
    }

    public void SetProcessingTime(float value)
    {
        processingTime = Mathf.Clamp(
            value,
            minimumProcessingTime,
            float.MaxValue
        );
    }

    public void SetMoneyPerCar(int value)
    {
        moneyPerCar = Mathf.Max(1, value);
    }

    private void RefreshQueuePositions()
    {
        if (boothStopPoint == null)
            return;

        Vector3 normalizedQueueDirection = queueDirection.normalized;

        for (int i = 0; i < queuedCars.Count; i++)
        {
            CarController car = queuedCars[i];

            if (car == null)
                continue;

            Vector3 targetPosition =
                boothStopPoint.position +
                normalizedQueueDirection * queueSpacing * i;

            car.SetQueueTarget(targetPosition);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (boothStopPoint == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 normalizedQueueDirection = queueDirection.normalized;

        for (int i = 0; i < 8; i++)
        {
            Vector3 position =
                boothStopPoint.position +
                normalizedQueueDirection * queueSpacing * i;

            Gizmos.DrawWireCube(
                position,
                new Vector3(1.5f, 0.5f, 2.5f)
            );
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPoint.position, 0.5f);
        }
    }

    public void IncreaseMoneyPerCar(int amount)
    {
        if (amount <= 0)
            return;

        moneyPerCar += amount;
    }
}