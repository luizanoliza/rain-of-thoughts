using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    // Музыка и звуки
    public AudioSource gameMusicSource;
    public AudioSource gameSfxSource;

    // (!) Должны совпадать ключи с MenuAudioSettings
    private const string KEY_GAME_MUSIC = "Volume_GameMusic";
    private const string KEY_SFX = "Volume_SFX";

    private const float DEFAULT_VOLUME = 1f;

    private void Start()
    {
        ApplySavedVolumes();
    }

    // Обновление громкости музыки и звуков
    public void ApplySavedVolumes()
    {
        float gameMusic = PlayerPrefs.GetFloat(KEY_GAME_MUSIC, DEFAULT_VOLUME);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_VOLUME);

        if (gameMusicSource != null) gameMusicSource.volume = Mathf.Clamp01(gameMusic);
        if (gameSfxSource != null) gameSfxSource.volume = Mathf.Clamp01(sfx);
    }
}
