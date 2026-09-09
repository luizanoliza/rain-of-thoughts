using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiChapterManager : MonoBehaviour
{
    [System.Serializable]
    public class ChapterColors
    {
        [Tooltip("Первый цвет для главы")]
        public Color color1 = Color.white;

        [Tooltip("Второй цвет для главы")]
        public Color color2 = Color.white;
    }

    [Header("UI объекты для каждой главы")]
    [Tooltip("UI объект для Главы 0")]
    public GameObject chapterUI0;

    [Tooltip("UI объект для Главы 1")]
    public GameObject chapterUI1;

    [Tooltip("UI объект для Главы 2")]
    public GameObject chapterUI2;

    [Tooltip("UI объект для Главы 3")]
    public GameObject chapterUI3;

    [Tooltip("UI объект для Главы 4")]
    public GameObject chapterUI4;

    [Header("Цвета для каждой главы")]
    [Tooltip("Цвета для Главы 0")]
    public ChapterColors colorsChapter0 = new ChapterColors();

    [Tooltip("Цвета для Главы 1")]
    public ChapterColors colorsChapter1 = new ChapterColors();

    [Tooltip("Цвета для Главы 2")]
    public ChapterColors colorsChapter2 = new ChapterColors();

    [Tooltip("Цвета для Главы 3")]
    public ChapterColors colorsChapter3 = new ChapterColors();

    [Tooltip("Цвета для Главы 4")]
    public ChapterColors colorsChapter4 = new ChapterColors();

    [Header("Объекты для применения цветов")]
    [Tooltip("Объекты для Color1 (включая всех детей)")]
    public List<GameObject> color1Objects = new List<GameObject>();

    [Tooltip("Объекты для Color2 (включая всех детей)")]
    public List<GameObject> color2Objects = new List<GameObject>();

    [Header("Настройки анимации")]
    [Tooltip("Длительность fade out (исчезновение)")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("Длительность fade in (появление)")]
    public float fadeInDuration = 0.5f;

    [Tooltip("Длительность перехода цветов")]
    public float colorTransitionDuration = 0.5f;

    [Tooltip("Кривая анимации для fade")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Кривая анимации для цветов")]
    public AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Текст главы")]
    [Tooltip("Текстовое поле для вывода описания главы")]
    public TextMeshProUGUI chapterDescriptionText;

    [Tooltip("Текст для Главы 0")]
    [TextArea(2, 4)]
    public string textChapter0 = "Фокус на ресурсах. Шукатиму те, що підбадьорює мене зараз.";

    [Tooltip("Текст для Главы 1")]
    [TextArea(2, 4)]
    public string textChapter1 = "Віднайти дію. Знайти рутини та дії, що допоможуть покращити мій фізичний стан.";

    [Tooltip("Текст для Главы 2")]
    [TextArea(2, 4)]
    public string textChapter2 = "Аналіз минулого. Зосереджуся на важливих подіях життя";

    [Tooltip("Текст для Главы 3")]
    [TextArea(2, 4)]
    public string textChapter3 = "Знешкодження самокритики. Знайти прикметники, якими я ображаю себе, щоб їх переписати.";

    [Tooltip("Текст для Главы 4")]
    [TextArea(2, 4)]
    public string textChapter4 = "Ядро конфлікту. Визначу ключові теми, які найбільше турбують та стоять на шляху.";

    [Tooltip("Анимировать появление текста при переходах")]
    public bool animateText = true;

    [Tooltip("Скорость печати текста (секунд на букву)")]
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.05f;

    [Header("Настройки")]
    [Tooltip("Ссылка на GridSpawner")]
    public GridSpawner gridSpawner;

    [Tooltip("Автоматически отслеживать смену главы")]
    public bool autoUpdateChapter = true;

    [Tooltip("Интервал проверки (секунды)")]
    public float updateInterval = 0.5f;

    private int currentChapterIndex = -1;
    private float nextUpdateTime = 0f;
    private Coroutine transitionCoroutine;
    private bool isFirstInitialization = true;

    // Хранение текущих цветов для плавного перехода
    private Color currentColor1;
    private Color currentColor2;

    // Словарь для хранения CanvasGroup компонентов
    private Dictionary<GameObject, CanvasGroup> canvasGroups = new Dictionary<GameObject, CanvasGroup>();

    private void Start()
    {
        if (gridSpawner == null)
        {
            gridSpawner = FindObjectOfType<GridSpawner>();
        }

        // Инициализируем CanvasGroup для всех UI объектов
        InitializeCanvasGroups();

        // Устанавливаем начальные цвета
        if (gridSpawner != null)
        {
            ChapterColors initialColors = GetColorsForChapter(gridSpawner.startChapterIndex);
            if (initialColors != null)
            {
                currentColor1 = initialColors.color1;
                currentColor2 = initialColors.color2;
            }

            // Первая инициализация БЕЗ анимации
            InitializeFirstChapter(gridSpawner.startChapterIndex);
        }
    }

    private void InitializeFirstChapter(int chapterIndex)
    {
        currentChapterIndex = chapterIndex;
        isFirstInitialization = false;

        // Деактивируем все UI
        DeactivateAllChapterUIs();

        // Активируем нужный UI с полной видимостью (без анимации)
        GameObject startUI = GetChapterUI(chapterIndex);
        if (startUI != null)
        {
            startUI.SetActive(true);
            if (canvasGroups.ContainsKey(startUI))
            {
                canvasGroups[startUI].alpha = 1f;
            }
        }

        // Подготавливаем текст для печати
        if (chapterDescriptionText != null)
        {
            PrepareChapterText(chapterIndex);

            // Если анимация включена, печатаем текст
            if (animateText)
            {
                StartCoroutine(TypewriterText(chapterIndex));
            }
            else
            {
                // Иначе показываем сразу
                UpdateChapterText(chapterIndex);
            }
        }

        // Применяем цвета сразу
        ChapterColors colors = GetColorsForChapter(chapterIndex);
        if (colors != null)
        {
            ApplyColorsImmediate(colors.color1, colors.color2);
        }

        Debug.Log($"UiChapterManager: Инициализирована Глава {chapterIndex}");
    }

    private void DeactivateAllChapterUIs()
    {
        if (chapterUI0 != null) chapterUI0.SetActive(false);
        if (chapterUI1 != null) chapterUI1.SetActive(false);
        if (chapterUI2 != null) chapterUI2.SetActive(false);
        if (chapterUI3 != null) chapterUI3.SetActive(false);
        if (chapterUI4 != null) chapterUI4.SetActive(false);
    }

    private void Update()
    {
        if (!autoUpdateChapter || gridSpawner == null) return;

        if (Time.time >= nextUpdateTime)
        {
            int chapterIndex = GetCurrentChapterIndexFromSpawner();
            if (chapterIndex != currentChapterIndex && chapterIndex >= 0)
            {
                SetChapter(chapterIndex);
            }

            nextUpdateTime = Time.time + Mathf.Max(0.1f, updateInterval);
        }
    }

    private void InitializeCanvasGroups()
    {
        canvasGroups.Clear();

        AddCanvasGroupIfNeeded(chapterUI0);
        AddCanvasGroupIfNeeded(chapterUI1);
        AddCanvasGroupIfNeeded(chapterUI2);
        AddCanvasGroupIfNeeded(chapterUI3);
        AddCanvasGroupIfNeeded(chapterUI4);
    }

    private void AddCanvasGroupIfNeeded(GameObject obj)
    {
        if (obj == null) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        canvasGroups[obj] = cg;
    }

    private int GetCurrentChapterIndexFromSpawner()
    {
        if (gridSpawner == null) return -1;

        try
        {
            var field = typeof(GridSpawner).GetField("currentChapterIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return (int)field.GetValue(gridSpawner);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UiChapterManager: Не удалось получить currentChapterIndex: {e.Message}");
        }

        return -1;
    }

    public void SetChapter(int chapterIndex)
    {
        if (chapterIndex == currentChapterIndex) return;

        // Останавливаем предыдущую анимацию если есть
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        GameObject oldUI = GetChapterUI(currentChapterIndex);
        GameObject newUI = GetChapterUI(chapterIndex);
        ChapterColors newColors = GetColorsForChapter(chapterIndex);

        int previousChapterIndex = currentChapterIndex;
        currentChapterIndex = chapterIndex;

        // Запускаем анимацию перехода
        transitionCoroutine = StartCoroutine(TransitionToChapter(oldUI, newUI, newColors, previousChapterIndex));

        Debug.Log($"UiChapterManager: Переход на Главу {chapterIndex}");
    }

    private IEnumerator TransitionToChapter(GameObject oldUI, GameObject newUI, ChapterColors newColors, int oldChapterIndex)
    {
        // Исчезновение старого UI
        if (oldUI != null && canvasGroups.ContainsKey(oldUI))
        {
            yield return StartCoroutine(FadeOut(canvasGroups[oldUI]));
            oldUI.SetActive(false);
        }

        // Небольшая пауза между переходами (опционально, не известно надо ли пока что)
        yield return new WaitForSeconds(0.1f);

        // Появление нового UI и изменение цветов одновременно
        if (newUI != null)
        {
            newUI.SetActive(true);

            if (canvasGroups.ContainsKey(newUI))
            {
                canvasGroups[newUI].alpha = 0f;
            }
        }

        // Подготавливаем текст главы (очищаем его)
        PrepareChapterText(currentChapterIndex);

        // Запускаем fade in и изменение цветов параллельно
        Coroutine fadeCoroutine = null;
        Coroutine colorCoroutine = null;
        Coroutine textCoroutine = null;

        if (newUI != null && canvasGroups.ContainsKey(newUI))
        {
            fadeCoroutine = StartCoroutine(FadeIn(canvasGroups[newUI]));
        }

        if (newColors != null)
        {
            colorCoroutine = StartCoroutine(TransitionColors(currentColor1, currentColor2, newColors.color1, newColors.color2));
        }

        if (animateText && chapterDescriptionText != null)
        {
            textCoroutine = StartCoroutine(TypewriterText(currentChapterIndex));
        }
        else
        {
            UpdateChapterText(currentChapterIndex);
        }

        // Ждем окончания всех анимаций
        if (fadeCoroutine != null)
        {
            yield return fadeCoroutine;
        }

        if (colorCoroutine != null)
        {
            yield return colorCoroutine;
        }

        if (textCoroutine != null)
        {
            yield return textCoroutine;
        }

        // Обновляем текущие цвета
        if (newColors != null)
        {
            currentColor1 = newColors.color1;
            currentColor2 = newColors.color2;
        }

        transitionCoroutine = null;
    }

    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float curveValue = fadeCurve.Evaluate(t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float curveValue = fadeCurve.Evaluate(t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator TransitionColors(Color fromColor1, Color fromColor2, Color toColor1, Color toColor2)
    {
        float elapsed = 0f;

        while (elapsed < colorTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / colorTransitionDuration);
            float curveValue = colorCurve.Evaluate(t);

            Color lerpedColor1 = Color.Lerp(fromColor1, toColor1, curveValue);
            Color lerpedColor2 = Color.Lerp(fromColor2, toColor2, curveValue);

            ApplyColorsImmediate(lerpedColor1, lerpedColor2);

            yield return null;
        }

        // Финальное применение точных цветов
        ApplyColorsImmediate(toColor1, toColor2);
    }

    private void ApplyColorsImmediate(Color color1, Color color2)
    {
        // Применяем Color1
        foreach (var obj in color1Objects)
        {
            if (obj != null)
            {
                ApplyColorToObjectAndChildren(obj, color1);
            }
        }

        // Применяем Color2
        foreach (var obj in color2Objects)
        {
            if (obj != null)
            {
                ApplyColorToObjectAndChildren(obj, color2);
            }
        }
    }

    private void ApplyColorToObjectAndChildren(GameObject obj, Color color)
    {
        if (obj == null) return;

        // Image
        var images = obj.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            img.color = color;
        }

        // TextMeshProUGUI
        var tmpTexts = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in tmpTexts)
        {
            txt.color = color;
        }

        // Text (leg.)
        var texts = obj.GetComponentsInChildren<Text>(true);
        foreach (var txt in texts)
        {
            txt.color = color;
        }

        // SpriteRenderer
        var sprites = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            sprite.color = color;
        }

        // RawImage
        var rawImages = obj.GetComponentsInChildren<RawImage>(true);
        foreach (var rawImg in rawImages)
        {
            rawImg.color = color;
        }
    }

    private void PrepareChapterText(int chapterIndex)
    {
        if (chapterDescriptionText == null) return;

        // Очищаем текст и делаем его полностью видимым
        chapterDescriptionText.text = "";
        Color c = chapterDescriptionText.color;
        c.a = 1f;
        chapterDescriptionText.color = c;
    }

    private void UpdateChapterText(int chapterIndex)
    {
        if (chapterDescriptionText == null) return;

        string text = GetTextForChapter(chapterIndex);

        if (!string.IsNullOrEmpty(text))
        {
            chapterDescriptionText.text = text;

            // Устанавливаем полную видимость
            Color c = chapterDescriptionText.color;
            c.a = 1f;
            chapterDescriptionText.color = c;
        }
    }

    private string GetTextForChapter(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 0: return textChapter0;
            case 1: return textChapter1;
            case 2: return textChapter2;
            case 3: return textChapter3;
            case 4: return textChapter4;
            default: return "";
        }
    }

    private IEnumerator TypewriterText(int chapterIndex)
    {
        if (chapterDescriptionText == null) yield break;

        string fullText = GetTextForChapter(chapterIndex);
        if (string.IsNullOrEmpty(fullText)) yield break;

        // Убеждаемся, что текст полностью видим
        Color c = chapterDescriptionText.color;
        c.a = 1f;
        chapterDescriptionText.color = c;

        // Очищаем текст
        chapterDescriptionText.text = "";

        // Печатаем по одной букве
        for (int i = 0; i <= fullText.Length; i++)
        {
            chapterDescriptionText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Убеждаемся, что весь текст отображен
        chapterDescriptionText.text = fullText;
    }

    private GameObject GetChapterUI(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 0: return chapterUI0;
            case 1: return chapterUI1;
            case 2: return chapterUI2;
            case 3: return chapterUI3;
            case 4: return chapterUI4;
            default: return null;
        }
    }

    private ChapterColors GetColorsForChapter(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 0: return colorsChapter0;
            case 1: return colorsChapter1;
            case 2: return colorsChapter2;
            case 3: return colorsChapter3;
            case 4: return colorsChapter4;
            default: return null;
        }
    }

    public void RefreshColors()
    {
        if (currentChapterIndex >= 0)
        {
            ChapterColors colors = GetColorsForChapter(currentChapterIndex);
            if (colors != null)
            {
                ApplyColorsImmediate(colors.color1, colors.color2);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test: Глава 0")]
    private void TestChapter0() { SetChapter(0); }

    [ContextMenu("Test: Глава 1")]
    private void TestChapter1() { SetChapter(1); }

    [ContextMenu("Test: Глава 2")]
    private void TestChapter2() { SetChapter(2); }

    [ContextMenu("Test: Глава 3")]
    private void TestChapter3() { SetChapter(3); }

    [ContextMenu("Test: Глава 4")]
    private void TestChapter4() { SetChapter(4); }

    [ContextMenu("Refresh Colors")]
    private void TestRefreshColors() { RefreshColors(); }

    [ContextMenu("Update Text")]
    private void TestUpdateText() { UpdateChapterText(currentChapterIndex); }
#endif
}