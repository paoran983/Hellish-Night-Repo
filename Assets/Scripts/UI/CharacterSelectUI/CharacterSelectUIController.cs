using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectUIController : MonoBehaviour
{
    [SerializeField] private MoveListPageUI moveList;
    [SerializeField] private MoveListPageUI actionMoveList;
    [SerializeField] private CharacterSelectPageUI moveLists;
    [SerializeField] private List<MoveListSO> moveListsData;
    [SerializeField] private List<MoveList> moveListsData2;
    [SerializeField] private InventorySO abilityList;
    [SerializeField] private RectTransform container;
    [SerializeField] private MoveListPageUI inventoryUI;
    [SerializeField] private bool isOpen;
    [SerializeField] private int inventorySize;
    [SerializeField] private MoveListSO MoveListData, emptyMoveData;
    [SerializeField] private AudioClip dropCLip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MouseFollower mouseFollower;
    private Dictionary<int, InventoryItem> abilityDic;
    private int curMoveListCount;
    private int maxMoveListIndex;
    private Unit curUnit;
    public event Action<int, MoveSO> onMagicAttack;
    public List<Move> initialItems = new List<Move>();

    public void Start()
    {
        /*IntalizeMoveListPages();
        // prepares UI
        PrepareUI();

        // creates inventory  
        PrepareInventory();
        ResetMoveList();*/

    }
}

