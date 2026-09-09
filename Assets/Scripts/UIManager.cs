using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject demoPanel;
    [SerializeField] private GameObject plotsPanel;

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    public MenuAudioSettings audioManager;
    public Toggle dontShowAgainToggle;
    private bool dontShowAgain;
    private static string KEY_DONT_SHOW_DEMO = "DontShowDemo";
    private bool isProcessingClick = false;

    private void Start()
    {
        dontShowAgain = PlayerPrefs.GetInt(KEY_DONT_SHOW_DEMO, 0) == 1;

        // Применяем к Toggle
        if (dontShowAgainToggle != null)
        {
            dontShowAgainToggle.isOn = dontShowAgain;
        }

        demoPanel.SetActive(!dontShowAgain);

        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        plotsPanel.SetActive(false);

        // AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }


    // Метод для проигрывания звука и ожидания его окончания
    private IEnumerator PlaySoundAndWait(System.Action action)
    {
        if (isProcessingClick) yield break; // Блокируем множ. клики

        isProcessingClick = true;

        // Проигрываем звук, если он назначен
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);

            // Ждем окончания звука
            yield return new WaitForSeconds(buttonClickSound.length);
        }

        // Выполняем действие после окончания звука
        action?.Invoke();

        isProcessingClick = false;
    }

    // Публичные методы теперь запускают корутины
    public void Exit()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            // Если в юнити то просто останавливаем игру
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }));
    }

    public void OpenSetting()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            settingsPanel.SetActive(true);
        }));
    }

    public void CloseSetting()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            settingsPanel.SetActive(false);
        }));
    }

    public void OpenPlots()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            plotsPanel.SetActive(true);
        }));
    }

    public void ClosePlots()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            plotsPanel.SetActive(false);
        }));
    }

    public void OpenCredits()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            creditsPanel.SetActive(true);
        }));
    }

    public void CloseCredits()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            creditsPanel.SetActive(false);
        }));
    }

    public void CloseDemo()
    {
        bool value = dontShowAgainToggle != null && dontShowAgainToggle.isOn;
        dontShowAgain = value;
        PlayerPrefs.SetInt(KEY_DONT_SHOW_DEMO, value ? 1 : 0);
        PlayerPrefs.Save();

        StartCoroutine(PlaySoundAndWait(() =>
        {
            demoPanel.SetActive(false);
        }));
    }


    public void OpenGameScene()
    {
        StartCoroutine(PlaySoundAndWait(() =>
        {
            SceneManager.LoadScene(1);
        }));
    }

    public void RestoreSettings()
    {
        audioManager.ResetSettings();
    }

    public void ResetEVERYTHING()
    {
        // Удаляем сохранённый ключ (возвращает к поведению по умолчанию)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Обновляем состояние в памяти и UI
        dontShowAgain = false;
        if (dontShowAgainToggle != null) dontShowAgainToggle.isOn = false;
        audioManager.ResetSettings();
    }
}