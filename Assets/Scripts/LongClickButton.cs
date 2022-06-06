using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool pointerDown;
    private float pointerDownTimer;

    [SerializeField]
    private GameObject Button;
    [SerializeField]
    private Sprite Button_Clicked;
    [SerializeField]
    private Sprite Button_def;

    

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        Button.GetComponent<Image>().sprite = Button_Clicked;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();

    }


    private void Reset()
    {
        pointerDown = false;

        Button.GetComponent<Image>().sprite = Button_def;
    }

}