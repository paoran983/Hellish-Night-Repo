using Inventory.Model;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class InventoryPageUI : ItemContainerUI
    {
        [SerializeField] private Transform equipped, abilites;
        [SerializeField] private Image profileImage;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private InventoryItemUI pageBackground;
        [SerializeField] private TMP_Text statText;
        [SerializeField] private TMP_Text statHeaderText;
        [SerializeField] EventSystem eventSystem;
        private List<InventoryItemUI> abilityList = new List<InventoryItemUI>();
        private List<InventoryItemUI> equipmentList = new List<InventoryItemUI>();
        private List<InventoryItemUI> inventoryList = new List<InventoryItemUI>();
        public event Action<int> onItemActionRequest, onStartDrag;
        public event Action<int, int> onSwapItems;
        public event Action<int, InventoryItemUI> onDescriptionRequest, onDescriptionClear;
        public event Action<InventoryItemUI, InventoryItemUI, int, int> onTransferItems;
        public InventoryItemUI m_hover;
        public void Start()
        {
            Hide();
            mouseFollower.Toggle(false);
            isDragging = false;

        }

        public override void InitalizeItems(List<InventoryItemUI> listToAddTo, InventoryItemUI curItem)
        {
            if (curItem != null)
            {

                itemList.Add(curItem);
                // handles what happens if itemUI is left clicked
                curItem.onItemClicked += HandleShowItemActions;
                // handles what happens if itemUI being dragged
                curItem.onItemBeginDrag += HandleBeginDrag;
                // handesl what happens if itemUI is being dropped on another itemUI
                curItem.onItemDroppedOn += HandleSwap;
                curItem.onAddingAbility += HandleAddingAbility;
                curItem.onEquipping += HandleEquipping;
                // handles what happens when item is dropped after being dragged.
                curItem.onItemEndDrag += HandleEndDrg;
                // handles what happens if  itemUI right clicked
                curItem.onRightMouseBtnClick += HandleShowItemActions;
                curItem.onPointerEnter += HandlePointerEnter;
                curItem.onPointerExit += HandlePointerExit;



            }
        }

        public void SetStatText()
        {
            if (curUnit != null)
            {
                statText.text = "";
                if (curUnit.Stats != null)
                {
                    statText.text += curUnit.Stats.Speed.ToString() + "\n";
                    statText.text += curUnit.Stats.Strength.ToString() + "\n";
                    statText.text += curUnit.Stats.Magic.ToString() + "\n";
                    statText.text += curUnit.Stats.Durability.ToString() + "\n";
                    statText.text += curUnit.Stats.Intelligence.ToString() + "\n";
                    statText.text += curUnit.Stats.Vitality.ToString() + "\n";
                    statText.text += curUnit.Stats.Charisma.ToString() + "\n";

                    statHeaderText.text = "";

                    statHeaderText.text += curUnit.Title.ToString() + "\n";
                    statHeaderText.text += "Lv: " + curUnit.Stats.Level.ToString() + "\n";
                    statHeaderText.text += "Hp: " + curUnit.Stats.CurHealth.ToString() + "/" + curUnit.Stats.Maxhealth.ToString() + "\n";
                    statHeaderText.text += "Mp: " + curUnit.Stats.CurMP.ToString() + "/" + curUnit.Stats.MaxMP.ToString() + "\n";
                    statHeaderText.text += "Exp: " + curUnit.Stats.CurExp.ToString() + "/" + curUnit.Stats.MaxExp.ToString() + "\n";
                    statHeaderText.text += "Money: " + curUnit.Stats.Money.ToString() + " $\n";

                }
                else
                {
                    Debug.Log("stats is null");
                }
            }
        }
        /*---------------------------------------------------------------------
        |  Method CreateInventoryUI(int inventorySize)
        |
        |  Purpose: Creates itemList by filling the list of inventoryUI
        |           with "inventorySeize" elements
        |  
        |   Parameters: int inventorySize = size of listoftiems 
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        public void CreateInventoryUI(int inventorySize)
        {
            // initlaizes invenotry based on how many cildren the controlPanel gameboject has.
            // this is determined in the unity editor
            // this collects all inventory item slots in the current scene
            int childCount = inventorySize;
            if (dataOfInventory != null)
            {
                childCount = dataOfInventory.GetSize();
            }
            SetStatText();
            for (int i = 0; i < childCount; i++)
            {

                InventoryItemUI curItem = Instantiate(item);
                curItem.transform.SetParent(container);
                curItem.gameObject.transform.localScale = Vector3.one;
                InitalizeItems(inventoryList, curItem);
                curItem.ItemContainer = this;


            }
            if (equipped != null)
            {
                foreach (Transform child in equipped)
                {
                    InventoryItemUI curItem = child.GetComponent<InventoryItemUI>();
                    curItem.type = InventoryItemUI.State.Equipped;
                    InitalizeItems(equipmentList, curItem);
                    equipmentList.Add(curItem);
                }
            }
            if (abilites != null)
            {
                foreach (Transform child in abilites)
                {
                    InventoryItemUI curItem = child.GetComponent<InventoryItemUI>();
                    curItem.type = InventoryItemUI.State.Ability;
                    InitalizeItems(abilityList, curItem);
                    abilityList.Add(curItem);
                }
            }


        }

        private void HandleEquipping(InventoryItemUI uI)
        {
            //throw new NotImplementedException();
        }

        private void HandleAddingAbility(InventoryItemUI uI)
        {
            // throw new NotImplementedException();
        }

        protected override void HandlePointerExit(InventoryItemUI currentItem)
        {

            int index = itemList.IndexOf(currentItem);

            if (index == -1)
            {
                return;
            }
            curSelectedItem = null;
            currentItem.Deselect();
            if (onDescriptionClear != null && !isDragging)
            {
                DeselectAllItems2();
                onDescriptionClear.Invoke(index, currentItem);
            }

        }

        protected override void HandlePointerEnter(InventoryItemUI currentItem)
        {
            int index = itemList.IndexOf(currentItem);
            // Debug.Log(" << pointer exit " + currentItem.ItemData.name + "  at " + index);
            if (index == -1 || curSelectedItem != null)
            {
                return;
            }
            currentItem.Select();

            // onDescriptionRequest?.Invoke(index);
            if (onDescriptionRequest != null && !isDragging)
            {
                onDescriptionRequest.Invoke(index, currentItem);
            }
            curSelectedItem = currentItem;
        }

        private void HandleSwapWithDifInvenotry(InventoryItemUI uI1, InventoryItemUI uI2)
        {
            //    Debug.Log("trying to swap " + uI1.name + "  " + uI2.name);
        }

        /*---------------------------------------------------------------------
        |  Method  HandleItemSelection(InventoryItemUI currentItem)
        |
        |  Purpose: Sends the inforamtion about the slot 
        |           being selected to inventory controller
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being selected
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        protected override void HandleItemSelection(InventoryItemUI currentItem)
        {
            int index = itemList.IndexOf(currentItem);

            if (index == -1)
            {
                return;
            }

        }
        /*---------------------------------------------------------------------
        |  Method HandleBeginDrag(InventoryItemUI currentItem)
        |
        |  Purpose: Sends the inforamtion about the slot 
        |           being dragged to inventory controller
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being dragged
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        protected override void HandleBeginDrag(InventoryItemUI currentItem)
        {

            int index = itemList.IndexOf(currentItem);
            if (index == -1)
            {
                mouseFollower.Toggle(false);
                return;
            }
            // grabs index of clicked on slot
            currentItemIndex = index;
            // selects current slot
            HandleItemSelection(currentItem);
            if (onStartDrag != null)
            {
                onStartDrag.Invoke(index);
            }

        }

        public override void CreateDraggedItem(Sprite sprite, int count)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, count);
            isDragging = true;
        }

        /*---------------------------------------------------------------------
        |  Method HandleSwap(InventoryItemUI currentItem)
        |
        |  Purpose: Sends the inforamtion about the slot 
        |           being swapped to inventory controller
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being swapped
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void HandleSwap(InventoryItemUI currentItem)
        {

            int index = itemList.IndexOf(currentItem);
            if (index <= -1 || currentItem == null || index >= itemList.Count)
            {
                return;
            }
            // InventoryItemUI itemToSwap = new InventoryItemUI();
            if (currentItemIndex >= 0)
            {
                InventoryItemUI itemToSwap = itemList[currentItemIndex];
                // tried to swap with invalid or out of bounds
                if (currentItem.GetParentInventory() != itemToSwap.GetParentInventory())
                {


                    if (onTransferItems != null)
                    {
                        onTransferItems.Invoke(itemToSwap, currentItem, itemToSwap.GetIndex(), currentItem.GetIndex());
                    }
                    return;
                }


                // swaps items
                if (onSwapItems != null)
                {
                    onSwapItems.Invoke(currentItemIndex, index);
                }
            }
            // selects currenly dragged item
            HandleItemSelection(currentItem);
        }

        /*---------------------------------------------------------------------
        |  Method HandleEndDrg(InventoryItemUI currentItem)
        |
        |  Purpose: Clears the draggedItem when mouse is let go
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being let go
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void HandleEndDrg(InventoryItemUI currentItem)
        {
            ResetDraggedItem();
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentItemIndex = -1;
            isDragging = false;
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemCount, ItemSO itemData)
        {
            if (itemList.Count > itemIndex)
            {
                itemList[itemIndex].SetData(itemImage, itemCount, itemData);
            }

        }
        public void UpdateAbilityData(int itemIndex, Sprite itemImage, int itemCount, ItemSO itemData)
        {
            if (abilityList.Count > itemIndex)
            {
                abilityList[itemIndex].SetData(itemImage, itemCount, itemData);
            }

        }
        public void UpdateEquipmentData(int itemIndex, Sprite itemImage, int itemCount, ItemSO itemData)
        {
            if (equipmentList.Count > itemIndex)
            {
                equipmentList[itemIndex].SetData(itemImage, itemCount, itemData);
            }

        }
        private void HandleShowItemActions(InventoryItemUI currentItem)
        {
            int index = itemList.IndexOf(currentItem);
            //check if slot tring to preform is in bounds
            if (index <= -1)
            {
                return;
            }
            onItemActionRequest?.Invoke(index);

        }
        /*---------------------------------------------------------------------
        |  Method Show()
        |
        |  Purpose: Shows the game object from view
        |
        |   Parameters: none
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        public override void Show()
        {
            gameObject.SetActive(true);
            ResetSelected();
            if (itemList[0] != null)
            {
                //  HandleShowItemActions(itemList[0]);
                //  itemList[0].
                // itemList[0].button.onClick.Invoke();
            }
        }

        /*---------------------------------------------------------------------
        |  Method Show()
        |
        |  Purpose: hides the game object from view
        |  
        |   Parameters: none
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        public override void Hide()
        {
            itemMenu.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();

        }

        public void ResetSelected()
        {
            //  itemDescription.ResetDescription();
            DeselectAllItems();
        }
        public void AssignUnit(Unit curUnit)
        {
            this.curUnit = curUnit;
            dataOfInventory = curUnit.Inventory;

        }

        public void AddAction(string actoionName, Action action)
        {
            itemMenu.AddButon(actoionName, action);
        }
        /*---------------------------------------------------------------------
        *  Method ShowItemAction(int currentItemIndex)
        *
        *  Purpose: shows action menu and moves it to current item clicked.
        *           is called bu inventoryUIController
        *
        *   Parameters: int currentItemIndex = index of current item clicked
        *
        *  Returns: none
        *-------------------------------------------------------------------*/
        public void ShowItemAction(int currentItemIndex)
        {
            itemMenu.Toggle(true);
            itemMenu.transform.position = itemList[currentItemIndex].transform.position;
        }
        private void DeselectAllItems()
        {
            // deselects all items 
            foreach (InventoryItemUI item in itemList)
            {
                item.Deselect();
            }
            // hides item action menu
            itemMenu.Toggle(false);
        }
        private void DeselectAllItems2()
        {
            // deselects all items 
            foreach (InventoryItemUI item in itemList)
            {
                item.Deselect();
            }
            // hides item action menu
            //itemMenu.Toggle(false);
        }
        public List<InventoryItemUI> GetItemList()
        {
            return itemList;
        }

        public List<InventoryItemUI> AbilityList
        {
            get
            {
                return abilityList;
            }
            set
            {
                abilityList = value;
            }
        }
        public List<InventoryItemUI> EquipmentList
        {
            get
            {
                return equipmentList;
            }
            set
            {
                equipmentList = value;
            }
        }
        public Image ProfileImage
        {
            get { return profileImage; }
            set { profileImage = value; }
        }
        public Unit CurUnit
        {
            get
            {
                return curUnit;
            }
            set
            {
                curUnit = value;
            }
        }
        /*---------------------------------------------------------------------
        *  Method UpdateDescription(int curItemIndex, Sprite image, string name, string description,InventoryItemUI itemUIData)
        *
        *  Purpose: Gets the description of the itemUI being hovered over/selected
        *           does so by first checking the tyoe of the UI. and then updtes the UI depenging
        *           on it its a normal, equipped, or ability item
        *
        *   Parameters: int curItemIndex = index of item to show 
        *               Sprite image = image of item to show
        *               string name = name of item to show
        *               string description = descriptin of itme to show
        *               InventoryItemUI itemUIData = UI data of item to showd
        *
        *  Returns: none
        *-------------------------------------------------------------------*/
        public void UpdateDescription(int curItemIndex, Sprite image, string name, string description, InventoryItemUI itemUIData)
        {

            // updates desctiption to currently slected item
            mouseFollower.SetDescriptionData(image, name, description);
            mouseFollower.ToggleDescription(true);
            // clears all current selections
            DeselectAllItems();
            // selects the desired slot based on itemUI type
            if (itemUIData.type == InventoryItemUI.State.Equipped)
            {
                equipmentList[curItemIndex].Select();
            }
            else if (itemUIData.type == InventoryItemUI.State.Ability)
            {
                abilityList[curItemIndex].Select();
            }
            else
            {
                itemList[curItemIndex].Select();
            }
        }

        public void ResetAllItems()
        {

            // int,InventoryItem> inventoryData = dataOfInventorys.GetCurrentInventoryState();
            for (int i = 0; i < dataOfInventory.GetSize(); i++)
            {
                itemList[i].ResetData();
                itemList[i].Deselect();
            }


        }

        public void ResetAllAbilites()
        {
            int abilityStart = dataOfInventory.GetSize() + equipmentList.Count;
            int abilityEnd = abilityStart + abilityList.Count;
            for (int i = abilityStart; i < abilityEnd; i++)
            {
                itemList[i].ResetData();
                itemList[i].Deselect();
            }

        }
        public void ResetAllInventory()
        {
            ResetAllItemsList(inventoryList);
        }
        public void ResetAllEquipment()
        {
            int equipmentStart = dataOfInventory.GetSize();
            int equipmentEnd = equipmentStart + equipmentList.Count;
            for (int i = equipmentStart; i < equipmentEnd; i++)
            {
                itemList[i].ResetData();
                itemList[i].Deselect();
            }

        }
        public void ResetAllItemsList(List<InventoryItemUI> itemListToResset)
        {
            foreach (InventoryItemUI item in itemListToResset)
            {
                item.ResetData();
                item.Deselect();
            }
        }

        public void ResetDescription()
        {
            //  itemDescription.ResetDescription();
            mouseFollower.ToggleDescription(false);
            mouseFollower.Description.ResetDescription();

        }
        public TMP_Text StatText
        {
            get
            {
                return statText;
            }
            set
            {
                statText = value;
            }
        }
        public TMP_Text StatHeaderText
        {
            get
            {
                return statHeaderText;
            }
            set
            {
                statHeaderText = value;
            }
        }

        public HealthBar HealthBar
        {
            get
            {
                return healthBar;
            }
            set
            {
                healthBar = value;
            }
        }


    }
}
