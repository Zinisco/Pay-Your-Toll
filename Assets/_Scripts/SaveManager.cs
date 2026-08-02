using System.Collections;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Game Systems")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private TollBooth tollBooth;
    [SerializeField] private CarSpawner carSpawner;

    [Header("Upgrades")]
    [SerializeField] private TollSpeedUpgrade speedUpgrade;
    [SerializeField] private TollIncomeUpgrade incomeUpgrade;
    [SerializeField] private TrafficUpgrade trafficUpgrade;

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
            autosaveRoutine = StartCoroutine(AutosaveRoutine());
    }

    public void SaveGame()
    {
        if (saveDeleted)
            return;

        if (!ReferencesAreValid())
            return;

        SaveData saveData = new SaveData
        {
            money = moneyManager.CurrentMoney,
            incomePerSecond = incomePerSecondUI.CurrentIncomePerSecond,

            processingTime = tollBooth.ProcessingTime,
            moneyPerCar = tollBooth.MoneyPerCar,
            spawnInterval = carSpawner.SpawnInterval,

            speedUpgradeLevel = speedUpgrade.UpgradeLevel,
            speedUpgradeCost = speedUpgrade.CurrentCost,

            incomeUpgradeLevel = incomeUpgrade.UpgradeLevel,
            incomeUpgradeCost = incomeUpgrade.CurrentCost,

            trafficUpgradeLevel = trafficUpgrade.UpgradeLevel,
            trafficUpgradeCost = trafficUpgrade.CurrentCost,

            automationUnlocked = carSpawner.AutomationUnlocked,
        };

        string json = JsonUtility.ToJson(saveData, true);

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
            Debug.Log("No save file found. Starting a new game.");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogWarning("Save file could not be read.");
                return;
            }

            moneyManager.SetMoney(saveData.money);

            tollBooth.SetProcessingTime(saveData.processingTime);
            tollBooth.SetMoneyPerCar(saveData.moneyPerCar);

            carSpawner.SetSpawnInterval(saveData.spawnInterval);

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

            moneyManager.SetMoney(saveData.money);

            incomePerSecondUI.LoadIncomePerSecond(
                saveData.incomePerSecond
            );

            carSpawner.SetAutomationUnlocked(
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
            tollBooth != null &&
            carSpawner != null &&
            incomePerSecondUI != null &&
            speedUpgrade != null &&
            incomeUpgrade != null &&
            trafficUpgrade != null;
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