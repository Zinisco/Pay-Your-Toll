using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TollIncomeUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TollBooth tollBooth;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Upgrade")]
    [SerializeField] private int startingCost = 10;
    [SerializeField] private float costMultiplier = 1.8f;
    [SerializeField] private int incomeIncrease = 1;

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
        if (tollBooth == null || moneyManager == null)
            return;

        if (!moneyManager.TrySpendMoney(currentCost))
            return;

        tollBooth.IncreaseMoneyPerCar(incomeIncrease);

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

    private void RefreshUI()
    {
        if (tollBooth == null || moneyManager == null)
            return;

        if (upgradeText != null)
        {
            upgradeText.text =
    $"Increase Toll\n" +
    $"${currentCost}\n" +
    $"${tollBooth.MoneyPerCar} → " +
    $"${tollBooth.MoneyPerCar + incomeIncrease}";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable =
                moneyManager.CurrentMoney >= currentCost;
        }
    }

    public void LoadState(int savedLevel, int savedCost)
    {
        upgradeLevel = Mathf.Max(0, savedLevel);
        currentCost = Mathf.Max(1, savedCost);

        RefreshUI();
    }
}