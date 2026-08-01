using UnityEngine;

public class MoneyPopupManager : MonoBehaviour
{
    [SerializeField] private MoneyPopup popupPrefab;

    public static MoneyPopupManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPopup(Vector3 position, int amount)
    {
        MoneyPopup popup =
            Instantiate(
                popupPrefab,
                position,
                Quaternion.identity);

        popup.Initialize(amount);
    }
}