using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TollSpeedUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TollBooth tollBooth;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Upgrade")]
    [SerializeField] private int startingCost = 5;
    [SerializeField] private float costMultiplier = 1.75f;
    [SerializeField] private float processingTimeReduction = 0.25f;

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
        if (tollBooth == null || moneyManager == null)
            return;

        if (tollBooth.ProcessingTime <= tollBooth.MinimumProcessingTime)
            return;

        if (!moneyManager.TrySpendMoney(currentCost))
            return;

        bool upgraded =
            tollBooth.ReduceProcessingTime(processingTimeReduction);

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
        if (tollBooth == null || moneyManager == null)
            return;

        bool maxed =
            tollBooth.ProcessingTime <= tollBooth.MinimumProcessingTime;

        if (upgradeText != null)
        {
            if (maxed)
            {
                upgradeText.text =
                    $"Faster Toll Booth\nMAX LEVEL";
            }
            else
            {
                upgradeText.text =
                    $"Faster Toll Booth\n" +
                    $"${currentCost}\n" +
                    $"{tollBooth.ProcessingTime:0.00}s";
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