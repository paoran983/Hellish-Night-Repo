using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CombatUIButton : ButtonUI, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public new event Action<CombatUIButton> onButtonClicked, onButtonReleased, onPointerEnter, onPointerExit;
    [SerializeField] public State type;

    [SerializeField]
    public enum State
    {
        Move,
        Magic,
        Item,
        Attack,
        Wait,
        Actions,
        Capture,
        Push,
        Run,
        TeamUp,
        SubmitTeamUp

    }
    // Start is called before the first frame update

    public override void InitalizeButton(string text = "")
    {
        if (defaultText == "")
        {
            buttonText.text = type.ToString();

        }
        else
        {
            buttonText.text = defaultText;
        }
        Deselct();
    }
    public void Deselct()
    {
        if (borderImage != null)
        {
            borderImage.enabled = false;
        }
    }
    public void Select()
    {
        if (borderImage != null)
        {
            borderImage.enabled = true;
        }

    }
    public void Press()
    {
        if (image != null)
        {
            image.color = selectedColor;
        }
    }

    public void Release()
    {
        if (image != null)
        {
            image.color = Color.white;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onButtonClicked != null)
        {
            onButtonClicked.Invoke(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onButtonReleased != null)
        {
            onButtonReleased.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null)
        {
            onPointerEnter.Invoke(this);
        };
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit != null)
        {
            onPointerExit.Invoke(this);
        }
    }

    public Image Image
    {
        get
        {
            return image;
        }
        set
        {
            image = value;
        }
    }
    public TMP_Text ButtonText { get { return buttonText; } }


}
