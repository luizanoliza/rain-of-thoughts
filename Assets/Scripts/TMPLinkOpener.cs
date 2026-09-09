using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMPClickableLink : MonoBehaviour, IPointerClickHandler
{
    [TextArea]
    public string url;

    private TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URL не задан", this);
            return;
        }

        tmpText.text = $"<link=\"{url}\">{tmpText.text}</link>";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Camera cam = tmpText.canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : tmpText.canvas.worldCamera;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            tmpText,
            eventData.position,
            cam
        );

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = tmpText.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
