using Inventory;
using Inventory.Model;
using System;
using UnityEngine;
//using UnityEngine.InputSystem;

/*---------------------------------------------------------------------
 *  Class: GridCombatSystem
 *
 *  Purpose: Controlls the player character based on player inputs
 *-------------------------------------------------------------------*/
public abstract class Controller : MonoBehaviour {
    protected StateItem baseState;
    protected StateItem curState;
    public StateItem BaseState { get { return baseState; } set { baseState = value; } }
    public StateItem CurState { get { return curState; } set { curState = value; } }

}
public class PlayerController : Controller {
    [SerializeField] private float moveSpeed;
    private Vector2 forceToAplly;
    [SerializeField] private float forceDamping;
    private Vector2 playerInput;
    private Vector2 moveForce, move;
    [SerializeField] private bool inCombat;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private LayerMask solidObjectLayer;
    [SerializeField] private bool isMoving;
    [SerializeField] bool takeAllLoot;
    [SerializeField] bool isBack;
    public HealthSystem healthSystem;
    public Rigidbody2D rb;
    private UnitStats characterStats;
    [SerializeField] private InventoryController inventoryController;

    [SerializeField] private PlayerInputManager playerInputManager;
    private bool isInventoryMenuOpen;
    [SerializeField] private bool isIneracting;
    [SerializeField] private bool isCaptureing;
    private bool isContinue;
    private PlayerInteract playerInteract;
    [SerializeField] private float moveToSpeed, diagGridSPeedOffset;
    public float timer;
    [SerializeField] private float arrivedOffset;
    [SerializeField] private float goneToFar;
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private float interactRange;
    [SerializeField] private LootingUIController lootUIController;
    [SerializeField] private ShopPageUIController shopPageUIController;
    private bool isAttacking;
    [SerializeField] private Unit unit;
    [Header("state machine")]
    private InMenuPlayerState menuState = new InMenuPlayerState();
    private IdlePlayerState idleState = new IdlePlayerState();
    private WalkPlayerState walkState = new WalkPlayerState();
    private CombatPlayerState combatState = new CombatPlayerState();
    private DialougeState dialougeState = new DialougeState();
    private InteractingPlayerState interactingState = new InteractingPlayerState();
    private BasePlayerState basePlayerState = new BasePlayerState();
    [SerializeField] string curStateRef;
    [SerializeField] private UnitSceneLocation playerInitalSecnePosData;

    public void Awake() {
        playerAnimator = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody2D>();
        // lootUIController = GetComponent<LootingUIController>();
        // shopPageUIController = GetComponent<ShopPageUIController>();
        characterStats = GetComponentInChildren<UnitStats>();
        moveSpeed = 5;
        forceDamping = 1.2f;
        healthSystem = new HealthSystem(100);
        isInventoryMenuOpen = false;
        inventoryController = GetComponent<InventoryController>();
        playerInteract = GetComponentInChildren<PlayerInteract>();
        isAttacking = false;
        unit = GetComponent<Unit>();
        isMoving = false;
        inCombat = false;
        unit.IsPlayer = true;
        curState = basePlayerState;
        basePlayerState.InitializePlayerState(unit, this);
        baseState = basePlayerState;
        walkState.InitializePlayerState(unit, this);
        interactingState.InitializePlayerState(unit, this);
        combatState.InitializePlayerState(unit, this);
        dialougeState.Initialize(unit, this);
        menuState.InitializePlayerState(unit, this);
        idleState.InitializePlayerState(unit, this);
        curState.EnterState();
        if (playerInitalSecnePosData != null) {
            // Debug.Log("moving to " + new Vector3(playerInitalSecnePosData.initalValue.x, playerInitalSecnePosData.initalValue.y, 0));
            transform.position = new Vector3(playerInitalSecnePosData.initalValue.x, playerInitalSecnePosData.initalValue.y, 0);
        }
    }
    public void Update() {
        if (curStateRef != null && curState != null) {
            curStateRef = curState.ToString();
        }
        isIneracting = playerInputManager.IsInteracting;
        isInventoryMenuOpen = playerInputManager.IsInventoryMenuOpen;
        isContinue = playerInputManager.IsContinue;
        takeAllLoot = playerInputManager.TakeAllLoot;
        isBack = playerInputManager.IsBack;
        if (curState != null) {
            curState.UpdateAllStates();
        }

    }
    public void FixedUpdate() {
        move = playerInputManager.Move;
        if (curState != null) {
            curState.FixedUpdateAllStates();
        }

    }


    public void InventoryMenu() {
        inventoryController.ActivateUI(isInventoryMenuOpen);
    }
    public void ActivateInventoryMemnu() {
        inventoryController.ActivateUI(true); ;
    }
    public void DeactivateInventoryMemnu() {
        inventoryController.ActivateUI(false); ;
    }

