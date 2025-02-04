using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<ButtonUI> onButtonClicked, onButtonReleased, onPointerEnter, onPointerExit;
    [SerializeField] protected TMP_Text buttonText;
    [SerializeField] protected Image image;
    [SerializeField] protected Image borderImage;
    [SerializeField] protected Color selectedColor;
    [SerializeField] protected String defaultText;

    // Start is called before the first frame update

    public virtual void InitalizeButton(String text)
    {
        this.buttonText.text = text;
        Deselct();
    }
    public virtual void Deselct()
    {
        if (borderImage != null)
        {
            borderImage.enabled = false;
        }
    }
    public virtual void Select()
    {
        if (borderImage != null)
        {
            borderImage.enabled = true;
        }

    }
    public virtual void Press()
    {
        if (image != null)
        {
            image.color = selectedColor;
        }
    }
    public void Activate(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }
    public virtual void Release()
    {
        if (image != null)
        {
            image.color = Color.white;
        }
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (onButtonClicked != null)
        {
            onButtonClicked.Invoke(this);
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (onButtonReleased != null)
        {
            onButtonReleased.Invoke(this);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null)
        {
            onPointerEnter.Invoke(this);
        };
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit != null)
        {
            onPointerExit.Invoke(this);
        }
    }

    public virtual Image Image
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
    public virtual TMP_Text ButtonText { get { return buttonText; } }


}

