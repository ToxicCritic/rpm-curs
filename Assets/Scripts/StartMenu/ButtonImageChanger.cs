using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buttonImage;
    public Color normalColor = new Color(137, 137, 137);
    public Color highlightedColor = Color.white;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = normalColor;
    }
}
