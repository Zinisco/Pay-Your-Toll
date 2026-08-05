using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TollSpeedUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrafficManager trafficManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Upgrade")]
    [SerializeField] private int startingCost = 5;
    [SerializeField] private float costMultiplier = 1.75f;
    [SerializeField] private float processingTimeReduction = 0.25f;
    [SerializeField] private float minimumProcessingTime = 0.5f;

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

        if (trafficManager.ProcessingTime <= minimumProcessingTime)
            return;

        if (!moneyManager.TrySpendMoney(currentCost))
            return;

        bool upgraded =
            trafficManager.ReduceProcessingTimeForAllLanes(
                processingTimeReduction,
                minimumProcessingTime
            );

        if (!upgraded)
        {
            moneyManager.AddMoney(currentCost);
            return;
        }

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
            trafficManager.ProcessingTime <= minimumProcessingTime;

        if (upgradeText != null)
        {
            if (maxed)
            {
                upgradeText.text =
                    "Faster Toll Booth\nMAX LEVEL";
            }
            else
            {
                float nextTime = Mathf.Max(
                    minimumProcessingTime,
                    trafficManager.ProcessingTime -
                    processingTimeReduction
                );

                upgradeText.text =
                    $"Faster Toll Booth\n" +
                    $"$ {currentCost}\n" +
                    $"{trafficManager.ProcessingTime:0.00}s → " +
                    $"{nextTime:0.00}s";
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