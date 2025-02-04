/*---------------------------------------------------------------------
*  Class: GridCombatSystem
*
*  Purpose: Control the flow of combat encounters. It does this by waiting for 
*           a combat encounter to be triggered. Once triggred the class will 
*           initate the combat system, which clears and rests all the revelant data 
*           and fields. Then the Class starts the encounter which selects the first 
*           unit in rotation and then waits for commands for that unit. 
*           Once a command is issued the class completes the command and ends the turn. 
*           Once the turn is over the next unit is selected and the class waits for thier command.
*           This process repeats until the encounter is over
*-------------------------------------------------------------------*/
using Inventory;
using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GridCombatSystem : MonoBehaviour
{
    private Vector3 target;

    [SerializeField] private Unit curUnit;
    [SerializeField] private CombatGrid grid;
    [SerializeField] private bool combatTriggered, forceSkip, combatEnded, comabtMove = false, isCompletingMove, teamUpSubmitted;
    [SerializeField] private List<Unit> units;
    private List<Unit> combatOrder;
    private HashSet<Unit> availableUnits;
    private Dictionary<Unit, int> combatOrderRef;
    [SerializeField] private Vector3 unitOffset;
    [SerializeField] private CameraDrag worldCamera;
    [SerializeField] private PlayerController player;
    [SerializeField] private CombatUIManager combatUI;
    [SerializeField] private InventoryController inventoryUI;
    [SerializeField] private Animator combatIconAnimator;
    [SerializeField] DialougeManager dialougeManager;
    [SerializeField] private Color enemyColor;
    [SerializeField] private Color frienldyColor;
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private float camerMoveDelay, moveToSpeed;
    [Header("Combat data")]
    [SerializeField] private int runDistance;
    [SerializeField]
    private bool attackComplete, combatStarted, moveComplete,
        isMakingHitableMove, isCurUnitBack, isMakingMove;
    [SerializeField]
    private int movesCompleted, movementsCompleted,
        attacksCompelted, magicAttacksCompleted;
    [SerializeField] private float teamUpAttackVal;
    [SerializeField] private SerializableDictionary<int, float> teamUpAttackMeter;
    [SerializeField] private State state;
    [SerializeField] private TextAsset defaultInkText;
    [SerializeField] private int cursorX, cursorY;
    private float camToUnitOffset;
    private HashSet<Node> validMovments;
    private List<Unit> blueTeamList, redTeamList;
    private int bluePoints, redPoints;
    private int redInitalCount, blueInitalCount;
    private bool isDebugging, isTalking, isHitChance, isTurnOver, makingMove;
    private List<Node> possibleValidNodes;
    private Node targetNode;
    private int curTurn;
    private Node lastVisitedNode;
    private Unit targetUnit, tempTargetUnit;
    private Node curSelectedForPath, moveingToNode;
    private Action onCompleteAfter;
    private bool menu = false, move = false;
    private int backCount;
    [SerializeField] private CaptureItemSO curCaptureItem;
    private PathRequestManager pathRequestManager;
    private HashSet<Node> curPossibleValidNodes = new HashSet<Node>();
    private Vector2 lastMousePos = Vector2.zero;
    private Vector2 curMousePos = Vector2.zero;

    private enum State
    {
        Normal,
        Waiting
    }
    [SerializeField] Command curCommand;


    public enum Command
    {
        Waiting,
        Move,
        Attack,
        Item,
        Skip,
        MagicAttack,
        Push,
        Capture,
        Special,
        TeamUp
    }
    // Start is called before the first frame update
    void Awake()
    {
        combatUI.onMagicSelect += HandleMagicSelected;
        combatUI.onMoveButtonSelect += HandleMoveButtonSelected;
        combatUI.onActionButtonSelect += HandleActionButtonSelected;
        combatUI.onWaitButtonSelect += HandleWaitButtonSelected;
        combatUI.onAttackSelect += HandleAttackSelected;
        combatUI.onPushButtonSelect += HandlePushButtonSelected;
        combatUI.onRunButtonSelect += HandleRunButtonSelected;
        combatUI.onItemButtonSelect += HandleItemButtonSelected;
        combatUI.onCaptureItemSelected += HandleCaptureItemSelected;
        combatUI.onTeamUpSelected += HandleTeamUpButtonSelected;
        combatUI.onSubmitTeamUpSelected += HandleSubmitTeamUpButtonSelected;
        dialougeManager.onSwitchTeamActivated += HandleTeamSwitch;
        dialougeManager.onIntimidated += HandleIntimidated;
        dialougeManager.onCharmed += HandleCharmed;
        dialougeManager.onTricked += HandleTricked;
        camToUnitOffset = 2f;
        pathRequestManager = GetComponent<PathRequestManager>();


    }
    public bool MakeRoll(float hitChnce, int max)
    {
        int moveRoll = UnityEngine.Random.Range(1, max);
        if (moveRoll <= hitChnce)
        {
            return true;
        }
        return false;
    }

    private void HandleRunButtonSelected(CombatUIManager manager)
    {
        curUnit.AttemptRun(redTeamList, runDistance, (int movesMade) =>
        {
            CompleteTurn(movesMade);
            ClearUnitCombtUI();
            attackComplete = true;
            state = State.Normal;
        }, (int movesMade) =>
        {
            HandleUnitDeath(curUnit, () =>
            {
                HandleWaitButtonSelected(manager);
            });
        });
    }

    private void HandleTricked()
    {
        if (targetUnit.Stats == null)
        {
            return;
        }
        curUnit.CaptureUnit(targetUnit, targetUnit.Stats.TrickRate, onUnitSwitchTeam, onUnitNotConvinced);
    }

    private void HandleCharmed()
    {
        if (targetUnit.Stats == null)
        {
            return;
        }
        curUnit.CaptureUnit(targetUnit, targetUnit.Stats.CharmRate, onUnitSwitchTeam, onUnitNotConvinced);
    }

    private void HandleIntimidated()
    {
        if (targetUnit.Stats == null)
        {
            return;
        }
        curUnit.CaptureUnit(targetUnit, targetUnit.Stats.IntimidateRate, onUnitSwitchTeam, onUnitNotConvinced);
    }

    // Update is called once per frame
    void Update()
    {
        if (combatEnded)
        {
            EndCombatEncounter();
        }
        if (combatTriggered && !combatStarted)
        {
            InitiateCombatSystem();
            StartCombatEncounter();
            //worldCamera.CurUnit = curUnit;
        }
        else if (combatStarted && combatEnded == false)
        {

            lastMousePos = curMousePos;
            curMousePos.x = Input.GetAxis("Mouse X");
            curMousePos.y = Input.GetAxis("Mouse Y");
            moveInput = playerInputManager.MoveNormalized;
            RunCombatEncounter();
        }



    }

    private void SelectNode(InputAction.CallbackContext context)
    {

        Vector2 moveInput = context.ReadValue<Vector2>();
        if (curSelectedForPath != null)
        {
            Debug.Log("--" + moveInput + "  " + curSelectedForPath.GridX + "," + curSelectedForPath.GridY);
            curSelectedForPath = grid.GetNodeAt((int)(curSelectedForPath.GridX + moveInput.x),
           (int)(curSelectedForPath.GridY + moveInput.y));
        }
    }



    public void MoveCursorToCenter()
    {
        Vector2 pos = new Vector2(Screen.width / 2, (Screen.height / 2) + 10);
        Mouse.current.WarpCursorPosition(pos);
        worldCamera.NodeToFollow = curUnit.CurNode;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleTeamSwitch()
     *
     *  Purpose: switches the team of the targetUnit to match curUnit and then refreshes combatUI to match
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleTeamSwitch()
    {
        Debug.Log("team Swtich!!");
        if (targetUnit == null || curUnit == null)
        {
            return;
        }
        int oppositeTeam = 0;
        // removes targetUnit from cur team
        if (targetUnit.Team == 0)
        {
            blueTeamList.Remove(targetUnit);
            oppositeTeam = 1;
        }
        else if (targetUnit.Team == 1)
        {
            redTeamList.Remove(targetUnit);
            oppositeTeam = 0;
        }
        // gives targetUnit new team
        int curTeam = curUnit.Team;
        targetUnit.Team = curTeam;
        // adds target to new team
        if (targetUnit.Team == 0)
        {
            blueTeamList.Add(targetUnit);
        }
        else if (targetUnit.Team == 1)
        {
            redTeamList.Add(targetUnit);
        }
        combatUI.AssignTeams();
        if (blueTeamList.Count == combatOrder.Count || redTeamList.Count == combatOrder.Count)
        {
            EndCombatEncounter();
        }


    }
    /*---------------------------------------------------------------------
     *  Method: HandleTeamSwitch()
     *
     *  Purpose: switches the team of the targetUnit to match curUnit and then refreshes combatUI to match
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void SwitchTeam(Unit curUnitToSwitch)
    {
        Debug.Log("team Swtich!!");
        if (curUnitToSwitch == null || curUnit == null)
        {
            return;
        }
        int oppositeTeam = 0;
        // removes targetUnit from cur team
        if (curUnitToSwitch.Team == 0)
        {
            blueTeamList.Remove(curUnitToSwitch);
            oppositeTeam = 1;
        }
        else if (curUnitToSwitch.Team == 1)
        {
            redTeamList.Remove(curUnitToSwitch);
            oppositeTeam = 0;
        }
        // gives targetUnit new team
        curUnitToSwitch.Team = oppositeTeam;
        // adds target to new team
        if (curUnitToSwitch.Team == 0)
        {
            blueTeamList.Add(curUnitToSwitch);
        }
        else if (curUnitToSwitch.Team == 1)
        {
            redTeamList.Add(curUnitToSwitch);
        }
        combatUI.AssignTeams();
        if (blueTeamList.Count == combatOrder.Count || redTeamList.Count == combatOrder.Count)
        {
            EndCombatEncounter();
        }


    }

    /*---------------------------------------------------------------------
     *  Method: HandleCaptureItemSelected(CombatUIManager manager, ItemSO curCaptureItem)
     *
     *  Purpose: Handles waht happens to the combat when capture item is used.
     *           It tells the combat system to reset for next move and shows 
     *           nodes that are within range and makes isMakingHitableMove so 
     *           units will show hitChance when hovered over
     *
     *  Parameters: mnone
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleCaptureItemSelected(CombatUIManager manager, ItemSO curCaptureItem)
    {
        //  throw new NotImplementedException();
        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();
        CaptureItemSO possibleCaptureItem = curCaptureItem as CaptureItemSO;
        if (possibleCaptureItem != null)
        {
            this.curCaptureItem = possibleCaptureItem;
        }
        int attackRange = curUnit.Stats.Strength;
        InitateMove(Command.Capture, attackRange);
        isMakingHitableMove = true;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleItemButtonSelected(CombatUIManager manager)
     *
     *  Purpose: Handles waht happens to the combat when item is used.
     *           It tells the combat system to reset for next move 
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleItemButtonSelected(CombatUIManager manager)
    {
        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();
    }

    /*---------------------------------------------------------------------
     *  Method: HandleTalkButtonSelected(CombatUIManager manager)
     *  
     *  Purpose: Handles waht happens to the combat when talk is slectec.
     *           It tells the combat system to set Command to talk and 
     *           shows valid noed that are in range and makes isMakingHitableMove so 
     *           units will show hitChance when hovered over
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandlePushButtonSelected(CombatUIManager manager)
    {
        ResetUnitForNextCommand();
        curCommand = Command.Push;
        //  isTalking = true;
        InitateMove(Command.Push, 1);
        isMakingHitableMove = true;
    }
    private void HandleTeamUpButtonSelected(CombatUIManager manager)
    {
        ResetUnitForNextCommand();
        curCommand = Command.TeamUp;
        //  isTalking = true;
        InitateMove(Command.TeamUp, curUnit.Stats.Speed);
        isMakingHitableMove = true;
        teamUpSubmitted = false;
    }

    private void HandleSubmitTeamUpButtonSelected(CombatUIManager manager)
    {
        teamUpSubmitted = true;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleWaitButtonSelected(CombatUIManager manager
     *  
     *  Purpose: Handles waht happens to the combat when wait is selected.
     *           It tells the combat system to skip turn
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleWaitButtonSelected(CombatUIManager manager)
    {
        if (curUnit == null || manager == null)
        {
            return;
        }
        forceSkip = true;
        ResetUnitForNextCommand();
        curCommand = Command.Waiting;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleActionButtonSelected(CombatUIManager manager)
     *  
     *  Purpose: Handles waht happens to the combat when action menu is selected.
     *           It tells the combat system to reset for next move and  
     *           waits while the player selects action
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleActionButtonSelected(CombatUIManager manager)
    {
        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();

        curCommand = Command.Waiting;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleMagicSelected(CombatUIManager manager)
     *  
     *  Purpose: Handles waht happens to the combat when magic is selected.
     *           It tells the combat system to reset for next move and shows 
     *           nodes that are within range and makes isMakingHitableMove so 
     *           units will show hitChance when hovered over
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleMagicSelected(CombatUIManager manager)
    {

        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();
        if (curUnit.CurMove != null)
        {
            if (curUnit.CurMove.Type == MoveSO.RangeType.Connect)
            {
                combatUI.InializeMultiSelectCount(curUnit.CurMove.SpreadRange);
                combatUI.ActivateMultiSelectCounter(true);
            }
            InitateMove(Command.MagicAttack, curUnit.CurMove.Range);
            isMakingHitableMove = true;
        }


    }
    /*---------------------------------------------------------------------
     *  Method: HandleSpecialSelected(CombatUIManager manager)
     *  
     *  Purpose: Handles waht happens to the combat when special move is selected.
     *           It sets the curUnits cuMove to its special move
     *           It tells the combat system to reset for next move and shows 
     *           nodes that are within range and makes isMakingHitableMove so 
     *           units will show hitChance when hovered over
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleSpeacilSelected(CombatUIManager manager)
    {

        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();
        curUnit.CurMove = curUnit.SpecialMove;
        if (curUnit.CurMove != null)
        {
            Debug.Log("special Move selected");
            InitateMove(Command.Special, curUnit.CurMove.Range);
            isMakingHitableMove = true;
        }


    }
    /*---------------------------------------------------------------------
     *  Method: HandleAttackSelected(CombatUIManager manager)
     *  
     *  Purpose: Handles waht happens to the combat when magic is selected.
     *           It tells the combat system to reset for next move and shows 
     *           nodes that are within range and makes isMakingHitableMove so 
     *           units will show hitChance when hovered over
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleAttackSelected(CombatUIManager manager)
    {

        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();
        // if unarmed
        int attackRange = 1;
        // finds weapons range if weapon is equipped
        if (curUnit.Equipment != null)
        {
            WeaponItemSO curWeapon = curUnit.Equipment.Weapon;
            if (curWeapon != null)
            {
                attackRange = curWeapon.Range;
            }
        }
        InitateMove(Command.Attack, attackRange);
        isMakingHitableMove = true;
    }

    /*---------------------------------------------------------------------
     *  Method: HandleMoveButtonSelected(CombatUIManager manager)
     *  
     *  Purpose: Handles waht happens to the combat when magic is selected.
     *           It tells the combat system to reset for next move and shows 
     *           nodes that are within range 
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void HandleMoveButtonSelected(CombatUIManager manager)
    {

        if (curUnit == null || manager == null)
        {
            return;
        }
        ResetUnitForNextCommand();
        InitateMove(Command.Move, curUnit.Stats.Speed);
    }
    public IEnumerator WaitForGrid()
    {
        state = State.Waiting;
        yield return new WaitForSeconds(0.25f);
        state = State.Normal;
    }
    /*---------------------------------------------------------------------
     *  Method: InitateMove(Command commandToChangeTo,int range)
     *  
     *  Purpose: shows nodes withing range of cur action and 
     *           sets curCommand for the current action 
     *
     *  Parameters: CombatUIManager manager = combatUI manager
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void InitateMove(Command commandToChangeTo, int range)
    {
        if (curUnit != null)
        {
            curUnit.SelectedTargets.Clear();
        }
        StartCoroutine("WaitForGrid");
        MoveCursorToCenter();
        GetValidNodes(range, commandToChangeTo);
        isMakingMove = true;
        curCommand = commandToChangeTo;



    }

    /*---------------------------------------------------------------------
     *  Method: EndCombatEncounter()
     *
     *  Purpose: Ends the combat endocunter by reseting the vals, the teams
     *           and making the curUnit and curNode null
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public Action EndCombatEncounter()
    {
        combatUI.Deactivate();
        ResetTeams(2);
        ResetCombatSystem();
        foreach (Unit curUnitInital in units)
        {
            curUnitInital.InCombat = false;
            Node curInitalNode = grid.NodeFromWorldPoint(curUnitInital.transform.position);
            curInitalNode.Unit = null;
            curInitalNode.Walkable = true;
            ActivateUnitCollider(curUnitInital, true);
            if (curUnitInital.UnitCombatUI != null)
            {
                curUnitInital.UnitCombatUI.ActivateBase(false);
            }
            curUnitInital.ActivateHealthBar(false);
            curUnitInital.ActivateManaBar(false);
            curUnitInital.GridCombatSystem = null;

        }
        combatUI.AssignUnit(player.Unit);
        combatUI.ResetUnitUIs();
        inventoryUI.AssignUnit(player.Unit);
        player.InCombat = false;
        combatStarted = false;
        combatEnded = false;
        Debug.Log("ended");
        // state = State.Waiting;
        return null;
    }

    /*---------------------------------------------------------------------
     *  Method: AssignTeam(Unit unit)
     *
     *  Purpose: assigns units to thier respective teams as well as set up 
     *           and show the team speciic UI like health/mana bars and the
     *           units combat base
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AssignTeam(Unit unit)
    {
        if (unit == null)
        {
            return;
        }
        if (unit.Stats == null)
        {
            return;
        }
        unit.ActivateHealthBar(true);
        // assigns units to thier teams
        if (unit.Team == 0)
        {
            blueTeamList.Add(unit);
            unit.Stats.HealthBar.Fill.color = Color.green;
            unit.ActivateManaBar(true);
        }
        else if (unit.Team == 1)
        {
            redTeamList.Add(unit);
            unit.Stats.HealthBar.Fill.color = Color.red;
        }
        unit.Stats.SetHealth();
        unit.Stats.SetMana();
        if (unit.UnitCombatUI != null)
        {
            unit.UnitCombatUI.ActivateBase(true);
            AssignUnitBaseColors(unit);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: MoveUnitToNode(Unit unit,Node nodeToMoveTo)
     *
     *  Purpose: Instanly move a unit to a mode without playing any animaiton
     *
     *  Parameters: Unit unit = unit to move 
     *              Node nodeToMoveTo = node to move unit to 
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void MoveUnitToNode(Unit unit, Node nodeToMoveTo)
    {
        // moves each unit to the nearest node
        if (unit == null || nodeToMoveTo == null)
        {
            return;
        }
        unit.transform.position = Vector3.MoveTowards(unit.transform.position, nodeToMoveTo.WorldPos + unit.NodeOffset, 200 * Time.deltaTime);
        if (unit.CurNode != null)
        {
            unit.CurNode.Walkable = true;
            unit.CurNode.Unit = null;
        }
    }

    /*---------------------------------------------------------------------
     *  Method: AssignUnitNode(Unit unit,Node node)
     *
     *  Purpose: Assigns a unit to a node and vise versa
     *
     *  Parameters: Unit unit = unit to assign 
     *              Node node = node to assign to
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AssignUnitNode(Unit unit, Node node)
    {
        node.Unit = unit;
        node.Walkable = false;
        unit.CurNode = node;
    }

    /*---------------------------------------------------------------------
     *  Method: ActivateUnitCollider(Unit unit,bool isActive) 
     *
     *  Purpose: Activates or deactivtes a units colliders
     *
     *  Parameters: Unit unit = unit modify collider 
     *              bool isActive = if or not it will be active
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ActivateUnitCollider(Unit unit, bool isActive)
    {
        // turns off each units collider 
        Collider2D curCollider = unit.GetComponent<Collider2D>();
        if (curCollider != null)
        {
            curCollider.enabled = isActive;
        }
    }

    /*---------------------------------------------------------------------
     *  Method: InitiateCombatSystem()
     *
     *  Purpose: Initalizes the combat system by reseting all the values,
     *           reseting the teams and markinng the nodes the units are currently on
     *
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void InitiateCombatSystem()
    {
        combatEnded = false;
        ResetTeams(2);
        Collider2D curCollider = null;
        combatOrderRef = new Dictionary<Unit, int>();
        combatOrder = new List<Unit>();
        availableUnits = new HashSet<Unit>();
        curPossibleValidNodes = new HashSet<Node>();
        //  possibleValidNodes = new List<Node>();
        int initativeRoll = 0;
        // sets up each unit for combat
        foreach (Unit curUnitInital in units)
        {
            curUnitInital.GridCombatSystem = this;
            // gets the node closet to each unit
            Node curInitalNode = null;
            if (curUnitInital.CombatBase != null)
            {
                curInitalNode = grid.NodeFromWorldPoint(curUnitInital.CombatBase.transform.position);
            }
            else
            {
                curInitalNode = grid.NodeFromWorldPoint(curUnitInital.transform.position);
            }
            MoveUnitToNode(curUnitInital, curInitalNode);
            // assigns teams and sets up unitUI
            AssignTeam(curUnitInital);
            // sets up node each unit is on
            AssignUnitNode(curUnitInital, curInitalNode);
            // sets up each unit for grid movent/combat
            curUnitInital.Grid = grid;
            curUnitInital.PathRequestManager = pathRequestManager;
            curUnitInital.InCombat = true;
            // turns off each units collider 
            ActivateUnitCollider(curUnitInital, false);
            // gets each units combat order
            initativeRoll = new System.Random().Next(1, 30);
            combatOrderRef.TryAdd(curUnitInital, initativeRoll + curUnitInital.Stats.Speed);
            combatOrder.Add(curUnitInital);
            availableUnits.Add(curUnitInital);
            teamUpAttackMeter.AddCheck(curUnitInital.Team, 0f);

        }
        HeapSortCombat();
        blueInitalCount = blueTeamList.Count;
        redInitalCount = redTeamList.Count;
        ResetCombatSystem();
        // sets up UI and camera for combat
        curTurn = -1;
        curUnit = player.Unit;
        InitateCombatUI();
        inventoryUI.AssignUnit(curUnit);
        worldCamera.ActivateForCombat(curUnit);
        SelectNextCurUnit();
    }

    /*---------------------------------------------------------------------
     *  Method: AssignUnitBaseColors(Unit unit)
     *
     *  Purpose: assigns a units base color based on thier team
     *
     * Parameters: unit = unit to assign base color
     *
     * Returns: none
     *-------------------------------------------------------------------*/
    public void AssignUnitBaseColors(Unit unit)
    {
        if (unit == null)
        {
            return;
        }
        if (unit.UnitCombatUI == null)
        {
            return;
        }
        if (unit.Team == 0)
        {
            unit.UnitCombatUI.ChangeBaseColor(frienldyColor);
        }
        else
        {
            unit.UnitCombatUI.ChangeBaseColor(enemyColor);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: InitateCombatUI()
     *
     *  Purpose: Initalizes the combatUI 
     *  
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void InitateCombatUI()
    {
        combatUI.Units = combatOrder;
        combatUI.InitiateUnitsForCombat(combatOrder);
        combatUI.AssignUnit(curUnit);
        combatUI.PrepareCombatList();
        combatUI.AssignTeams();
    }

    /*---------------------------------------------------------------------
     *  Method: HeapSortCombat() 
     *
     *  Purpose: sorts the units based on speed/ initaive roll
     *  
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void HeapSortCombat()
    {
        int count = combatOrder.Count;
        // builds heap
        for (int i = (count / 2) - 1; i >= 0; i--)
        {
            heapifyCombatOrder(count, i);
        }
        for (int i = count - 1; i > 0; i--)
        {
            // moves largest to the end to sort in reverse
            Unit temp = combatOrder[0];
            combatOrder[0] = combatOrder[i];
            combatOrder[i] = temp;
            // heapify unsorted units
            heapifyCombatOrder(i, 0);
        }
    }
    private void heapifyCombatOrder(int count, int root)
    {
        if (root > count || root < 0)
        {
            return;
        }
        int child1 = 2 * root;
        int child2 = (2 * root) + 1;
        int child1Val = 0;
        int child2Val = 0;
        int largestChild = root;
        int rootVal = combatOrderRef[combatOrder[root]];
        int largestChildVal = rootVal;
        // gets the values of i's children and saves the largest one
        if ((child1 < count && child1 > 0))
        {
            child1Val = combatOrderRef[combatOrder[child1]];
            if (child1Val < largestChildVal)
            {
                largestChild = child1;
                largestChildVal = child1Val;
            }
        }
        if ((child2 < count && child2 > 0))
        {
            child2Val = combatOrderRef[combatOrder[child2]];
            if (child2Val < largestChildVal)
            {
                largestChild = child2;
                largestChildVal = child2Val;
            }
        }

        // swaps root with children if needed
        if (largestChild != root)
        {
            Unit tempSwap = combatOrder[root];
            combatOrder[root] = combatOrder[largestChild];
            combatOrder[largestChild] = tempSwap;
            heapifyCombatOrder(count, largestChild);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: RunCombatEncounter()
     *
     *  Purpose: Runs the combat encounter by looking for commands for the current unit,
     *           Then completing thesse commands, before ending the turn and swithcing to
     *           the next valid unit if the command is finished
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void RunCombatEncounter()
    {
        switch (state)
        {
            // if player is not in combar or waiting for command
            case State.Normal:


                // if done talking 
                if (isTalking && player.DialougeManager.IsDoneTalking)
                {
                    isTalking = false;
                    player.DialougeManager.IsDoneTalking = false;
                    ResetUnitForNextCommand();
                }
                // complests actions of units if inputed
                if (player.CurState == player.CombatState)
                {

                    CompleteUnitAction();
                }
                // if move is completed
                if (curUnit != null)
                {
                    if (curUnit.ReachedDest && moveComplete == true)
                    {
                        ResetUnitForNextCommand();
                    }
                }
                // if attack completed
                if (attackComplete)
                {
                    Debug.Log("--- attack complete");
                    ResetUnitForNextCommand();
                }
                // is curUnit's turn is over
                if (movesCompleted >= 2 || forceSkip)
                {

                    forceSkip = false;
                    EndUnitTurn();
                }
                // if turn is over and camera is moving to next unit
                if (isTurnOver)
                {
                    float camDistanceFromUnit = MathF.Abs(worldCamera.transform.position.sqrMagnitude -
                        (curUnit.CurNode.WorldPos + worldCamera.Offset).sqrMagnitude);
                    // if camera has moved to next unit and turn switch is complete
                    if (camDistanceFromUnit < camToUnitOffset)
                    {
                        ResetCobatUI();
                        //InitateMove(Command.Move, curUnit.Stats.MaxMoveDistance);

                        isTurnOver = false;
                    }
                }
                break;
            // if player is moving or in the middle of an action
            case State.Waiting:
                break;
        }
        return;
    }

    /*---------------------------------------------------------------------
     *  Method: EndUnitTurn()
     *
     *  Purpose: Ends the turn of a unit by first clearing the valid movment nodes for the unit,
     *           Then by chanign the postion of the unit on the physical grid(phyGrid), Then
     *           selecting the next valid unit, and finall getting the valid movment nodes 
     *           for the next unit. The code then tells the combat system to wait 
     *           for the action to complete
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void EndUnitTurn()
    {
        ResetUnitForNextCommand();
        combatUI.ResetTurns();
        movesCompleted = 0;
        movementsCompleted = 0;
        attacksCompelted = 0;
        magicAttacksCompleted = 0;
        // clears for valid nodes from last unit
        if (curUnit.CurNode != null)
        {
            curUnit.CurNode.Walkable = false;
        }
        else
        {
            grid.NodeFromWorldPoint(curUnit.transform.position).Walkable = false;
        }
        if (teamUpAttackMeter.ContainsKey(curUnit.Team))
        {
            teamUpAttackMeter[curUnit.Team] += 2;
        }
        curUnit.ClearLastPath();
        SelectNextCurUnit();
        isTurnOver = true;
    }

    /*---------------------------------------------------------------------
     *  Method: ClearUnitCombtUI()
     *
     *  Purpose: Stops the hitChance animaiton for all units
     *  
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ClearUnitCombtUI()
    {
        foreach (Unit unit in units)
        {
            unit.CombatIconAnimator.SetBool("isHitChance", false);
        }
        isHitChance = false;

    }

    /*---------------------------------------------------------------------
     *  Method: ClearUnitCombtUI()
     *
     *  Purpose: Resets game board for next move
     *  
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ResetUnitForNextCommand()
    {
        ClearLastSelectedNodesForAttack();
        worldCamera.NodeToFollow = null;
        curUnit.ReachedDest = false;
        attackComplete = false;
        moveComplete = false;
        curUnit.ClearLastPath();
        curUnit.ClearLastSpot();
        foreach (Unit unit in combatOrder)
        {
            grid.ClearNode(unit.transform.position);
        }
        curUnit.CurNode.Walkable = true;
        isMakingHitableMove = false;
        isMakingMove = false;
        ClearUnitCombtUI();
        //  curSelectedForPath = null;
        isTurnOver = false;
        ResetCobatUI();
        backCount = 0;
        ClearLastSelectedNodesForAttack();
        curSelectedForPath = curUnit.CurNode;
        teamUpSubmitted = false;
        // state = State.Normal;
    }
    public void ResetCobatUI()
    {
        combatUI.ActivateCombatMenu(true);
        combatUI.ActivateMultiSelectCounter(false);
        combatUI.ActivateSubmitTeamUp(false);

        if (curUnit != null)
        {

            if (teamUpAttackMeter.ContainsKey(curUnit.Team))
            {
                combatUI.HandeleTeamUp(teamUpAttackVal, teamUpAttackMeter[curUnit.Team]);
                if (teamUpAttackMeter[curUnit.Team] < teamUpAttackVal)
                {
                    combatUI.ActivateTeamUp(false);
                }
                else
                {
                    combatUI.ActivateTeamUp(true);
                }
            }
        }
    }
    /*---------------------------------------------------------------------
    *  Method: ResetTeams(int count) 
    *
    *  Purpose: Resets the teams of units
    *  Parameters: int count = determines how many teams are resset
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void ResetTeams(int count)
    {
        if (count == 2)
        {
            blueTeamList = new List<Unit>();
            redTeamList = new List<Unit>();
        }
        else if (count == 1)
        {
            redTeamList = new List<Unit>();
        }
    }

    /*---------------------------------------------------------------------
    *  Method: ResetCombatSystem()
    *
    *  Purpose: Resets the vlues of the comabt systsm
    *  Parameters: none
    * Returns: none
    *-------------------------------------------------------------------*/
    public void ResetCombatSystem()
    {
        state = State.Normal;
        validMovments = new HashSet<Node>();
        attackComplete = false;
        moveComplete = false;
        combatTriggered = false;
        combatStarted = false;
        worldCamera.InCombat = false;
        isHitChance = false;
        isMakingMove = false;
        makingMove = false;
        isCompletingMove = false;
        target = grid.WorldBottomLeft;
        movesCompleted = 0;
        movementsCompleted = 0;
        attacksCompelted = 0;
        magicAttacksCompleted = 0;
        bluePoints = 0;
        redPoints = 0;
        curTurn = 0;
        curUnit = null;
        foreach (Unit curUnit in units)
        {
            curUnit.ClearLastPath();
            curUnit.ClearLastSpot();

        }

    }

    /*---------------------------------------------------------------------
     *  Method: HandleBackButtonPress()
     *
     *  Purpose: Handles what happens when back button is pressed. 
     *           It figures out what 'CompleteBackPress' should do when the back button pressed.
     *           
     *            - If a move was just made and the user hasn't selected thier next move 
     *           it simply undos the last movemnt and sets up curUnit for movement.
     *           
     *           - If a move was selected it will first bring back the combat menu.
     *           
     *           - If the combat menu is back and the user presses the backbutton again 
     *           it will tell the func 'CompleteBackPress' to undo curUnit's last movement
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void HandleBackButtonPress()
    {

        // stops from checking if player isBack constantly
        if (player.IsBack != isCurUnitBack)
        {
            isCurUnitBack = player.IsBack;
            // detects 1x when player pressed back button 
            if (player.IsBack)
            {
                backCount++;
                // resets if back presed 2x
                if (backCount > 2)
                {
                    backCount = 1;
                }
                if (curCommand == Command.TeamUp)
                {
                    Debug.Log("back on team");
                    if (curUnit != null)
                    {
                        if (curUnit.SelectedTargets != null)
                        {
                            if (curUnit.SelectedTargets.Count > 1)
                            {
                                curUnit.SelectedTargets.RemoveAt(curUnit.SelectedTargets.Count - 1);
                                move = false;
                                menu = false;
                                worldCamera.NodeToFollow = null;
                                MoveCursorToCenter();
                                curSelectedForPath = null;
                                //     ShowSelectedNodes();
                                return;
                            }

                        }
                    }
                }
                // if in the middle of an action
                if (isMakingMove)
                {
                    menu = true;
                    return;
                }
                // if movemnt was made as first action
                if (movementsCompleted == 1 && movesCompleted == 1 && menu == false)
                {



                    // if in the middle of another action it first restets the menu before reversing lst move
                    if ((curCommand == Command.Attack || curCommand == Command.MagicAttack
                   || curCommand == Command.Push || curCommand == Command.Capture || curCommand == Command.Special))
                    {
                        if (!isMakingMove)
                        {
                            move = true;
                            return;
                        }
                    }

                    // if no action is being made or just finished a movemnt it reverts last movement right away 
                    else
                    {


                        move = true;
                        return;
                    }

                }

            }

        }
    }

    /*---------------------------------------------------------------------
     *  Method: CompleteBackPress()
     *
     *  Purpose: Completes the action detemrmined by 'HandleBackPress'
     *
     *           -if menu it brings back the combat menu
     *           
     *           -if moveBack it undos curUnit's last movement annd sets it up for movement
     * 
     *  Parameters: none
     * Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteBackPress()
    {

        if (menu)
        {

            ResetUnitForNextCommand();
            menu = false;

            return;
        }
        // if back after moement is made to revert movement  
        else if (move)
        {

            // undoes movement
            curUnit.transform.position = lastVisitedNode.WorldPos;
            ChangeUnitPos(curUnit, lastVisitedNode.WorldPos);
            InitateMove(Command.Move, curUnit.Stats.MaxMoveDistance);
            movementsCompleted -= 1;
            movesCompleted -= 1;
            move = false;
            // undoes turn
            combatUI.UndoTurn();
            ResetUnitForNextCommand();
            return;
        }

    }

    /*---------------------------------------------------------------------
     *  Method: CompleteUnitAction()
     *
     *  Purpose: Completes the action of a unit by first checking if the player clicked 
     *           on a location, then chekcing if a node exist at that location.
     *           If there is a node at that location. The code checks the curCommand completes the 
     *           necessary action
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteUnitAction()
    {
        HandleBackButtonPress();
        CompleteBackPress();
        // if player made a move
        if ((Input.GetMouseButtonDown(0)) && isMakingMove == true
            && curSelectedForPath != null && isCompletingMove == false)
        {
            ClearUnitCombtUI();

            switch (curCommand)
            {
                case Command.Move:
                    CompleteMoveAction(curSelectedForPath);
                    break;
                case Command.MagicAttack:
                    CompleteMagicAttackAction(curSelectedForPath);
                    break;
                case Command.Attack:
                    CompleteAttackAction(curSelectedForPath);
                    break;
                case Command.Push:
                    CompletePushAction(curSelectedForPath);
                    break;
                case Command.Capture:
                    CompleteCaptureAction(curSelectedForPath);
                    break;
                case Command.Special:
                    CompleteSpecialAction(curSelectedForPath);
                    break;
                case Command.TeamUp:
                    AddForTeamAttackAction(curSelectedForPath);
                    break;
                case Command.Waiting:
                    break;
            }
        }
        else if ((playerInputManager.SubmitAction.WasPerformedThisFrame() || teamUpSubmitted)
             && isMakingMove == true && curSelectedForPath != null && isCompletingMove == false)
        {
            CompleteTeamAttackAction(curSelectedForPath);
        }
        // if player is making move
        else if (isCompletingMove == false)
        {
            ShowSelectedNodes();
        }
    }

    /*---------------------------------------------------------------------
     *  Method: ShowSelectedNodes()
     *
     *  Purpose: Shows the nodes avaible for either a attack/item or movement 
     *           also updates the viaul of the node to either be for attack or for movement
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ShowSelectedNodes()
    {
        if (isMakingMove == true && curCommand == Command.Move && worldCamera.NodeToFollow != null)
        {
            ShowSelectedNodesHelper((Node selectedNode) =>
            {
                // shows path to selected node
                curUnit.ClearLastPath();
                curSelectedForPath = selectedNode;
                curUnit.PrintPath(curUnit.CurNode.WorldPos, selectedNode.WorldPos);
                target = selectedNode.WorldPos;
                worldCamera.NodeToFollow = selectedNode;
            });
        }
        else if (isMakingMove == true && (curCommand == Command.Attack ||
             curCommand == Command.Capture))
        {
            ShowSelectedNodesHelper((Node selectedNode) =>
            {
                ClearLastSelectedNodesForAttack();
                // selectes node mouse is over
                curSelectedForPath = selectedNode;
                curPossibleValidNodes.Add(selectedNode);
                grid.ActivateNodeForCurTurn(selectedNode.WorldPos);
                if (selectedNode.Unit != null && availableUnits.Contains(selectedNode.Unit))
                {
                    combatUI.HandleUnitPointerEnter(selectedNode.Unit.UnitCombatUI);
                }
                worldCamera.NodeToFollow = selectedNode;
            });

        }
        else if (isMakingMove == true && (curCommand == Command.MagicAttack || curCommand == Command.Special))
        {
            ShowSelectedNodesHelper((Node selectedNode) =>
            {
                ClearLastSelectedNodesForAttack();
                // selects in range nodes adn adds them to curPossibleValidNodes

                curSelectedForPath = selectedNode;
                if (selectedNode.InRange == false)
                {
                    return;
                }
                if (curUnit.CurMove.Type == MoveSO.RangeType.Default || curUnit.CurMove.Type == MoveSO.RangeType.Connect)
                {
                    curPossibleValidNodes = new HashSet<Node>();
                    curPossibleValidNodes.Add(selectedNode);
                    // grid.ActivateNodeForCurTurn(selectedNode.WorldPos);
                    PushModiferSO pushModifer = null;
                    int pushVal = 0;
                    foreach (ModifierData modifers in curUnit.CurMove.ModifierData)
                    {

                        pushModifer = modifers.statModifier as PushModiferSO;
                        if (pushModifer != null && selectedNode.Unit != null)
                        {
                            pushVal += (int)modifers.value;

                        }
                    }


                    foreach (Node node in grid.GetPushPath(curUnit.CurNode, selectedNode, pushVal))
                    {
                        curPossibleValidNodes.Add((Node)node);
                    }
                    SelectValidNodesForAttack(curPossibleValidNodes, selectedNode, curCommand);
                    if (selectedNode.Unit != null)
                    {
                        combatUI.HandleUnitPointerEnter(selectedNode.Unit.UnitCombatUI);
                    }
                }
                else if (curUnit.CurMove.Type == MoveSO.RangeType.AOE)
                {
                    grid.ActivateNodeForCurTurn(selectedNode.WorldPos);
                    curPossibleValidNodes = new HashSet<Node>();
                    curPossibleValidNodes = grid.GetNeighborsInRange(selectedNode, curUnit.CurMove.SpreadRange);
                    curPossibleValidNodes.Add(selectedNode);
                    SelectValidNodesForAttack(curPossibleValidNodes, selectedNode, curCommand);
                }
                else if (curUnit.CurMove.Type == MoveSO.RangeType.Line)
                {
                    curPossibleValidNodes = grid.GetValidLineNodesHash(curUnit.CurNode, selectedNode, curUnit.CurMove.Range);
                    SelectValidNodesForAttack(curPossibleValidNodes, selectedNode, curCommand);
                }
                else if (curUnit.CurMove.Type == MoveSO.RangeType.Spread)
                {
                    curPossibleValidNodes = new HashSet<Node>();
                    curPossibleValidNodes = grid.GetSpread(curUnit.CurNode, selectedNode, curUnit.CurMove.Range, curUnit.CurMove.SpreadRange);
                    Debug.Log(" after found " + curPossibleValidNodes.Count);
                    //curPossibleValidNodes.Add(selectedNode);
                    SelectValidNodesForAttack(curPossibleValidNodes, selectedNode, curCommand);
                }
            });

        }
        else if (isMakingMove == true && curCommand == Command.Push)
        {
            ShowSelectedNodesHelper((Node selectedNode) =>
            {
                ClearLastSelectedNodesForAttack();
                curSelectedForPath = selectedNode;
                if (selectedNode.InRange == false)
                {
                    return;
                }
                curPossibleValidNodes = new HashSet<Node>();
                curPossibleValidNodes.Add(selectedNode);
                if (curUnit.Push != null)
                {
                    foreach (Node node in grid.GetPushPath(curUnit.CurNode, selectedNode, curUnit.Push.Range))
                    {
                        curPossibleValidNodes.Add((Node)node);
                    }
                }
                SelectValidNodesForAttack(curPossibleValidNodes, selectedNode, curCommand);
                if (selectedNode.Unit != null)
                {
                    combatUI.HandleUnitPointerEnter(selectedNode.Unit.UnitCombatUI);
                }
            });
        }
        else if (isMakingMove == true && curCommand == Command.TeamUp)
        {
            ShowSelectedNodesHelper((Node selectedNode) =>
            {
                ClearLastSelectedNodesForAttack();
                curSelectedForPath = selectedNode;
                if (selectedNode.InRange == false)
                {
                    return;
                }
                curPossibleValidNodes = new HashSet<Node>();
                curPossibleValidNodes.Add(selectedNode);
                List<Unit> unitsForTeamUp = new List<Unit>();
                foreach (Unit unit in curUnit.SelectedTargets)
                {
                    unitsForTeamUp.Add(unit);
                }
                if (selectedNode.Unit != null)
                {
                    if (selectedNode.Unit.Team == curUnit.Team && !curUnit.SelectedTargets.Contains(selectedNode.Unit))
                    {

                        unitsForTeamUp.Add(selectedNode.Unit);

                    }

                }
                Unit lastUnit = curUnit;

                unitsForTeamUp.Add(curUnit);
                List<Node> vertexNodes = new List<Node>();
                List<Node> borderNodes = new List<Node>();
                foreach (Unit unit in unitsForTeamUp)
                {
                    if (unit != null)
                    {
                        if (unit.CurNode != null)
                        {
                            vertexNodes.Add(unit.CurNode);

                        }

                    }
                }

                foreach (Node node in grid.GetShapeByVertex(vertexNodes, curUnit.CurNode))
                {
                    curPossibleValidNodes.Add((Node)node);
                }

                foreach (Node node in curPossibleValidNodes)
                {
                    if (node.Unit != null && !curUnit.SelectedTargets.Contains(node.Unit) && curUnit.CurNode != node)
                    {
                        combatUI.HandleUnitPointerEnter(node.Unit.UnitCombatUI);
                    }
                }


                SelectValidNodesForAttack(curPossibleValidNodes, selectedNode, curCommand);
            });
        }

    }

    /*---------------------------------------------------------------------
     *  Method: ClearLastSelectedNodesForAttack()
     *
     *  Purpose: Clears the last selected node sand sets the
     *           visal back to be for attack
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ClearLastSelectedNodesForAttack()
    {
        if (curSelectedForPath != null)
        {
            foreach (Node node in curPossibleValidNodes)
            {

                grid.ResetNode(node.WorldPos);

            }
            // grid.ResetNode(curSelectedForPath.WorldPos);
            ClearUnitCombtUI();
            curPossibleValidNodes.Clear();
        }
    }

    /*---------------------------------------------------------------------
     *  Method: ShowSelectedNodesHelper(Action<Node> toComplete)
     *
     *  Purpose: sets up the camera and finds the node selected by the player
     * 
     *  Parameters: Action<Node> toComplete the action to complete 
     *              once the selected node is found
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ShowSelectedNodesHelper(Action<Node> toComplete)
    {
        if (moveingToNode == null)
        {
            moveingToNode = curSelectedForPath;
        }
        if (curSelectedForPath == null)
        {
            curSelectedForPath = curUnit.CurNode;
        }
        Node selectedNode = curSelectedForPath;
        if (moveInput != Vector2.zero)
        {
            Vector2 newCoord = new Vector2((int)(curSelectedForPath.GridX + moveInput.x),
            (int)(curSelectedForPath.GridY + moveInput.y));
            if (comabtMove == false)
            {
                selectedNode = grid.GetNodeAt((int)newCoord.x, (int)newCoord.y);



                StartCoroutine(SelectCurSelectedNode(toComplete, selectedNode));
            }

        }
        else if (lastMousePos != curMousePos && comabtMove == false)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target = new Vector3(worldPosition.x, worldPosition.y, 0);
            selectedNode = grid.NodeFromWorldPoint(target);
            //  StartCoroutine(SelectCurSelectedNode(toComplete, selectedNode));
            if (selectedNode != null)
            {
                if (selectedNode.InRange)
                {
                    if (selectedNode != curSelectedForPath)
                    {
                        worldCamera.NodeToFollow = selectedNode;
                        toComplete(selectedNode);
                    }
                }
            }
        }

    }
    public IEnumerator SelectCurSelectedNode(Action<Node> toComplete, Node selectedNode)
    {
        if (selectedNode != null)
        {
            if (selectedNode.InRange)
            {
                if (!Vector2.Equals(selectedNode.Coords, curSelectedForPath.Coords))
                {

                    comabtMove = true;
                    yield return new WaitForSeconds(camerMoveDelay);
                    worldCamera.NodeToFollow = selectedNode;
                    moveingToNode = curSelectedForPath;
                    comabtMove = false;
                    //Debug.Log(curSelectedForPath.Coords + " to " + selectedNode.Coords.ToString());
                    if (worldCamera.isMoving!)
                    {

                        //Debug.Log("input = " + moveInput.ToString() + "  " + curSelectedForPath.Coords + " to " + selectedNode.Coords + " was " + newCoord);
                    }
                    toComplete(selectedNode);



                }
            }
        }



    }

    /*---------------------------------------------------------------------
     *  Method: SelectValidNodesForAttack(List<Node> curPossibleValidNodes, Node selectedNode)
     *
     *  Purpose: Changes the provided nodes visual to be for attack and shows 
     *           hit chance of unit if they are selected
     * 
     *  Parameters: List<Node> curPossibleValidNodes = nodes that are available based on selected node
     *              Node selectedNode = node the player selected
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SelectValidNodesForAttack(HashSet<Node> curPossibleValidNodes, Node selectedNode, Command curCommand)
    {
        foreach (Node possibleNode in curPossibleValidNodes)
        {
            if (possibleNode == null)
            {
                continue;
            }

            grid.ActivateNodeForCurTurn(possibleNode.WorldPos);
            if (possibleNode.Unit != null && availableUnits.Contains(possibleNode.Unit))
            {

                float hitChance = 0;
                if (curCommand == Command.MagicAttack)
                {
                    hitChance = curUnit.GetMagicAttackHitChance(curUnit.CurMove, possibleNode);
                }
                else if (curCommand == Command.Attack)
                {
                    hitChance = curUnit.GetAttackHitChance(possibleNode.Unit);
                }
                else if (curCommand == Command.Attack)
                {
                    hitChance = curUnit.GetPushChance(possibleNode.Unit);
                }
                combatUI.ShowUnitHitChance(possibleNode.Unit.UnitCombatUI, (int)hitChance);
            }

        }

        if (isHitChance)
        {
            this.isHitChance = true;

        }

    }

    public void ChangeNodes(List<Node> possibleNodes, bool isHitChance, Node selectedNode, Action<Node> toComplete)
    {
        // shows hit chance for all affeected units
        foreach (Node possibleNode in possibleNodes)
        {
            if (possibleNode == null)
            {
                continue;
            }
            if (possibleNode.InRange)
            {
                toComplete(possibleNode);
            }
        }
        if (isHitChance)
        {
            this.isHitChance = true;

        }
        //grid.ActivateNodeForCurTurn(selectedNode.WorldPos);

    }

    public void CompleteTurn(int movesMade)
    {
        state = State.Waiting;
        movesCompleted += movesMade;
        movementsCompleted += movesMade;
        combatUI.CompleteTurn(movesMade);
        isCompletingMove = false;
    }

    /*---------------------------------------------------------------------
     *  Method: CompleteAttackAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by attacking by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the attack 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompletePushAction(Node selectedNode)
    {
        if (selectedNode == null)
        {
            return;
        }
        // if player isnt being moved and target is within range 
        if (!curUnit.IsMoving && selectedNode.InRange)
        {
            // gets the node the player is trying to move to
            targetNode = selectedNode;
            // chekcing is target is a unit
            if (targetNode != null)
            {
                targetUnit = targetNode.Unit;
                // if node contains a unit and is attacking
                if (targetUnit != null)
                {
                    if (targetUnit.Stats.CurHealth <= 0)
                    {
                        return;
                    }
                    if (selectedNode.InRange)
                    {
                        isCompletingMove = true;
                        //CompleteTurn(2);
                        curUnit.SelectedTargets.Add(targetUnit);
                        curUnit.PushUnit(targetUnit, CompleteAttack);

                        //Debug.Log("Talking to!!! " + targetUnit.Team + "  " + curUnit.Team);
                    }
                    else if (!targetUnit.isEnemy(curUnit))
                    {
                        // Debug.Log("Freindly " + targetUnit.Team + "  " + curUnit.Team);
                    }
                    else
                    {
                        // Debug.Log("trying to talk to node that is out of range");
                    }
                    return;
                }
            }
        }

    }
    public void onUnitTalkedTo(int movesMade)
    {
        dialougeManager.EnterDialougeMode(targetUnit.DialogeManager.ComatText, () =>
        {
            state = State.Normal;
        });
    }
    public void AttackFailed(int movesMade)
    {
        attackComplete = true;
        CompleteTurn(movesMade);
        state = State.Normal;
    }
    public void CompleteAttack(int movesMade)
    {
        isCompletingMove = false;
        CompleteTurn(movesMade);
        ClearUnitCombtUI();
        attackComplete = true;
        state = State.Normal;
        if (curUnit != null)
        {
            if (teamUpAttackMeter.ContainsKey(curUnit.Team))
            {
                teamUpAttackMeter[curUnit.Team] += 10;
                combatUI.HandeleTeamUp(teamUpAttackVal, teamUpAttackMeter[curUnit.Team]);
            }
        }
    }
    /*---------------------------------------------------------------------
     *  Method: CompleteAttackAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by attacking by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the attack 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteCaptureAction(Node selectedNode)
    {
        if (selectedNode == null)
        {
            return;
        }
        // if player isnt being moved and target is within range 
        if (!curUnit.IsMoving && selectedNode.InRange)
        {
            // gets the node the player is trying to move to
            targetNode = selectedNode;
            // chekcing is target is a unit
            if (targetNode != null)
            {
                targetUnit = targetNode.Unit;
                // if node contains a unit and is attacking
                if (targetUnit != null)
                {
                    if (targetUnit.Stats.CurHealth <= 0)
                    {
                        return;
                    }
                    if (targetUnit.isEnemy(curUnit) && selectedNode.InRange)
                    {
                        isCompletingMove = true;

                        if (curCaptureItem != null)
                        {
                            player.CaptureUnit(targetUnit, curCaptureItem.CaptureItemRate, onUnitCaught, onUnitGotAway);

                        }

                    }
                    else if (!targetUnit.isEnemy(curUnit))
                    {
                        //Debug.Log("Freindly " + targetUnit.Team + "  " + curUnit.Team);
                    }
                    else
                    {
                        // Debug.Log("trying to talk to node that is out of range");
                    }
                    return;
                }
            }
        }

    }

    /*---------------------------------------------------------------------
     *  Method: CompleteMagicAttackAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by attacking by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the attack 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteMagicAttackAction(Node selectedNode)
    {
        if (selectedNode == null || attackComplete || curUnit == null)
        {
            return;
        }
        if (curUnit.CurMove == null)
        {
            return;
        }
        Debug.Log(" >>> ");
        makingMove = true;
        if (IsAttackValid(curUnit, selectedNode, curUnit.CurMove.MoveCost))
        {
            targetNode = selectedNode;
            // gets the node the player is trying to move to
            targetUnit = selectedNode.Unit;
            if (curUnit.CurMove.Type == MoveSO.RangeType.Connect)
            {
                if (curUnit.CurMove.SpreadRange > curUnit.SelectedTargets.Count && targetUnit != null)
                {
                    curUnit.SelectedTargets.Add(targetUnit);
                    combatUI.SelectMultiSelect();
                    if (curUnit.CurMove.SpreadRange == curUnit.SelectedTargets.Count)
                    {
                        combatUI.ResetMutliSelectCount();
                        combatUI.ActivateMultiSelectCounter(false);

                    }
                    else
                    {
                        state = State.Normal;
                        makingMove = false;
                        isCompletingMove = false;
                        return;
                    }

                }

            }
            else
            {
                curUnit.SelectedTargets.Add(targetUnit);
            }

            isCompletingMove = true;
            curUnit.MagicAttackUnit(selectedNode, curUnit.CurMove, CompleteAttack);

            return;
        }

    }
    /*---------------------------------------------------------------------
     *  Method: CompleteMagicAttackAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by attacking by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the attack 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AddForTeamAttackAction(Node selectedNode)
    {
        if (selectedNode == null || attackComplete || curUnit == null)
        {
            return;
        }
        Debug.Log(" >>> ");
        makingMove = true;
        if (IsTeamUpValid(curUnit, selectedNode))
        {
            targetNode = selectedNode;
            // gets the node the player is trying to move to
            targetUnit = selectedNode.Unit;


            if (teamUpSubmitted == false && targetUnit != null)
            {
                Debug.Log(" >>> ");
                curUnit.SelectedTargets.Add(targetUnit);

                state = State.Normal;
                makingMove = false;
                isCompletingMove = false;
                return;

            }


        }

    }
    /*---------------------------------------------------------------------
     *  Method: CompleteMagicAttackAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by attacking by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the attack 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteTeamAttackAction(Node selectedNode)
    {

        Debug.Log("making team move");
        isCompletingMove = true;
        if (teamUpAttackMeter.ContainsKey(curUnit.Team))
        {
            teamUpAttackMeter[curUnit.Team] = 0f;
        }
        curUnit.TeamAttack(CompleteAttack);
        return;




    }

    /*---------------------------------------------------------------------
     *  Method: CompleteSpecialAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by using special move by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the move 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void CompleteSpecialAction(Node selectedNode)
    {
        if (selectedNode == null)
        {
            return;
        }
        curUnit.CurMove = curUnit.SpecialMove;
        if (curUnit.CurMove == null)
        {
            return;
        }
        // if player isnt being moved and target is within range 
        if (IsAttackValid(curUnit, selectedNode, curUnit.CurMove.MoveCost))
        {
            targetNode = selectedNode;
            isCompletingMove = true;
            // gets the node the player is trying to move to
            targetUnit = selectedNode.Unit;
            curUnit.MagicAttackUnit(selectedNode, curUnit.CurMove, CompleteAttack);

        }
    }
    public void AttckUnit(Node selectedNode, Action attack)
    {
        targetNode = selectedNode;
        // chekcing is target is a unit
        if (targetNode != null)
        {
            targetUnit = targetNode.Unit;
            // if node contains a unit and is attacking
            if (targetUnit != null)
            {
                // if hitting dead unit
                if (targetUnit.Stats.CurHealth <= 0)
                {
                    return;
                }
                // if curUnit doenst have enough mp
                if (curUnit.CurMove != null)
                {
                    if (curUnit.Stats.CurMP - curUnit.CurMove.MpCost < 0)
                    {
                        return;
                    }
                }
                if (targetUnit.isEnemy(curUnit) && selectedNode.InRange)
                {

                    attack();

                    // Debug.Log("Attacking!!! " + targetUnit.Team + "  " + curUnit.Team);
                }

                return;
            }
        }
    }

    /*---------------------------------------------------------------------
     *  Method: CompleteAttackAction(Node selectedNode)
     *
     *  Purpose: Completes the action of a unit by attacking by chekcing if selectedNode
     *           is an enemy uit. If it is it preforms the attack 
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteAttackAction(Node selectedNode)
    {
        if (selectedNode == null)
        {
            return;
        }
        // if player isnt being moved and target is within range 
        if (!curUnit.IsMoving && selectedNode.InRange)
        {
            // gets the node the player is trying to move to
            int moveCost = curUnit.GetAttackCost();
            targetNode = selectedNode;
            if (IsAttackValid(curUnit, targetNode, moveCost))
            {
                isCompletingMove = true;
                curUnit.AttackUnit(selectedNode, targetUnit, (int movesMade) =>
                {
                    if (targetUnit.Stats.CurHealth <= 0)
                    {
                        HandleUnitDeath(targetUnit);
                    }
                    CompleteAttack(movesMade);
                });

            }

        }

    }

    public bool IsAttackValid(Unit curUnit, Node targetNode, int moveCost)
    {
        if (curUnit == null || targetNode == null)
        {
            Debug.Log(" user or raget is null");
            return false;
        }
        if (curUnit == targetNode.Unit)
        {
            Debug.Log("clicked user");
            return false;
        }
        if (movesCompleted + moveCost > 2)
        {
            Debug.Log("move costs too much");
            return false;
        }
        // if move allows hitting objects
        // if using a magic move
        if (curUnit.CurMove != null)
        {
            if (curUnit.Stats.CurMP - curUnit.CurMove.MpCost < 0)
            {
                Debug.Log("not enough mana");
                return false;
            }
            if (curUnit.CurMove.HitObjects && targetNode.Walkable && targetNode.InRange)
            {
                Debug.Log("targret node is valid");
                return true;
            }
        }
        // if moves is only to used on units
        if (targetNode.Unit == null)
        {
            Debug.Log("need to hit unit");
            return false;
        }

        targetUnit = targetNode.Unit;
        // if node contains a unit and is attacking
        if (targetUnit != null)
        {
            // if hitting dead unit
            if (targetUnit.Stats.CurHealth <= 0)
            {
                return false;
            }
            // if curUnit doenst have enough mp
            if (curUnit.CurMove != null)
            {
                if (curUnit.Stats.CurMP - curUnit.CurMove.MpCost < 0)
                {
                    return false;
                }
            }
            // if in range and on the other team
            if (targetNode.InRange && !curUnit.IsMoving)
            {

                return true;

            }

        }
        return false;
    }
    public bool IsTeamUpValid(Unit curUnit, Node targetNode)
    {
        if (curUnit == null || targetNode == null)
        {
            Debug.Log(" user or raget is null");
            return false;
        }
        if (curUnit == targetNode.Unit)
        {
            Debug.Log("clicked user");
            return false;
        }
        if (movesCompleted + 1 > 2)
        {
            Debug.Log("move costs too much");
            return false;
        }

        // if moves is only to used on units
        if (targetNode.Unit == null)
        {
            Debug.Log("need to hit unit");
            return false;
        }

        targetUnit = targetNode.Unit;
        // if node contains a unit and is attacking
        if (targetUnit != null)
        {
            // if hitting dead unit
            if (targetUnit.Stats.CurHealth <= 0)
            {
                return false;
            }
            // if curUnit doenst have enough mp
            if (curUnit.CurMove != null)
            {
                if (curUnit.Stats.CurMP - curUnit.CurMove.MpCost < 0)
                {
                    //    return false;
                }
            }
            if (curUnit.SelectedTargets.Contains(targetUnit))
            {
                return false;
            }
            // if in range and on the other team
            if (targetUnit.isEnemy(curUnit) == false && targetNode.InRange && !curUnit.IsMoving)
            {

                return true;

            }

        }
        return false;
    }

    /*---------------------------------------------------------------------
     *  Method: CompleteMoveAction(Node selectedNode)
     *
     *  Purpose: Completes the movemnt of a unit by attacking by chekcing if selectedNode
     *           is valid and if so it moves to the node
     * 
     *  Parameters: Node selectedNode the node clikced on
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CompleteMoveAction(Node selectedNode)
    {
        if (selectedNode == null)
        {
            return;
        }
        // if player isnt being moved and target is within range 
        if (!curUnit.IsMoving && selectedNode.InRange)
        {
            // gets the node the player is trying to move to
            targetNode = selectedNode;
            // chekcing is target is a unit
            targetUnit = targetNode.Unit;
            if (targetNode != null && targetNode.Unit == null)
            {
                // if node is free of units
                isCompletingMove = true;
                worldCamera.NodeToFollow = null;
                lastVisitedNode = grid.NodeFromWorldPoint(curUnit.transform.position);
                curUnit.MoveToPoint(targetNode.WorldPos, () =>
                {
                    ChangeUnitPos(curUnit, target);
                    CompleteTurn(1);
                    moveComplete = true;
                    state = State.Normal;

                }, () =>
                {
                    state = State.Normal;

                });
                return;

            }
        }

    }

    /*---------------------------------------------------------------------
     *  Method: PlayAttackHitIcon()
     *
     *
     *  Purpose: Plays hit/miss animation for targetUnit and waits
     *           until animaion is done to resert for next move
     * 
     *  Parameters: Vector3 target = the location of the unit being checked
     *  Returns: none
     *-------------------------------------------------------------------*/
    public IEnumerator PlayAttackHitIcon()
    {
        targetUnit.CombatIconAnimator.SetTrigger("Miss");
        float timeToWait = targetUnit.CombatIconAnimator.GetCurrentAnimatorStateInfo(0).length + 0.5f;
        yield return new WaitForSecondsRealtime(timeToWait);
        onCompleteAfter();

    }

    /*---------------------------------------------------------------------
     *  Method: onUnitCaught()
     *
     *
     *  Purpose: Remoes/kills the targetUnit when caught and 
     *           sets hit/miss text to 'Caught' and plays miss/hit animaiton
     * 
     *  Parameters: mnone
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void onUnitCaught()
    {
        if (targetUnit == null) { return; }
        CompleteTurn(1);
        targetUnit.CombatHitIconText.text = ("Caught"); ;
        onCompleteAfter = () =>
        {
            HandleUnitDeath(targetUnit, () =>
            {
                state = State.Normal;
                attackComplete = true;
                ClearUnitCombtUI();
            });
        };
        StartCoroutine("PlayAttackHitIcon");
    }

    /*---------------------------------------------------------------------
     *  Method: onUnitCaught()
     *
     *
     *  Purpose: Remoes/kills the targetUnit when caught and 
     *           sets hit/miss text to 'Caught' and plays miss/hit animaiton
     * 
     *  Parameters: mnone
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void onUnitSwitchTeam()
    {
        if (targetUnit == null) { return; }
        CompleteTurn(1);
        dialougeManager.StartCoroutine("ExitDialougeMode");
        targetUnit.CombatHitIconText.text = ("Convinced");
        onCompleteAfter = () =>
        {
            state = State.Normal;
            HandleTeamSwitch();
            attackComplete = true;
            ClearUnitCombtUI();
        };
        StartCoroutine("PlayAttackHitIcon");



    }
    /*---------------------------------------------------------------------
    *  Method: onUnitCaught()
    *
    *
    *  Purpose: Remoes/kills the targetUnit when caught and 
    *           sets hit/miss text to 'Caught' and plays miss/hit animaiton
    * 
    *  Parameters: mnone
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void onUnitSwitchTeamBack()
    {
        if (targetUnit == null) { return; }
        dialougeManager.StartCoroutine("ExitDialougeMode");
        targetUnit.CombatHitIconText.text = ("Turned Back");
        onCompleteAfter = () =>
        {
            state = State.Normal;
            SwitchTeam(curUnit);
            attackComplete = true;
            ClearUnitCombtUI();
        };
        StartCoroutine("PlayAttackHitIcon");



    }
    public void onUnitNotConvinced()
    {
        if (targetUnit == null) { return; }
        CompleteTurn(1);
        dialougeManager.StartCoroutine("ExitDialougeMode");
        targetUnit.CombatHitIconText.text = ("Got Away");
        onCompleteAfter = () =>
        {
            state = State.Normal;
            attackComplete = true;
            ClearUnitCombtUI();
        };
        StartCoroutine("PlayAttackHitIcon");

    }

    /*---------------------------------------------------------------------
     *  Method: onUnitGotAway()
     *
     *
     *  Purpose:  sets hit/miss text to 'Got Away' and plays miss/hit animaiton
     * 
     *  Parameters: mnone
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void onUnitGotAway()
    {
        if (targetUnit == null) { return; }
        CompleteTurn(1);
        targetUnit.CombatHitIconText.text = ("Got Away");
        onCompleteAfter = () =>
        {
            state = State.Normal;
            attackComplete = true;
            ClearUnitCombtUI();
        };
        StartCoroutine("PlayAttackHitIcon");

    }


    /*---------------------------------------------------------------------
     *  Method: HandleUnitDeath(Unit unitToDie, Action onCompleteAfter = null)
     *
     *  Purpose: Handles what happens when unit dies
     * 
     *  Parameters: mnone
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void HandleUnitDeath(Unit unitToDie, Action onCompleteAfter = null)
    {
        if (unitToDie.Team == 0)
        {
            blueTeamList.Remove(unitToDie);
            redPoints++;
        }
        else
        {
            redTeamList.Remove(unitToDie);
            bluePoints++;
        }
        int indexOfDead = combatOrder.IndexOf(unitToDie);
        combatOrder.Remove(unitToDie);
        combatOrderRef.Remove(unitToDie);
        availableUnits.Remove(unitToDie);
        combatUI.HandleDeadUnit(indexOfDead, unitToDie);
        if (onCompleteAfter != null)
        {

            onCompleteAfter();
        }
        if (blueTeamList.Count > 0 && redTeamList.Count <= 0)
        {
            Debug.Log("blue team wins!!");
            EndCombatEncounter();
        }
        else if (blueTeamList.Count <= 0 && redTeamList.Count > 0)
        {
            Debug.Log("red team wins!!");
            EndCombatEncounter();
        }

    }

    /*---------------------------------------------------------------------
     *  Method: ChangeUnitPos(Unit unit,Vector3 newPos)
     *
     *  Purpose: updates the game board information to match a units 
     *           new postion when moved from its previous curNode
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ChangeUnitPos(Unit unit, Vector3 newPos)
    {
        Node nodeToChangeTo = grid.NodeFromWorldPoint(newPos);
        Node nodeToChagneFrom = unit.CurNode;
        nodeToChagneFrom.Walkable = true;
        nodeToChagneFrom.Unit = null;
        nodeToChangeTo.Unit = unit;
        unit.CurNode = nodeToChangeTo;
        unit.CurNode.Walkable = true;
    }

    /*---------------------------------------------------------------------
     *  Method: StartCombatEncounter()
     *
     *  Purpose: Starts a combat encounter by selecting the frist unit in rotation
     *           and then finding all its valid movemnt nodes, and finally telling  
     *           the camera the combat has started.
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void StartCombatEncounter()
    {

        // tells camera combat has started
        combatUI.Activate();
        player.InCombat = true;
        movesCompleted = 0;
        movementsCompleted = 0;
        attacksCompelted = 0;
        magicAttacksCompleted = 0;
        combatTriggered = false;
        combatStarted = true;
        ResetUnitForNextCommand();
        combatUI.ActivateCombatMenu(true);
        //playerInputManager.MoveAction.performed += SelectNode;
        // sets the default action for the next unit to be move
        // InitateMove(Command.Move, curUnit.Stats.MaxMoveDistance);
    }


    /*---------------------------------------------------------------------
     *  Method: SelectNextCurUnit()
     *
     *  Purpose: Selects the next curUnit by determing which team is active and then 
     *           picking the next availibale unit from the opposing team
     *           or by selecign the first available blue unit if curUnit is null
     * 
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void SelectNextCurUnit()
    {
        if (curUnit == null)
        {
            Debug.Log("curUnit is null");
            return;
        }// sets up curNode of the previous curUnit and the previous curUnit's base color
        curUnit.CurNode.Walkable = false;
        AssignUnitBaseColors(curUnit);
        // selects next unit 
        curTurn++;
        if (curTurn >= combatOrder.Count)
        {
            curTurn = 0;
        }
        curUnit = combatOrder[curTurn];
        // assigns curUnit for invenotry and combat UI, as well as the camera
        combatUI.AssignUnit(curUnit);
        combatUI.SelectNextUnitTurn(curUnit);
        inventoryUI.AssignUnit(curUnit);
        if (curUnit.UnitCombatUI != null)
        {
            curUnit.UnitCombatUI.SelectUnitForTurn();
        }
        worldCamera.AssignUnit(curUnit);
        curUnit.CurNode.Walkable = true;
        // apply status effect at start of turn
        if (curUnit.Stats != null)
        {


            if (curUnit.Stats.IsBurned)
            {
                curUnit.Stats.BurnUnit();
            }

            if (curUnit.Stats.IsShocked)
            {
                curUnit.Stats.ShockUnit();
            }

            if (curUnit.Stats.IsFrozen)
            {
                curUnit.Stats.FreezeUnit();
            }
            if (curUnit.Stats.IsPosioned)
            {
                curUnit.Stats.PosionUnit();
            }
        }
        if (curUnit.IsTalkTurn)
        {
            // if curUnit switches back to original team
            if (!curUnit.CheckTalkTurnBack())
            {
                onUnitSwitchTeamBack();

            }
        }
        grid.GetCentroid(units);
        // Debug.Log(curUnit.name + " IS AT (" + curUnit.CurNode.GridX + "," + curUnit.CurNode.GridY + ")");

    }


    /*---------------------------------------------------------------------
     *  Method: GetValidNodes(Vector3 target)
     *
     *
     *  Purpose: Gets the valid nodes that within range of a units 
     *           based on curCommand or what attack or move is being made
     * 
     *  Parameters: mnone
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void GetValidNodes(int range, Command curCommand)
    {

        if (curUnit.CurNode.Walkable == false || target == null)
        {
            curUnit.CurNode.Walkable = true;
        }
        Node curPos = curUnit.CurNode;
        int curX = curPos.GridX;
        int curY = curPos.GridY;
        // if node is valid if it is (range) nodes away from the source
        if (curCommand == Command.MagicAttack || curCommand == Command.Attack ||
            curCommand == Command.Push || curCommand == Command.Capture || curCommand == Command.Special || curCommand == Command.TeamUp)
        {
            curUnit.GetValidAttackNodes(curCommand);
            return;
        }
        // if node is valid if it is reachable by path as long as range
        else if (curCommand == Command.Move)
        {
            curUnit.GetValidMovements(range);

        }
    }

    public void PLayHitIcon(Unit unit, String hitText, Action completeAfterIcon)
    {
        unit.CombatHitIconText.text = (hitText);
        unit.CombatHitIconText.color = Color.red;
        tempTargetUnit = targetUnit;
        targetUnit = unit;
        onCompleteAfter = () =>
        {
            completeAfterIcon();
            targetUnit = tempTargetUnit;
        };
        StartCoroutine("PlayAttackHitIcon");
    }
    public void Log(String message)
    {
        if (isDebugging)
        {
            Debug.Log(message);
        }
    }
    /*---------------------------------------------------------------------
     *  Getters and setters
     *-------------------------------------------------------------------*/
    public HashSet<Node> ValidMovements { get { return validMovments; } set { validMovments = value; } }
    public bool IsHitChance { get { return isHitChance; } set { isHitChance = value; } }
    public Command CurCommand { get { return curCommand; } }
    public bool IsMakingHitableMove { get { return isMakingHitableMove; } set { isMakingHitableMove = value; } }
    public bool AttackComplete { get { return attackComplete; } set { attackComplete = value; } }
    public CaptureItemSO CurCaptureItem { get { return curCaptureItem; } set { curCaptureItem = value; } }
    public HashSet<Unit> AvailableUnits { get { return availableUnits; } }
    public SerializableDictionary<int, float> TeamUpAttackMeter { get { return teamUpAttackMeter; } }
    public Unit TempTargetUnit { get { return tempTargetUnit; } set { tempTargetUnit = value; } }
    public Unit TargetUnit { get { return targetUnit; } set { targetUnit = value; } }
    public Action OnCompletedAfter { get { return onCompleteAfter; } set { onCompleteAfter = value; } }
    public CombatGrid Grid { get { return grid; } }
    public Vector3 UnitOffest { get { return unitOffset; } }
    public float TeamUpAttackVal { get { return teamUpAttackVal; } }
}
