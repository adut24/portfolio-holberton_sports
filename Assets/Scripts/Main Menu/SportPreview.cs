using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Updates the preview depending on the button hovered.
/// </summary>
public class SportPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const string DEFAULT_TEXT = "CHOOSE A SPORT";

    [SerializeField]
    private Sprite sportImage;
    [SerializeField]
    private Image previewImage;
    [SerializeField]
    private TextMeshProUGUI sportText;
    [SerializeField]
    private string sportName;
    [SerializeField]
    private Sprite _defaultImage;

    /// <summary>
    /// Event called when the pointer starts hovering on the button.
    /// </summary>
    /// <param name="eventData">Information about the action</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        sportText.text = sportName;
        previewImage.sprite = sportImage;
    }


    /// <summary>
    /// Event called when the pointer stops hovering on the button.
    /// </summary>
    /// <param name="eventData">Information about the action</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        sportText.text = DEFAULT_TEXT;
        previewImage.sprite = _defaultImage;
    }
}
