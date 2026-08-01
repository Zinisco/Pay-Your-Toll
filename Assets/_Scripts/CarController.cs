using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.05f;

    private Vector3 targetPosition;
    private bool hasTarget;
    private bool isLeaving;

    public bool HasReachedTarget { get; private set; }

    private void Update()
    {
        if (!hasTarget)
            return;

        MoveTowardTarget();
    }

    public void SetQueueTarget(Vector3 newTarget)
    {
        targetPosition = newTarget;
        hasTarget = true;
        isLeaving = false;
        HasReachedTarget = false;
    }

    public void Leave(Vector3 exitPosition)
    {
        targetPosition = exitPosition;
        hasTarget = true;
        isLeaving = true;
        HasReachedTarget = false;
    }

    private void MoveTowardTarget()
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= stoppingDistance * stoppingDistance)
        {
            transform.position = new Vector3(
                targetPosition.x,
                transform.position.y,
                targetPosition.z
            );

            HasReachedTarget = true;

            if (isLeaving)
                Destroy(gameObject);

            return;
        }

        Vector3 normalizedDirection = direction.normalized;

        transform.position +=
            normalizedDirection * moveSpeed * Time.deltaTime;

        if (normalizedDirection != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(normalizedDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}