using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutomationUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrafficManager trafficManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseText;

    [Header("Upgrade")]
    [SerializeField] private int automationCost = 50;

    [Header("Unlock UI")]
    [SerializeField] private GameObject trafficUpgradeObject;

    private void OnEnable()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(BuyAutomation);

        if (moneyManager != null)
            moneyManager.MoneyChanged += HandleMoneyChanged;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(BuyAutomation);

        if (moneyManager != null)
            moneyManager.MoneyChanged -= HandleMoneyChanged;
    }

    private void Start()
    {
        RefreshTrafficUpgradeVisibility();
    }

    private void BuyAutomation()
    {
        if (trafficManager == null || moneyManager == null)
            return;

        if (trafficManager.AutomationUnlocked)
            return;

        if (!moneyManager.TrySpendMoney(automationCost))
            return;

        trafficManager.UnlockAutomation();

        RefreshTrafficUpgradeVisibility();
        RefreshUI();

        SaveManager.Instance?.SaveGame();
    }

    private void HandleMoneyChanged(int currentMoney)
    {
        RefreshUI();
    }

    private void RefreshTrafficUpgradeVisibility()
    {
        if (trafficUpgradeObject != null)
        {
            trafficUpgradeObject.SetActive(
                trafficManager != null &&
                trafficManager.AutomationUnlocked
            );
        }
    }

    private void RefreshUI()
    {
        if (trafficManager == null || moneyManager == null)
            return;

        bool unlocked = trafficManager.AutomationUnlocked;

        if (purchaseText != null)
        {
            purchaseText.text = unlocked
                ? "Automatic Traffic\nUNLOCKED"
                : $"Automatic Traffic\n$ {automationCost}";
        }

        if (purchaseButton != null)
        {
            purchaseButton.interactable =
                !unlocked &&
                moneyManager.CurrentMoney >= automationCost;
        }
    }
}