/*---------------------------------------------------------------------
 *  Method: ResetMoveList()
 *
 *  Purpose: Clears the move list pages and then fills them based on 
 *           the data from the curUnit's ability list
 *
 *  Parameters: none
 *
 *  Returns: none
 *-------------------------------------------------------------------

public void ResetMoveList()
{
    if (abilityList == null)
    {
        return;
    }
    // initlaizes the move lsit pages and thier data
    for (int i = 0; i < maxMoveListIndex; i++)
    {
        CharacterSelectPageUI curMoveList = moveLists[i];
        if (curMoveList != null)
        {
            if (curMoveList.MoveData != null)
            {
                curMoveList.MoveData.Initialize();
            }

            curMoveList.ResetAllItems();
        }
    }
    // refilling move lsit pages with data from curUnit
    abilityDic = abilityList.GetCurrentInventoryState();
    if (abilityDic != null)
    {
        for (int i = 0; i < abilityDic.Count; i++)
        {
            InventoryItem curAbility = InventoryItem.GetEmptyItem();
            if (abilityDic.TryGetValue(i, out curAbility))
            {
                curAbility = abilityDic[i];
            }
            if (curAbility.IsEmpty())
            {
                continue;
            }
            CharacterSelectPageUI curMoveList = moveLists[i];


        }
    }
}

public void IntalizeMoveListPages()
{
    curMoveListCount = 0;
    maxMoveListIndex = 3;
    if (abilityList != null)
    {
        //  abilityList.Initialize();
    }
    moveLists = new List<MoveListPageUI>();
    for (int i = 0; i < maxMoveListIndex; i++)
    {
        MoveList curMoveListData = new MoveList(maxMoveListIndex);
        MoveListPageUI curMoveList = Instantiate(moveList);
        curMoveList.MouseFollower = mouseFollower;
        curMoveList.transform.SetParent(container);
        curMoveList.Hide();
        curMoveList.MouseFollower.Toggle(false);
        curMoveList.gameObject.transform.localScale = Vector3.one;
        curMoveList.MoveData = curMoveListData;
        moveLists.Add(curMoveList);
        curMoveListData.Initialize();
    }
}
public void Activate(bool isActive)
{


    if (isActive)
    {
        //Debug.Log("activating !!");
        foreach (CharacterSelectPageUI curMoveList in moveLists)
        {
            curMoveList.Show();
            //updates UI to match data
            MoveList curMoveListData = curMoveList.MoveData;
            foreach (var item in curMoveListData.GetCurrentInventoryState())
            {
                curMoveList.UpdateData(item.Key,
                item.Value.item.GetImage(),
                item.Value.count);
            }
        }

        isOpen = true;

        return;
    }
    else
    {
        // Debug.Log("deactivating");
        foreach (CharacterSelectPageUI moveList in moveLists)
        {
            if (moveList == null) continue;
            moveList.Hide();
        }
        isOpen = false;
    }

}
public void PrepareInventory()
{

    // inventoryData.Initialize();
    abilityList.onInventoryUpdated += UpdateInventoryUI2;
    foreach (MoveListSO moveListData in moveListsData)
    {

        // moveListData.Initialize();
        moveListData.onInventoryUpdated += UpdateInventoryUI;
        foreach (Move item in initialItems)
        {
            if (item.IsEmpty())
            {
                continue;
            }
            moveListData.AddItem(item);
        }
    }
}

---------------------------------------------------------------------
|  Method UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate)
|
|  Purpose: Updates the UI when data is changed in the model
|
|   Parameters: Dictionary<int, InventoryItem> inventorySate dict that 
|               represents the current data in the inventory model
|
|  Returns: none
*-------------------------------------------------------------------
private void UpdateInventoryUI(Dictionary<int, Move> inventorySate, MoveListSO curMoveListData)
{

    foreach (MoveListPageUI moveList in moveLists)
    {

        if (moveList.MoveData.Equals(curMoveListData))
        {
            moveList.ResetAllItems();
            // updates UI with current inventory data
            foreach (var item in inventorySate)
            {
                //  Debug.Log("---ww "+item.Value.count);
                moveList.UpdateData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count);
            }
        }



    }
}
private void UpdateInventoryUI2(Dictionary<int, InventoryItem> inventorySate)
{
    //  Debug.Log("updating moves");
    if (curMoveListCount >= maxMoveListIndex)
    {
        return;
    }
    ResetMoveList();
}

/---------------------------------------------------------------------
|  Method PrepareUI()
|
|  Purpose: Sets up UI toaccept draggs,swaps, and selecting items
|
|   Parameters: none
|
|  Returns: none
*-------------------------------------------------------------------
public void PrepareUI()
{

    foreach (MoveListPageUI moveList in moveLists)
    {

        moveList.CreateInventoryUI();
        moveList.onDescriptionRequest += HandleDescriptionRequest;
        moveList.onItemActionRequest += HandleItemActionRequest;
        moveList.onTransferItems += HandleTransferItemsl;
        moveList.onDescriptionClear += HandleDescriptionClear;
        moveList.onItemSelect += HandleItemSelect;


    }

}

private void HandleItemSelect(int curMoveIndex, MoveListPageUI moveList)
{
    //Debug.Log("clciked item " + uI.itemList[arg1].name);
    Move curMoveUI = moveList.MoveData.GetItemAt(curMoveIndex);
    // avoids empty slots
    if (curMoveUI.IsEmpty())
    {
        moveList.ResetDescription();
        return;
    }
    // updates description page
    MoveSO curItem = curMoveUI.item;
    if (onMagicAttack != null)
    {
        onMagicAttack.Invoke(curMoveIndex, curItem);
    }
    moveList.ResetDescription();
}

private void HandleDescriptionClear(int arg1, MoveListPageUI moveList)
{
    moveList.ResetDescription();
}

private void HandleTransferItemsl(MoveUI item1UI_1, MoveUI itemUI_2, int itemIndex1, int itemIndex2)
{
    //Debug.Log(" -**- switching invenoty");
    MoveListSO inventory1 = item1UI_1.GetParentInventory();
    MoveListSO inventory2 = itemUI_2.GetParentInventory();
    MoveListSO temp = inventory1;

}

private void HandleItemActionRequest(int curItemIndex)
{
}

/*---------------------------------------------------------------------
|  Method HandleSwapItems(int curitemIndex, int itemIndexSwap)
|
|  Purpose: Handles when showing description when clicking on item
|
|   Parameters: int curitemIndex = index of item being selected
|
|  Returns: none
*-------------------------------------------------------------------
private void HandleDescriptionRequest(int curMoveIndex, MoveListPageUI moveList)
{
    Move curMoveUI = moveList.MoveData.GetItemAt(curMoveIndex);
    // avoids empty slots
    if (curMoveUI.IsEmpty())
    {
        moveList.ResetDescription();
        return;
    }
    // updates description page
    MoveSO curItem = curMoveUI.item;
    moveList.UpdateDescription(curMoveIndex, curItem.GetImage(),
        curItem.GetName(), curItem.description);

}

public bool IsOpen()
{
    return isOpen;
}

public List<MoveListPageUI> MoveLists
{
    get
    {
        return moveLists;
    }
    set { moveLists = value; }
}
public List<MoveListSO> MoveListsData
{
    get
    {
        return moveListsData;
    }
    set
    {
        moveListsData = value;
    }
}
public InventorySO AbilityList
{
    get { return abilityList; }
    set { abilityList = value; }
}

public Unit CurUnit { get { return curUnit; } set { curUnit = value; } }
}
*/