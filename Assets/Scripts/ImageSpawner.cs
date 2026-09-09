using UnityEngine;
using UnityEngine.UI;

public class ImageSpawner : MonoBehaviour
{
    [Header("Префаб для спавна")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Настройки позиций")]
    [SerializeField] private float goodStartX = -423f;
    [SerializeField] private float neutralStartX = 515f;
    [SerializeField] private float badStartX = 60f;

    [SerializeField] private float startY = 0f;
    [SerializeField] private float yOffset = -490f;

    [Header("Пути к папкам (относительно Resources/)")]
    [SerializeField] private string goodFolderPath = "Images/Good";
    [SerializeField] private string neutralFolderPath = "Images/Neutral";
    [SerializeField] private string badFolderPath = "Images/Bad";

    [Header("Настройки отображения")]
    [SerializeField] private Sprite placeholderSprite; // Спрайт заглушки (?)

    private const string GOOD_COUNT_KEY = "GoodImagesShown";
    private const string NEUTRAL_COUNT_KEY = "NeutralImagesShown";
    private const string BAD_COUNT_KEY = "BadImagesShown";

    void Start()
    {
        SpawnItems();
    }

    public void SpawnItems()
    {
        // Получаем ОБЩЕЕ количество картинок в папках
        int totalGood = GetImageCount(goodFolderPath);
        int totalNeutral = GetImageCount(neutralFolderPath);
        int totalBad = GetImageCount(badFolderPath);

        // Получаем количество ПОКАЗАННЫХ картинок из PlayerPrefs
        int shownGood = PlayerPrefs.GetInt(GOOD_COUNT_KEY, 0);
        int shownNeutral = PlayerPrefs.GetInt(NEUTRAL_COUNT_KEY, 0);
        int shownBad = PlayerPrefs.GetInt(BAD_COUNT_KEY, 0);

        Debug.Log($"Всего картинок - Good: {totalGood}, Neutral: {totalNeutral}, Bad: {totalBad}");
        Debug.Log($"Показано - Good: {shownGood}, Neutral: {shownNeutral}, Bad: {shownBad}");

        // Спавним ВСЕ префабы, но показываем картинки только для тех, что игрок видел
        SpawnColumn(goodStartX, totalGood, shownGood, goodFolderPath, "Good");
        SpawnColumn(neutralStartX, totalNeutral, shownNeutral, neutralFolderPath, "Neutral");
        SpawnColumn(badStartX, totalBad, shownBad, badFolderPath, "Bad");
    }

    private int GetImageCount(string folderPath)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"Картинки не найдены в Resources/{folderPath}");
            return 0;
        }

        return sprites.Length;
    }

    private void SpawnColumn(float xPosition, int totalCount, int shownCount, string folderPath, string columnName)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("Префаб не назначен!");
            return;
        }

        // Спавним ВСЕ префабы по количеству файлов
        for (int i = 0; i < totalCount; i++)
        {
            GameObject spawnedItem = Instantiate(itemPrefab, transform);
            spawnedItem.name = $"{columnName}_Item_{i + 1}";

            // Работаем с RectTransform для UI элементов
            RectTransform rectTransform = spawnedItem.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                // Сбрасываем scale до (1, 1, 1)
                rectTransform.localScale = Vector3.one;

                // Устанавливаем позицию через anchoredPosition
                rectTransform.anchoredPosition = new Vector2(
                    xPosition,
                    startY + (yOffset * i)
                );
            }
            else
            {
                // Если это не UI элемент, используем обычный Transform
                spawnedItem.transform.localScale = Vector3.one;
                spawnedItem.transform.localPosition = new Vector3(
                    xPosition,
                    startY + (yOffset * i),
                    0f
                );
            }

            // Проверяем- игрок видел эту картинку или нет?
            bool isUnlocked = (i + 1) <= shownCount;

            // Загружаем картинку или заглушку
            LoadImageToItem(spawnedItem, folderPath, i + 1, isUnlocked);
        }
    }

    private void LoadImageToItem(GameObject item, string folderPath, int imageNumber, bool isUnlocked)
    {
        // Ищем Image компонент в префабе
        Image imageComponent = item.GetComponent<Image>();

        if (imageComponent == null)
        {
            imageComponent = item.GetComponentInChildren<Image>();
        }

        if (imageComponent == null)
        {
            Debug.LogWarning($"Image компонент не найден в префабе {item.name}!");
            return;
        }

        if (isUnlocked)
        {
            // Игрок УЖЕ видел эту картинку - загружаем реальную
            string imagePath = $"{folderPath}/{imageNumber}";
            Sprite sprite = Resources.Load<Sprite>(imagePath);

            if (sprite != null)
            {
                imageComponent.sprite = sprite;
                Debug.Log($"✅ Открыта картинка: {imagePath}");
            }
            else
            {
                Debug.LogWarning($"Не удалось загрузить картинку: Resources/{imagePath}");
                SetPlaceholder(imageComponent);
            }
        }
        else
        {
            // Игрок ЕЩЁ НЕ видел эту картинку - ставим заглушку
            SetPlaceholder(imageComponent);
            Debug.Log($"🔒 Закрыта картинка: {folderPath}/{imageNumber}");
        }
    }

    private void SetPlaceholder(Image imageComponent)
    {
        if (placeholderSprite != null)
        {
            imageComponent.sprite = placeholderSprite;
        }
        else
        {
            // Если заглушка не назначена, можно создать простой серый квадрат
            imageComponent.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }
}