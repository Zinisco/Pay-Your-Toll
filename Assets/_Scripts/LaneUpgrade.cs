using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaneUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrafficManager trafficManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Cost")]
    [SerializeField] private int startingCost = 100;
    [SerializeField] private float costMultiplier = 2.5f;

    private int currentCost;
    private int purchasedLanes;

    public int PurchasedLanes => purchasedLanes;
    public int CurrentCost => currentCost;

    private void Awake()
    {
        currentCost = startingCost;
    }

    private void OnEnable()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(BuyLane);

        if (moneyManager != null)
            moneyManager.MoneyChanged += HandleMoneyChanged;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(BuyLane);

        if (moneyManager != null)
            moneyManager.MoneyChanged -= HandleMoneyChanged;
    }

    private void BuyLane()
    {
        if (trafficManager == null || moneyManager == null)
            return;

        if (trafficManager.LaneCount >=
            trafficManager.MaximumLaneCount)
        {
            return;
        }

        if (!moneyManager.TrySpendMoney(currentCost))
            return;

        if (!trafficManager.BuildNextLane())
        {
            moneyManager.AddMoney(currentCost);
            return;
        }

        purchasedLanes++;

        currentCost = Mathf.CeilToInt(
            currentCost * costMultiplier
        );

        RefreshUI();

        SaveManager.Instance?.SaveGame();
    }

    private void HandleMoneyChanged(int currentMoney)
    {
        RefreshUI();
    }

    public void LoadState(
     int savedPurchasedLanes,
     int savedCost
 )
    {
        int maximumPurchasableLanes =
            Mathf.Max(
                0,
                trafficManager.MaximumLaneCount - 1
            );

        purchasedLanes = Mathf.Clamp(
            savedPurchasedLanes,
            0,
            maximumPurchasableLanes
        );

        currentCost = Mathf.Max(
            1,
            savedCost
        );

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (trafficManager == null || moneyManager == null)
            return;

        bool maxed =
            trafficManager.LaneCount >=
            trafficManager.MaximumLaneCount;

        if (upgradeText != null)
        {
            upgradeText.text = maxed
                ? "Build Lane\nMAX"
                : $"Build Lane\n$ {currentCost}";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable =
                !maxed &&
                moneyManager.CurrentMoney >= currentCost;
        }
    }
}