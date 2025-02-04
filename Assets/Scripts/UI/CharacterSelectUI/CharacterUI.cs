using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image borderImage;
    private MoveListSO parentInventory;
    // delagate that allows to call the following functions 
    public event Action<CharacterUI> onItemClicked, onRightMouseBtnClick, onPointerEnter, onPointerExit, onPointerClick;
    public bool empty = true;
    private int index;
    private Unit unitData;

    public void Awake()
    {
        ResetData();
        Deselect();
    }

    public void Deselect()
    {
        borderImage.enabled = false;
    }
    public void SetIndex(int newIndex)
    {
        index = newIndex;
    }

    public int GetIndex()
    {
        return index;
    }
    public void ResetData()
    {
        itemImage.gameObject.SetActive(false);
        empty = true;

    }
    public int GetCount()
    {
        int countVal;
        int.TryParse(countText.text, out countVal);
        return countVal;
    }


    /* public void SetParentInventory(InventorySO parInventory) {
         parentInventory = parInventory;
         transform.SetParent(parInventory.GetContainer());
     }*/
    public MoveListSO GetParentInventory()
    {
        return parentInventory;
    }
    public void SetData(Sprite sprite, int qunatity)
    {
        if (sprite == null || qunatity < 0)
        {

            empty = true;
            return;
        }
        //Debug.Log("setting good");
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = sprite;
        countText.text = qunatity + "";
        empty = false;
    }

    public void Select()
    {
        itemImage.enabled = true;
        borderImage.enabled = true;

    }

    public void OnPointerClick(PointerEventData pointerData)
    {

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            //  if OnRightMouseBtnClick is not bull then run code
            //  onRightMouseBtnClick?.Invoke(this);
            if (onRightMouseBtnClick != null)
            {
                onRightMouseBtnClick.Invoke(this);
            }

        }
        // left click
        else
        {
            if (onItemClicked != null)
            {
                onItemClicked.Invoke(this);
            }
            //onItemClicked?.Invoke(this);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null)
        {
            onPointerEnter.Invoke(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit != null)
        {
            onPointerExit.Invoke(this);
        }
    }
}
