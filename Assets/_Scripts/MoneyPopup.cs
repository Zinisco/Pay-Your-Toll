using TMPro;
using UnityEngine;

public class MoneyPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro amountText;

    [Header("Animation")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float randomHorizontalOffset = 0.3f;

    private float timer;

    private Vector3 moveDirection;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        moveDirection = Vector3.up;

        transform.position += new Vector3(
            Random.Range(
                -randomHorizontalOffset,
                randomHorizontalOffset),
            0f,
            Random.Range(
                -randomHorizontalOffset,
                randomHorizontalOffset));
    }

    public void Initialize(int amount)
    {
        amountText.text = $"+${amount}";

        transform.rotation =
    Camera.main.transform.rotation;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position +=
            moveDirection * floatSpeed * Time.deltaTime;

        float alpha = 1f - (timer / lifetime);

        amountText.alpha = alpha;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}