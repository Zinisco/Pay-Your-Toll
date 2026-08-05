using System.Collections;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Game Systems")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private TrafficManager trafficManager;

    [Header("Upgrades")]
    [SerializeField] private TollSpeedUpgrade speedUpgrade;
    [SerializeField] private TollIncomeUpgrade incomeUpgrade;
    [SerializeField] private TrafficUpgrade trafficUpgrade;
    [SerializeField] private LaneUpgrade laneUpgrade;

    [Header("UI")]
    [SerializeField] private IncomePerSecondUI incomePerSecondUI;

    [Header("Autosave")]
    [SerializeField] private bool autosaveEnabled = true;
    [SerializeField] private float autosaveInterval = 10f;

    private string savePath;
    private bool saveDeleted;
    private Coroutine autosaveRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        savePath = Path.Combine(
            Application.persistentDataPath,
            "toll_booth_save.json"
        );
    }

    private void Start()
    {
        LoadGame();

        if (autosaveEnabled)
        {
            autosaveRoutine =
                StartCoroutine(AutosaveRoutine());
        }
    }

    public void SaveGame()
    {
        if (saveDeleted || !ReferencesAreValid())
            return;

        SaveData saveData = new SaveData
        {
            money = moneyManager.CurrentMoney,

            incomePerSecond =
                incomePerSecondUI.CurrentIncomePerSecond,

            processingTime =
                trafficManager.ProcessingTime,

            moneyPerCar =
                trafficManager.MoneyPerCar,

            spawnInterval =
                trafficManager.SpawnInterval,

            automationUnlocked =
                trafficManager.AutomationUnlocked,

            speedUpgradeLevel =
                speedUpgrade.UpgradeLevel,

            speedUpgradeCost =
                speedUpgrade.CurrentCost,

            incomeUpgradeLevel =
                incomeUpgrade.UpgradeLevel,

            incomeUpgradeCost =
                incomeUpgrade.CurrentCost,

            trafficUpgradeLevel =
                trafficUpgrade.UpgradeLevel,

            trafficUpgradeCost =
                trafficUpgrade.CurrentCost,

            laneCount =
                trafficManager.LaneCount,

            purchasedLanes =
                laneUpgrade.PurchasedLanes,

            laneUpgradeCost =
                laneUpgrade.CurrentCost
        };

        string json =
            JsonUtility.ToJson(saveData, true);

        try
        {
            File.WriteAllText(savePath, json);
        }
        catch (IOException exception)
        {
            Debug.LogError(
                $"Could not save game: {exception.Message}",
                this
            );
        }
    }

    public void LoadGame()
    {
        if (!ReferencesAreValid())
            return;

        if (!File.Exists(savePath))
        {
            Debug.Log(
                "No save file found. Starting a new game."
            );

            return;
        }

        try
        {
            string json =
                File.ReadAllText(savePath);

            SaveData saveData =
                JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogWarning(
                    "Save file could not be read."
                );

                return;
            }

            moneyManager.SetMoney(saveData.money);

            trafficManager.SetProcessingTimeForAllLanes(
                saveData.processingTime
            );

            trafficManager.SetMoneyPerCarForAllLanes(
                saveData.moneyPerCar
            );

            trafficManager.SetSpawnInterval(
                saveData.spawnInterval
            );

            int savedLaneCount =
                Mathf.Max(1, saveData.laneCount);

            trafficManager.BuildLanesUntilCount(
                savedLaneCount
            );

            speedUpgrade.LoadState(
                saveData.speedUpgradeLevel,
                saveData.speedUpgradeCost
            );

            incomeUpgrade.LoadState(
                saveData.incomeUpgradeLevel,
                saveData.incomeUpgradeCost
            );

            trafficUpgrade.LoadState(
                saveData.trafficUpgradeLevel,
                saveData.trafficUpgradeCost
            );

            laneUpgrade.LoadState(
                saveData.purchasedLanes,
                saveData.laneUpgradeCost
            );

            incomePerSecondUI.LoadIncomePerSecond(
                saveData.incomePerSecond
            );

            trafficManager.SetAutomationUnlocked(
                saveData.automationUnlocked
            );

            Debug.Log("Game loaded.");
        }
        catch (IOException exception)
        {
            Debug.LogError(
                $"Could not load game: {exception.Message}",
                this
            );
        }
    }

    public void ResetGame()
    {
        try
        {
            if (File.Exists(savePath))
                File.Delete(savePath);

            saveDeleted = true;

            moneyManager.ResetMoney();
            incomePerSecondUI.ResetIncomePerSecond();
            trafficManager.SetAutomationUnlocked(false);

            Debug.Log("Save deleted.");
        }
        catch (IOException exception)
        {
            Debug.LogError(
                $"Could not delete save: {exception.Message}",
                this
            );
        }
    }

    private IEnumerator AutosaveRoutine()
    {
        WaitForSeconds wait =
            new WaitForSeconds(autosaveInterval);

        while (true)
        {
            yield return wait;
            SaveGame();
        }
    }

    private bool ReferencesAreValid()
    {
        return
            moneyManager != null &&
            trafficManager != null &&
            incomePerSecondUI != null &&
            speedUpgrade != null &&
            incomeUpgrade != null &&
            trafficUpgrade != null &&
            laneUpgrade != null;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}