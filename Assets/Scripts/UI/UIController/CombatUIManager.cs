using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CombatUIManager : MonoBehaviour
{
    [SerializeField] private List<CombatUIButton> buttons;
    [SerializeField] private CombatMenuUI actionMenu;
    public event Action<CombatUIButton> onDescriptionRequest, onItemActionRequest, onStartDrag;
    public event Action<CombatUIManager> onMagicSelect, onMoveButtonSelect, onActionButtonSelect, onWaitButtonSelect,
        onMagicMenuButtonSelect, onAttackSelect, onPushButtonSelect, onItemButtonSelect, onRunButtonSelect, onTeamUpSelected, onSubmitTeamUpSelected;
    public event Action<CombatUIManager, ItemSO> onCaptureItemSelected;
    public event Action<UnitCombatUIContainer, UnitCombatUI> onPointerEnterUnit, onPointerExitUnit;
    [SerializeField] private bool isActionMenuOpen, isCombatMenuOpen;
    [SerializeField] private CombatMenuUI combatMenu;
    [SerializeField] private MoveListUIController moveListUIController;
    [SerializeField] private Unit InitialUnit;
    [SerializeField] private Unit curUnit;
    [SerializeField] private CombatItemPageUI combatItemPage;
    [SerializeField] private InventorySO unitInventory;
    [SerializeField] private UnitStatusUI unitStatusUI;
    [SerializeField] private ProfileImageUI unitProfilePrefab;
    [SerializeField] private UnitProfileListUI unitCombatListUI;
    [SerializeField] private PlayerController player;
    [SerializeField] private UnitCombatUIContainer unitCombatUIContainer;
    [SerializeField] private GridCombatSystem combatSystem;
    [SerializeField] private TurnCountUIController turnCountUIController;
    [SerializeField] private HealthBar teamUpStatusBar;
    [SerializeField] private MouseFollower mouseFollower;
    [SerializeField] private ButtonUI submitTeamupButton;
    [SerializeField] private ButtonUI teamupButton;
    [SerializeField] private Animator teamStatsuAnimator;
    [SerializeField] private TMP_Text teamStatsText;
    private bool isItemPageOpen;
    private bool isMagicMenuOpen;
    private List<Unit> units;
    private bool isActive;
    // Start is called before the first frame update
    void Start()
    {
        combatMenu = GetComponentInChildren<CombatMenuUI>();
        AssignUnit(InitialUnit);
        InitailizePages();
        PrepareInventory();
        Deactivate();


    }

    public void CompleteTurn(int movesMade)
    {
        turnCountUIController.CompleteTurn(movesMade);
    }
    public void UndoTurn()
    {
        turnCountUIController.UndoTurn();
    }
    public void InializeTurnCount(int size)
    {
        turnCountUIController.InitalizeTurns(size);
    }
    public void InializeMultiSelectCount(int size)
    {
        mouseFollower.InitliazeTurnCounter(size);
    }
    public void ResetMutliSelectCount()
    {
        mouseFollower.ResetTurnCounter();
    }
    public void ResetTurns()
    {
        turnCountUIController.ResetTurns();
    }
    private void PrepareInventory()
    {
        unitInventory.onInventoryUpdated += UpdateInventoryUI;
        moveListUIController.onMagicAttack += HandleMagicAttack;
        combatMenu.onAttackButtonClick += HanldeAttack;

    }
    public void ActivateCombatMenu(bool isActive)
    {
        combatMenu.Activate(isActive);
        turnCountUIController.Activate(isActive);
        submitTeamupButton.Activate(false);

    }


    /*---------------------------------------------------------------------
    *  Method: ActivateItem(bool isActive)
    *
    *  Purpose: Closes or opens the item page
    *           
    *  Parameters: bool isActive = if open or not
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void ActivateItem(bool isActive)
    {
        if (isActive)
        {
            combatItemPage.Show();
            // updates UI to match data
            InventorySO inventoryData = combatItemPage.DataOfInventory;
            int count = 0;
            foreach (var item in inventoryData.GetCurrentInventoryState())
            {
                if (item.Value.item.type.Equals(ItemSO.Type.Consumable) || item.Value.item.type.Equals(ItemSO.Type.Capture))
                {
                    combatItemPage.UpdateData(count,
                        item.Value.item.GetImage(),
                        item.Value.count);
                    count++;
                }
                else
                {
                    continue;
                }
            }

            isItemPageOpen = true;
            return;
        }
        else
        {

            combatItemPage.Hide();
            isItemPageOpen = false;
        }
    }

    /*---------------------------------------------------------------------
     *  Method: HanldeAttack(CombatUIButton button)
     *
     *  Purpose: Handles what happends to the UI when attack buton is clicked\
     *           it closes all other pages and tells combat System attack was selected
     *
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HanldeAttack(CombatUIButton button)
    {
        button.Press();
        if (onAttackSelect != null)
        {
            onAttackSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);


    }

    /*---------------------------------------------------------------------
     *  Method: PrepareCombatList()
     *
     *  Purpose: Prepares combat order UI
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void PrepareCombatList()
    {
        unitCombatListUI.ClearInventoryUI();
        unitCombatListUI.CreateInventoryUI(units);
        unitCombatListUI.onDescriptionRequest += HandleProfileDescriptionRequest;
        unitCombatListUI.onDescriptionClear += HandleDescriptionClear;
    }

    /*---------------------------------------------------------------------
     *  Method: AssignTeams()
     *
     *  Purpose: Assigns unts for combat order UI
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AssignTeams()
    {
        unitCombatListUI.AssignTeams(units);
    }

    /*---------------------------------------------------------------------
     *  Method: HandleProfileDescriptionRequest(int unitIndex)
     *
     *  Purpose: Handles when player hovers over unit profile in unit comat order UI
     *
     *  Parameters: int unitIndex = index of profile selected
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleProfileDescriptionRequest(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= units.Count)
        {
            Debug.Log("out of bounds");
            return;
        }
        Unit curUNit = units[unitIndex];
        if (curUNit == null)
        {
            Debug.Log("invalid unit");
            return;
        }
        unitCombatListUI.UpdateDescription(unitIndex, curUnit.Stats.ProfileImage, curUnit.name, curUnit.Stats.CurHealth.ToString());

    }

    /*---------------------------------------------------------------------
     *  Method: InitiateUnitsForCombat(List<Unit> units)
     *
     *  Purpose: Initiate units unitCombatUI so apporate UI appears when hovered over
     *
     *  Parameters: List<Unit> unit = units to initalize
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void InitiateUnitsForCombat(List<Unit> units)
    {
        unitCombatUIContainer.InitateForCombat(units);
        //   unitCombatUIContainer.onPointerEnter += HandleUnitPointerEnter;
        //  unitCombatUIContainer.onPointerExit += HandleUnitPointerExit;
        unitCombatUIContainer.onUnitClicked += HandleUnitPointerClick;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleUnitPointerClick(UnitCombatUI uI)
     *
     *  Purpose: Handles when player clicks on unit during combat
     *
     *  Parameters: UnitCombatUI uI = uI of unit clicked
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleUnitPointerClick(UnitCombatUI uI)
    {

    }

    /*---------------------------------------------------------------------
     *  Method: HandleUnitPointerExit(UnitCombatUI uI)
     *
     *  Purpose: Handles when player stops hovering over unit during combat
     *
     *  Parameters: UnitCombatUI uI = uI of unit clicked
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleUnitPointerExit(UnitCombatUI uI)
    {

        /*if (unitCombatUIContainer == null || uI == null) {
            return;
        }
        combatSystem.IsHitChance = false;
        uI.Unit.CombatIconAnimator.SetBool("isHitChance", combatSystem.IsHitChance);*/
    }

    /*---------------------------------------------------------------------
     *  Method: HandleUnitPointerEnter(UnitCombatUI uI)
     *
     *  Purpose: Handles when player starts hovering over unit during combat
     *
     *  Parameters: UnitCombatUI uI = uI of unit clicked
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void HandleUnitPointerEnter(UnitCombatUI uI)
    {


        int hitChance = 0;
        switch (combatSystem.CurCommand)
        {
            case GridCombatSystem.Command.Move:
                break;
            case GridCombatSystem.Command.MagicAttack:
                hitChance = (int)curUnit.GetMagicAttackHitChance(curUnit.CurMove, uI.Unit.CurNode);
                break;
            case GridCombatSystem.Command.Special:
                hitChance = (int)curUnit.GetMagicAttackHitChance(curUnit.CurMove, uI.Unit.CurNode);
                break;
            case GridCombatSystem.Command.Attack:
                hitChance = (int)curUnit.GetAttackHitChance(uI.Unit);
                break;
            case GridCombatSystem.Command.Push:
                hitChance = (int)curUnit.GetPushChance(uI.Unit);
                break;
            case GridCombatSystem.Command.Capture:
                hitChance = (int)player.GetCaptureRate(uI.Unit, combatSystem.CurCaptureItem.CaptureItemRate);
                break;
            case GridCombatSystem.Command.Waiting:
                break;

        }
        ShowUnitHitChance(uI, hitChance);
        combatSystem.IsHitChance = true;
    }
    public void SetUITextColor(UnitCombatUI uI)
    {
        if (unitCombatUIContainer == null || uI == null || combatSystem.IsMakingHitableMove == false)
        {
            return;
        }
        if (uI.Unit.CombatIconAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {

            return;
        }

        if (uI.Unit.Team != curUnit.Team)
        {
            uI.Unit.CombatHitIconText.color = Color.red;
        }
        else
        {
            uI.Unit.CombatHitIconText.color = Color.green;
        }

    }
    public void ShowUnitHitChance(UnitCombatUI curUI, int hitChance)
    {
        SetUITextColor(curUI);
        if (curUI.Unit.Team != curUnit.Team)
        {
            curUI.Unit.CombatHitIconText.color = Color.red;
        }
        else
        {
            curUI.Unit.CombatHitIconText.color = Color.green;
        }
        curUI.Unit.CombatHitIconText.text = hitChance.ToString() + " %";
        if (!combatSystem.IsHitChance)
        {
            curUI.Unit.CombatIconAnimator.SetBool("isHitChance", true);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: HandleDeadUnit(int deadINdex,Unit deadUnit
     *
     *  Purpose: Handles when unit dies and remves them from combat order UI
     *
     *  Parameters: int deadINdex = index of unt who died in combat order UI
     *              Unit deadUnit == unit who died
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void HandleDeadUnit(int deadIndex, Unit deadUnit)
    {
        unitCombatListUI.RemoveUnit(deadIndex, deadUnit);
    }

    /*---------------------------------------------------------------------
     *  Method: UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate)
     *
     *  Purpose: Updates the items page to match the curUnits invenotry
     *
     *  Parameters: Dictionary<int, InventoryItem> inventorySate = state of curUnits inventory
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate)
    {
        combatItemPage.ResetAllItems();
        // updates UI with current inventory data
        int count = 0;
        foreach (var item in inventorySate)
        {
            if (item.Value.item.type.Equals(ItemSO.Type.Consumable) || item.Value.item.type.Equals(ItemSO.Type.Capture))
            {
                combatItemPage.UpdateData(count,
                item.Value.item.GetImage(),
                item.Value.count);
                count++;
            }
            else
            {
                continue;
            }

        }
    }
    /*---------------------------------------------------------------------
     *  Method: InitailizePages()
     *
     *  Purpose: initalize the pages of the combat UI
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void InitailizePages()
    {
        if (unitInventory == null)
        {
            return;
        }
        turnCountUIController.CreateTurnOrdes(2);
        turnCountUIController.InitalizeTurns(2);
        int childCount = gameObject.transform.childCount;
        isActionMenuOpen = false;
        isCombatMenuOpen = false;
        combatItemPage.CreateInventoryUI(unitInventory.GetSize());
        combatItemPage.onDescriptionRequest += HandleDescriptionRequest;
        combatItemPage.onDescriptionClear += HandleDescriptionClear;
        combatItemPage.onItemActionRequest += HandleItemActionRequest;
        if (combatMenu != null)
        {
            combatMenu.InitalizeMenu();
            combatMenu.onButtonClick += HandleButtonClick;
            combatMenu.onActionsMenuButtonClick += HandleActionsMenuButtonClick;
            combatMenu.onItemMenuButtonClick += HandleItemMenuButtonClick;
            combatMenu.onWaitButtonClick += HandleWaitButtonClick;
            combatMenu.onMoveButtonClick += HandleMoveButtonClick;
            combatMenu.onSelectButton += HandleSelectButton;
            combatMenu.onDeselectButton += HandleDeselectButton;
            combatMenu.onReleaseButton += HandleReleaseButton;
            combatMenu.onMagicMenuButtonClick += HandleMagicMenuButtonClick;
            combatMenu.onPushButtonClick += HandlePushButtonClick;
            combatMenu.onRunButtonClick += HandleRunButtonClick;
            combatMenu.onCaptureButtonClick += HandleCaptureButtonClick;
            combatMenu.onTeamUpButtonClick += HandleTeamUpButtonClick;
            combatMenu.onSubmitTeamUpClick += HandleSubmitTeamUpButtonClick;
        }
    }

    /*---------------------------------------------------------------------
   *  Method: HandleMagicAttack(int arg1, MoveSO curMove)
   *
   *  Purpose: Handles what happends to the UI when magic attack buton is clicked
   *           it closes all other pages and tells combat System magic attack was selected
   *
   *  Parameters: int arg1 = index of the magiv move chosen
   *              MoveSO curMove = mmagic move being made
   *
   *  Returns: none
   *-------------------------------------------------------------------*/
    private void HandleMagicAttack(int arg1, MoveSO curMove)
    {
        curUnit.CurMove = curMove;
        if (onMagicSelect != null)
        {
            onMagicSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);

    }

    /*---------------------------------------------------------------------
     *  Method: HandleCaptureButtonClick(CombatUIButton button = null)
     *
     *  Purpose: Handles what happens when capture item is used.
     *           It closes all other pages and tells the combat system the curUnit
     *           is trying to capture
     *
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleCaptureButtonClick(CombatUIButton button = null)
    {
        if (button != null)
        {
            button.Press();
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);
    }

    /*---------------------------------------------------------------------
     *  Method: HandleTalkButtonClick(CombatUIButton button)
     *
     *  Purpose: Handles what happens when talk button is pressed.
     *           It closes all other pages and tells the combat system the curUnit
     *           is trying to talk
     *
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandlePushButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onPushButtonSelect != null)
        {
            onPushButtonSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);


    }
    /*---------------------------------------------------------------------
     *  Method: HandleTalkButtonClick(CombatUIButton button)
     *
     *  Purpose: Handles what happens when talk button is pressed.
     *           It closes all other pages and tells the combat system the curUnit
     *           is trying to talk
     *
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleRunButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onRunButtonSelect != null)
        {
            onRunButtonSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);


    }
    /*---------------------------------------------------------------------
     *  Method: HandleActionsMenuButtonClick(CombatUIButton button)
     *
     *  Purpose: Handles what happens when action menu button is pressed.
     *           It closes all other pages and opens the action menu 
     *           and tells combat system the action menu is pressed
     *
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleActionsMenuButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onActionButtonSelect != null)
        {
            onActionButtonSelect.Invoke(this);
        }
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        isActionMenuOpen = !isActionMenuOpen;
        actionMenu.Activate(isActionMenuOpen);
        submitTeamupButton.Activate(false);


    }

    /*---------------------------------------------------------------------
     *  Method: HandleWaitButtonClick(CombatUIButton button)
     *
     *  Purpose: Handles what happens when wait button is pressed.
     *           It closes all other pages and opens the action menu 
     *           and tells combat system to skip the turn
     *           
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleWaitButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onWaitButtonSelect != null)
        {
            onWaitButtonSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);

    }

    /*---------------------------------------------------------------------
    *  Method: HandleMagicMenuButtonClick(CombatUIButton button)
    *
    *  Purpose: Handles what happens when item button is pressed.
    *           It closes all other pages, opens the magic move page and tells combat 
    *           system the item page was opened
    *           
    *  Parameters: CombatUIButton button = button pressed
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    private void HandleMagicMenuButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onActionButtonSelect != null)
        {
            onActionButtonSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isItemPageOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateItem(isItemPageOpen);
        isMagicMenuOpen = !isMagicMenuOpen;
        moveListUIController.Activate(isMagicMenuOpen);
        submitTeamupButton.Activate(false);


    }

    /*---------------------------------------------------------------------
     *  Method: HandleMoveButtonClick(CombatUIButton button)
     *
     *  Purpose: Handles what happens when move button is pressed.
     *           It closes all other pages and tells combat 
     *           system the curUnit is moving
     *           
     *  Parameters: CombatUIButton button = button pressed
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleMoveButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onMoveButtonSelect != null)
        {
            onMoveButtonSelect.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);


    }
    /*---------------------------------------------------------------------
    *  Method: HandleMoveButtonClick(CombatUIButton button)
    *
    *  Purpose: Handles what happens when move button is pressed.
    *           It closes all other pages and tells combat 
    *           system the curUnit is moving
    *           
    *  Parameters: CombatUIButton button = button pressed
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    private void HandleTeamUpButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onTeamUpSelected != null)
        {
            onTeamUpSelected.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(true);


    }
    /*---------------------------------------------------------------------
   *  Method: HandleMoveButtonClick(CombatUIButton button)
   *
   *  Purpose: Handles what happens when move button is pressed.
   *           It closes all other pages and tells combat 
   *           system the curUnit is moving
   *           
   *  Parameters: CombatUIButton button = button pressed
   *
   *  Returns: none
   *-------------------------------------------------------------------*/
    private void HandleSubmitTeamUpButtonClick(CombatUIButton button)
    {
        button.Press();
        if (onSubmitTeamUpSelected != null)
        {
            onSubmitTeamUpSelected.Invoke(this);
        }
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        isItemPageOpen = false;
        isCombatMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        ActivateCombatMenu(isCombatMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);

    }
    /*---------------------------------------------------------------------
    *  Method: HandleItemMenuButtonClick(CombatUIButton button)
    *
    *  Purpose: Handles what happens when item button is pressed.
    *           It closes all other pages, opens the item page and tells combat 
    *           system the item page was opened
    *           
    *  Parameters: CombatUIButton button = button pressed
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    private void HandleItemMenuButtonClick(CombatUIButton button)
    {
        if (combatItemPage == null || unitInventory == null)
        {
            return;
        }
        if (onItemButtonSelect != null)
        {
            onItemButtonSelect.Invoke(this);
        }
        button.Press();
        isActionMenuOpen = false;
        isMagicMenuOpen = false;
        actionMenu.Activate(isActionMenuOpen);
        moveListUIController.Activate(isMagicMenuOpen);
        isItemPageOpen = !isItemPageOpen;
        ActivateItem(isItemPageOpen);
        submitTeamupButton.Activate(false);

    }

    /*---------------------------------------------------------------------
     *  Method: HandleItemActionRequest(int curItemIndex)
     *
     *  Purpose: Handles what happens when item is clicked.
     *           It opens and fills the item actionMenu with
     *           the actions specific to the item chosen
     *           
     *  Parameters: int curItemIndex = index of the item chosen
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleItemActionRequest(int curItemIndex)
    {
        if (curItemIndex < 0 || curItemIndex >= unitInventory.GetSize())
        {
            return;
        }
        // InventoryItem curInventoryItem = unitInventory.GetItemAt(curItemIndex);
        ItemSO curItem = null;
        Dictionary<int, InventoryItem> unitInventoryData = unitInventory.GetCurrentInventoryState();
        if (unitInventoryData == null)
        {
            return;
        }
        int count = 0;
        foreach (KeyValuePair<int, InventoryItem> curItemData in unitInventoryData)
        {
            if (curItemData.Value.IsEmpty())
            {
                continue;
            }
            curItem = curItemData.Value.item;
            if (curItemData.Value.item.type.Equals(ItemSO.Type.Consumable) || curItemData.Value.item.type.Equals(ItemSO.Type.Capture))
            {
                // correct item is found
                Debug.Log("looking for item 2");
                if (curItemIndex == count)
                {
                    combatItemPage.ShowItemAction(curItemIndex);
                    IItemAction itemAction = curItemData.Value.item as IItemAction;
                    if (itemAction != null)
                    {
                        combatItemPage.AddAction(itemAction.ActionName, () => PreformAction(curItemIndex));
                    }
                    CaptureItemAction captureItemAction = curItemData.Value.item as CaptureItemAction;
                    if (captureItemAction != null)
                    {
                        combatItemPage.AddAction(captureItemAction.ActionName, () => PreformAction(curItemIndex));

                    }
                    return;
                }
                count++;
            }
        }
    }

    /*---------------------------------------------------------------------
     *  Method: HandleDescriptionClear(int obj)
     *
     *  Purpose: Clears the description of the item page
     *           
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleDescriptionClear()
    {
        combatItemPage.ResetDescription();
        unitCombatListUI.ResetDescription();
    }

    /*---------------------------------------------------------------------
     *  Method: getCombatItem(int curItemIndex)
     *
     *  Purpose:gets the correct item index for the item page 
     *  so only consumables,capture item are available
     *           
     *  Parameters: int curItemIndex = index of item in overall unit inventory
     *
     *  Returns: = the index of the item chosen in the combatItem page
     *-------------------------------------------------------------------*/
    private int getCombatItem(int curItemIndex)
    {
        if (curItemIndex < 0 || curItemIndex >= unitInventory.GetSize())
        {
            return -1;
        }
        InventoryItem curInventoryItem = unitInventory.GetItemAt(curItemIndex);
        ItemSO curItem = curInventoryItem.item;
        Dictionary<int, InventoryItem> unitInventoryData = unitInventory.GetCurrentInventoryState();
        if (unitInventoryData == null)
        {
            return -1;
        }
        int count = 0;
        int index = 0;
        // finds correct consumeable
        foreach (KeyValuePair<int, InventoryItem> curItemData in unitInventoryData)
        {
            if (curItemData.Value.IsEmpty())
            {
                continue;
            }
            curItem = curItemData.Value.item;
            if (curItemData.Value.item.type.Equals(ItemSO.Type.Consumable) || curItemData.Value.item.type.Equals(ItemSO.Type.Capture))
            {
                // correct item is found
                if (curItemIndex == count)
                {
                    return curItemData.Key;
                }
                count++;
            }
            index++;
        }

        return -1;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleDescriptionRequest(int curItemIndex)
     *
     *  Purpose: gets the description of item being hovered over and displays it 
     *           
     *  Parameters: int curItemIndex = index of item being hovered over
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleDescriptionRequest(int curItemIndex)
    {
        if (curItemIndex < 0 || curItemIndex >= unitInventory.GetSize())
        {
            return;
        }
        ItemSO curItem = null;
        Dictionary<int, InventoryItem> unitInventoryData = unitInventory.GetCurrentInventoryState();
        if (unitInventoryData == null)
        {
            return;
        }
        int count = 0;
        // finds correct consumeable
        foreach (KeyValuePair<int, InventoryItem> curItemData in unitInventoryData)
        {
            if (curItemData.Value.IsEmpty())
            {
                continue;
            }
            curItem = curItemData.Value.item;
            if (curItemData.Value.item.type.Equals(ItemSO.Type.Consumable) || curItemData.Value.item.type.Equals(ItemSO.Type.Capture))
            {
                // correct item is found
                if (curItemIndex == count)
                {
                    combatItemPage.UpdateDescription(curItemIndex, curItem.GetImage(),
                    curItem.GetName(), PrepareDescription(curItemData.Value));
                    return;
                }
                count++;
            }
        }
        combatItemPage.ResetDescription();
        return;

    }

    /*---------------------------------------------------------------------
     *  Method: PreformAction(int curItemIndex)
     *
     *  Purpose: preforms the action of a specific item based on its type
     *           
     *  Parameters: int curItemIndex = item being used
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void PreformAction(int curItemIndex)
    {
        int combatIndex = getCombatItem(curItemIndex);
        InventoryItem inventoryItem = unitInventory.GetItemAt(combatIndex);
        // avoids empty slots
        if (inventoryItem.IsEmpty())
        {
            return;
        }

        IItemAction itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            itemAction.PerformAction(gameObject, inventoryItem.itemState);
            inventoryItem = new InventoryItem(inventoryItem.item, inventoryItem.count, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem.level + 1);
            // audioSource.PlayOneShot(itemAction.actionSFX);
            if (unitInventory.GetItemAt(combatIndex).IsEmpty())
            {
                combatItemPage.ResetSelected();
            }
        }
        CaptureItemAction capureItemAction = inventoryItem.item as CaptureItemAction;
        if (capureItemAction != null)
        {
            if (inventoryItem.item.type == ItemSO.Type.Capture)
            {
                capureItemAction.PerformCaptureAction(player);
                HandleCaptureButtonClick();
                if (onCaptureItemSelected != null)
                {
                    onCaptureItemSelected.Invoke(this, inventoryItem.item);
                }
            }
            inventoryItem = new InventoryItem(inventoryItem.item, inventoryItem.count, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem.level + 1);
            // audioSource.PlayOneShot(itemAction.actionSFX);
            if (unitInventory.GetItemAt(combatIndex).IsEmpty())
            {
                combatItemPage.ResetSelected();
            }
        }
        IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            //Debug.Log("remvoing action");
            unitInventory.RemoveItem(combatIndex, 1);
        }

        combatItemPage.ItemMenu.Toggle(false);
    }

    /*---------------------------------------------------------------------
     *  Method: PrepareDescription(InventoryItem inventoryItem)
     *
     *  Purpose: prepares description for item
     *           
     *  Parameters: InventoryItem inventoryItem = item being hovered over
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private string PrepareDescription(InventoryItem inventoryItem)
    {
        String newDescription = "";
        if (inventoryItem.IsEmpty())
        {
            return "";
        }
        newDescription += inventoryItem.item.description + "\n\n";
        for (int i = 0; i < inventoryItem.itemState.Count; i++)
        {
            newDescription += inventoryItem.itemState[i].itemParameter.ParameterName.ToString() + ": " +
                inventoryItem.itemState[i].val.ToString() + "/" + inventoryItem.itemState[i].val.ToString() + "\n";
        }

        return newDescription;
    }

    /*---------------------------------------------------------------------
     *  Method: AssignUnit(Unit unit)
     *
     *  Purpose: Assign the owner of the combatUI
     *           assigns the combat item page, 
     *           magic move page, and invenotry to the data of the unit
     *           
     *  Parameters: int curItemIndex = item being used
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AssignUnit(Unit unit)
    {
        // clears last unit listeners
        if (curUnit != null)
        {
            curUnit.Stats.onDamage -= HandleUnitDamage;
            curUnit.Stats.onHeal -= HandelUnitHeal;
            curUnit.Stats.onUseMana -= HandleUseMana;
            curUnit.Stats.onAddMana -= HandleAddMana;
            if (unitInventory != null)
            {
                unitInventory.onInventoryUpdated -= UpdateInventoryUI;
            }
        }
        // sets up next unit
        curUnit = unit;
        if (unit != null)
        {
            curUnit.Stats.SetHealth();
            curUnit.Stats.SetMana();
            // sets up status page UI
            curUnit.Stats.onDamage += HandleUnitDamage;
            curUnit.Stats.onHeal += HandelUnitHeal;
            curUnit.Stats.onUseMana += HandleUseMana;
            curUnit.Stats.onAddMana += HandleAddMana;
            unitStatusUI.AssignUnit(curUnit);
            // sets up inventory
            unitInventory = unit.Inventory;
            if (unitInventory != null)
            {
                unitInventory.onInventoryUpdated += UpdateInventoryUI;
                combatItemPage.ClearInventoryUI();
                combatItemPage.CreateInventoryUI(unitInventory.GetSize());
            }
            // matches item page to unit's data
            combatItemPage.DataOfInventory = unitInventory;
            combatItemPage.ResetAllItems();

            unitStatusUI.ProfileImage.sprite = curUnit.Stats.ProfileImage;
            // matches magic move to unit's data
            moveListUIController.AbilityList = unit.AbilityList;
            if (moveListUIController.AbilityList != null)
            {
                moveListUIController.CurUnit = curUnit;
                moveListUIController.PrepareInventory();
                moveListUIController.ResetMoveList();
            }
            isActionMenuOpen = false;
            isMagicMenuOpen = false;
            isItemPageOpen = false;
            // activates necessary UI pages
            actionMenu.Activate(isActionMenuOpen);
            moveListUIController.Activate(isMagicMenuOpen);
            ActivateItem(isItemPageOpen);
        }
    }
    public void ActivateMultiSelectCounter(bool val)
    {
        mouseFollower.TurnCounter(val);
    }
    public void ActivateSubmitTeamUp(bool val)
    {
        submitTeamupButton.Activate(val);
    }
    public void ActivateTeamUp(bool val)
    {
        teamupButton.Activate(val);
    }
    public void SelectMultiSelect()
    {
        mouseFollower.AdvanceTurnCounter(1);
    }

    private void HandleAddMana(UnitStats stats, float manaAmount)
    {
        unitStatusUI.SetMana();
    }

    private void HandleUseMana(UnitStats stats, float manaAmount)
    {
        unitStatusUI.SetMana();
    }

    private void HandelUnitHeal(UnitStats stats, float healAmount)
    {
        unitStatusUI.SetHealth();
    }
    public void HandeleTeamUp(float teamUpMax, float teamUpDatatus)
    {
        if (teamUpStatusBar == null) return;

        teamUpStatusBar.SetSliderMax(teamUpMax);
        teamUpStatusBar.SetSlider(teamUpDatatus);
        if (teamUpDatatus > 0)
        {
            teamStatsText.text = '+' + teamUpDatatus.ToString();
            //  teamStatsText.color = Color.cyan;
            StartCoroutine(PlayTeamUpText());
        }

    }
    public IEnumerator PlayTeamUpText()
    {

        // unitHitAnimsPlayed += 1;
        teamStatsuAnimator.SetTrigger("StatusChange");
        yield return null;

    }
    private void HandleUnitDamage(UnitStats stats, float damageAmount)
    {
        unitStatusUI.SetHealth();
    }

    private void HandleReleaseButton(CombatUIButton button)
    {
        button.Release();
    }
    private void HandleSelectButton(CombatUIButton button)
    {
        button.Select();
    }
    private void HandleDeselectButton(CombatUIButton button)
    {
        button.Deselct();
    }
    private void HandleButtonClick(CombatUIButton button)
    {
        button.Press();
    }
    public void Activate()
    {
        isActive = true;
        this.gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        isActive = false;
        moveListUIController.Activate(false);
        combatItemPage.Hide();
        gameObject.SetActive(false);
        actionMenu.Activate(false);
    }
    public void SelectNextUnitTurn(Unit curUnit)
    {
        unitCombatListUI.SelectCurUnitForComabt(curUnit);
    }
    public void SelectNextUnitDescription(int curIndex)
    {
        unitCombatListUI.SelectCurUnit(curIndex);
    }
    public void ResetUnitUIs()
    {
        //  unitCombatUIContainer.onPointerEnter -= HandleUnitPointerEnter;
        //  unitCombatUIContainer.onPointerExit -= HandleUnitPointerExit;
        unitCombatUIContainer.onUnitClicked -= HandleUnitPointerClick;
    }
    public List<Unit> Units { get { return units; } set { units = value; } }
    public bool IsActive { get { return isActive; } }


}
