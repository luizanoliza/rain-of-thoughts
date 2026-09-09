using UnityEngine;
using UnityEngine.UI;

public class MenuAudioSettings : MonoBehaviour
{
    [Header("AudioSources")]
    public AudioSource menuMusicSource; // музыка меню
    public AudioSource menuSfxSource;   // звуковые эффекты

    [Header("UI Sliders")]
    public Slider sliderMenuMusic; // управляет музыкой в меню
    public Slider sliderGameMusic; // управляет музыкой на игровой сцене (сохраняется)
    public Slider sliderSFX;       // управляет звуками (sfx)

    // Ключи для PlayerPrefs
    private const string KEY_MENU_MUSIC = "Volume_MenuMusic";
    private const string KEY_GAME_MUSIC = "Volume_GameMusic";
    private const string KEY_SFX = "Volume_SFX";

    // Значения по умолчанию
    private const float DEFAULT_VOLUME = 1f;

    private void Awake()
    {
        // Убедимся, что слайдеры имеют корректный диапазон (0 - 1)
        SetSliderRangeIfNeeded(sliderMenuMusic);
        SetSliderRangeIfNeeded(sliderGameMusic);
        SetSliderRangeIfNeeded(sliderSFX);
    }

    private void Start()
    {
        // Загрузка значений (если не существует - будет DEFAULT_VOLUME)
        float menuMusic = PlayerPrefs.GetFloat(KEY_MENU_MUSIC, DEFAULT_VOLUME);
        float gameMusic = PlayerPrefs.GetFloat(KEY_GAME_MUSIC, DEFAULT_VOLUME);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_VOLUME);

        // Применяем к слайдерам (это обновит UI)
        if (sliderMenuMusic != null) sliderMenuMusic.value = menuMusic;
        if (sliderGameMusic != null) sliderGameMusic.value = gameMusic;
        if (sliderSFX != null) sliderSFX.value = sfx;

        // Применяем к AudioSource, которые находятся в меню
        if (menuMusicSource != null) menuMusicSource.volume = menuMusic;
        if (menuSfxSource != null) menuSfxSource.volume = sfx;

        // Подписываемся на изменения слайдеров, если они заданы
        if (sliderMenuMusic != null) sliderMenuMusic.onValueChanged.AddListener(OnMenuMusicChanged);
        if (sliderGameMusic != null) sliderGameMusic.onValueChanged.AddListener(OnGameMusicChanged);
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(OnSfxChanged);
    }

    private void OnDestroy()
    {
        // Убираем слушателей, чтобы избежать утечки, решило пару рандомных багов
        if (sliderMenuMusic != null) sliderMenuMusic.onValueChanged.RemoveListener(OnMenuMusicChanged);
        if (sliderGameMusic != null) sliderGameMusic.onValueChanged.RemoveListener(OnGameMusicChanged);
        if (sliderSFX != null) sliderSFX.onValueChanged.RemoveListener(OnSfxChanged);
    }

    // Методы, которые вызываются при изменении слайдера
    public void OnMenuMusicChanged(float value)
    {
        value = Mathf.Clamp01(value);
        if (menuMusicSource != null) menuMusicSource.volume = value;
        PlayerPrefs.SetFloat(KEY_MENU_MUSIC, value);
        PlayerPrefs.Save();
    }

    public void OnGameMusicChanged(float value)
    {
        value = Mathf.Clamp01(value);
        // Тут в меню мы не имеем AudioSource для игровой музыки, поэтому только сохраняем
        PlayerPrefs.SetFloat(KEY_GAME_MUSIC, value);
        PlayerPrefs.Save();
    }

    public void OnSfxChanged(float value)
    {
        value = Mathf.Clamp01(value);
        if (menuSfxSource != null) menuSfxSource.volume = value;
        PlayerPrefs.SetFloat(KEY_SFX, value);
        PlayerPrefs.Save();
    }

    private void SetSliderRangeIfNeeded(Slider s)
    {
        if (s == null) return;
        // если у слайдера нестандартные мин/макс, устанавливаем 0..1
        if (s.minValue != 0f || s.maxValue != 1f)
        {
            s.minValue = 0f;
            s.maxValue = 1f;
        }
    }

    public void ResetSettings()
    {
        sliderMenuMusic.value = DEFAULT_VOLUME;
        sliderGameMusic.value = DEFAULT_VOLUME;
        sliderSFX.value = DEFAULT_VOLUME;
    }
}
