using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrafficUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Upgrade")]
    [SerializeField] private int startingCost = 15;
    [SerializeField] private float costMultiplier = 1.8f;
    [SerializeField] private float spawnIntervalReduction = 0.25f;

    private int currentCost;
    private int upgradeLevel;

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
        if (carSpawner == null || moneyManager == null)
            return;

        if (carSpawner.SpawnInterval <= carSpawner.MinimumSpawnInterval)
            return;

        if (!moneyManager.TrySpendMoney(currentCost))
            return;

        bool upgraded = carSpawner.ReduceSpawnInterval(
            spawnIntervalReduction
        );

        if (!upgraded)
            return;

        upgradeLevel++;

        currentCost = Mathf.CeilToInt(
            currentCost * costMultiplier
        );

        RefreshUI();
    }

    private void HandleMoneyChanged(int newAmount)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (carSpawner == null || moneyManager == null)
            return;

        bool maxed =
            carSpawner.SpawnInterval <=
            carSpawner.MinimumSpawnInterval;

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
                    carSpawner.MinimumSpawnInterval,
                    carSpawner.SpawnInterval -
                    spawnIntervalReduction
                );

                upgradeText.text =
                    $"More Traffic\n" +
                    $"${currentCost}\n" +
                    $"{carSpawner.SpawnInterval:0.00}s → " +
                    $"{nextInterval:0.00}s";
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