using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Размер кнопки в нормальном состоянии
    [SerializeField] private Vector2 normalSize = new Vector2(325f, 85f);
    [SerializeField] private Vector2 hoverSize = new Vector2(333f, 100f);

    // Смещение кнопки по оси Y при наведении
    [SerializeField] private float hoverOffsetY = 4f;

    // Длительность и кривая анимации
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine currentAnimation;
    private bool isHovered = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Сохраняем исходную позицию
        originalPosition = rectTransform.anchoredPosition;
        // Устанавливаем начальный размер
        rectTransform.sizeDelta = normalSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovered) return;
        isHovered = true;

        // Останавливаем текущую анимацию, если она есть
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Запускаем анимацию наведения
        currentAnimation = StartCoroutine(AnimateButton(hoverSize, originalPosition + new Vector2(0, hoverOffsetY)));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovered) return;
        isHovered = false;

        // Останавливаем текущую анимацию, если она есть
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Запускаем анимацию возврата
        currentAnimation = StartCoroutine(AnimateButton(normalSize, originalPosition));
    }

    private IEnumerator AnimateButton(Vector2 targetSize, Vector2 targetPosition)
    {
        Vector2 startSize = rectTransform.sizeDelta;
        Vector2 startPosition = rectTransform.anchoredPosition;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            // Применяем кривую анимации для плавности
            float curveValue = animationCurve.Evaluate(t);

            // Интерполяция размера
            rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, curveValue);

            // Интерполяция позиции
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);

            yield return null;
        }

        // Устанавливаем финальные значения
        rectTransform.sizeDelta = targetSize;
        rectTransform.anchoredPosition = targetPosition;
    }

    // Публичный метод для сброса состояния (на случай, если кнопка деактивируется)
    public void ResetToNormal()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        isHovered = false;
        rectTransform.sizeDelta = normalSize;
        rectTransform.anchoredPosition = originalPosition;
    }

    private void OnDisable()
    {
        // Сбрасываем состояние при деактивации
        ResetToNormal();
    }
}