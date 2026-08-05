using UnityEngine;

public class RoadLane : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private TollBooth tollBooth;

    public CarSpawner CarSpawner => carSpawner;
    public TollBooth TollBooth => tollBooth;

    public int TrafficCount =>
        carSpawner != null
            ? carSpawner.TotalRequestedCars
            : int.MaxValue;

    public bool CanAcceptCar =>
        carSpawner != null &&
        carSpawner.CanAcceptCar;

    public void Initialize(MoneyManager moneyManager)
    {
        if (tollBooth != null)
            tollBooth.Initialize(moneyManager);
    }

    public bool TrySpawnCar()
    {
        return carSpawner != null &&
               carSpawner.TrySpawnCar();
    }
}