    public static int Round(float value) {
        if (value > 0) {
            return (int)Mathf.Ceil(value);
        }
        else {
            return (int)Mathf.Floor(value);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: MovePlayer(Vector2 move)
     *
     *  Purpose: Moves player in the direction of the Vector2 move
     *           It does this by applying force to the player in the direction of move.
     *           Then having the animation match that direaction
     *
     *  Parameters: Vector2 move = the direction the player will move in
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void MovePlayer(Vector2 move) {

        // normalize player input so dialoganal is same spped as vert and horiziontal
        playerInput = (move * Time.deltaTime).normalized;

        // applies current moveSpeed
        moveForce = (playerInput * moveSpeed);
        // allows for knockback from other physics obejcts
        moveForce += forceToAplly;
        forceToAplly /= forceDamping;
        if (Mathf.Abs(forceToAplly.x) <= 0.01f && Mathf.Abs(forceToAplly.y) <= 0.0f) {
            forceToAplly = Vector2.zero;
        }

        rb.velocity = moveForce;
        // determines if the player is moving or not
        if (rb.velocity != Vector2.zero) {
            //gets input values for idle animation direction
            playerAnimator.SetFloat("moveX", move.x);
            playerAnimator.SetFloat("moveY", move.y);

            isMoving = true;

        }
        else {
            isMoving = false;
        }
        playerAnimator.SetBool("isMoving", isMoving);
    }//MovePlayer(Vector2 move)

    public void AnimateInDir(float x, float y) {
        playerAnimator.SetFloat("moveX", x);
        playerAnimator.SetFloat("moveY", y);
    }
    public void AnimateMoving(bool isMoving) {
        playerAnimator.SetBool("isMoving", isMoving);
    }
    public void OnCollisionEnter2D(Collision2D collision) {
    }
    public void OnTriggerEnter2D(Collider2D collision) {
        // Debug.Log(collision.gameObject.name+" layer = "+collision.layerOverridePriority);
        if (collision.gameObject.tag == "Healing") {
            Debug.Log("--Heal");
            characterStats.Heal(10);
        }
        else if (collision.gameObject.tag == "Damaging") {
            Debug.Log("---Damaging");
            characterStats.Damage(10);
        }
        ItemPickUp item = collision.GetComponent<ItemPickUp>();
        if (item != null) {
            // adds item and gets the amount of left over item(s) if 
            // couldnt be added
            if (item.GetInventoryItem().type == ItemSO.Type.Money) {
                characterStats.Money += item.GetCount();
            }
            int reminder = unit.Inventory.AddItem(item.GetInventoryItem(), item.GetCount());
            // if none are left it destroys object
            if (reminder == 0)
                item.DestroyItem();
            // if any left it updates the count
            else
                item.SetCount(reminder);
        }

    }
    public float GetCaptureRate(Unit targetUnit, float captureItemRate) {
        if (targetUnit == null) {
            return 0;
        }
        int captureUnitRate = targetUnit.Stats.CaptureUnitRate;
        float targetMaxHp = targetUnit.Stats.Maxhealth;
        float targetCurHp = targetUnit.Stats.CurHealth;
        float statusRate = 1;
        float captureRate = (((1 + (targetMaxHp * 3 - targetCurHp * 2) * captureUnitRate * captureItemRate * statusRate) / (targetMaxHp * 3)) / 256) * 100;
        return captureRate;
    }

    public void CaptureUnit(Unit targetUnit, int captureItemRate, Action onAfterCaught, Action onAfterGotAway) {
        if (targetUnit == null) {
            return;
        }
        if (targetUnit.Stats == null) {
            return;
        }
        float captureRate = GetCaptureRate(targetUnit, captureItemRate);
        int moveRoll = UnityEngine.Random.Range(1, 100);
        if (moveRoll <= captureRate) {
            Debug.Log(targetUnit.name + " was caught!! with chance of " + captureRate + " and roll of " + moveRoll);
            onAfterCaught();
            return;
        }
        else {
            Debug.Log("shoot they got away!! with chance of " + captureRate + " and roll of " + moveRoll);
            onAfterGotAway();
            return;
        }

    }
    public bool GetIsInentoryOpen() {
        return isInventoryMenuOpen;
    }

    public void StartCapture() {
        isCaptureing = true;
    }

    /*---------------------------------------------------------------------
     *  Getters and setters
     *-------------------------------------------------------------------*/
    public Vector2 Move { get { return move; } }
    public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
    public bool IsInteracting { get { return isIneracting; } set { playerInputManager.IsInteracting = value; } }
    public Unit Unit { get { return unit; } set { unit = value; } }
    public bool InCombat { get { return inCombat; } set { inCombat = value; } }
    public bool IsInventoryMenuOpen { get { return isInventoryMenuOpen; } }
    public bool TakeAllLoot { get { return takeAllLoot; } }
    public InteractingPlayerState InteractingState { get { return interactingState; } }
    public IdlePlayerState IdleState { get { return idleState; } }
    public PlayerInteract PlayerInteract { get { return playerInteract; } }
    public CombatPlayerState CombatState { get { return combatState; } }
    public InMenuPlayerState MenuState { get { return menuState; } }
    public DialougeState DialougeState { get { return dialougeState; } }
    public WalkPlayerState WalkState { get { return walkState; } }
    public BasePlayerState BasePlayerState { get { return basePlayerState; } }
    public PlayerInputManager PlayerInputManager { get { return playerInputManager; } }
    public LootingUIController LootUIController { get { return lootUIController; } }
    public ShopPageUIController ShopPageUIController { get { return shopPageUIController; } }
    public bool IsBack { get { return isBack; } set { isBack = value; } }
    public bool IsCaptureing { get { return isCaptureing; } set { isCaptureing = value; } }

    public DialougeManager DialougeManager { get { return dialougeManager; } }
}
