using Inventory.Model;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Inventory.UI
{
    public class InventoryItemUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler,
        IEndDragHandler, IDropHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Image borderImage;
        [SerializeField] private Transform textBackground;
        private InventorySO parentInventory;
        // delagate that allows to call the following functions 
        public event Action<InventoryItemUI> onItemClicked, onItemDroppedOn,
            onItemBeginDrag, onItemEndDrag, onRightMouseBtnClick, onPointerEnter, onPointerExit, onEquipping, onAddingAbility;
        public event Action<InventoryItemUI, InventoryItemUI> onDroppedOnDifInventory;
        public bool empty = true;
        private int index;
        private ItemSO itemData;
        [SerializeField] private ItemContainerUI itemContainer;
        [SerializeField] public State type;
        public Button button;

        [SerializeField]
        public enum State
        {
            Default,
            Equipped,
            Ability
        }
        public void Awake()
        {
            ResetData();
            Deselect();
            button = GetComponent<Button>();

        }
        void OnMouseOver()
        {
            //If your mouse hovers over the GameObject with the script attached, output this message
            Debug.Log("Mouse is over GameObject.");
        }

        void OnMouseExit()
        {
            //The mouse is no longer hovering over the GameObject so output this message each frame
            Debug.Log("Mouse is no longer on GameObject.");
        }
        public void Deselect()
        {
            if (borderImage != null)
            {
                borderImage.enabled = false;
            }
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
            if (itemImage != null)
            {
                itemImage.gameObject.SetActive(false);
            }
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
        public InventorySO GetParentInventory()
        {
            return parentInventory;
        }
        public void SetData(Sprite sprite, int qunatity, ItemSO itemData = null)
        {
            if (itemImage == null)
            {
                return;
            }
            if (sprite == null || qunatity < 0)
            {
                if (itemImage != null)
                {
                    itemImage.gameObject.SetActive(false);
                }
                empty = true;
                this.itemData = itemData;
                return;
            }
            //Debug.Log("setting good");
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            if (this.type != State.Default)
            {
                textBackground.gameObject.SetActive(false);
            }
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (empty)
            {
                return;
            }
            if (onItemBeginDrag != null)
            {
                onItemBeginDrag.Invoke(this);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {

            if (onItemEndDrag != null)
            {
                onItemEndDrag.Invoke(this);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryItemUI curItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();


            if (type == InventoryItemUI.State.Equipped)
            {
                Debug.Log("euipping " + name);
                if (onEquipping != null)
                {
                    onEquipping.Invoke(this);
                    return;
                }

            }
            else if (type == InventoryItemUI.State.Ability)
            {
                Debug.Log("Ability " + name);
                if (onAddingAbility != null)
                {
                    onAddingAbility.Invoke(this);
                    return;
                }
            }




            if (empty)
            {
                //InvenItem invenItem = eventData.pointerDrag.GetComponent<InvenItem>();
                //invenItem.parentAfterDrag = transform;

            }

            else
            {

            }
            if (onItemDroppedOn != null)
            {
                onItemDroppedOn.Invoke(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {

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





        public TMP_Text CountText
        {
            get
            {
                return countText;
            }
        }
        public Transform TextBackground
        {
            get { return textBackground; }
        }
        public ItemContainerUI ItemContainer { get { return itemContainer; } set { itemContainer = value; } }
        public ItemSO ItemData { get { return itemData; } }
    }
}
