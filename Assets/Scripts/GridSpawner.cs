using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;

// Маркер для объектов префаба. Хранит идентификатор ряда
public class RowMarker : MonoBehaviour
{
    public int rowId;
}

public class GridSpawner : MonoBehaviour
{
    // По префабу на главу
    [Header("Prefabs для каждой главы")]
    [Tooltip("Префабы для Главы 0")]
    public GameObject[] prefabsChapter0;
    [Tooltip("Префабы для Главы 1")]
    public GameObject[] prefabsChapter1;
    [Tooltip("Префабы для Главы 2")]
    public GameObject[] prefabsChapter2;
    [Tooltip("Префабы для Главы 3")]
    public GameObject[] prefabsChapter3;
    [Tooltip("Префабы для Главы 4")]
    public GameObject[] prefabsChapter4;

    [Header("Grid Settings")]
    public RectTransform parentRect;
    public int columns = 3;
    public int initialRows = 10;

    [Header("Начальная пачка")]
    [Tooltip("Сколько рядов спавнится сразу при старте уровня (будет не больше initialRows если он >=0)")]
    public int initialSpawnPack = 3;

    [Header("Размеры (пиксели)")]
    public Vector2 prefabSize = new Vector2(200f, 100f);
    public float spacingX = 20f;
    public float spacingY = 20f;

    [Header("Файл истории")]
    [Tooltip("Имя текстового файла (пример: story.txt). Попробует Resources, StreamingAssets, затем поиск в Assets.")]
    public string storyFileName = "story.txt";

    [Header("Начальная глава")]
    public int startChapterIndex = 0;

    [Header("Скорость / сложность")]
    public float minFallSpeed = 50f;
    public float maxFallSpeed = 200f;
    public float levelDuration = 60f;
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Клики (попытки)")]
    [Tooltip("<= 0 - неограничено")]
    public int maxClicks = 10;

    [Header("UI")]
    public TextMeshProUGUI countersText;
    public TextMeshProUGUI remainingClicksText;
    public TextMeshProUGUI responseText;

    [Header("Слайдер скорости")]
    [Tooltip("Слайдер, который уменьшается на скорость падения каждую секунду")]
    public UnityEngine.UI.Slider speedSlider;
    [Tooltip("Максимальное значение слайдера")]
    public float sliderMaxValue = 1000f;
    [Tooltip("Текст для отображения текущего значения слайдера (опционально)")]
    public TextMeshProUGUI sliderValueText;
    [Tooltip("Длительность анимации заполнения слайдера в секундах")]
    public float sliderFillDuration = 2f;

    [Header("Анимация Response Text")]
    [Tooltip("Анимировать появление response текста при кликах")]
    public bool animateResponseText = true;
    [Tooltip("Скорость печати текста (секунд на букву)")]
    [Range(0.01f, 0.2f)]
    public float responseTypewriterSpeed = 0.05f;

    [Header("Аудио")]
    [Tooltip("Звук при клике на любой триггер (проигрывается один раз)")]
    public AudioClip clickSound;
    [Tooltip("Звук при неправильном ответе (проигрывается один раз)")]
    public AudioClip wrongAnswerSound;
    [Tooltip("Звук во время печати текста (проигрывается в цикле)")]
    public AudioClip typingSound;

    [Header("Screenshake при неправильном ответе")]
    [Tooltip("Включить эффект тряски при неправильном ответе")]
    public bool enableScreenshake = true;
    [Tooltip("Сила тряски (амплитуда смещения в пикселях)")]
    [Range(5f, 100f)]
    public float shakeIntensity = 25f;
    [Tooltip("Длительность тряски в секундах")]
    [Range(0.1f, 2f)]
    public float shakeDuration = 0.4f;
    [Tooltip("Частота колебаний (циклов в секунду)")]
    [Range(10f, 60f)]
    public float shakeFrequency = 30f;
    [Tooltip("Скорость затухания (чем выше, тем быстрее затухает)")]
    [Range(1f, 10f)]
    public float shakeDecay = 3f;
    [Tooltip("Применять тряску по оси X")]
    public bool shakeAxisX = true;
    [Tooltip("Применять тряску по оси Y")]
    public bool shakeAxisY = true;
    [Tooltip("Применять вращение при тряске")]
    public bool shakeRotation = false;
    [Tooltip("Максимальный угол вращения при тряске (градусы)")]
    [Range(0f, 10f)]
    public float shakeRotationAmount = 2f;

    [Header("Центр спавна")]
    public RectTransform spawnCenterRect;

    [Header("Поведение")]
    public bool infiniteRows = false;

    [Header("Оптимизация: удаление упавших объектов")]
    [Tooltip("Если у префаба anchoredPosition.y станет меньше этой величины, объект будет уничтожен.")]
    public float destroyYThreshold = -1500f;
    [Tooltip("Интервал проверки на удаление (в секундах).")]
    public float cleanupInterval = 0.25f;

    [Header("Система показа картинок")]
    [Tooltip("Менеджер для показа картинок между уровнями")]
    public ImagePopupManager imagePopupManager;

    [Tooltip("Показывать картинки каждые N уровней (0 = отключено, -1 = авто)")]
    [Range(-1, 10)]
    public int showImageEveryNLevels = -1; // -1 = автоматически (зависит от количества уровней в главе)

    [Tooltip("Менять главу во время показа картинки (префабы, цвета загружаются в фоне)")]
    public bool changeChapterDuringImage = true;

    [Tooltip("Включить дебаг информацию о показе картинок")]
    public bool debugImageSystem = false;

    [HideInInspector]
    public float clickSpeedMultiplier = 1f;

    public float maxClickSpeedMultiplier = 10f;
    public float level1Multiplier = 1.25f;
    public float level2Multiplier = 1.125f;
    public float level3Multiplier = 1.0f;

    // SCREENSHAKE система
    private class ScreenshakeController
    {
        private RectTransform target;
        private Vector2 originalPosition;
        private float originalRotation;

