using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Управляет показом картинок между уровнями с сохранением прогресса в PlayerPrefs
// Прописать старался как можно подробнее, как и все настройки, что бы можно было легко менять в инспекторе
public class ImagePopupManager : MonoBehaviour
{
    [Header("UI Элементы")]
    [Tooltip("Панель с картинкой ((!)должна быть дочерней к Canvas)")]
    public GameObject popupPanel;

    [Tooltip("Image компонент для отображения картинки")]
    public Image imageDisplay;

    [Tooltip("Опциональный текст с подсказкой (например: 'Нажмите ЛКМ или Пробел')")]
    public TextMeshProUGUI hintText;

    [Header("Папки с картинками (относительно Resources/)")]
    [Tooltip("Папка для картинок хорошего прохождения (например: Images/Good)")]
    public string goodPerformanceFolder = "Images/Good";

    [Tooltip("Папка для картинок нейтрального прохождения (например: Images/Neutral)")]
    public string neutralPerformanceFolder = "Images/Neutral";

    [Tooltip("Папка для картинок плохого прохождения (например: Images/Bad)")]
    public string badPerformanceFolder = "Images/Bad";

    [Header("Настройки анимации")]
    [Tooltip("Длительность появления картинки (секунды)")]
    [Range(0.1f, 3f)]
    public float fadeInDuration = 0.5f;

    [Tooltip("Длительность исчезновения картинки (секунды)")]
    [Range(0.1f, 3f)]
    public float fadeOutDuration = 0.5f;

    [Tooltip("Тип анимации появления")]
    public AnimationType fadeInAnimation = AnimationType.EaseOut;

    [Tooltip("Тип анимации исчезновения")]
    public AnimationType fadeOutAnimation = AnimationType.EaseIn;

    [Header("Дополнительные эффекты")]
    [Tooltip("Масштабировать картинку при появлении")]
    public bool scaleOnFadeIn = true;

    [Tooltip("Начальный масштаб (если scaleOnFadeIn включен)")]
    [Range(0.5f, 1f)]
    public float startScale = 0.8f;

    [Tooltip("Звук при появлении картинки")]
    public AudioClip popupSound;

    [Tooltip("Звук при закрытии картинки")]
    public AudioClip closeSound;

    [Header("Дебаг")]
    [Tooltip("Выводить дебаг информацию в консоль")]
    public bool debugMode = false;

    public enum AnimationType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    // PlayerPrefs ключи
    private const string GOOD_COUNT_KEY = "GoodImagesShown";
    private const string NEUTRAL_COUNT_KEY = "NeutralImagesShown";
    private const string BAD_COUNT_KEY = "BadImagesShown";

    // Счетчики показанных картинок для каждого типа
    private int goodImagesShown = 0;
    private int neutralImagesShown = 0;
    private int badImagesShown = 0;

    private AudioSource audioSource;
    private bool isShowing = false;
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        // Проверяем наличие необходимых компонентов
        if (popupPanel == null)
        {
            Debug.LogError("ImagePopupManager: popupPanel не назначен!");
            return;
        }

        if (imageDisplay == null)
        {
            Debug.LogError("ImagePopupManager: imageDisplay не назначен!");
            return;
        }

