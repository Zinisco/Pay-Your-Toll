using System;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [Header("Starting Money")]
    [SerializeField] private int startingMoney;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;

    private int currentMoney;

    public int CurrentMoney => currentMoney;

    public event Action<int> MoneyChanged;

    private void Awake()
    {
        currentMoney = startingMoney;
        RefreshUI();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        currentMoney += amount;

        RefreshUI();
        MoneyChanged?.Invoke(currentMoney);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentMoney < amount)
            return false;

        currentMoney -= amount;

        RefreshUI();
        MoneyChanged?.Invoke(currentMoney);

        return true;
    }

    private void RefreshUI()
    {
        if (moneyText != null)
            moneyText.text = $"${currentMoney}";
    }
}