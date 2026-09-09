using UnityEngine;
using UnityEngine.EventSystems;

public class CustomScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    // Настройки скролла
    [SerializeField] private float scrollSensitivity = 50f;
    [SerializeField] private float dragSensitivity = 1f;
    [SerializeField] private float smoothTime = 0.1f;

    // Лимиты
    [SerializeField] private float minY = -5000f;
    [SerializeField] private float maxY = 0f;

    // Инерция
    [SerializeField] private bool useInertia = true;
    [SerializeField] private float deceleration = 0.95f;

    private RectTransform rectTransform;
    private Vector2 lastDragPosition;
    private float currentVelocity;
    private float targetY;
    private bool isDragging;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetY = rectTransform.anchoredPosition.y;
    }

    void Update()
    {
        // Применяем инерцию
        if (!isDragging && useInertia && Mathf.Abs(currentVelocity) > 0.1f)
        {
            targetY += currentVelocity * Time.deltaTime;
            currentVelocity *= deceleration;
        }

        // Ограничиваем позицию
        targetY = Mathf.Clamp(targetY, minY, maxY);

        // Плавное движение
        Vector2 currentPos = rectTransform.anchoredPosition;
        float newY = Mathf.Lerp(currentPos.y, targetY, smoothTime * 10f * Time.deltaTime);
        rectTransform.anchoredPosition = new Vector2(currentPos.x, newY);
    }

    // Скролл колесиком мыши
    public void OnScroll(PointerEventData eventData)
    {
        float scrollDelta = eventData.scrollDelta.y * scrollSensitivity;
        targetY += scrollDelta;
        currentVelocity = scrollDelta * 10f; // Добавляем инерцию от скролла
    }

    // Начало перетаскивания
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        lastDragPosition = eventData.position;
        currentVelocity = 0f;
    }

    // Процесс перетаскивания
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastDragPosition;
        float dragAmount = delta.y * dragSensitivity;

        targetY += dragAmount;
        currentVelocity = dragAmount * 50f; // Запоминаем скорость для инерции

        lastDragPosition = eventData.position;
    }

    // Конец перетаскивания
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    // Метод для программного скроллинга к определенной позиции
    public void ScrollTo(float yPosition)
    {
        targetY = Mathf.Clamp(yPosition, minY, maxY);
        currentVelocity = 0f;
    }

    // Скроллинг к верху
    public void ScrollToTop()
    {
        ScrollTo(maxY);
    }

    // Скроллинг к низу
    public void ScrollToBottom()
    {
        ScrollTo(minY);
    }
}