        // Добавляем CanvasGroup для плавной анимации альфы всей панели
        panelCanvasGroup = popupPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = popupPanel.AddComponent<CanvasGroup>();
        }

        // Создаем AudioSource для звуков
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Скрываем панель в начале
        popupPanel.SetActive(false);

        // Загружаем прогресс из PlayerPrefs
        LoadProgress();
    }

    // Загрузить прогресс из PlayerPrefs
    private void LoadProgress()
    {
        goodImagesShown = PlayerPrefs.GetInt(GOOD_COUNT_KEY, 0);
        neutralImagesShown = PlayerPrefs.GetInt(NEUTRAL_COUNT_KEY, 0);
        badImagesShown = PlayerPrefs.GetInt(BAD_COUNT_KEY, 0);

        if (debugMode)
        {
            Debug.Log($"ImagePopupManager: Загружен прогресс - Good: {goodImagesShown}, Neutral: {neutralImagesShown}, Bad: {badImagesShown}");
        }
    }

    // Показать картинку в зависимости от типа прохождения
    public void ShowImage(string performanceType, System.Action onComplete = null)
    {
        if (isShowing)
        {
            Debug.LogWarning("ImagePopupManager: Картинка уже показывается! Пропускаем показ, но вызываем callback, чтобы не подвесить вызывающий код.");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(ShowImageCoroutine(performanceType, onComplete));
    }

    private IEnumerator ShowImageCoroutine(string performanceType, System.Action onComplete)
    {
        isShowing = true;

        // Определяем папку и номер картинки
        string folder = "";
        int imageNumber = 1;

        switch (performanceType)
        {
            case "Хорошее":
                folder = goodPerformanceFolder;
                goodImagesShown++;
                imageNumber = goodImagesShown;
                PlayerPrefs.SetInt(GOOD_COUNT_KEY, goodImagesShown);
                PlayerPrefs.Save();
                break;
            case "Нейтральное":
                folder = neutralPerformanceFolder;
                neutralImagesShown++;
                imageNumber = neutralImagesShown;
                PlayerPrefs.SetInt(NEUTRAL_COUNT_KEY, neutralImagesShown);
                PlayerPrefs.Save();
                break;
            case "Плохое":
                folder = badPerformanceFolder;
                badImagesShown++;
                imageNumber = badImagesShown;
                PlayerPrefs.SetInt(BAD_COUNT_KEY, badImagesShown);
                PlayerPrefs.Save();
                break;
            default:
                Debug.LogError($"ImagePopupManager: Неизвестный тип прохождения: {performanceType}");
                isShowing = false;
                onComplete?.Invoke();
                yield break;
        }

        // Загружаем картинку
        string imagePath = $"{folder}/{imageNumber}";
        Sprite sprite = Resources.Load<Sprite>(imagePath);

        if (sprite == null)
        {
            Debug.LogWarning($"ImagePopupManager: Не удалось загрузить картинку: Resources/{imagePath}.png");
            // Пробуем загрузить без расширения
            sprite = Resources.Load<Sprite>($"{folder}/{imageNumber}");

            if (sprite == null)
            {
                Debug.LogError($"ImagePopupManager: Картинка не найдена в Resources/{folder}/");
                isShowing = false;
                onComplete?.Invoke();
                yield break;
            }
        }

        if (debugMode)
        {
            Debug.Log($"ImagePopupManager: Показываем картинку #{imageNumber} для прохождения '{performanceType}' из {imagePath}");
        }

        // Устанавливаем картинку
        imageDisplay.sprite = sprite;

        // Активируем панель
        popupPanel.SetActive(true);

        // Начальные значения для анимации
        panelCanvasGroup.alpha = 0f;
        if (scaleOnFadeIn)
        {
            popupPanel.transform.localScale = Vector3.one * startScale;
        }

        // Проигрываем звук появления
        if (popupSound != null)
        {
            audioSource.PlayOneShot(popupSound);
        }

        // Анимация появления
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            float animatedT = ApplyAnimationCurve(t, fadeInAnimation);

            panelCanvasGroup.alpha = animatedT;

            if (scaleOnFadeIn)
            {
                float scale = Mathf.Lerp(startScale, 1f, animatedT);
                popupPanel.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        // Убеждаемся что полностью видно
        panelCanvasGroup.alpha = 1f;
        if (scaleOnFadeIn)
        {
            popupPanel.transform.localScale = Vector3.one;
        }

        // Показываем подсказку
        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);
        }

        // Ждем нажатия ЛКМ или пробела
        while (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // Скрываем подсказку
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
        }

        // Проигрываем звук закрытия
        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        // Анимация исчезновения
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float animatedT = ApplyAnimationCurve(t, fadeOutAnimation);

            panelCanvasGroup.alpha = 1f - animatedT;

            if (scaleOnFadeIn)
            {
                float scale = Mathf.Lerp(1f, startScale, animatedT);
                popupPanel.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        // Полностью скрываем
        panelCanvasGroup.alpha = 0f;
        popupPanel.SetActive(false);

        isShowing = false;

        // Вызываем callback
        onComplete?.Invoke();
    }

    private float ApplyAnimationCurve(float t, AnimationType type)
    {
        switch (type)
        {
            case AnimationType.Linear:
                return t;
            case AnimationType.EaseIn:
                return t * t;
            case AnimationType.EaseOut:
                return 1f - (1f - t) * (1f - t);
            case AnimationType.EaseInOut:
                return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            default:
                return t;
        }
    }

    // Сбросить счетчики показанных картинок (например, при начале новой игры)
    public void ResetCounters()
    {
        goodImagesShown = 0;
        neutralImagesShown = 0;
        badImagesShown = 0;

        PlayerPrefs.DeleteKey(GOOD_COUNT_KEY);
        PlayerPrefs.DeleteKey(NEUTRAL_COUNT_KEY);
        PlayerPrefs.DeleteKey(BAD_COUNT_KEY);
        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log("ImagePopupManager: Счетчики сброшены и PlayerPrefs очищены");
        }
    }

    // Получить количество показанных картинок для типа прохождения
    public int GetShownCount(string performanceType)
    {
        switch (performanceType)
        {
            case "Хорошее":
                return goodImagesShown;
            case "Нейтральное":
                return neutralImagesShown;
            case "Плохое":
                return badImagesShown;
            default:
                return 0;
        }
    }
}