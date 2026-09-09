using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class PrefabItem : MonoBehaviour
{
    [Header("UI в префабе")]
    public Image mainImage;
    public TextMeshProUGUI textField;

    // assigned at spawn
    private GridSpawner spawner;
    private GridSpawner.SpawnTrigger localSpawnTrigger;
    private float fallSpeed = 50f;
    private RectTransform rt;
    private bool clicked = false;

    // Initialize вызывается GridSpawner(ом) и передаёт SpawnTrigger
    public void Initialize(GridSpawner spawner, object spawnTriggerObj, float fallSpeed, RectTransform parentRect)
    {
        this.spawner = spawner;
        this.fallSpeed = fallSpeed;
        rt = GetComponent<RectTransform>();

        localSpawnTrigger = spawnTriggerObj as GridSpawner.SpawnTrigger;

        if (textField != null)
        {
            string nameToShow = localSpawnTrigger?.data?.Name ?? "item";
            textField.text = nameToShow;
        }

        if (mainImage != null)
        {
            Button b = mainImage.GetComponent<Button>();
            if (b == null) b = mainImage.gameObject.AddComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(OnImageClicked);
        }
    }

    void Update()
    {
        if (clicked) return;
        if (rt == null) rt = GetComponent<RectTransform>();

        float baseSpeed;
        if (spawner != null)
        {
            // учитываем глобальный множитель кликов, который пересчитывается каждую регистрацию клика
            baseSpeed = spawner.GetCurrentFallSpeed() * spawner.clickSpeedMultiplier;
        }
        else
        {
            baseSpeed = fallSpeed;
        }

        Vector2 pos = rt.anchoredPosition;
        pos.y -= baseSpeed * Time.deltaTime;
        rt.anchoredPosition = pos;
    }

    private void OnImageClicked()
    {
        if (clicked) return;
        if (spawner == null)
        {
            Debug.LogWarning("PrefabItem: spawner не назначен.");
            return;
        }

        bool accepted = false;
        try
        {
            accepted = spawner.RegisterClick(localSpawnTrigger);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("PrefabItem: ошибка при RegisterClick - " + ex.Message);
            accepted = false;
        }

        if (accepted)
        {
            clicked = true;
            Destroy(gameObject);
        }
    }
}