        private float intensity;
        private float duration;
        private float frequency;
        private float decay;
        private bool axisX;
        private bool axisY;
        private bool rotation;
        private float rotationAmount;

        private float elapsedTime;
        private float randomSeed;
        private bool isActive;

        public bool IsActive => isActive;

        public ScreenshakeController(RectTransform targetTransform)
        {
            target = targetTransform;
            isActive = false;
        }

        public void StartShake(float intensity, float duration, float frequency, float decay,
                              bool axisX, bool axisY, bool rotation, float rotationAmount)
        {
            if (target == null) return;

            this.intensity = intensity;
            this.duration = duration;
            this.frequency = frequency;
            this.decay = decay;
            this.axisX = axisX;
            this.axisY = axisY;
            this.rotation = rotation;
            this.rotationAmount = rotationAmount;

            elapsedTime = 0f;
            randomSeed = UnityEngine.Random.Range(0f, 1000f);
            originalPosition = target.anchoredPosition;
            originalRotation = target.localEulerAngles.z;

            isActive = true;
        }

        public void Update()
        {
            if (!isActive || target == null) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= duration)
            {
                StopShake();
                return;
            }

            float progress = elapsedTime / duration;
            float decayFactor = Mathf.Pow(1f - progress, decay);
            float currentIntensity = intensity * decayFactor;

            Vector2 offset = Vector2.zero;
            float time = elapsedTime * frequency;

            if (axisX)
            {
                float noiseX = Mathf.PerlinNoise(randomSeed + time, 0f) * 2f - 1f;
                offset.x = noiseX * currentIntensity;
            }

            if (axisY)
            {
                float noiseY = Mathf.PerlinNoise(0f, randomSeed + time) * 2f - 1f;
                offset.y = noiseY * currentIntensity;
            }

            target.anchoredPosition = originalPosition + offset;

            if (rotation && rotationAmount > 0f)
            {
                float noiseRot = Mathf.PerlinNoise(randomSeed + time * 0.7f, randomSeed + time * 0.7f) * 2f - 1f;
                float rotOffset = noiseRot * rotationAmount * decayFactor;
                target.localEulerAngles = new Vector3(0f, 0f, originalRotation + rotOffset);
            }
        }

        public void StopShake()
        {
            if (!isActive || target == null) return;

            isActive = false;
            target.anchoredPosition = originalPosition;

            if (rotation)
            {
                target.localEulerAngles = new Vector3(0f, 0f, originalRotation);
            }
        }

