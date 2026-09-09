using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIRotateOnHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Rotation Settings")]
    public float hoverAngle = 20f;
    public float normalAngle = 0f;
    public float rotateSpeed = 8f;

    [Header("Sound Settings")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    private bool isHovering;
    private bool isLoadingScene;
    private RectTransform rectTransform;

    void Awake()
    {
        float sfxVolume = PlayerPrefs.GetFloat("Volume_SFX", 1f);
        rectTransform = GetComponent<RectTransform>();

        // Если AudioSource не назначен, пытаемся получить его с объекта
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Если всё ещё нет, создаём новый
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clickSound;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = sfxVolume;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isLoadingScene)
        {
            isHovering = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isLoadingScene)
        {
            StartCoroutine(PlaySoundAndLoadScene());
        }
    }

    private IEnumerator PlaySoundAndLoadScene()
    {
        isLoadingScene = true;
        isHovering = false;

        // Воспроизводим звук, если он назначен
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);

            // Ждём окончания звука
            yield return new WaitForSeconds(clickSound.length);
        }

        // Загружаем сцену
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (!isLoadingScene)
        {
            float targetAngle = isHovering ? hoverAngle : normalAngle;

            float smooth = Mathf.LerpAngle(
                rectTransform.localEulerAngles.z,
                targetAngle,
                rotateSpeed * Time.deltaTime
            );

            rectTransform.localEulerAngles = new Vector3(0f, 0f, smooth);
        }
    }
}