using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutomationUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSpawner carSpawner;
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
        if (carSpawner == null || moneyManager == null)
            return;

        if (carSpawner.AutomationUnlocked)
            return;

        if (!moneyManager.TrySpendMoney(automationCost))
            return;

        carSpawner.UnlockAutomation();

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
                carSpawner != null &&
                carSpawner.AutomationUnlocked
            );
        }
    }

    private void RefreshUI()
    {
        if (carSpawner == null || moneyManager == null)
            return;

        bool unlocked = carSpawner.AutomationUnlocked;

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