        public void ForceStop()
        {
            if (target == null) return;
            isActive = false;
        }
    }

    private ScreenshakeController screenshake;

    public class SpawnTrigger
    {
        public StoryDataLoader.TriggerData data;
        public int chapterIndex;
    }

    private List<SpawnTrigger> allTriggers = new List<SpawnTrigger>();
    private List<StoryDataLoader.ChapterData> loadedChapters = new List<StoryDataLoader.ChapterData>();
    private int currentChapterIndex = 0;
    private StoryDataLoader.ChapterData currentChapter;
    private int currentLevelInChapter = 0;
    private float[] cumulativeWeights;
    private Dictionary<string, int> counts = new Dictionary<string, int>();
    private int totalClicks = 0;
    private HashSet<string> uniqueTriggersClickedInChapter = new HashSet<string>();
    private float nextCleanupTime = 0f;

    private int nextRowId = 1;
    private Dictionary<int, int> rowChildCounts = new Dictionary<int, int>();
    private int remainingRowsToSpawn = 0;
    private bool infiniteSpawnMode = false;
    private bool levelActive = false;
    private float levelStartTime = 0f;
    private bool isAnimatingClicks = false;
    private Coroutine responseTextCoroutine;

    // Отслеживание для системы картинок
    private int completedLevelsInChapter = 0;
    private List<string> performanceHistory = new List<string>(); // История прохождений для текущей главы

    // Флаг для пропуска анимаций восполнения (если они уже прошли во время картинки)
    private bool skipRefillAnimations = false;

    // Аудио компоненты
    private AudioSource clickAudioSource;
    private AudioSource wrongAnswerAudioSource;
    private AudioSource typingAudioSource;

    // Слайдер (новые переменные для плавной анимации)
    private float currentSliderValue;
    private float targetSliderValue;
    private bool isSliderAnimating = false;
    private Coroutine sliderFillCoroutine;

    private class SpeedApplier
    {
        public Component target;
        public MethodInfo method;
        public FieldInfo field;
        public PropertyInfo prop;

        public GameObject OwnerGameObject => target != null ? target.gameObject : null;

        public void Apply(float v)
        {
            if (target == null) return;
            try
            {
                if (method != null)
                {
                    method.Invoke(target, new object[] { v });
                    return;
                }
                if (field != null)
                {
                    if (field.FieldType == typeof(float))
                        field.SetValue(target, v);
                    else
                    {
                        object conv = Convert.ChangeType(v, field.FieldType);
                        field.SetValue(target, conv);
                    }
                    return;
                }
                if (prop != null && prop.CanWrite)
                {
                    if (prop.PropertyType == typeof(float))
                        prop.SetValue(target, v);
                    else
                    {
                        object conv = Convert.ChangeType(v, prop.PropertyType);
                        prop.SetValue(target, conv);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SpeedApplier: не удалось применить скорость к {target.GetType().Name}: {e.Message}");
            }
        }
    }

    private List<SpeedApplier> activeSpeedAppliers = new List<SpeedApplier>();
    private float lastAppliedSpeed = -1f;
    private const float SPEED_EPS = 0.01f;

    // Получение префабов для текущей главы
    private GameObject[] GetPrefabsForCurrentChapter()
    {
        switch (currentChapterIndex)
        {
            case 0:
                if (prefabsChapter0 != null && prefabsChapter0.Length > 0)
                    return prefabsChapter0;
                break;
            case 1:
                if (prefabsChapter1 != null && prefabsChapter1.Length > 0)
                    return prefabsChapter1;
                break;
            case 2:
                if (prefabsChapter2 != null && prefabsChapter2.Length > 0)
                    return prefabsChapter2;
                break;
            case 3:
                if (prefabsChapter3 != null && prefabsChapter3.Length > 0)
                    return prefabsChapter3;
                break;
            case 4:
                if (prefabsChapter4 != null && prefabsChapter4.Length > 0)
                    return prefabsChapter4;
                break;
        }

        Debug.LogError($"GridSpawner: Нет префабов для главы {currentChapterIndex}!");
        return null;
    }

    private void Start()
    {
        if (parentRect == null)
        {
            Debug.LogError("GridSpawner: parentRect не назначен.");
            return;
        }

        if (spawnCenterRect == null)
        {
            RectTransform selfRt = GetComponent<RectTransform>();
            spawnCenterRect = selfRt != null ? selfRt : parentRect;
        }

        screenshake = new ScreenshakeController(parentRect);
        InitializeAudioSources();
        InitializeSlider();

        TryLoadStoryFile();
        LoadChapter(startChapterIndex);

        nextCleanupTime = Time.time + cleanupInterval;
    }

    private void InitializeAudioSources()
    {
        float sfxVolume = PlayerPrefs.GetFloat("Volume_SFX", 1f);

        if (clickSound != null)
        {
            clickAudioSource = gameObject.AddComponent<AudioSource>();
            clickAudioSource.clip = clickSound;
            clickAudioSource.playOnAwake = false;
            clickAudioSource.loop = false;
            clickAudioSource.volume = sfxVolume;
        }

        if (wrongAnswerSound != null)
        {
            wrongAnswerAudioSource = gameObject.AddComponent<AudioSource>();
            wrongAnswerAudioSource.clip = wrongAnswerSound;
            wrongAnswerAudioSource.playOnAwake = false;
            wrongAnswerAudioSource.loop = false;
            wrongAnswerAudioSource.volume = sfxVolume;
        }

        if (typingSound != null)
        {
            typingAudioSource = gameObject.AddComponent<AudioSource>();
            typingAudioSource.clip = typingSound;
            typingAudioSource.playOnAwake = false;
            typingAudioSource.loop = true;
            typingAudioSource.volume = sfxVolume;
        }
    }

    private void InitializeSlider()
    {
        if (speedSlider != null)
        {
            speedSlider.maxValue = sliderMaxValue;
            speedSlider.minValue = 0f;
            currentSliderValue = 0f;
            targetSliderValue = 0f;
            speedSlider.value = 0f;

            UpdateSliderValueText();
        }
    }

    private void Update()
    {
        if (Time.time >= nextCleanupTime)
        {
            CleanupFallenObjects();
            nextCleanupTime = Time.time + Mathf.Max(0.01f, cleanupInterval);
        }

        float targetSpeed = GetCurrentFallSpeed() * Mathf.Max(0.0001f, clickSpeedMultiplier);

        if (Mathf.Abs(targetSpeed - lastAppliedSpeed) > SPEED_EPS)
        {
            ApplySpeedToActive(targetSpeed);
            lastAppliedSpeed = targetSpeed;
        }

        if (screenshake != null && screenshake.IsActive)
        {
            screenshake.Update();
        }

        UpdateSliderSmooth();
    }

    private void UpdateSliderSmooth()
    {
        if (speedSlider == null || !levelActive || isSliderAnimating) return;

        float currentSpeed = GetCurrentFallSpeed() * Mathf.Max(0.0001f, clickSpeedMultiplier);
        targetSliderValue -= currentSpeed * Time.deltaTime;
        targetSliderValue = Mathf.Max(0f, targetSliderValue);
        currentSliderValue = Mathf.Lerp(currentSliderValue, targetSliderValue, Time.deltaTime * 10f);
        speedSlider.value = currentSliderValue;
        UpdateSliderValueText();

        if (currentSliderValue <= 0.1f && targetSliderValue <= 0f)
        {
            OnSliderReachedZero();
        }
    }

    private void UpdateSliderValueText()
    {
        if (sliderValueText != null)
        {
            sliderValueText.text = Mathf.CeilToInt(currentSliderValue).ToString();
        }
    }

    private void OnSliderReachedZero()
    {
        Debug.Log("Слайдер достиг нуля!");

        if (levelActive)
        {
            StartCoroutine(EndLevelSequence());
        }
    }

    private void ResetSlider()
    {
        if (speedSlider != null)
        {
            if (sliderFillCoroutine != null)
            {
                StopCoroutine(sliderFillCoroutine);
            }

            sliderFillCoroutine = StartCoroutine(AnimateSliderFill());
        }
    }

    private IEnumerator AnimateSliderFill()
    {
        if (speedSlider == null) yield break;

        isSliderAnimating = true;

        float startValue = currentSliderValue;
        targetSliderValue = sliderMaxValue;

        float elapsed = 0f;
        float duration = sliderFillDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Интерполируем от текущего значения до максимума
            currentSliderValue = Mathf.Lerp(startValue, sliderMaxValue, smoothT);
            targetSliderValue = currentSliderValue;
            speedSlider.value = currentSliderValue;

            UpdateSliderValueText();

            yield return null;
        }

        currentSliderValue = sliderMaxValue;
        targetSliderValue = sliderMaxValue;
        speedSlider.value = sliderMaxValue;
        UpdateSliderValueText();

        isSliderAnimating = false;
        sliderFillCoroutine = null;
    }

    private void ApplySpeedToActive(float speed)
    {
        for (int i = activeSpeedAppliers.Count - 1; i >= 0; i--)
        {
            if (activeSpeedAppliers[i].target == null)
                activeSpeedAppliers.RemoveAt(i);
        }

        for (int i = 0; i < activeSpeedAppliers.Count; i++)
        {
            activeSpeedAppliers[i].Apply(speed);
        }
    }

    private void TryLoadStoryFile()
    {
        if (string.IsNullOrEmpty(storyFileName))
        {
            Debug.LogWarning("GridSpawner: storyFileName пуст.");
            return;
        }

        string txt = StoryDataLoader.LoadTextFile(storyFileName);
        if (string.IsNullOrEmpty(txt))
        {
            Debug.LogWarning("GridSpawner: не удалось загрузить story файл: " + storyFileName);
            return;
        }

        loadedChapters = StoryDataLoader.Parse(txt);
        BuildAllTriggers();
        Debug.Log($"GridSpawner: загружено глав: {loadedChapters.Count}, триггеров всего: {allTriggers.Count}");
    }

    private void BuildAllTriggers()
    {
        allTriggers.Clear();
        for (int ci = 0; ci < loadedChapters.Count; ci++)
        {
            var ch = loadedChapters[ci];
            if (ch == null || ch.triggers == null) continue;
            foreach (var t in ch.triggers)
            {
                allTriggers.Add(new SpawnTrigger { data = t, chapterIndex = ci });
            }
        }

        counts.Clear();
        foreach (var st in allTriggers)
        {
            var name = st.data?.Name ?? "unnamed";
            if (!counts.ContainsKey(name)) counts[name] = 0;
        }
    }

    private void LoadChapter(int index)
    {
        if (currentChapter != null && uniqueTriggersClickedInChapter.Count > 0)
        {
            LogChapterResults(currentChapterIndex, uniqueTriggersClickedInChapter.Count);
        }

        if (loadedChapters == null || loadedChapters.Count == 0)
        {
            Debug.LogError("GridSpawner: нет загруженных глав.");
            return;
        }

        if (index < 0 || index >= loadedChapters.Count)
        {
            Debug.Log("GridSpawner: главы закончились - завершаем игру.");
            EndGame();
            return;
        }

        currentChapterIndex = index;
        currentChapter = loadedChapters[currentChapterIndex];
        currentLevelInChapter = 0;

        // Сброс счетчиков для новой главы
        completedLevelsInChapter = 0;
        performanceHistory.Clear();

        counts.Clear();
        totalClicks = 0;
        clickSpeedMultiplier = 1f;
        uniqueTriggersClickedInChapter.Clear();

        foreach (var st in allTriggers)
        {
            var name = st.data?.Name ?? "unnamed";
            if (!counts.ContainsKey(name)) counts[name] = 0;
        }

        if (countersText != null)
            countersText.text = "";
        if (responseText != null)
            responseText.text = "";

        PrepareWeights();

        var prefabsForChapter = GetPrefabsForCurrentChapter();
        if (prefabsForChapter == null || prefabsForChapter.Length == 0)
        {
            Debug.LogError($"GridSpawner: Нет префабов для главы {currentChapterIndex}. Невозможно начать уровень.");
            return;
        }

        Debug.Log($"GridSpawner: Загружена глава {currentChapterIndex}, префабов: {prefabsForChapter.Length}");

        StartLevel();
    }

    private void LogChapterResults(int chapterIndex, int uniqueTriggerCount)
    {
        string performance = GetChapterPerformance(chapterIndex, uniqueTriggerCount);
        Debug.Log($"Глава {chapterIndex}: Уникальных триггеров нажато: {uniqueTriggerCount} | Тип прохождения: {performance}");
    }

    private string GetChapterPerformance(int chapterIndex, int triggerCount)
    {
        switch (chapterIndex) //(тут дурдом начинается)
        {
            case 0:
                if (triggerCount >= 15 && triggerCount <= 18) return "Хорошее";
                if (triggerCount < 12) return "Плохое";
                if (triggerCount >= 12 && triggerCount < 15) return "Нейтральное";
                break;
            case 1:
                if (triggerCount >= 27 && triggerCount <= 36) return "Хорошее";
                if (triggerCount < 20) return "Плохое";
                if (triggerCount >= 20 && triggerCount < 27) return "Нейтральное";
                break;
            case 2:
                if (triggerCount >= 27 && triggerCount <= 33) return "Хорошее";
                if (triggerCount < 21) return "Плохое";
                if (triggerCount >= 21 && triggerCount < 27) return "Нейтральное";
                break;
            case 3:
                if (triggerCount >= 10 && triggerCount <= 12) return "Хорошее";
                if (triggerCount < 8) return "Плохое";
                if (triggerCount >= 8 && triggerCount < 10) return "Нейтральное";
                break;
            case 4:
                if (triggerCount >= 5 && triggerCount <= 6) return "Хорошее";
                if (triggerCount < 4) return "Плохое";
                if (triggerCount == 4) return "Нейтральное";
                break;
            default:
                return "Неизвестная глава";
        }

        return "Нейтральное";
    }

    private void StartLevel()
    {
        levelStartTime = Time.time;
        levelActive = true;

        ClearAllSpawned();

        var keys = new List<string>(counts.Keys);
        foreach (var k in keys) counts[k] = 0;
        totalClicks = 0;
        clickSpeedMultiplier = 1f;
        UpdateCountersText();

        //  Запускаем анимации только если они не были запущены во время картинки
        if (!skipRefillAnimations)
        {
            StartCoroutine(AnimateRemainingClicks());
            ResetSlider();
        }
        else
        {
            // Сбрасываем флаг для следующего уровня
            skipRefillAnimations = false;
            Debug.Log("[ImageSystem] Анимации восполнения пропущены (уже прошли во время картинки)");
        }

        rowChildCounts.Clear();
        nextRowId = 1;

        if (initialRows == -1)
        {
            infiniteSpawnMode = true;
            remainingRowsToSpawn = 0;
        }
        else
        {
            infiniteSpawnMode = false;
            remainingRowsToSpawn = Mathf.Max(0, initialRows - Mathf.Max(1, initialSpawnPack));
        }

        int pack = Mathf.Max(1, initialSpawnPack);
        if (!infiniteSpawnMode && initialRows >= 0)
            pack = Mathf.Min(pack, initialRows);

        for (int i = 0; i < pack; i++)
            SpawnRowAbove();

        lastAppliedSpeed = -1f;
    }

    private float GetTopRowY()
    {
        float topY = float.MinValue;
        for (int i = 0; i < parentRect.childCount; i++)
        {
            RectTransform childRt = parentRect.GetChild(i) as RectTransform;
            if (childRt == null) continue;
            if (childRt.anchoredPosition.y > topY) topY = childRt.anchoredPosition.y;
        }

        if (topY == float.MinValue)
        {
            if (spawnCenterRect != null) return spawnCenterRect.anchoredPosition.y;
            return 0f;
        }

        return topY;
    }

    private void SpawnRowAbove()
    {
        float topY = GetTopRowY();
        float y = topY + prefabSize.y + spacingY;
        SpawnRowAtY(y);
    }

    private void SpawnRowAtY(float y)
    {
        int thisRowId = nextRowId++;
        rowChildCounts[thisRowId] = 0;

        float totalWidth = columns * prefabSize.x + (columns - 1) * spacingX;
        float leftX = -totalWidth / 2f;

        for (int c = 0; c < columns; c++)
        {
            float x = leftX + prefabSize.x / 2f + c * (prefabSize.x + spacingX);
            SpawnPrefabAtRow(x, y, thisRowId);
            rowChildCounts[thisRowId]++;
        }
    }

    private void SpawnPrefabAtRow(float x, float y, int rowId)
    {
        var spawnTrig = ChooseRandomTrigger();
        if (spawnTrig == null) return;

        GameObject[] availablePrefabs = GetPrefabsForCurrentChapter();
        if (availablePrefabs == null || availablePrefabs.Length == 0)
        {
            Debug.LogError("GridSpawner: Нет доступных префабов для спавна!");
            return;
        }

        GameObject prefab = availablePrefabs[UnityEngine.Random.Range(0, availablePrefabs.Length)];
        GameObject go = Instantiate(prefab, parentRect);
        RectTransform rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.sizeDelta = prefabSize;
        rt.anchoredPosition = new Vector2(x, y);

        RowMarker rm = go.GetComponent<RowMarker>();
        if (rm == null) rm = go.AddComponent<RowMarker>();
        rm.rowId = rowId;

        PrefabItem itemComp = go.GetComponent<PrefabItem>();
        float initialSpeed = GetCurrentFallSpeed() * Mathf.Max(0.0001f, clickSpeedMultiplier);
        if (itemComp != null)
            itemComp.Initialize(this, spawnTrig, initialSpeed, parentRect);

        RegisterSpeedAppliersFor(go);
    }

    private void RegisterSpeedAppliersFor(GameObject go)
    {
        if (go == null) return;

        string[] methodNames = new string[] { "SetFallSpeed", "SetSpeed", "UpdateSpeed", "SetVelocity" };
        string[] fieldNames = new string[] { "fallSpeed", "speed", "FallSpeed", "Speed", "velocity" };

        var components = go.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp == null) continue;

            Type t = comp.GetType();
            SpeedApplier ap = new SpeedApplier { target = comp };

            MethodInfo foundMethod = null;
            foreach (var mname in methodNames)
            {
                var mi = t.GetMethod(mname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(float) }, null);
                if (mi != null)
                {
                    foundMethod = mi;
                    break;
                }
            }
            if (foundMethod != null)
            {
                ap.method = foundMethod;
                activeSpeedAppliers.Add(ap);
                continue;
            }

            FieldInfo foundField = null;
            foreach (var fname in fieldNames)
            {
                var fi = t.GetField(fname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null)
                {
                    foundField = fi;
                    break;
                }
            }
            if (foundField != null)
            {
                ap.field = foundField;
                activeSpeedAppliers.Add(ap);
                continue;
            }

            PropertyInfo foundProp = null;
            foreach (var pname in fieldNames)
            {
                var pi = t.GetProperty(pname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pi != null && pi.CanWrite)
                {
                    foundProp = pi;
                    break;
                }
            }
            if (foundProp != null)
            {
                ap.prop = foundProp;
                activeSpeedAppliers.Add(ap);
                continue;
            }
        }
    }

    private void CleanupFallenObjects()
    {
        if (parentRect == null) return;
        if (parentRect.childCount == 0) return;

        List<Transform> toDestroy = null;
        for (int i = parentRect.childCount - 1; i >= 0; i--)
        {
            Transform child = parentRect.GetChild(i);
            RectTransform rt = child as RectTransform;
            if (rt == null) continue;

            if (rt.anchoredPosition.y < destroyYThreshold)
            {
                if (toDestroy == null) toDestroy = new List<Transform>();
                toDestroy.Add(child);
            }
        }

        if (toDestroy == null) return;

        foreach (var t in toDestroy)
        {
            RowMarker rm = t.GetComponent<RowMarker>();
            if (rm != null)
            {
                int id = rm.rowId;
                if (rowChildCounts.ContainsKey(id))
                {
                    rowChildCounts[id] = Mathf.Max(0, rowChildCounts[id] - 1);
                    if (rowChildCounts[id] == 0)
                    {
                        rowChildCounts.Remove(id);
                        OnRowFullyRemoved(id);
                    }
                }
            }

            RemoveSpeedAppliersFor(t.gameObject);
            Destroy(t.gameObject);
        }
    }

    private void RemoveSpeedAppliersFor(GameObject go)
    {
        if (go == null) return;
        for (int i = activeSpeedAppliers.Count - 1; i >= 0; i--)
        {
            var owner = activeSpeedAppliers[i].OwnerGameObject;
            if (owner == null || owner == go)
                activeSpeedAppliers.RemoveAt(i);
        }
    }

    private void OnRowFullyRemoved(int removedRowId)
    {
        if (!levelActive) return;

        if (infiniteSpawnMode)
        {
            SpawnRowAbove();
        }
        else if (remainingRowsToSpawn > 0)
        {
            SpawnRowAbove();
            remainingRowsToSpawn--;
        }
    }

    public bool RegisterClick(SpawnTrigger spawnTrig)
    {
        if (!levelActive) return false;

        string tag = spawnTrig?.data?.Name ?? "";
        if (maxClicks > 0 && totalClicks >= maxClicks)
            return false;

        int prevCount = counts.ContainsKey(tag) ? counts[tag] : 0;

        bool isWrongAnswer = IsWrongAnswer(spawnTrig, prevCount);

        string toShow = DecideResponseText(spawnTrig, prevCount);

        if (clickAudioSource != null && clickSound != null)
        {
            clickAudioSource.Play();
        }

        if (isWrongAnswer && wrongAnswerAudioSource != null && wrongAnswerSound != null)
        {
            wrongAnswerAudioSource.Play();
        }

        if (isWrongAnswer && enableScreenshake && screenshake != null)
        {
            screenshake.StartShake(
                shakeIntensity,
                shakeDuration,
                shakeFrequency,
                shakeDecay,
                shakeAxisX,
                shakeAxisY,
                shakeRotation,
                shakeRotationAmount
            );
        }

        if (animateResponseText && responseText != null)
        {
            if (responseTextCoroutine != null)
            {
                StopCoroutine(responseTextCoroutine);
                if (typingAudioSource != null && typingAudioSource.isPlaying)
                {
                    typingAudioSource.Stop();
                }
            }
            responseTextCoroutine = StartCoroutine(TypewriterResponseText(toShow));
        }
        else
        {
            if (responseText != null)
                responseText.text = toShow;
            else
                Debug.Log($"Response: {toShow}");
        }

        if (!counts.ContainsKey(tag)) counts[tag] = 0;
        counts[tag]++;
        totalClicks++;

        if (!string.IsNullOrEmpty(tag))
        {
            uniqueTriggersClickedInChapter.Add(tag);
        }

        RecalculateClickSpeedMultiplier();

        UpdateCountersText();
        UpdateRemainingClicksText();

        if (maxClicks > 0 && totalClicks >= maxClicks)
        {
            StartCoroutine(EndLevelSequence());
        }

        return true;
    }

    private bool IsWrongAnswer(SpawnTrigger spawnTrig, int prevCount)
    {
        if (spawnTrig == null || spawnTrig.data == null) return false;

        var d = spawnTrig.data;

        int positiveVariants = 1;
        if (!string.IsNullOrEmpty(d.Ts1)) positiveVariants = 2;
        if (!string.IsNullOrEmpty(d.Ts2)) positiveVariants = 3;

        bool alreadyFullyPositive = prevCount >= positiveVariants;
        bool isFromEarlierChapter = spawnTrig.chapterIndex < currentChapterIndex;

        return isFromEarlierChapter && !alreadyFullyPositive;
    }

    private IEnumerator TypewriterResponseText(string fullText)
    {
        if (responseText == null) yield break;
        if (string.IsNullOrEmpty(fullText)) yield break;

        Color c = responseText.color;
        c.a = 1f;
        responseText.color = c;

        responseText.text = "";

        if (typingAudioSource != null && typingSound != null)
        {
            typingAudioSource.Play();
        }

        for (int i = 0; i <= fullText.Length; i++)
        {
            responseText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(responseTypewriterSpeed);
        }

        responseText.text = fullText;

        if (typingAudioSource != null && typingAudioSource.isPlaying)
        {
            typingAudioSource.Stop();
        }

        responseTextCoroutine = null;
    }

    private void RecalculateClickSpeedMultiplier()
    {
        float mult = 1f;
        foreach (var kv in counts)
        {
            int cnt = kv.Value;
            if (cnt <= 0) continue;

            float triggerMult = 1f;
            if (cnt == 1) triggerMult = level1Multiplier;
            else if (cnt == 2) triggerMult = level2Multiplier;
            else triggerMult = level3Multiplier;

            mult *= triggerMult;
            if (mult >= maxClickSpeedMultiplier)
            {
                mult = maxClickSpeedMultiplier;
                break;
            }
        }

        clickSpeedMultiplier = Mathf.Min(mult, maxClickSpeedMultiplier);
    }

    private IEnumerator EndLevelSequence()
    {
        levelActive = false;

        if (screenshake != null && screenshake.IsActive)
        {
            screenshake.StopShake();
        }

        ClearAllSpawned();

        // Сохраняем результат этого уровня
        completedLevelsInChapter++;
        string currentPerformance = GetChapterPerformance(currentChapterIndex, uniqueTriggersClickedInChapter.Count);
        performanceHistory.Add(currentPerformance);

        Debug.Log($"[ImageSystem] ~~~ ЗАВЕРШЕН УРОВЕНЬ ~~~");
        Debug.Log($"[ImageSystem] Глава: {currentChapterIndex}, Уровень: {completedLevelsInChapter}/{currentChapter.Amount}");
        Debug.Log($"[ImageSystem] Прохождение: {currentPerformance}");
        Debug.Log($"[ImageSystem] Уникальных триггеров: {uniqueTriggersClickedInChapter.Count}");

        // Проверяем, это последний уровень главы?
        bool isLastLevelInChapter = completedLevelsInChapter >= currentChapter.Amount;

        // Проверяем, нужно ли показывать картинку
        bool shouldShowImage = ShouldShowImageNow();

        Debug.Log($"[ImageSystem] Показывать картинку? {shouldShowImage}");
        Debug.Log($"[ImageSystem] Последний уровень главы? {isLastLevelInChapter}");
        Debug.Log($"[ImageSystem] ImagePopupManager назначен? {(imagePopupManager != null ? "ДА" : "НЕТ!")}");
        Debug.Log($"[ImageSystem] showImageEveryNLevels = {showImageEveryNLevels}");

        if (shouldShowImage)
        {
            yield return new WaitForSeconds(1f); // Небольшая пауза перед показом картинки

            // Определяем какое прохождение показать
            string performanceToShow = DeterminePerformanceForImage();

            Debug.Log($"[ImageSystem] >>> ПОКАЗЫВАЕМ КАРТИНКУ ДЛЯ: {performanceToShow}");
            Debug.Log($"[ImageSystem] Запускаем анимации восполнения ПАРАЛЛЕЛЬНО с картинкой...");

            // Запускаем анимации восполнения параллельно с картинкой
            bool animationsCompleted = false;
            StartCoroutine(RefillResourcesCoroutine(() => { animationsCompleted = true; }));

            // Если это последний уровень главы И включена опция - готовим смену главы
            bool chapterChangeStarted = false;
            if (isLastLevelInChapter && changeChapterDuringImage)
            {
                Debug.Log($"[ImageSystem] СМЕНА ГЛАВЫ во время картинки включена!");
                Debug.Log($"[ImageSystem] Подготавливаем переход с главы {currentChapterIndex} на {currentChapterIndex + 1}...");
                chapterChangeStarted = true;
            }

            // Показываем картинку и ждем её закрытия
            bool imageCompleted = false;
            if (imagePopupManager != null)
            {
                Debug.Log($"[ImageSystem] Вызываем imagePopupManager.ShowImage()...");
                imagePopupManager.ShowImage(performanceToShow, () => {
                    imageCompleted = true;
                    Debug.Log($"[ImageSystem] Callback: картинка закрыта!");
                });

                // Ждем пока закроется картинка
                while (!imageCompleted)
                {
                    yield return null;
                }

                Debug.Log($"[ImageSystem] Картинка закрыта!");

                // Ждем завершения анимаций восполнения (если еще не завершились)
                while (!animationsCompleted)
                {
                    Debug.Log($"[ImageSystem] Ждем завершения анимаций восполнения...");
                    yield return new WaitForSeconds(0.1f);
                }

                Debug.Log($"[ImageSystem] Анимации восполнения завершены!");

                // Если была подготовка смены главы - выполняем её сейчас
                if (chapterChangeStarted)
                {
                    Debug.Log($"[ImageSystem] 🎨 Применяем смену главы!");
                    // Устанавливаем флаг - глава уже загружена, не нужно делать паузы
                    skipRefillAnimations = true;
                }
                else
                {
                    // Обычный случай - устанавливаем флаг чтобы StartLevel не запускал анимации повторно
                    skipRefillAnimations = true;
                }
            }
            else
            {
                Debug.LogError("[ImageSystem] ОШИБКА: ImagePopupManager = NULL!");
            }
        }
        else
        {
            Debug.Log($"[ImageSystem] Картинка НЕ показывается на этом уровне.");
        }

        // Если картинка показывалась, уменьшаем паузу
        if (shouldShowImage && imagePopupManager != null)
        {
            yield return new WaitForSeconds(0.5f); // Короткая пауза для плавности
        }
        else
        {
            yield return new WaitForSeconds(2f); // Обычная пауза
        }

        EndLevelAndAdvance();
    }

    // Корутина для восполнения ресурсов (слайдер + клики)
    private IEnumerator RefillResourcesCoroutine(System.Action onComplete)
    {
        Debug.Log("[ImageSystem] Начало восполнения ресурсов...");

        // Запускаем обе анимации
        ResetSlider(); // Запускает AnimateSliderFill
        StartCoroutine(AnimateRemainingClicks());

        // Ждем завершения анимации слайдера
        while (isSliderAnimating)
        {
            yield return null;
        }

        Debug.Log("[ImageSystem] Слайдер восполнен!");

        // Ждем завершения анимации кликов
        while (isAnimatingClicks)
        {
            yield return null;
        }

        Debug.Log("[ImageSystem] Клики восполнены!");
        Debug.Log("[ImageSystem] Все ресурсы восполнены!");

        onComplete?.Invoke();
    }

    // Определяет, нужно ли показывать картинку сейчас
    private bool ShouldShowImageNow()
    {
        if (imagePopupManager == null) return false;
        if (showImageEveryNLevels == 0) return false; // Система отключена

        int totalLevelsInChapter = currentChapter.Amount;
        int checkInterval = showImageEveryNLevels;

        // Автоматический режим: определяем интервал на основе количества уровней
        if (showImageEveryNLevels == -1)
        {
            if (totalLevelsInChapter <= 2)
            {
                checkInterval = 2; // Показываем после каждых 2 уровней
            }
            else
            {
                // Для большего количества уровней делим на 2
                checkInterval = Mathf.Max(1, totalLevelsInChapter / 2);
            }
        }

        // Проверяем, прошло ли нужное количество уровней
        bool shouldShow = (completedLevelsInChapter % checkInterval) == 0;

        if (debugImageSystem)
        {
            Debug.Log($"[ImageSystem] Проверка: уровней пройдено {completedLevelsInChapter}, интервал {checkInterval}, показывать: {shouldShow}");
        }

        return shouldShow;
    }

    // Определяет какой тип прохождения показать (на основе последних уровней)
    // Можно было написать по другому, но так проще и нагляднее
    private string DeterminePerformanceForImage()
    {
        if (performanceHistory.Count == 0) return "Нейтральное";

        int levelsToConsider = showImageEveryNLevels;
        if (showImageEveryNLevels == -1)
        {
            levelsToConsider = Mathf.Max(1, currentChapter.Amount / 2);
        }

        // Берем последние N результатов
        int startIndex = Mathf.Max(0, performanceHistory.Count - levelsToConsider);
        int goodCount = 0;
        int badCount = 0;
        int neutralCount = 0;

        for (int i = startIndex; i < performanceHistory.Count; i++)
        {
            switch (performanceHistory[i])
            {
                case "Хорошее":
                    goodCount++;
                    break;
                case "Плохое":
                    badCount++;
                    break;
                case "Нейтральное":
                    neutralCount++;
                    break;
            }
        }

        if (debugImageSystem)
        {
            Debug.Log($"[ImageSystem] Статистика: Хорошее={goodCount}, Нейтральное={neutralCount}, Плохое={badCount}");
        }

        // Определяем преобладающий тип
        if (goodCount > badCount && goodCount > neutralCount)
            return "Хорошее";
        if (badCount > goodCount && badCount > neutralCount)
            return "Плохое";

        return "Нейтральное";
    }

    private void EndLevelAndAdvance()
    {
        levelActive = false;
        currentLevelInChapter++;

        // Проверяем, это последний уровень главы?
        bool isLastLevelInChapter = currentLevelInChapter >= currentChapter.Amount;

        if (!isLastLevelInChapter)
        {
            // Это не последний уровень - просто запускаем следующий
            StartCoroutine(WaitThenStartNextLevel());
        }
        else
        {
            // Это последний уровень главы - переходим к следующей главе
            Debug.Log($"[ImageSystem] Последний уровень главы {currentChapterIndex} завершен!");
            LoadChapter(currentChapterIndex + 1);
        }
    }

    private IEnumerator WaitThenStartNextLevel()
    {
        ClearAllSpawned();

        // Если анимации уже прошли во время картинки, не делаем паузу
        if (!skipRefillAnimations)
        {
            yield return new WaitForSeconds(3f);
        }
        else
        {
            Debug.Log("[ImageSystem] Пропускаем паузу - ресурсы уже восполнены!");
            yield return new WaitForSeconds(0.5f); // Минимальная пауза для плавности
        }

        StartLevel();
    }

    private IEnumerator AnimateRemainingClicks()
    {
        if (remainingClicksText == null) yield break;

        isAnimatingClicks = true;

        if (maxClicks <= 0)
        {
            remainingClicksText.text = "∞";
            isAnimatingClicks = false;
            yield break;
        }

        float duration = sliderFillDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            int currentValue = Mathf.RoundToInt(Mathf.Lerp(0f, maxClicks, smoothT));
            remainingClicksText.text = currentValue.ToString();

            yield return null;
        }

        remainingClicksText.text = maxClicks.ToString();

        isAnimatingClicks = false;
    }

    private void ClearAllSpawned()
    {
        if (screenshake != null && screenshake.IsActive)
        {
            screenshake.ForceStop();
        }

        for (int i = parentRect.childCount - 1; i >= 0; i--)
        {
            var child = parentRect.GetChild(i);
            Destroy(child.gameObject);
        }

        rowChildCounts.Clear();
        activeSpeedAppliers.Clear();
        lastAppliedSpeed = -1f;
    }

    private string DecideResponseText(SpawnTrigger spawnTrig, int prevCount)
    {
        if (spawnTrig == null || spawnTrig.data == null) return "";

        var d = spawnTrig.data;

        int positiveVariants = 1;
        if (!string.IsNullOrEmpty(d.Ts1)) positiveVariants = 2;
        if (!string.IsNullOrEmpty(d.Ts2)) positiveVariants = 3;

        string tag = d.Name ?? "";
        bool alreadyFullyPositive = prevCount >= positiveVariants;
        bool isFromEarlierChapter = spawnTrig.chapterIndex < currentChapterIndex;
        if (isFromEarlierChapter && !alreadyFullyPositive)
        {
            string firstWrong = !string.IsNullOrEmpty(d.F) ? d.F
                                : (!string.IsNullOrEmpty(d.T) ? d.T : d.Name ?? "");
            string secondWrong = !string.IsNullOrEmpty(d.Fs1) ? d.Fs1 : firstWrong;
            string thirdWrong = !string.IsNullOrEmpty(d.Fs2) ? d.Fs2 : secondWrong;

            if (prevCount == 0) return firstWrong;
            if (prevCount == 1) return secondWrong;
            return thirdWrong;
        }
        else
        {
            string firstCorrect = !string.IsNullOrEmpty(d.T) ? d.T : (d.Name ?? "");
            string secondCorrect = !string.IsNullOrEmpty(d.Ts1) ? d.Ts1 : firstCorrect;
            string thirdCorrect = !string.IsNullOrEmpty(d.Ts2) ? d.Ts2 : secondCorrect;

            if (prevCount == 0) return firstCorrect;
            if (prevCount == 1) return secondCorrect;
            return thirdCorrect;
        }
    }

    private void UpdateCountersText()
    {
        if (countersText == null) return;
        List<string> parts = new List<string>();
        foreach (var kv in counts)
        {
            if (kv.Value > 0)
                parts.Add($"{kv.Key}: {kv.Value}");
        }
        countersText.text = string.Join(", ", parts);
    }

    private void UpdateRemainingClicksText()
    {
        if (remainingClicksText == null) return;

        if (isAnimatingClicks) return;

        remainingClicksText.text = (maxClicks <= 0) ? "∞" : Mathf.Max(0, maxClicks - totalClicks).ToString();
    }

    private void PrepareWeights()
    {
        if (allTriggers == null || allTriggers.Count == 0)
        {
            cumulativeWeights = new float[0];
            return;
        }

        cumulativeWeights = new float[allTriggers.Count];
        float total = 0f;
        for (int i = 0; i < allTriggers.Count; i++)
        {
            total += 1f;
            cumulativeWeights[i] = total;

            var name = allTriggers[i].data?.Name;
            if (!string.IsNullOrEmpty(name) && !counts.ContainsKey(name))
                counts[name] = 0;
        }
    }

    private SpawnTrigger ChooseRandomTrigger()
    {
        if (allTriggers == null || allTriggers.Count == 0) return null;
        float total = cumulativeWeights[cumulativeWeights.Length - 1];
        float r = UnityEngine.Random.Range(0f, total);
        for (int i = 0; i < cumulativeWeights.Length; i++)
            if (r <= cumulativeWeights[i])
                return allTriggers[i];
        return allTriggers[allTriggers.Count - 1];
    }

    public float GetCurrentFallSpeed()
    {
        float duration = Mathf.Max(1f, levelDuration);
        float t = Mathf.Clamp01((Time.time - levelStartTime) / duration);
        float factor = (difficultyCurve != null) ? difficultyCurve.Evaluate(t) : t;
        float speed = Mathf.Lerp(minFallSpeed, maxFallSpeed, Mathf.Clamp01(factor));
        return Mathf.Max(1f, speed);
    }

    private void EndGame()
    {
        if (currentChapter != null && uniqueTriggersClickedInChapter.Count > 0)
        {
            LogChapterResults(currentChapterIndex, uniqueTriggersClickedInChapter.Count);
        }

        if (screenshake != null && screenshake.IsActive)
        {
            screenshake.StopShake();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}