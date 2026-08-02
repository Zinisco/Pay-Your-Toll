using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncomePerSecondUI : MonoBehaviour
{
    private struct IncomeEntry
    {
        public float time;
        public int amount;

        public IncomeEntry(float time, int amount)
        {
            this.time = time;
            this.amount = amount;
        }
    }

    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private TMP_Text incomeText;

    [Header("Display")]
    [SerializeField] private float measurementWindow = 5f;

    private readonly Queue<IncomeEntry> incomeEntries = new();

    private int incomeInsideWindow;
    private float loadedIncomePerSecond;
    private bool hasReceivedNewIncome;

    public float CurrentIncomePerSecond
    {
        get
        {
            if (!hasReceivedNewIncome)
                return loadedIncomePerSecond;

            if (measurementWindow <= 0f)
                return 0f;

            return incomeInsideWindow / measurementWindow;
        }
    }

    private void OnEnable()
    {
        if (moneyManager != null)
            moneyManager.MoneyEarned += RecordIncome;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (moneyManager != null)
            moneyManager.MoneyEarned -= RecordIncome;
    }

    private void Update()
    {
        RemoveExpiredEntries();
        RefreshUI();
    }

    public void LoadIncomePerSecond(float value)
    {
        loadedIncomePerSecond = Mathf.Max(0f, value);
        hasReceivedNewIncome = false;

        incomeEntries.Clear();
        incomeInsideWindow = 0;

        RefreshUI();
    }

    public void ResetIncomePerSecond()
    {
        loadedIncomePerSecond = 0f;
        hasReceivedNewIncome = false;

        incomeEntries.Clear();
        incomeInsideWindow = 0;

        RefreshUI();
    }

    private void RecordIncome(int amount)
    {
        if (amount <= 0)
            return;

        if (!hasReceivedNewIncome)
        {
            hasReceivedNewIncome = true;
            loadedIncomePerSecond = 0f;
        }

        incomeEntries.Enqueue(
            new IncomeEntry(Time.unscaledTime, amount)
        );

        incomeInsideWindow += amount;

        RefreshUI();
    }

    private void RemoveExpiredEntries()
    {
        float oldestAllowedTime =
            Time.unscaledTime - measurementWindow;

        while (
            incomeEntries.Count > 0 &&
            incomeEntries.Peek().time < oldestAllowedTime
        )
        {
            IncomeEntry expiredEntry = incomeEntries.Dequeue();
            incomeInsideWindow -= expiredEntry.amount;
        }
    }

    private void RefreshUI()
    {
        if (incomeText == null)
            return;

        incomeText.text =
            $"$ {CurrentIncomePerSecond:0.0} / sec";
    }
}