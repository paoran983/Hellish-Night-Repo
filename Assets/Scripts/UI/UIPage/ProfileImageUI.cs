using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileImageUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    private InventorySO parentInventory;
    // delagate that allows to call the following functions 
    public event Action<ProfileImageUI> onItemClicked, onItemDroppedOn,
        onItemBeginDrag, onItemEndDrag, onRightMouseBtnClick, onPointerEnter, onPointerExit, onEquipping, onAddingAbility;
    public event Action<ProfileImageUI, ProfileImageUI> onDroppedOnDifInventory;
    public bool empty = true;
    private int index;
    [SerializeField] private ItemContainerUI itemContainer;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color teamColor0;
    [SerializeField] private Color teamColor1;
    private Unit unit;
    private int team = -1;


    public void Awake() {
        ResetData();
        Deselect();
    }
    void OnMouseOver() {
        //If your mouse hovers over the GameObject with the script attached, output this message
    }

    void OnMouseExit() {
        //The mouse is no longer hovering over the GameObject so output this message each frame

    }
    public void Deselect() {
        if (borderImage != null) {
            borderImage.enabled = false;
        }
    }
    public void SetIndex(int newIndex) {
        index = newIndex;
    }
    public void AssignTeam(int team) {
        this.team = team;
        if (backgroundImage == null) {
            return;
        }
        if (team == 0) {
            backgroundImage.color = teamColor0;
        }
        else if(team == 1) {
            backgroundImage.color = teamColor1;
        }
        else {
            backgroundImage.color = Color.white;
        }
    }
    public int GetIndex() {
        return index;
    }
    public void ResetData() {
        if (itemImage != null) {
            itemImage.gameObject.SetActive(false);
        }
        empty = true;

    }
    public int GetCount() {
        int countVal;
        int.TryParse(titleText.text, out countVal);
        return countVal;
    }


    /* public void SetParentInventory(InventorySO parInventory) {
         parentInventory = parInventory;
         transform.SetParent(parInventory.GetContainer());
     }*/
    public InventorySO GetParentInventory() {
        return parentInventory;
    }
    public void SetData(Sprite sprite, String title) {
        if (sprite == null) {
            if (itemImage != null) {
                itemImage.gameObject.SetActive(false);
            }
            empty = true;
            return;
        }
        //Debug.Log("setting good");
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = sprite;
        titleText.text = title;
        empty = false;
    }

    public void Select() {
        itemImage.enabled = true;
        borderImage.enabled = true;

    }
    public void SelectedForCombat() {
        if(backgroundImage != null) {
            backgroundImage.color = selectedColor;
        }
    }
    public void DeselectedForCombat() {
        if (backgroundImage != null) {
            AssignTeam(team);
        }
    }
    public void OnPointerClick(PointerEventData pointerData) {

        if (pointerData.button == PointerEventData.InputButton.Right) {
            //  if OnRightMouseBtnClick is not bull then run code
            //  onRightMouseBtnClick?.Invoke(this);
            if (onRightMouseBtnClick != null) {
                onRightMouseBtnClick.Invoke(this);
            }

        }
        // left click
        else {
            if (onItemClicked != null) {
                onItemClicked.Invoke(this);
            }
            //onItemClicked?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (onPointerEnter != null) {
            onPointerEnter.Invoke(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {

        if (onPointerExit != null) {
            onPointerExit.Invoke(this);
        }
    }
    public TMP_Text TitleText {
        get {
            return titleText;
        }
    }
    public ItemContainerUI ItemContainer { get { return itemContainer; } set { itemContainer = value; } }
    public Unit Unit { get { return unit; } set { unit = value; } }
}

