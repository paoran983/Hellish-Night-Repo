using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Tilemaps;
using UnityEngine;
/*---------------------------------------------------------------------
*  Class: Unit
*
*  Purpose: Control the movment, actions, and status of a unit, Which is a gameobject
*           that can be commanded to move and complete actions
*-------------------------------------------------------------------*/
/**** Functions *****
 * ReachedDestination(Vector3 target)
 * MoveToPoint(Vector3 moveTarget, Action onReachedPosition, Action onReachedPositionFail)
 * TestPath(Vector3 pathStart, Vector3 pathEnd, int range)
 * GetDistance(Node startNode, Node targetNode)
 * PrintPath(Vector3 pathStart, Vector3 pathEnd)
 * TeamAttack(Action<int> OnCompletedAttack)
*********************/
public class Unit : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;
    [ContextMenu("Genetate id")]
    private void GenerateId()
    {
        if (id.Length > 0)
        {
            return;
        }
        id = System.Guid.NewGuid().ToString();
    }
    [Header("Combat Setup")]
    [SerializeField] private float speed = 5;
    [SerializeField] private CombatGrid grid;
    [SerializeField] private bool isAttacking, reachedDest, inCombat, isTalkTurn;
    [SerializeField] private int team;
    [SerializeField] private PathRequestManager pathRequestManager;
    [SerializeField] private MoveSO curMove;
    //[SerializeField] private List<MoveListSO> moveLists;
    [Header("Inventory Data")]
    // [SerializeField] private MoveListSO moveList, actionMoveList;
    [SerializeField] private InventorySO inventory;
    [SerializeField] private InventorySO abilityList, equipmentList;
    [SerializeField] private string title;
    [SerializeField] private AgentWeapon equipment;
    [Header("Combat Icon UI Setup")]
    [SerializeField] private TMP_Text combatHitIconText;
    [SerializeField] private Animator combatIconAnimator;
    [SerializeField] private GameObject interactIcon;
    [Header("Setup")]
    [SerializeField] private UnitCombatUI unitCombatUI;
    [SerializeField] private AbilityItemSO specialMoveItem;
    [SerializeField] private MoveSO specialMove;
    [SerializeField] private MoveSO push;
    private UnitStats stats;
    private Vector3 target;
    private List<Vector3> path;
    private List<Vector3> dir;
    private int targetIndex,
        unitsHitAnimsPlayed = 0,
        unitsHitAnimsToPlay = 0,
        teamUpUnitAnimsPlayed = 0,
        teanUpUnitAnimsToPlay = 0;
    private PathFinding pathFinding;
    private Animator unitAnimator;
    private Node curNode;
    private bool isHit;
    private bool isMoving;
    private HashSet<Node> validMovementNodes = new HashSet<Node>();
    private HashSet<Vector3> printPathSteps = new HashSet<Vector3>();
    private PathAndDir pathAndDir = new PathAndDir();
    private PathAndDir pathAndDirForPrint = new PathAndDir();
    private UnitDialgeManager dialogeManager;
    private bool isPlayer = false;
    private StateItem curState;
    internal int Money;
    private Action afterActionComplete, afterActionFail;
    //private Unit targetUnit;
    private PathResult lastPathResult;
    private GridCombatSystem gridCombatSystem;
    [SerializeField] private List<Unit> selectedTargets;
    int curAttackDistance;
    float talkTurnChance;
    int talkTurnCount, multiSelectTargetCount;
    private Queue<ModifierData> queue = new Queue<ModifierData>();
    private Vector3 nodeOffest;
    private GameObject combatBase;
    private string lastLayer;
    private int lastLayerOrder;
    [SerializeField] private int curLayer;
    [SerializeField] private Tile lastFloor;
    [SerializeField] private String lastFloorName;

    private void Awake()
    {

        isAttacking = false;
        pathFinding = GetComponent<PathFinding>();
        stats = GetComponent<UnitStats>();
        unitAnimator = GetComponent<Animator>();
        equipment = GetComponent<AgentWeapon>();
        dialogeManager = GetComponent<UnitDialgeManager>();
        unitCombatUI = GetComponent<UnitCombatUI>();
        if (unitCombatUI != null)
        {
            if (unitCombatUI.UnitBase != null)
            {
                nodeOffest = transform.position - unitCombatUI.UnitBase.transform.position;
                combatBase = unitCombatUI.UnitBase;
            }

        }
        combatBase = unitCombatUI.UnitBase;
        selectedTargets = new List<Unit>();
        equipment.AbilityList = abilityList;
        interactIcon.SetActive(false);
        if (specialMoveItem != null)
        {
            if (specialMoveItem.MoveList.Count >= 1)
            {
                specialMove = specialMoveItem.MoveList[0].item;
            }
        }
        curLayer = this.gameObject.layer;
        reachedDest = false;
        isHit = false;
        multiSelectTargetCount = 0;
        //inventory = new InventorySO();
    }
    private void Start()
    {

    }
    private void FixedUpdate()
    {
        /* if (grid != null)
         {
             Collider2D tileCollider = Physics2D.OverlapBox(transform.position, grid.Box, 90);

             if (tileCollider != null)
             {
                 if (tileCollider.gameObject != null)
                 {
                     Tile tile = tileCollider.gameObject.GetComponent<Tile>();
                     if (tile != null && (tile != lastFloor || lastFloor == null))
                     {



                     }

                 }
             }
         }*/
    }

    /*---------------------------------------------------------------------
     *  Method: ReachedDestination(Vector3 target) 
     *
     *  Purpose: Determins if a unit has reached a specific location 
     *           returns true if so and false if not
     *
     *  Parameters: Vector3 target = target location 
     *
     *  Returns: true if unit is at target and false if not
     *-------------------------------------------------------------------*/
    public bool ReachedDestination(Vector3 target)
    {
        if (grid != null)
        {
            if (grid.ReachedDestination(transform.position, target))
            {
                return true;
            }
        }
        return false;
    }

    /*---------------------------------------------------------------------
     *  Method: MoveToPoint(Vector3 moveTarget)
     *
     *  Purpose: Moves player to specific location by sending a new pathRequest
     *           to the PathRequestManager. It then tells the PathRequestManager
     *           to run OnPathFound when path is found so the unti follows the path 
     *
     *  Parameters: Vector3 movetTarget = target location 
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void MoveToPoint(Vector3 moveTarget, Action onReachedPosition, Action onReachedPositionFail)
    {
        float startTime = Time.realtimeSinceStartup;
        reachedDest = false;
        pathRequestManager.RequestPath(new PathRequest(this, transform.position, moveTarget, 999, true, OnPathFound));
        afterActionComplete = onReachedPosition;
        afterActionFail = onReachedPositionFail;

    }

    /*---------------------------------------------------------------------
     *  Method: TestPath(Vector3 moveTarget,Vector3 moveStart)
     *
     *  Purpose: Tests if path is valid based on a units movementStats. 
     *           It does this by sending a new PathRequest to the
     *           PathRequestManager and tells it to run onTestMovementValidity 
     *           once the path is found. This will compare the path length to the 
     *           units's movement and then sets node to be inRange as well as
     *           activate the tile at new inRange node.
     *             
     *  Parameters: Vector3 moveTarget = the target of the path 
     *              Vector3 moveStart = the start of the path
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void TestPath(Vector3 pathStart, Vector3 pathEnd, int range)
    {

        pathRequestManager.RequestPath(new PathRequest(this, pathStart, pathEnd, range, true, onTestMovementValidity));
    }
    /*---------------------------------------------------------------------
     *  Method: TestPath(Vector3 moveTarget,Vector3 moveStart)
     *
     *  Purpose: Tests if path is valid based on a units movementStats. 
     *           It does this by sending a new PathRequest to the
     *           PathRequestManager and tells it to run onTestMovementValidity 
     *           once the path is found. This will compare the path length to the 
     *           units's movement and then sets node to be inRange as well as
     *           activate the tile at new inRange node.
     *             
     *  Parameters: Vector3 moveTarget = the target of the path 
     *              Vector3 moveStart = the start of the path
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void TestAttack(Vector3 pathStart, Vector3 pathEnd, int range)
    {

        pathRequestManager.RequestPath(new PathRequest(this, pathStart, pathEnd, range, true, onTesAttackValidity));
    }

    /*---------------------------------------------------------------------
     *  Method: TestPath(Vector3 moveTarget,Vector3 moveStart)
     *
     *  Purpose: Tests if path is valid based on a units movementStats. 
     *           It does this by sending a new PathRequest to the
     *           PathRequestManager and tells it to run onTestMovementValidity 
     *           once the path is found. This will compare the path length to the 
     *           units's movement and then sets node to be inRange as well as
     *           activate the tile at new inRange node.
     *             
     *  Parameters: Vector3 moveTarget = the target of the path 
     *              Vector3 moveStart = the start of the path
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public int GetDistance(Node startNode, Node targetNode)
    {
        int distX = Mathf.Abs(startNode.GridX - targetNode.GridX);
        int distY = Mathf.Abs(startNode.GridY - targetNode.GridY);
        int distZ = Mathf.Abs(startNode.Height - targetNode.Height);

        int distance = Mathf.FloorToInt(Mathf.Sqrt(Mathf.Pow(distX, 2) + Mathf.Pow(distY, 2) + Mathf.Pow(distZ, 2)));
        return distance;


    }
    public void ChangeHeight(int change)
    {
        if (curNode != null)
        {
            curNode.Height += change;
        }
        if (this.transform != null)
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - change);
        }
    }
    public bool CheckTalkTurnBack()
    {
        float hitChance = talkTurnChance;
        int moveRoll = UnityEngine.Random.Range(1, 100);
        if (moveRoll <= hitChance && talkTurnCount < 4)
        {
            isTalkTurn = true;
            talkTurnCount++;
            return true;
        }
        isTalkTurn = false;
        talkTurnCount--;
        return false;
    }
    /*---------------------------------------------------------------------
     *  Method: TestPath(Vector3 moveTarget,Vector3 moveStart)
     *
     *  Purpose: Tests if path is valid based on a units movementStats. 
     *           It does this by sending a new PathRequest to the
     *           PathRequestManager and tells it to run onTestMovementValidity 
     *           once the path is found. This will compare the path length to the 
     *           units's movement and then sets node to be inRange as well as
     *           activate the tile at new inRange node.
     *             
     *  Parameters: Vector3 moveTarget = the target of the path 
     *              Vector3 moveStart = the start of the path
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void PrintPath(Vector3 pathStart, Vector3 pathEnd)
    {
        ClearLastPath();

        pathRequestManager.RequestPath(new PathRequest(this, pathStart, pathEnd, 999, true, false, OnPrintPathFound));
    }

    /*---------------------------------------------------------------------
     *  Method: TestPath(Vector3 moveTarget,Vector3 moveStart)
     *
     *  Purpose: Tests if path is valid based on a units movementStats. 
     *           It does this by sending a new PathRequest to the
     *           PathRequestManager and tells it to run onTestMovementValidity 
     *           once the path is found. This will compare the path length to the 
     *           units's movement and then sets node to be inRange as well as
     *           activate the tile at new inRange node.
     *             
     *  Parameters: Vector3 moveTarget = the target of the path 
     *              Vector3 moveStart = the start of the path
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void PrintAttackPath(Vector3 pathStart, Vector3 pathEnd)
    {
        ClearLastPath();

        pathRequestManager.RequestPath(new PathRequest(this, pathStart, pathEnd, 999, false, OnPrintPathFound));
    }

    /*---------------------------------------------------------------------
     *  Method: GetValidAttackNodes(GridCombatSystem.Command curCommand
     *
     *  Purpose: Gets all the nodes that within range of an attack based on the 
     *           type of attack being made as well as what 
     *           move or weapon is being used for that attack 
     *             
     *  Parameters: GridCombatSystem.Command curCommand = the type of attack being made
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void GetValidAttackNodes(GridCombatSystem.Command curCommand)
    {
        HashSet<Node> possibleNodes = new HashSet<Node>();
        HashSet<Node> invisValidNodes = new HashSet<Node>();
        if (curNode == null) return;
        if (curCommand == GridCombatSystem.Command.Attack)
        {
            if (equipment.Weapon != null)
            {
                possibleNodes = grid.GetNeighborsInRange(curNode, equipment.Weapon.Range);
            }
            else
            {
                possibleNodes = grid.GetNeighborsInRange(curNode, 2);
            }

        }
        else if (curCommand == GridCombatSystem.Command.MagicAttack || curCommand == GridCombatSystem.Command.Special)
        {
            if (curMove == null) return;
            if (curMove.Type == MoveSO.RangeType.Line)
            {
                possibleNodes = grid.GetAllValidLineNodes(curNode, curMove.Range);
            }
            else if (curMove.Type == MoveSO.RangeType.Spread)
            {
                possibleNodes = grid.GetAllValidLineNodes(curNode, curMove.Range);
                invisValidNodes = grid.GetAllSpreadNodes(curNode, curMove.Range, curMove.SpreadRange);
            }
            else if (curMove.Type == MoveSO.RangeType.AOE)
            {
                possibleNodes = grid.GetNeighborsInRange(curNode, curMove.Range);
            }
            else
            {
                possibleNodes = grid.GetNeighborsInRange(curNode, curMove.Range);
            }
        }
        else if (curCommand == GridCombatSystem.Command.Push)
        {
            possibleNodes = grid.GetNeighborsInRange(curNode, 1);
        }
        else if (curCommand == GridCombatSystem.Command.TeamUp)
        {
            possibleNodes = grid.GetNeighborsInRange(curNode, stats.Speed * 2);
        }
        foreach (Node node in possibleNodes)
        {
            if (node == null) continue;

            //node.InRange = true;
            // validMovementNodes.Add(node);
            TestAttack(this.curNode.WorldPos, node.WorldPos, 99);
            // grid.ActivateNodeForAttack(node.WorldPos);
        }
        foreach (Node node in invisValidNodes)
        {
            if (node == null) continue;
            if (possibleNodes.Contains(node)) continue;
            //node.InRange = true;
            //validMovementNodes.Add(node);
            TestAttack(this.curNode.WorldPos, node.WorldPos, 99);

        }

    }

    /*---------------------------------------------------------------------
     *  Method: OnPathFound(PathAndDir pathAndDir, bool pathSuccess,int length, Node startNode,Unit sender)
     *
     *  Purpose: Follows the path if successful
     *             
     *  Parameters: PathAndDir pathAndDir = contains a list of positions of a path as
     *                                      well as a list of directions the unit 
     *                                      most move in in to reach it
     *                         bool pathSuccess = if the path was sucsessful or not 
     *                         int length = length of path
     *                         Node startNode = the start of the path
     *                         Unit sender = the unit who sent the pathRequest
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void OnPathFound(PathResult result)
    {
        if (result.Success)
        {
            this.pathAndDir = result.PathAndDir;
            this.lastPathResult = result;
            path = pathAndDir.path;
            dir = pathAndDir.dir;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");

        }
        else
        {
            afterActionFail();
        }
    }


    /*---------------------------------------------------------------------
    *  Method: isEnemy(Unit curUnit)
    *
    *  Purpose: Corutine that follows the path of a unit by going throught the 
    *           list and moving the unit to each waypoint
    *             
    *  Parameters: none
    *  Returns: none
    *-------------------------------------------------------------------*/
    public IEnumerator FollowPath()
    {
        //  OffsetPath();

        Vector3 curWaypoint = new Vector3(path[0].x, path[0].y, path[0].z);
        Vector3 nextWaypoint = new Vector3(path[0].x, path[0].y, path[0].z);
        //Node curNode = grid.NodeFromWorldPoint(transform.position + nodeOffest);
        Node curWayPointNode = grid.NodeFromWorldPoint(curWaypoint);
        Node nextWayPointNode = grid.NodeFromWorldPoint(curWaypoint);
        GameObject unitBase = null;
        if (unitCombatUI != null)
        {
            unitBase = unitCombatUI.UnitBase;

        }
        if (unitBase != null)
        {

            isMoving = true;
            while (true)
            {

                // if waypoint is reached

                if (unitBase.transform.position == curWaypoint)
                {
                    targetIndex++;

                    // if unit has reached destination or has collided with target
                    if (targetIndex >= path.Count)
                    {
                        isMoving = false;
                        AnimateMoving(false);
                        targetIndex = 0;
                        reachedDest = true;
                        afterActionComplete();
                        yield break;

                    }
                    // gets next waypoint to follow
                    curWaypoint = new Vector3(path[targetIndex].x, path[targetIndex].y, transform.position.z);
                    // checks if next wayPoint is a unit if it is it stops path
                    curWayPointNode = grid.NodeFromWorldPoint(curWaypoint);

                }

                // moves unit twords curWayPoint
                transform.position = Vector3.MoveTowards(transform.position, curWaypoint + nodeOffest, speed * Time.deltaTime);

                // animates unit
                AnimateInDir(dir[targetIndex].x, dir[targetIndex].y);
                AnimateMoving(true);
                isMoving = false;
                yield return null;

            }
        }
    }
    /*---------------------------------------------------------------------
     *  Method: ClearLastSpot()
     *
     *  Purpose: Clears the valid movemnt nodes of a unit by going throught 
     *           the units children and making any GridStats objects out of range
     *           as well as make the parent back to its original parent
     *             
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ClearLastSpot()
    {
        foreach (Node node in validMovementNodes)
        {
            node.InRange = false;
            if (node.Unit != null)
            {
                node.Walkable = false;
            }
            if (grid != null)
            {
                grid.ClearNode(node.WorldPos);
            }
        }
        grid.ClearNode(transform.position);
        validMovementNodes.Clear();
    }

    /*---------------------------------------------------------------------
     *  Method: ClearLastPath()
     *
     *  Purpose: Clears the path of a unit by reseting all nodes in printPathSteps
     *             
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ClearLastPath()
    {
        foreach (Vector3 nodePos in printPathSteps)
        {
            //  node.InRange = false;
            if (grid != null)
            {
                grid.ResetNode(nodePos);
            }
        }
        printPathSteps.Clear();
    }

    /*---------------------------------------------------------------------
     *  Method: GetValidMovements(Vector3 target)
     *
     *  Purpose: Gets the valid movemnts nodes of units current location
     *           by chekcing the surrounding nodes and seeing if the path
     *           to them is within the bounds of the units move distacne
     *             
     *  Parameters: Vector3 target = the location of the unit 
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void GetValidMovements(int range)
    {
        if (grid != null)
        {

            Node curPos = grid.NodeFromWorldPoint(transform.position);
            foreach (Node curNode in grid.GetNeighborsInRange(curPos, range))
            {

                TestPath(this.curNode.WorldPos, curNode.WorldPos, range);
            }
        }
    }
    /*---------------------------------------------------------------------
    *  Method: GetValidMovements(Vector3 target)
    *
    *  Purpose: Gets the valid movemnts nodes of units current location
    *           by chekcing the surrounding nodes and seeing if the path
    *           to them is within the bounds of the units move distacne
    *             
    *  Parameters: Vector3 target = the location of the unit 
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void GetValidMovements(int range, List<Node> nodes)
    {
        if (grid != null)
        {

            Node curPos = grid.NodeFromWorldPoint(transform.position);
            foreach (Node curNode in nodes)
            {

                TestPath(this.curNode.WorldPos, curNode.WorldPos, range);
            }
        }
    }


    /*---------------------------------------------------------------------
    *  Method: onTestMovementValidity(PathAndDir pathAndDir, bool pathSuccess,int length,Node startNode,Unit sender)
    *
    *  Purpose: Tests if node is inRange by comparing the length of the found 
    *           path to the maxMoveDistance and if within range the target 
    *           node is made inRange and the tile at that location is activated
    *             
    *  Parameters: PathAndDir pathAndDir = contains a list of positions of a path as
    *                                      well as a list of directions the unit 
    *                                      most move in in to reach it
    *                         bool pathSuccess = if the path was sucsessful or not 
    *                         int length = length of path
    *                         Node startNode = the start of the path
    *                         Unit sender = the unit who sent the pathRequest
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void onTestMovementValidity(PathResult result)
    {
        if (result.Success)
        {
            Node curStep = grid.NodeFromWorldPoint(result.EndNode.WorldPos);
            grid.ActivateNodeForRange(curStep.WorldPos);
            result.Sender.validMovementNodes.Add(curStep);
            curStep.InRange = true;
            /*result.Sender.validMovementNodes.Add(result.EndNode);
            result.EndNode.InRange = true;
            if (grid != null)
            {
                grid.ActivateNodeForRange(result.EndNode.WorldPos);
            }
            Debug.Log(" range = " + result.Range + "  " + result.PathAndDir.length + "  " + result.PathAndDir.path.Count);*/
            /*  Debug.Log(" range = " + result.Range + "  " + result.PathAndDir.length + result.PathAndDir.path.Count);
              for (int i = 0; i < result.PathAndDir.path.Count; i++)
              {

                  Node curStep = grid.NodeFromWorldPoint(result.PathAndDir.path[i]);
                  if (i > 4)
                  {
                      return;
                  }

                  else
                  {
                      Debug.Log(i);
                      result.Sender.validMovementNodes.Add(curStep);
                      curStep.InRange = true;
                      grid.ActivateNodeForRange(curStep.WorldPos);
                      // result.Sender.printPathSteps.Add(curStep.WorldPos);
                  }




              }*/




        }
    }
    /*---------------------------------------------------------------------
    *  Method: onTestMovementValidity(PathAndDir pathAndDir, bool pathSuccess,int length,Node startNode,Unit sender)
    *
    *  Purpose: Tests if node is inRange by comparing the length of the found 
    *           path to the maxMoveDistance and if within range the target 
    *           node is made inRange and the tile at that location is activated
    *             
    *  Parameters: PathAndDir pathAndDir = contains a list of positions of a path as
    *                                      well as a list of directions the unit 
    *                                      most move in in to reach it
    *                         bool pathSuccess = if the path was sucsessful or not 
    *                         int length = length of path
    *                         Node startNode = the start of the path
    *                         Unit sender = the unit who sent the pathRequest
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void onTesAttackValidity(PathResult result)
    {
        if (result.Success)
        {
            Debug.Log("attack good");
            Node curStep = grid.NodeFromWorldPoint(result.EndNode.WorldPos);
            grid.ActivateNodeForAttack(curStep.WorldPos);
            result.Sender.validMovementNodes.Add(curStep);
            curStep.InRange = true;
        }
    }
    /*---------------------------------------------------------------------
     *  Method: OnPrintPathFound(PathResult result)
     *
     *  Purpose: gets a path to a trget and then draws the path to that target
     *           by making each step of the path activated for CurMove
     *           and then adds the steps of the path to printPathSteps
     *             
     *  Parameters: PathResult result = the result from an attempt to find path to a target
     *  Returns: None
     *-------------------------------------------------------------------*/
    private void OnPrintPathFound(PathResult result)
    {
        if (result.Success)
        {
            this.pathAndDir = result.PathAndDir;
            this.lastPathResult = result;
            //grid.ActivateNodeForCurTurn(result.EndNode.WorldPos);
            // result.Sender.printPathSteps.Add(result.EndNode.WorldPos);
            for (int i = 0; i < pathAndDir.path.Count; i++)
            {
                if (pathAndDir.path[i] == result.StartNode.WorldPos)
                {
                    //continue;
                }

                // Debug.Log(" drawing path = " + result.Range + "  " + result.PathAndDir.length + "  " + result.PathAndDir.path.Count);
                Node curStep = grid.NodeFromWorldPoint(pathAndDir.path[i]);
                grid.ActivateNodeForCurTurn(curStep.WorldPos);
                result.Sender.printPathSteps.Add(curStep.WorldPos);

                /* if (pathAndDir.dir[i] != null) {
                     if (pathAndDir.dir[i].x== -1 && pathAndDir.dir[i].y == 0) {

                         grid.ActivateNodeForArrowRight(curStep.WorldPos);
                     }
                     else if (pathAndDir.dir[i].x == 1 && pathAndDir.dir[i].y == 0) {
                         grid.ActivateNodeArrowLeft(curStep.WorldPos);
                     }
                     else if (pathAndDir.dir[i].x == 0 && pathAndDir.dir[i].y == -1) {

                         grid.ActivateNodeForArrowUp(curStep.WorldPos);
                     }
                     else if (pathAndDir.dir[i].x == 0 && pathAndDir.dir[i].y == 1) {

                         grid.ActivateNodeForArrowDwon(curStep.WorldPos);
                     }
                     else if (pathAndDir.dir[i].x == 1 && pathAndDir.dir[i].y == 1 || pathAndDir.dir[i].x == -1 && pathAndDir.dir[i].y == -1) {
                         grid.ActivateNodeForDiagRight(curStep.WorldPos);
                     }
                     else if (pathAndDir.dir[i].x == 1 && pathAndDir.dir[i].y == -1 || pathAndDir.dir[i].x == -1 && pathAndDir.dir[i].y == 1) {
                         grid.ActivateNodeForDiagLeft(curStep.WorldPos);
                     }
                 }*/
            }
        }
    }

    /*---------------------------------------------------------------------
     *  Method: GetDistanceModifier(Node source, Node target, int minEffectiveRange, int maxEffectiveRange, int pentaly)
     *
     *  Purpose: Gets the distance modifier of a move based on the location of the target and source
     *             
     *  Parameters: Node source = soruce of attack
     *              Node target = target of attack
     *              int minEffectiveRange = the min distance that a move is effective
     *                                      or will get a postive distance modifer
     *              int maxEffectiveRange = the max distance that a move is effective
     *                                      or will get a postive distance modifer
     *              int pentaly = the pentalty for being outside of a moves effective range
     *  Returns: float distanceModifer = distnace modifer of move
     *-------------------------------------------------------------------*/
    public float GetDistanceModifier(Node source, Node target, int minEffectiveRange, int maxEffectiveRange, int pentaly)
    {
        float distanceModifer = 0;
        float curDisFromCenter = GetDistance(source, target);
        float effectiveRange = Math.Abs(maxEffectiveRange - minEffectiveRange);
        float effetiveRangeStep = 10 / effectiveRange;
        float curMoveDamge;
        if (curDisFromCenter < minEffectiveRange || curDisFromCenter > maxEffectiveRange)
        {
            distanceModifer = pentaly;
        }
        else if (curDisFromCenter >= minEffectiveRange && curDisFromCenter <= maxEffectiveRange)
        {
            distanceModifer = (effectiveRange - (curDisFromCenter - minEffectiveRange - 1)) * effetiveRangeStep;
        }
        distanceModifer += GetHeightModifer(source, target);
        return distanceModifer;
    }

    public float GetHeightModifer(Node source, Node target)
    {
        if (source.Unit == null || target.Unit == null)
        {
            return 0;
        }
        float mod = 0f;
        int startLayer = source.Unit.gameObject.layer;
        int targetLayer = target.Unit.gameObject.layer;
        if (grid.IsLineBlocked(source, target, startLayer, targetLayer) && startLayer > targetLayer)
        {
            mod = 10;
        }
        else if (grid.IsLineBlocked(source, target, startLayer, targetLayer) && startLayer <= targetLayer)
        {
            mod = -10;
        }

        return mod;
    }
    /*---------------------------------------------------------------------
     *  Method: GetDistanceModifier(Node source, Node target, int minEffectiveRange, int maxEffectiveRange, int pentaly)
     *
     *  Purpose: Gets the distance modifier of a move based on the location of the target and source
     *             
     *  Parameters: Node source = soruce of attack
     *              Node target = target of attack
     *              int minEffectiveRange = the min distance that a move is effective
     *                                      or will get a postive distance modifer
     *              int maxEffectiveRange = the max distance that a move is effective
     *                                      or will get a postive distance modifer
     *              int pentaly = the pentalty for being outside of a moves effective range
     *  Returns: float distanceModifer = distnace modifer of move
     *-------------------------------------------------------------------*/
    public float GetSpreadDistanceModifier(Node source, Node target, int minEffectiveRange, int maxEffectiveRange, int pentaly)
    {
        float distanceModifer = 0;
        int difX = 0;
        int difY = 0;
        if (((int)target.GridX - (int)source.GridX) > 0)
        {
            difX = 1;
        }
        else if (((int)target.GridX - (int)source.GridX) < 0)
        {
            difX = -1;
        }
        if (((int)target.GridY - (int)source.GridY) > 0)
        {
            difY = 1;
        }
        else if (((int)target.GridY - (int)source.GridY) < 0)
        {
            difY = -1;
        }
        int absX = Math.Abs(difX);
        int absY = Math.Abs(difY);
        float curDisFromCenter = Mathf.Abs(source.GridX - target.GridX);
        float effectiveRange = Math.Abs(maxEffectiveRange - minEffectiveRange);
        float effetiveRangeStep = 10 / effectiveRange;
        float curMoveDamge;
        if (curDisFromCenter < minEffectiveRange || curDisFromCenter > maxEffectiveRange)
        {
            distanceModifer = pentaly;
        }
        else if (curDisFromCenter >= minEffectiveRange && curDisFromCenter <= maxEffectiveRange)
        {
            distanceModifer = (effectiveRange - (curDisFromCenter - minEffectiveRange - 1)) * effetiveRangeStep;
        }
        return distanceModifer;
    }

    /*---------------------------------------------------------------------
     *  Method: MakeRoll(float hitChnce, int max)
     *
     *  Purpose: Make roll and returns false if lower 
     *           than hitChance and false if not
     *             
     *  Parameters: float hitChnce = hitChance to beat
     *              int max = the highest roll possible
     *  Returns: bool true if successfaul or false if not
     *-------------------------------------------------------------------*/
    public bool MakeRoll(float hitChnce, int max)
    {
        int moveRoll = UnityEngine.Random.Range(1, 100);
        if (moveRoll <= hitChnce)
        {
            return true;
        }
        return false;
    }

    /*---------------------------------------------------------------------
     *  Method: AffectUnit(Unit curAffected, float moveDamage, float distanceModifer)
     *
     *  Purpose: affect a unit's stats and then plays hitIcon 
     *           animation to show change of status
     *             
     *  Parameters: Unit curAffected = unit to affect
     *              float moveDamage = damge/heal to affect unit with
     *              float distanceModifer = the distance modifier of the current move
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AffectUnit(Node target, Unit curAffected, int targetCount, float moveDamage, int moveCost, List<ModifierData> mods, Action<int> onCompletedAttack)
    {
        if (curAffected == null || target == null) return;

        if (MakeRoll(1, 10))
        {
            moveDamage += (moveDamage / 10);
            if (curAffected.CombatHitIconText != null)
            {
                curAffected.CombatHitIconText.text = (" CRITIAL HIT\n- " + (int)moveDamage);
            }
        }
        else
        {
            if (curAffected.CombatHitIconText != null)
            {
                curAffected.CombatHitIconText.text = ("- " + (int)moveDamage);
            }
        }
        // Debug.Log(" hit with damage of " + curMoveDamge);
        StartCoroutine(ApllyModifersToTarget(target, curAffected, mods, moveDamage, () =>
        {
            Debug.Log("playing animains");

            StartCoroutine(PlayHitAnimation(curAffected, () =>
                    {
                        isAttacking = true;
                        isAttacking = false;
                    }));
            StartCoroutine(PlayAttackHitIcon(curAffected, moveCost, onCompletedAttack));
        }));
    }

    /*---------------------------------------------------------------------
     *  Method: ApllyModifersToTarget(Unit target, List<ModifierData> modifiers, float moveDamage) 
     *
     *  Purpose: affects a target unit's stats based on modifers list
     *             
     *  Parameters: Unit target = unit to affect
     *           List<ModifierData> modifiers = modifers to apply to target
     *           float moveDamage = damge of move 
     *  Returns: none
     *-------------------------------------------------------------------*/
    public IEnumerator ApllyModifersToTarget(Node targetNode, Unit target, List<ModifierData> modifiers, float moveDamage, Action toComplete)
    {
        // targetUnit = target;
        TalkToModiferSO talkToModifer = null;
        TeleportModiferSO teleportModifier = null;
        ModifierData teleportData = null;
        PushModiferSO pushToModifier = null;
        ModifierData pushData = null;
        bool modsApplied = false;
        queue = new Queue<ModifierData>();

        foreach (ModifierData data in modifiers)
        {



            if (data.statModifier.type == CharcterStatModifierSO.Type.Damage)
            {
                data.statModifier.AffectCharacter(target.gameObject, moveDamage);
                if (target.Stats.CurHealth <= 0)
                {
                    gridCombatSystem.HandleUnitDeath(target);
                }
            }
            else
            {
                queue.Enqueue(data);
                Debug.Log(data + " is added");
            }


        }
        if (gridCombatSystem != null)
        {

            queue.Enqueue(null);
            ModifierData firstMod = queue.Dequeue();
            ApplyStagedModifers(firstMod, targetNode, selectedTargets, modsApplied);
            while (queue.Count > 0)
            {
                yield return null;
            }
            if (queue.Count <= 0)
            {
                if (toComplete != null)
                {
                    toComplete();
                }
            }
        }

    }
    /*---------------------------------------------------------------------
     *  Method: ApplyStagedModifers(ModifierData data, Node targetNode, List<Unit> targets, bool modsApplied)
     *
     *  Purpose: Helper for a functin that applys list of modifers onto a target unit it applies the mods in the order 
     *           they appear in the moves's modiferData this avoids bugs with mods like push.
     *           This function recursilvey calls iitself to apply mods while also witing for 
     *           each mod to finish before doing the next mod
     *             
     *  Parameters: Unit target = unit to affect
     *           List<ModifierData> modifiers = modifers to apply to target
     *           float moveDamage = damge of move 
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void ApplyStagedModifers(ModifierData data, Node targetNode, List<Unit> targets, bool modsApplied)
    {

        if (data == null || queue == null) return;
        if (data != null && queue.Count > 0)
        {
            //Debug.Log("--applying staged mod count=" + queue.Count + "  " + targets.Count);

            TalkToModiferSO talkToModifer = null;
            TeleportModiferSO teleportModifier = null;
            PushModiferSO pushToModifier = null;
            talkToModifer = data.statModifier as TalkToModiferSO;
            teleportModifier = data.statModifier as TeleportModiferSO;
            pushToModifier = data.statModifier as PushModiferSO;
            if (talkToModifer != null)
            {
                talkToModifer.AffectUnit(data, this, targetNode, targets, gridCombatSystem.onUnitTalkedTo, gridCombatSystem.AttackFailed, data.value);
                return;
            }
            else if (pushToModifier != null)
            {
                foreach (Unit target in targets)
                {
                    Node pushToDest = pushToModifier.GetPushDestination(target.curNode, this, targets, data.value);
                    if (target != null && data.isDone == true)
                    {
                        data.isDone = false;

                        //Debug.Log("moving " + targetNode.WorldPos + " to " + pushToDest.WorldPos);
                        StartCoroutine(moveTowardsModifer(target, pushToDest.WorldPos, data, () =>
                        {
                            if (data != null)
                            {
                                data.isDone = true;
                                ModifierData nextMod = queue.Dequeue();
                                ApplyStagedModifers(nextMod, targetNode, targets, modsApplied);
                            }
                        }));
                        return;
                    }
                }
            }
            else
            {
                data.statModifier.AffectUnit(data, this, targetNode, targets, null, null, data.value);
                ModifierData nextMod = queue.Dequeue();
                ApplyStagedModifers(nextMod, targetNode, targets, modsApplied);
                return;
            }

        }
    }
    public IEnumerator moveTowardsModifer(Unit user, Vector3 target, ModifierData data, Action toComplete)
    {
        GameObject userObj = user.gameObject;
        if (target == null)
        {
            if (data != null)
            {
                data.isDone = true;
            }
            yield return null;
        }

        if (target != null)
        {




            while (Vector3.Distance(userObj.transform.position, target) > 0.1f)
            {
                // Move the object a step closer to the target each frame
                userObj.transform.position = Vector3.MoveTowards(
                    userObj.transform.position,
                    target,
                    6 * Time.deltaTime
                );

                // Yield here so it continues in the next frame
                yield return null;
            }

            // Optional: snap to target position if needed
            userObj.transform.position = target;
            user.curNode.Walkable = true;
            user.curNode.Unit = null;
            user.curNode = grid.NodeFromWorldPoint(target);
            user.curNode.Unit = user;
            if (toComplete != null && data.isDone == false)
            {
                if (data != null)
                {

                    data.isDone = true;
                }
                Debug.Log("moving is done");
                toComplete();

            }

        }
    }
    /*---------------------------------------------------------------------
     *  Method: ApllyModifersToTarget(Unit target, List<ModifierData> modifiers, float moveDamage) 
     *
     *  Purpose: affects a target unit's stats based on modifers list
     *             
     *  Parameters: Unit target = unit to affect
     *           List<ModifierData> modifiers = modifers to apply to target
     *           float moveDamage = damge of move 
     *  Returns: none
     *-------------------------------------------------------------------*/
    public float GetDamageModiferData(List<ModifierData> modifiers)
    {
        float moveDamage = 0;
        foreach (ModifierData data in modifiers)
        {
            if (data.statModifier.type == CharcterStatModifierSO.Type.Damage)
            {
                moveDamage += data.value;
            }

        }
        return moveDamage;
    }

    /*---------------------------------------------------------------------
     *  Method: PlayAttackHitIcon
     *
     *  Purpose: plays a units Miss animation and 
     *           then deploys onAferCompleteAction if possible
     *           also increments the int unitsHitAnimsPlayed and only deploys
     *           onAferCompleteAction when unitsHitAnimsPlayed >= unitsHitAnimsToPlay
     *           or when all units's nit animation have been played.
     *           This should be final set of animtins that play after an attack 
     *             
     *  Parameters: Unit target = the unit to attack 
     *              int damage = the amount of damage to do
     *  Returns: none
     *-------------------------------------------------------------------*/
    public IEnumerator PlayAttackHitIcon(Unit targetUnit, int movesMade, Action<int> onCompletedAttack)
    {
        if (targetUnit != null)
        {
            unitsHitAnimsPlayed++;
            targetUnit.CombatIconAnimator.SetTrigger("Miss");
            targetUnit.CombatHitIconText.color = Color.red;
            float timeToWait = targetUnit.CombatIconAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSecondsRealtime(timeToWait);
            if (onCompletedAttack != null && unitsHitAnimsPlayed >= unitsHitAnimsToPlay)
            {
                onCompletedAttack(movesMade);
                onCompletedAttack = null;
            }
        }
        else if (onCompletedAttack != null)
        {
            onCompletedAttack(movesMade);
            onCompletedAttack = null;
        }
    }

    /*---------------------------------------------------------------------
     *  Method: PlayAttackHitIcon
     *
     *  Purpose: plays a units Miss animation and 
     *           then deploys onAferCompleteAction if possible
     *             
     *  Parameters: Unit target = the unit to attack 
     *              int damage = the amount of damage to do
     *  Returns: none
     *-------------------------------------------------------------------*/
    public IEnumerator PlayTeamUpAttackAnimation(Unit unit, Action toComeplete)
    {

        if (unit != null)
        {
            teamUpUnitAnimsPlayed++;
            unit.unitAnimator.SetTrigger("Attack");
            float timeToWait = unitAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSecondsRealtime(timeToWait);
            if (toComeplete != null && teamUpUnitAnimsPlayed >= teanUpUnitAnimsToPlay)
            {
                toComeplete();
            }
        }
        else if (toComeplete != null)
        {
            toComeplete();
        }



    }
    /*---------------------------------------------------------------------
    *  Method: PlayAttackHitIcon
    *
    *  Purpose: plays a units Miss animation and 
    *           then deploys onAferCompleteAction if possible
    *             
    *  Parameters: Unit target = the unit to attack 
    *              int damage = the amount of damage to do
    *  Returns: none
    *-------------------------------------------------------------------*/
    public IEnumerator PlayHitAnimation(Unit targetUnit, Action toComeplete)
    {
        if (targetUnit != null)
        {
            // animsPlayed++;
            Debug.Log("playuing hit animaiton");
            targetUnit.unitAnimator.SetTrigger("Attack");
            float timeToWait = unitAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return null;

            if (toComeplete != null)
            {
                toComeplete();
            }
        }
        else
        {
            Debug.Log("cant  hit animaiton");
            if (toComeplete != null)
            {
                toComeplete();
            }
        }
    }
    /*---------------------------------------------------------------------
    *  Method: PlayAttackHitIcon
    *
    *  Purpose: plays a units Miss animation and 
    *           then deploys onAferCompleteAction if possible
    *             
    *  Parameters: Unit target = the unit to attack 
    *              int damage = the amount of damage to do
    *  Returns: none
    *-------------------------------------------------------------------*/
    public IEnumerator PlayAnimation(String name, Action toComeplete)
    {

        // unitHitAnimsPlayed += 1;
        this.unitAnimator.SetTrigger(name);
        float timeToWait = unitAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(timeToWait);
        if (toComeplete != null)
        {
            toComeplete();
        }
    }


    /*---------------------------------------------------------------------------------------------------------------
     *  attack
     *---------------------------------------------------------------------------------------------------------------*/
    /*---------------------------------------------------------------------
     *  Method: GetAttackHitChance(Unit target)
     *
     *  Purpose: Gets and returns the hit chance of a attack 
     *           based on distnace from target, equipped weapon and stats
     *             
     *  Parameters: None
     *  Returns: float hitChance = hit chance of attack
     *-------------------------------------------------------------------*/
    public float GetAttackHitChance(Unit target)
    {
        if (stats == null || target == null)
        {
            return 0;
        }
        if (curNode == null | target.curNode == null)
        {
            return 0;
        }
        if (target == this)
        {
            return 100;
        }

        float moveAccuracy = 30 + stats.Strength + stats.Speed;
        float distanceModifer = 0;
        if (equipment != null)
        {
            WeaponItemSO curWeapon = equipment.Weapon;
            if (curWeapon != null)
            {
                distanceModifer = GetDistanceModifier(curNode, target.curNode, curWeapon.MinEffectiveRange, curWeapon.MaxEffectiveRange, -10);
                moveAccuracy = curWeapon.Accuracy;
                if (curWeapon.type == ItemSO.Type.Sword)
                {
                    moveAccuracy += stats.Strength;
                }
                else if (curWeapon.type == ItemSO.Type.CurvedSword)
                {
                    moveAccuracy += stats.Speed;
                }
                else if (curWeapon.type == ItemSO.Type.Pistol)
                {
                    moveAccuracy += stats.Intelligence;
                }

            }
            // if unarmed
            else
            {
                distanceModifer = 0;
            }
        }



        float hitChance = moveAccuracy + distanceModifer;
        return hitChance;
    }

    /*---------------------------------------------------------------------
     *  Method: GetAttackDamge()
     *
     *  Purpose: Gets and reutsn the damage of a physical attack 
     *           based on stats and  equiped weapon
     *             
     *  Parameters: None
     *  Returns: float moveDamage = damge of physcil attack
     *-------------------------------------------------------------------*/
    public float GetAttackDamge()
    {
        if (stats == null || curNode == null)
        {
            return 0;
        }
        UnitStatsData tempStats = stats.GetStatusInflictedStats();
        // unarmed damage
        float moveDamage = 10 + tempStats.strength;
        if (equipment != null)
        {
            WeaponItemSO curWeapon = equipment.Weapon;
            if (curWeapon != null)
            {
                if (curWeapon.type == ItemSO.Type.Sword)
                {
                    moveDamage = curWeapon.Damage + tempStats.strength;
                }
                else if (curWeapon.type == ItemSO.Type.CurvedSword)
                {
                    moveDamage = curWeapon.Damage + tempStats.speed;
                }
                else if (curWeapon.type == ItemSO.Type.Pistol)
                {
                    moveDamage = curWeapon.Damage + tempStats.intelligence;
                }
            }

        }
        return moveDamage;
    }
    /*---------------------------------------------------------------------
     *  Method: GetAttackDamge()
     *
     *  Purpose: Gets and reutsn the damage of a physical attack 
     *           based on stats and  equiped weapon
     *             
     *  Parameters: None
     *  Returns: float moveDamage = damge of physcil attack
     *-------------------------------------------------------------------*/
    public int GetAttackCost()
    {
        if (stats == null || curNode == null)
        {
            return 0;
        }
        UnitStatsData tempStats = stats.GetStatusInflictedStats();
        // unarmed damage
        int moveCost = 1;
        if (equipment != null)
        {
            WeaponItemSO curWeapon = equipment.Weapon;
            if (curWeapon != null)
            {
                moveCost = curWeapon.MoveCost;
            }

        }
        return moveCost;
    }

    /*---------------------------------------------------------------------
     *  Method: AttackUnit(Unit target,int damage)
     *
     *  Purpose: completes an attack on another unit
     *             
     *  Parameters: Unit target = the unit to attack 
     *              int damage = the amount of damage to do
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AttackUnit(Node targetNode, Unit target, Action<int> OnCompletedAttack)
    {

        if (stats == null || target == null)
        {
            return;
        }
        unitsHitAnimsToPlay = 1;
        unitsHitAnimsPlayed = 0;
        StartCoroutine(PlayAnimation("Attack", () =>
        {



            // Debug.Log("trying to attack");
            float moveDamage = GetAttackDamge();
            float hitChance = GetAttackHitChance(target);
            int moveCost = GetAttackCost();
            int moveRoll = UnityEngine.Random.Range(1, 100);
            if (moveRoll <= hitChance)
            {
                target.CombatHitIconText.text = ("- " + (int)moveDamage);
                // Debug.Log("hit!! with chance of " + hitChance + " and roll of " + moveRoll);
                // inflicts status effects
                if (equipment != null)
                {
                    WeaponItemSO curWeapon = equipment.Weapon;
                    if (curWeapon != null)
                    {
                        // Debug.Log("damaging with armed " + moveDamage);
                        StartCoroutine(PlayHitAnimation(target, () =>
                        {


                            StartCoroutine(ApllyModifersToTarget(targetNode, target, curWeapon.ModifierData, moveDamage, () =>
                            {
                                isAttacking = true;
                                StartCoroutine(PlayAttackHitIcon(target, moveCost, OnCompletedAttack));
                            }));
                        }));
                    }
                    // If unarmed
                    else if (target.stats != null)
                    {
                        //  Debug.Log("damaging with unarmed " + moveDamage);
                        target.stats.Damage(moveDamage);
                        isAttacking = true;
                        StartCoroutine(PlayHitAnimation(target, () =>
                        {
                            StartCoroutine(PlayAttackHitIcon(target, moveCost, OnCompletedAttack));
                        }));
                    }

                }



            }
            else
            {
                // Debug.Log("miss!! with chance of " + hitChance + " and roll of " + moveRoll);
                target.CombatHitIconText.text = ("MISS");
                StartCoroutine(PlayAttackHitIcon(target, moveCost, OnCompletedAttack));
            }
        }));


    }
    /*---------------------------------------------------------------------------------------------------------------
     *  Magic attack
     *---------------------------------------------------------------------------------------------------------------*/
    /*---------------------------------------------------------------------
     *  Method: GetMagicAttackHitChance(Unit target)
     *
     *  Purpose: Gets and returns the hit chance of a magic attack 
     *           based on distnace from target, curMove and stats
     *             
     *  Parameters: None
     *  Returns: float hitChance = hit chance of magic attack
     *-------------------------------------------------------------------*/
    public float GetMagicAttackHitChance(MoveSO curMove, Node target)
    {
        float moveAccuracy = 0;
        if (stats == null || target == null)
        {
            return 0;
        }
        if (curNode == null | target == null)
        {
            return 0;
        }
        if (curMove != null)
        {
            moveAccuracy = curMove.Accuracy;

        }
        if (target.Unit == this)
        {
            return 100;
        }
        UnitStatsData tempStats = stats.GetStatusInflictedStats();
        float distanceModifer = GetDistanceModifier(curNode, target, curMove.MinEffectiveRange, curMove.MaxEffectiveRange, -10);
        float hitChance = moveAccuracy + tempStats.magic + distanceModifer;
        return hitChance;

    }


    /*---------------------------------------------------------------------
     *  Method: GetMagicAffectedUnits(Node target)
     *
     *  Purpose: Gets the list of units the represeting the units effected by a magic move
     *             
     *  Parameters: Node target = target of magic move
     *  Returns: List<Unit> affectedUnits = the list of units the represeting the
     *                                      units affected by an attack. Will be emtpy 
     *                                      target is invalid
     *-------------------------------------------------------------------*/
    public List<Unit> GetMagicAffectedUnits(Node target)
    {
        List<Unit> affectedUnits = new List<Unit>();
        if (target.InRange == false) return affectedUnits;
        if (curMove.Type == MoveSO.RangeType.AOE)
        {
            HashSet<Node> curPossibleValidNodes = new HashSet<Node>();
            curPossibleValidNodes = grid.GetNeighborsInRange(target, curMove.SpreadRange);
            // finds all units within range of aoe attack
            foreach (Node possibleNode in curPossibleValidNodes)
            {
                if (gridCombatSystem != null)
                {
                    if (gridCombatSystem.AvailableUnits.Contains(possibleNode.Unit))
                    {
                        if (possibleNode.Unit != null)
                        {
                            affectedUnits.Add(possibleNode.Unit);
                        }
                    }
                }

            }
        }
        else if (curMove.Type == MoveSO.RangeType.Line)
        {
            List<Node> curPossibleValidNodes = new List<Node>();
            curPossibleValidNodes = grid.GetValidLineNodes(curNode, target, curMove.Range);
            // finds all units within range of aoe attack
            foreach (Node possibleNode in curPossibleValidNodes)
            {
                if (gridCombatSystem != null)
                {
                    if (possibleNode.InRange && gridCombatSystem.AvailableUnits.Contains(possibleNode.Unit))
                    {
                        if (possibleNode.Unit != null)
                        {
                            affectedUnits.Add(possibleNode.Unit);
                        }
                    }
                }

            }
        }
        else if (curMove.Type == MoveSO.RangeType.Spread)
        {
            HashSet<Node> curPossibleValidNodes = new HashSet<Node>();
            curPossibleValidNodes = grid.GetSpread(curNode, target, curMove.Range, curMove.SpreadRange);
            // finds all units within range of aoe attack
            foreach (Node possibleNode in curPossibleValidNodes)
            {
                if (gridCombatSystem != null)
                {
                    if (gridCombatSystem.AvailableUnits.Contains(possibleNode.Unit))
                    {
                        if (possibleNode.Unit != null)
                        {
                            affectedUnits.Add(possibleNode.Unit);
                        }
                    }
                }

            }
        }
        else if (curMove.Type == MoveSO.RangeType.Connect)
        {
            if (selectedTargets != null)
            {
                return selectedTargets;
            }

        }
        else
        {
            if (target.Unit != null)
            {
                affectedUnits.Add(target.Unit);

            }
        }
        return affectedUnits;
    }

    /*---------------------------------------------------------------------
     *  Method: MagicAttackUnit(Node target, MoveSO curMove, Action OnCompletedAttack)
     *
     *  Purpose: completes an magic attack on another unit
     *             
     *  Parameters: Unit target = the unit to attack 
     *              int damage = the amount of damage to do
     *              Action OnCompletedAttack = fucntion to deploy after hit animation is done
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void MagicAttackUnit(Node target, MoveSO curMove, Action<int> onCompletedAttack)
    {

        if (stats == null || target == null)
        {
            return;
        }
        StartCoroutine(PlayAnimation("Attack", () =>
        {

            /*GameObject moveProjectile = Instantiate(curMove.Propjectile);
            moveProjectile.transform.SetParent(this.gameObject.transform);
            moveProjectile.gameObject.transform.localScale = Vector3.one;
            Debug.Log("shooting ");
            StartCoroutine(grid.FollowPath(grid.GetLine(curNode, target), moveProjectile, () =>
            {
                //Destroy(moveProjectile);
                float moveDamage = 0;
                // uses Mp
                stats.UseMana(curMove.MpCost);
                float hitChance = 0;
                int count = 0;
                float distanceModifer = 0;
                float curMoveDamge;
                List<Node> curPossibleValidNodes = new List<Node>();
                List<Unit> affectedUnits = GetMagicAffectedUnits(target);
                if (affectedUnits.Count > 0)
                {
                    CompleteMagicAttack(target, affectedUnits, curMove, onCompletedAttack);

                }
                else
                {
                    onCompletedAttack(curMove.MoveCost);
                }
            }));*/
            List<Unit> affectedUnits = GetMagicAffectedUnits(target);
            if (affectedUnits.Count > 0)
            {
                CompleteMagicAttack(target, affectedUnits, curMove, onCompletedAttack);

            }
            else
            {
                onCompletedAttack(curMove.MoveCost);
            }
        }));
    }
    /*---------------------------------------------------------------------
     *  Method: MagicAttackUnit(Node target, MoveSO curMove, Action OnCompletedAttack)
     *
     *  Purpose: completes an magic attack on another unit
     *             
     *  Parameters: Unit target = the unit to attack 
     *              int damage = the amount of damage to do
     *              Action OnCompletedAttack = fucntion to deploy after hit animation is done
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void TeamAttack(Action<int> OnCompletedAttack)
    {

        if (stats == null || target == null)
        {
            return;
        }

        List<Unit> teamUpUnits = new List<Unit>(selectedTargets);
        teamUpUnits.Add(this);
        teanUpUnitAnimsToPlay = teamUpUnits.Count;
        teamUpUnitAnimsPlayed = 0;
        foreach (Unit teamUpUnit in teamUpUnits)
        {
            StartCoroutine(PlayTeamUpAttackAnimation(teamUpUnit, () =>
            {


                float moveDamage = 0;
                // uses Mp
                //stats.UseMana(curMove.MpCost);
                float hitChance = 0;
                int count = 0;
                float distanceModifer = 0;
                float comboMoveDamage = 0;
                List<Node> curSelectedNodes = new List<Node>();
                List<Node> teamUpNodes = new List<Node>();
                List<ModifierData> comboAttackMods = new List<ModifierData>();
                foreach (Unit teamUpUnit in teamUpUnits)
                {
                    if (teamUpUnit == null) continue;
                    if (teamUpUnit.stats == null) continue;
                    if (teamUpUnit.curNode == null) continue;
                    teamUpNodes.Add(teamUpUnit.curNode);
                    if (teamUpUnit.stats.SpecialMove != null)
                    {
                        if (teamUpUnit.stats.SpecialMove.ModifierData != null)
                        {
                            foreach (ModifierData mod in teamUpUnit.stats.SpecialMove.ModifierData)
                            {
                                comboAttackMods.Add(mod);
                            }

                        }
                        comboMoveDamage += GetMagicDamge(teamUpUnit.stats.SpecialMove);
                    }
                }
                List<Unit> affectedUnits = new List<Unit>();
                foreach (Node node in grid.GetShapeByVertex(teamUpNodes, CurNode))
                {
                    if (node.Unit == null) continue;
                    if (node.Unit.team == this.team) continue;

                    affectedUnits.Add(node.Unit);

                }
                unitsHitAnimsToPlay = affectedUnits.Count;
                unitsHitAnimsPlayed = 0;
                foreach (Unit target in affectedUnits)
                {
                    if (target.curNode == null) continue;
                    AffectUnit(target.curNode, target, affectedUnits.Count, comboMoveDamage, 1, comboAttackMods, OnCompletedAttack);
                }
                OnCompletedAttack(1);
            }));
        }
    }
    /*---------------------------------------------------------------------
     *  Method: GetMagicDamge(MoveSO curMove, Unit target, float distanceModifer)
     *
     *  Purpose: Gets and returns the damage of a magic attack 
     *           based on move damage and distance
     *             
     *  Parameters: MoveSO curMove = move being used
     *              float distanceModifer = disance modifer of move based on 
     *              distance from user and target
     *  Returns: float moveDamage = damge of magic attack
     *-------------------------------------------------------------------*/
    public float GetMagicDamge(MoveSO curMove)
    {
        float moveDamage = 0;
        if (curMove != null)
        {
            moveDamage = GetDamageModiferData(curMove.ModifierData);
        }

        return moveDamage;
    }
    public Vector2 GetMoveDamgeAndDistance(Node target, Unit targetUnit, MoveSO curMove)
    {
        Vector2 moveData = Vector2.zero;
        float moveDamage = 0;
        float distanceModifer = 0;
        if (curMove.Type == MoveSO.RangeType.AOE)
        {
            distanceModifer = GetDistanceModifier(target, targetUnit.curNode, 0, curMove.SpreadRange, 12);
            distanceModifer += GetDistanceModifier(curNode, target, 0, curMove.Range, 3);
            moveDamage = GetMagicDamge(curMove);

        }
        if (curMove.Type == MoveSO.RangeType.Line || curMove.Type == MoveSO.RangeType.Connect || curMove.Type == MoveSO.RangeType.Spread)
        {
            distanceModifer = GetDistanceModifier(curNode, targetUnit.curNode,
                curMove.MinEffectiveRange, curMove.MaxEffectiveRange, 12);
            moveDamage = GetMagicDamge(curMove);
        }
        else
        {
            distanceModifer = GetDistanceModifier(curNode, targetUnit.curNode,
                curMove.MinEffectiveRange, curMove.MaxEffectiveRange, 10);
            moveDamage = GetMagicDamge(curMove);
        }
        moveData.x = distanceModifer;
        moveData.y = moveDamage;
        return moveData;
    }
    /*---------------------------------------------------------------------
     *  Method: CompleteMagicAttack(Node target, Unit targetUnit, MoveSO curMove)
     *
     *  Purpose: completes a magic attak on a specific unit
     *             
     *  Parameters: Node target = the target of the attack
     *              Unit targetUnit = the unit being affected by the attack
     *              MoveSO curMove = the move being used
     *  Returns: true if attack was succseful or false if missed
     *-------------------------------------------------------------------*/
    public void CompleteMagicAttack(Node target, List<Unit> targetUnits, MoveSO curMove, Action<int> onCompletedAttack)
    {
        if (target == null || curMove == null) return;
        float hitChance = 0;
        float moveDamage = 0;
        float avgDistanceModifer = 0;
        float distanceModifer = 0;
        Vector2 moveData = new Vector2();
        // defines amounto f hit animaitons to apply before going to next turn
        unitsHitAnimsToPlay = targetUnits.Count;
        unitsHitAnimsPlayed = 0;
        // if treating all targets as one hit
        // Debug.Log("trying to attack " + unitsHitAnimsToPlay);
        if (curMove.MultiHit)
        {
            hitChance = 0;
            moveDamage = 0;
            avgDistanceModifer = 0;
            distanceModifer = 0;

            // gets the avergae distnaceModifer and damage based on each target unit
            foreach (Unit targetUnit in targetUnits)
            {
                if (targetUnit.curNode == null) continue;
                hitChance += GetMagicAttackHitChance(curMove, target);

                distanceModifer = 0;
                moveData = GetMoveDamgeAndDistance(target, targetUnit, curMove);
                distanceModifer = moveData.x;
                moveDamage += moveData.y;
                avgDistanceModifer += distanceModifer;
            }
            hitChance = hitChance / targetUnits.Count;
            moveDamage = moveDamage / targetUnits.Count;
            avgDistanceModifer = avgDistanceModifer / targetUnits.Count;
            // sees if attack hits
            if (MakeRoll(hitChance, 100))
            {
                moveDamage -= avgDistanceModifer;
                //  AffectUnit(target, targetUnit, targetUnits.Count, moveDamage, curMove.MoveCost, curMove.ModifierData, onCompletedAttack);
                return;
            }
            else
            {
                foreach (Unit targetUnit in targetUnits)
                {
                    //  this.targetUnit = targetUnit;
                    targetUnit.CombatHitIconText.text = ("MISS");
                    StartCoroutine(PlayAttackHitIcon(targetUnit, curMove.MoveCost, onCompletedAttack));
                }
                return;
            }
        }

        // if each target has differnt hit chance and damage
        foreach (Unit targetUnit in targetUnits)
        {
            if (targetUnit.curNode == null) continue;
            hitChance = GetMagicAttackHitChance(curMove, target);
            moveDamage = 0;

            distanceModifer = 0;
            if (MakeRoll(hitChance, 100))
            {
                moveData = GetMoveDamgeAndDistance(target, targetUnit, curMove);
                distanceModifer = moveData.x;
                moveDamage += moveData.y;
                moveDamage -= distanceModifer;
                Debug.Log("hit with chance of " + hitChance);
                AffectUnit(target, targetUnit, targetUnits.Count, moveDamage, curMove.MoveCost, curMove.ModifierData, onCompletedAttack);
            }
            else
            {
                Debug.Log("mis with chance of " + hitChance);
                //  this.targetUnit = targetUnit;
                targetUnit.CombatHitIconText.text = ("MISS");
                StartCoroutine(PlayAttackHitIcon(targetUnit, curMove.MoveCost, onCompletedAttack));
            }
        }
        return;

    }
    /*---------------------------------------------------------------------------------------------------------------
     *  Other actions run,talk,
     *---------------------------------------------------------------------------------------------------------------*/
    /*---------------------------------------------------------------------
     *  Method: GetTalkChance(Unit target)
     *
     *  Purpose: Gets and returns the hit chance of a talk attack 
     *           based on distnace from target
     *             
     *  Parameters: None
     *  Returns: float hitChance = hit chance of talk attack
     *-------------------------------------------------------------------*/
    public float GetPushChance(Unit target)
    {
        if (stats == null || target == null)
        {
            return 0;
        }
        if (curNode == null | target.curNode == null)
        {
            return 0;
        }
        if (target == this)
        {
            return 100;
        }
        UnitStatsData tempStats = stats.GetStatusInflictedStats();
        float moveAccuracy = tempStats.strength * 10;
        return moveAccuracy;
    }

    /*---------------------------------------------------------------------
     *  Method: TalkToUnit(Unit target, Action onTalkSuccessful, Action onTalkFailed)
     *
     *  Purpose: tries to Talks to unit and then plays units hitIcon animation to 
     *           reflect result and then deploys onTalkSuccessful is succesful 
     *           and onTalkFailed if not
     *          
     *             
     *  Parameters: Unit target = target to try to talk to
     *              Action onTalkSuccessful = what to do if talk works
     *              Action onTalkFailed = what to do if talk fails
     *  Returns: none
     *-------------------------------------------------------------------
    public void TalkToUnit(Unit target, Action<int> onTalkSuccessful, Action<int> onTalkFailed)
    {

        if (stats == null || target == null)
        {
            return;
        }

        targetUnit = target;
        talkTurnCount = 0;
        float hitChance = GetTalkChance(target);
        int moveRoll = UnityEngine.Random.Range(1, 100);
        if (moveRoll <= hitChance)
        {
            onAfterCompleteAttack = onTalkSuccessful;
            Debug.Log("hit!! with chance of " + hitChance + " and roll of " + moveRoll);
            target.CombatHitIconText.text = ("Talked to");
            StartCoroutine(PlayAttackHitIcon(1));
            talkTurnChance = hitChance;
            isTalkTurn = true;

        }
        else
        {
            onAfterCompleteAttack = onTalkFailed;
            Debug.Log("miss!! with chance of " + hitChance + " and roll of " + moveRoll);
            target.CombatHitIconText.text = ("MISS");
            StartCoroutine(PlayAttackHitIcon(1));
            talkTurnChance = 0;
            isTalkTurn = false;
        }
    }*/
    public void PushUnit(Unit target, Action<int> OnCompletedAttack)
    {
        if (target == null || OnCompletedAttack == null)
        {
            return;
        }
        int hitChance = (int)GetPushChance(target);
        if (push != null)
        {


            StartCoroutine(ApllyModifersToTarget(target.curNode, target, push.ModifierData, 0, () =>
            {
                isAttacking = true;
                StartCoroutine(PlayAttackHitIcon(target, push.MoveCost, OnCompletedAttack));
                isAttacking = false;
            }));
        }
        else
        {
            Debug.Log("miss with chance of " + hitChance);
            // this.targetUnit = target;
            target.CombatHitIconText.text = ("MISS");
            StartCoroutine(PlayAttackHitIcon(target, push.MoveCost, OnCompletedAttack));
        }
    }

    /*---------------------------------------------------------------------
     *  Method: AttemptRun(List<Unit> enemies, int runDistance, Action OnCeompltedAfterFail, Action OnCompletedAttack) 
     *
     *  Purpose: attempts to run away from combat. If the unit if runDistacne amount of 
     *          nodes away from the center of all the enmeies the attempt can be succseful
     *          there is a default chance of 70 if possible if not pissble the attempt is always field.
     *          Once the reuslt is determined either OnCompletedAttack or OnCeompltedAfterFail is deployed and 
     *          the hitIcon animation is played to reflect result.
     *          
     *             
     *  Parameters: List<Unit> enemies = enemies in comabt
     *              int runDistance = disnce the unit must be from center of enmeies to attempt run 
     *              Action OnCeompltedAfterFail = what happend if run fails 
     *              Action OnCompletedAttack) = what happens if runs away
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AttemptRun(List<Unit> enemies, int runDistance, Action<int> OnCeompltedAfterFail, Action<int> OnCompletedAttack)
    {

        if (stats == null || target == null)
        {
            return;
        }
        Node centerOfEnemies = grid.GetCentroid(enemies);
        if (centerOfEnemies != null)
        {
            if (grid.GetDistance(curNode, centerOfEnemies) >= runDistance && MakeRoll(70, 100))
            {
                combatHitIconText.text = ("ESCAPED");
                StartCoroutine(PlayAttackHitIcon(this, 2, OnCompletedAttack));
            }
            else
            {
                combatHitIconText.text = ("FAILED");
                StartCoroutine(PlayAttackHitIcon(this, 2, OnCompletedAttack));
            }
        }
    }

    /*---------------------------------------------------------------------
     *  Method: GetCaptureRate(Unit targetUnit, float captureItemRate)
     *
     *  Purpose: Gets the possible of capturing a unit based on the item
     *           used and the unit as wellas the units current health
     *          
     *             
     *  Parameters: Unit target = target to try to capture
     *              float captureItemRate = capture rate of item used to capture
     *  Returns: none
     *-------------------------------------------------------------------*/
    public float GetCaptureRate(Unit targetUnit, float captureItemRate)
    {
        if (targetUnit == null)
        {
            return 0;
        }
        int captureUnitRate = targetUnit.Stats.CaptureUnitRate;
        float targetMaxHp = targetUnit.Stats.Maxhealth;
        float targetCurHp = targetUnit.Stats.CurHealth;
        float statusRate = 1;
        float captureRate = (((1 + (targetMaxHp * 3 - targetCurHp * 2) *
            captureUnitRate * captureItemRate * statusRate) / (targetMaxHp * 3)) / 256) * 100;
        return captureRate;
    }

    /*---------------------------------------------------------------------
     *  Method: CaptureUnit(Unit targetUnit, int captureItemRate, Action onAfterCaught, Action onAfterGotAway)
     *
     *  Purpose: tries to capture a unit then plays units hitIcon animation to 
     *           reflect result and then deploys onAfterCaught is succesful 
     *           and onAfterGotAway if not
     *             
     *  Parameters: Unit target = target to try to capture
     *              float captureItemRate = capture rate of item used to capture
     *              Action onAfterCaught = what to do if catch works
     *              Action onAfterGotAway = what to do if catch fails
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CaptureUnit(Unit targetUnit, int captureItemRate, Action onAfterCaught, Action onAfterGotAway)
    {
        if (targetUnit == null)
        {
            return;
        }
        if (targetUnit.Stats == null)
        {
            return;
        }
        float captureRate = GetCaptureRate(targetUnit, captureItemRate);
        if (MakeRoll(captureRate, 100))
        {
            onAfterCaught();
            return;
        }
        else
        {
            onAfterGotAway();
            return;
        }
    }

    /*---------------------------------------------------------------------
     *  Method: AnimateInDir(float x, float y)
     *
     *  Purpose: Animates the unit in the direction specfied
     *             
     *  Parameters: float x = x axis of direction
     *              float y = y axis of direction
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AnimateInDir(float x, float y)
    {
        if (unitAnimator != null)
        {
            unitAnimator.SetFloat("moveX", x);
            unitAnimator.SetFloat("moveY", y);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: AnimateMoving(bool isMoving)
     *
     *  Purpose: Animates the unit to be moving
     *             
     *  Parameters: bool isMOving = if moving ot not
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void AnimateMoving(bool isMoving)
    {
        if (unitAnimator != null)
        {
            unitAnimator.SetBool("isMoving", isMoving);
        }
    }
    /*---------------------------------------------------------------------
     *  Method: isEnemy(Unit curUnit)
     *
     *  Purpose: determines if another unit is on the same team or not
     *             
     *  Parameters: Unit targetUnit = the unit to check
     *  Returns: true if on the same team and false if not
     *-------------------------------------------------------------------*/
    public bool isEnemy(Unit targetUnit)
    {
        if (targetUnit.team == this.team)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("_--------" + collision.gameObject.name);
        if (collision.gameObject != null)
        {

            Tile tile = collision.gameObject.GetComponent<Tile>();
            if (tile != null)
            {
                // Debug.Log("_--------" + collision.gameObject.name);
                if (tile.type == Tile.Type.stiars)
                {
                    tile.EnterStairs(collision, this);

                }

            }
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != null)
        {
            //  Debug.Log("_---leaving-----" + collision.gameObject.name);
            Tile tile = collision.gameObject.GetComponent<Tile>();
            if (tile != null)
            {


            }
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        //  Debug.Log("_--------" + collision.gameObject.name);
        isHit = true;


    }
    public void IgnoreCollisionLayer(int curLayer, int LayerToAvoid)
    {
        this.gameObject.layer = curLayer;
        Physics2D.IgnoreLayerCollision(curLayer, LayerToAvoid);

    }

    public void ActivateHealthBar(bool isActive)
    {
        stats.HealthBar.gameObject.SetActive(isActive);
    }
    public void ActivateManaBar(bool isActive)
    {
        if (stats.ManaBar == null)
        {
            return;
        }
        stats.ManaBar.gameObject.SetActive(isActive);
    }
    /*---------------------------------------------------------------------------------------------------------------
     *  Data Persistance
     *---------------------------------------------------------------------------------------------------------------*/
    public void LoadData(GameData data)
    {
        Vector3 savedPos = Vector3.zero;
        if (data.unitsPos.TryGetValue(id, out savedPos))
        {
            transform.position = savedPos;
        }
        // inventory
        UnitInvenotryData savedInv = null;
        if (data.unitsInventory.TryGetValue(id, out savedInv))
        {
            if (savedInv != null)
            {
                inventory.InventoryItems = savedInv.unitInventory;
                inventory.SetSize(savedInv.size);
                inventory.Weight = savedInv.weight;
            }
        }
        // abilites
        UnitInvenotryData savedAbilites = null;
        if (data.unitsAbilities.TryGetValue(id, out savedAbilites))
        {
            if (savedAbilites != null)
            {
                abilityList.InventoryItems = savedAbilites.unitInventory;
                abilityList.SetSize(savedAbilites.size);
                abilityList.Weight = savedAbilites.weight;
            }
        }
        // equipment
        UnitInvenotryData savedEquipment = null;
        if (data.unitsEquipment.TryGetValue(id, out savedEquipment))
        {
            if (savedEquipment != null)
            {
                equipmentList.InventoryItems = savedEquipment.unitInventory;
                equipmentList.SetSize(savedEquipment.size);
                equipmentList.Weight = savedEquipment.weight;
            }
        }
        // stats
        UnitStatsData savedStats = null;
        if (data.unitsStats.TryGetValue(id, out savedStats))
        {
            if (savedStats != null)
            {
                stats.LoadUnitStatsData(savedStats);
            }
        }
    }

    public void SaveData(GameData data)
    {
        if (data.unitsPos.ContainsKey(id))
        {
            data.unitsPos.Remove(id);
        }
        data.unitsPos.Add(id, transform.position);
        // invntory
        if (data.unitsInventory.ContainsKey(id))
        {
            data.unitsInventory.Remove(id);
        }
        UnitInvenotryData dataInv = new UnitInvenotryData(inventory.InventoryItems,
            inventory.GetSize(), inventory.Weight);
        if (dataInv != null)
        {
            data.unitsInventory.Add(id, dataInv);
        }
        // abilites
        if (data.unitsAbilities.ContainsKey(id))
        {
            data.unitsAbilities.Remove(id);
        }
        UnitInvenotryData dataAbilites = new UnitInvenotryData(abilityList.InventoryItems,
            abilityList.GetSize(), abilityList.Weight);
        if (dataAbilites != null)
        {
            data.unitsAbilities.Add(id, dataAbilites);
        }
        // equipment
        if (data.unitsEquipment.ContainsKey(id))
        {
            data.unitsEquipment.Remove(id);
        }
        UnitInvenotryData dataEquipment = new UnitInvenotryData(equipmentList.InventoryItems,
            equipmentList.GetSize(), equipmentList.Weight);
        if (dataEquipment != null)
        {
            data.unitsEquipment.Add(id, dataEquipment);
        }
        // stats
        if (data.unitsStats.ContainsKey(id))
        {
            data.unitsStats.Remove(id);
        }
        UnitStatsData dataStats = stats.GetUnitStatData();
        if (dataStats != null)
        {
            data.unitsStats.Add(id, dataStats);
        }

    }

    /*---------------------------------------------------------------------------------------------------------------
     *  Getters and setters
     *---------------------------------------------------------------------------------------------------------------*/
    public PathAndDir PathAndDir { get { return this.pathAndDir; } }
    public StateItem CurState { get { return this.curState; } set { curState = value; } }
    public CombatGrid Grid { get { return this.grid; } set { grid = value; } }
    public HashSet<Node> ValidMovementNodes { get { return this.validMovementNodes; } set { validMovementNodes = value; } }
    public HashSet<Node> GetValidMovementNodes { get { return this.validMovementNodes; } set { validMovementNodes = value; } }
    public AgentWeapon Equipment { get { return this.equipment; } set { equipment = value; } }
    public bool IsMoving { get { return this.isMoving; } set { this.isMoving = value; } }
    public bool IsHit { get { return this.isHit; } set { this.isHit = value; } }
    public UnitStats Stats { get { return this.stats; } }
    // public MoveListSO ActionMoveList { get { return this.actionMoveList; } set { actionMoveList = value; } }
    public InventorySO AbilityList { get { return equipment.AbilityList; } set { equipment.AbilityList = value; } }
    public int Team { get { return this.team; } set { this.team = value; } }
    public PathRequestManager PathRequestManager { get { return this.pathRequestManager; } set { pathRequestManager = value; } }
    public PathFinding PathFinding { get { return pathFinding; } }
    public UnitDialgeManager DialogeManager { get { return dialogeManager; } }
    public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
    public bool ReachedDest { get { return reachedDest; } set { reachedDest = value; } }
    // public MoveListSO MoveList { get { return moveList; } set { moveList = value; } }
    //public List<MoveListSO> MoveLists { get { return moveLists; } set { moveLists = value; } }
    public InventorySO Inventory { get { return inventory; } set { inventory = value; } }
    public TMP_Text CombatHitIconText { get { return combatHitIconText; } set { combatHitIconText = value; } }
    public Animator CombatIconAnimator { get { return combatIconAnimator; } set { combatIconAnimator = value; } }
    public UnitCombatUI UnitCombatUI { get { return unitCombatUI; } set { unitCombatUI = value; } }
    public bool IsPlayer { get { return isPlayer; } set { isPlayer = value; } }
    public MoveSO CurMove { get { return curMove; } set { curMove = value; } }
    public MoveSO SpecialMove { get { return specialMove; } set { specialMove = value; } }
    public bool InCombat { get { return inCombat; } set { inCombat = value; } }
    public InventorySO EquipmentList { get { return equipmentList; } set { equipmentList = value; } }
    public bool IsTalkTurn { get { return isTalkTurn; } set { isTalkTurn = value; } }
    public GridCombatSystem GridCombatSystem { get { return gridCombatSystem; } set { gridCombatSystem = value; } }
    public int TalkTurnCount { get { return talkTurnCount; } set { talkTurnCount = value; } }
    public List<Unit> SelectedTargets { get { return selectedTargets; } set { selectedTargets = value; } }
    public string Id { get { return id; } }
    public MoveSO Push { get { return push; } }
    public Vector3 NodeOffset { get { return nodeOffest; } }
    public GameObject CombatBase { get { return combatBase; } }
    public int CurLayer { get { return curLayer; } set { curLayer = value; } }
    public Node CurNode
    {
        get
        {
            return this.curNode;
        }
        set
        {
            if (curNode != null)
            {
                curNode.Walkable = true;
                curNode.Unit = null;
            }
            this.curNode = value;
            curNode.Unit = this;
        }
    }


    public String Title
    {
        get
        {
            if (title == "")
            {
                return this.transform.parent.name;
            }
            else
            {
                return title;
            }
        }
        set
        {
            title = value;
        }
    }




    /*public void OnDrawGizmos() {
if (path != null) {
for (int i = targetIndex; i < path.Length; i++) {
Gizmos.color = Color.black;
Gizmos.DrawCube(path[i], new Vector3(0.25f,0.25f,0.25f));
if (i == targetIndex) {
Gizmos.DrawLine(transform.position, path[i]);
}
else {
Gizmos.DrawLine(path[i - 1], path[i]);
}
}
}
}*/









}