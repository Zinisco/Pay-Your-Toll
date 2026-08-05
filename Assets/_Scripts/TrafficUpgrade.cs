using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrafficUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrafficManager trafficManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Upgrade")]
    [SerializeField] private int startingCost = 15;
    [SerializeField] private float costMultiplier = 1.8f;
    [SerializeField] private float spawnIntervalReduction = 0.25f;

    private int currentCost;
    private int upgradeLevel;

    public int UpgradeLevel => upgradeLevel;
    public int CurrentCost => currentCost;

    private void Awake()
    {
        currentCost = startingCost;
    }

    private void OnEnable()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(BuyUpgrade);

        if (moneyManager != null)
            moneyManager.MoneyChanged += HandleMoneyChanged;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(BuyUpgrade);

        if (moneyManager != null)
            moneyManager.MoneyChanged -= HandleMoneyChanged;
    }

    private void BuyUpgrade()
    {
        if (trafficManager == null || moneyManager == null)
            return;

        if (trafficManager.SpawnInterval <= trafficManager.MinimumSpawnInterval)
            return;

        if (!moneyManager.TrySpendMoney(currentCost))
            return;

        bool upgraded = trafficManager.ReduceSpawnInterval(
            spawnIntervalReduction
        );

        if (!upgraded)
            return;

        upgradeLevel++;

        currentCost = Mathf.CeilToInt(
            currentCost * costMultiplier
        );

        RefreshUI();

        SaveManager.Instance?.SaveGame();
    }

    private void HandleMoneyChanged(int newAmount)
    {
        RefreshUI();
    }

    public void LoadState(int savedLevel, int savedCost)
    {
        upgradeLevel = Mathf.Max(0, savedLevel);
        currentCost = Mathf.Max(1, savedCost);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (trafficManager == null || moneyManager == null)
            return;

        bool maxed =
            trafficManager.SpawnInterval <=
            trafficManager.MinimumSpawnInterval;

        if (upgradeText != null)
        {
            if (maxed)
            {
                upgradeText.text =
                    "More Traffic\nMAX LEVEL";
            }
            else
            {
                float nextInterval = Mathf.Max(
     trafficManager.MinimumSpawnInterval,
     trafficManager.SpawnInterval -
     spawnIntervalReduction
 );

                float currentCarsPerSecond =
                    1f / trafficManager.SpawnInterval;

                float nextCarsPerSecond =
                    1f / nextInterval;

                upgradeText.text =
                    $"More Cars\n" +
                    $"$ {currentCost}\n" +
                    $"{currentCarsPerSecond:0.00} → " +
                    $"{nextCarsPerSecond:0.00} cars/sec";
            }
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable =
                !maxed &&
                moneyManager.CurrentMoney >= currentCost;
        }
    }
}