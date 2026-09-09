using UnityEngine;

public class RotateZ : MonoBehaviour
{
    [Header("Настройки вращения")]
    public float rotationSpeed = 50f;

    [Header("Настройки движения")]
    public Transform point1;
    public Transform point2;
    public float moveSpeed = 2f;

    [Header("Настройки прыжка")]
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.3f;
    public float minJumpInterval = 3f;
    public float maxJumpInterval = 6f;

    private RectTransform rectTransform;
    private bool movingToPoint2 = true; // Направление движения

    // Переменные для прыжка
    private float nextJumpTime;
    private bool isJumping = false;
    private float jumpStartTime;
    private float originalY;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        SetNextJumpTime();
    }

    void Update()
    {
        if (point1 != null && point2 != null)
        {
            MoveAndRotate();
        }
        else
        {
            // Если точки не указаны, просто вращаемся как раньше
            rectTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }

        // Обработка прыжка
        HandleJump();
    }

    void MoveAndRotate()
    {
        // Определяем целевую точку
        Vector3 targetPosition = movingToPoint2 ? point2.position : point1.position;

        // Двигаемся к целевой точке
        Vector3 newPosition = Vector3.MoveTowards(
            rectTransform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Сохраняем Y позицию для прыжка (если не прыгаем)
        if (!isJumping)
        {
            originalY = newPosition.y;
        }

        rectTransform.position = newPosition;

        // Вращаемся в зависимости от направления движения
        if (movingToPoint2)
        {
            // Движемся к точке 2 - вращение против часовой (-rotationSpeed)
            rectTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Движемся к точке 1 - вращение по часовой (+rotationSpeed)
            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        // Проверяем, достигли ли целевой точки
        float distance = Vector3.Distance(rectTransform.position, targetPosition);
        if (distance < 0.01f) // Порог близости к точке
        {
            // Меняем направление движения
            movingToPoint2 = !movingToPoint2;
        }
    }

    void HandleJump()
    {
        // Проверяем, пора ли прыгать
        if (!isJumping && Time.time >= nextJumpTime)
        {
            StartJump();
        }

        // Обрабатываем прыжок
        if (isJumping)
        {
            float jumpProgress = (Time.time - jumpStartTime) / jumpDuration;

            if (jumpProgress >= 1f)
            {
                // Прыжок завершен
                isJumping = false;
                rectTransform.position = new Vector3(
                    rectTransform.position.x,
                    originalY,
                    rectTransform.position.z
                );
                SetNextJumpTime();
            }
            else
            {
                // Вычисляем высоту прыжка по синусоиде для плавности
                float jumpOffset = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
                rectTransform.position = new Vector3(
                    rectTransform.position.x,
                    originalY + jumpOffset,
                    rectTransform.position.z
                );
            }
        }
    }

    void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        originalY = rectTransform.position.y;
    }

    void SetNextJumpTime()
    {
        float randomInterval = Random.Range(minJumpInterval, maxJumpInterval);
        nextJumpTime = Time.time + randomInterval;
    }
}