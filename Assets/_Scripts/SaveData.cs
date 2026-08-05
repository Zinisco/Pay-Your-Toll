using System;

[Serializable]
public class SaveData
{
    public int money;
    public float incomePerSecond;

    public float processingTime;
    public int moneyPerCar;
    public float spawnInterval;
    public bool automationUnlocked;

    public int speedUpgradeLevel;
    public int speedUpgradeCost;

    public int incomeUpgradeLevel;
    public int incomeUpgradeCost;

    public int trafficUpgradeLevel;
    public int trafficUpgradeCost;

    public int laneCount;
    public int purchasedLanes;
    public int laneUpgradeCost;
}