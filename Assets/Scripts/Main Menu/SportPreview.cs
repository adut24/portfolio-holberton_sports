using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Updates the preview depending on the button hovered.
/// </summary>
public class SportPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const string DEFAULT_TEXT = "CHOOSE A SPORT";

    public Sprite sportImage;
    public Image previewImage;
    public Text sportText;
    public string sportName;

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
        previewImage.sprite = _defaultImage;
        sportText.text = DEFAULT_TEXT;
    }
}
