using UnityEngine;
using UnityEditor;
using TMPro;

public class TMPFontChanger : EditorWindow
{
    public TMP_FontAsset newFont;

    [MenuItem("Tools/Change TMP Font (All Objects)")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontChanger>("Change TMP Font");
    }

    void OnGUI()
    {
        GUILayout.Label("Select new TMP Font", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Apply to all TMP_Text in scene (including disabled)"))
        {
            if (newFont == null)
            {
                Debug.LogWarning("No font selected!");
                return;
            }

            TMP_Text[] allTexts = GetAllTMPTextsInScene();
            Undo.RecordObjects(allTexts, "Change TMP Font");

            foreach (var text in allTexts)
            {
                text.font = newFont;
                EditorUtility.SetDirty(text);
            }

            Debug.Log($"Changed font on {allTexts.Length} TMP_Text objects (including disabled).");
        }
    }

    // Рекурсивно ищем все TMP_Text в сцене, включая выключенные объекты
    static TMP_Text[] GetAllTMPTextsInScene()
    {
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        System.Collections.Generic.List<TMP_Text> result = new System.Collections.Generic.List<TMP_Text>();

        foreach (var root in rootObjects)
        {
            result.AddRange(root.GetComponentsInChildren<TMP_Text>(true)); // true = include inactive
        }

        return result.ToArray();
    }
}
