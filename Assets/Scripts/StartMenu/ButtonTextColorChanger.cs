using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.yellow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }
}
