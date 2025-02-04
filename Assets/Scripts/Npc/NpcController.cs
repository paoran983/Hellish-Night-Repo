using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : Controller
{

    private bool isBeingMoved, isAttacking;
    //private InMenuState menuState = new InMenuState();
    private IdleNpcState idleState = new IdleNpcState();
   // private WalkState walkState = new WalkState();
    private CombatState combatState = new CombatState();
    private DialougeState dialougeState = new DialougeState();
   // private InteractingState interactingState = new InteractingState();
    private BaseNpcState baseNpcState = new BaseNpcState();
    [SerializeField] private Unit unit;
    private string curStateRef;
    public void Start() {
        unit = GetComponent<Unit>();
  
        curState = baseNpcState;
        baseNpcState.InitializeNpcState(unit, this);
        baseState = baseNpcState;
        combatState.Initialize(unit, this);
        dialougeState.Initialize(unit, this);
        idleState.InitializeNpcState(unit, this);
        curState.EnterState();
 
    }
    public void Update() {
        if (curState!=null)
        {
            curState.UpdateAllStates();
        }
    }

    public void FixedUpdate() {
        if (curState != null) {
            curState.FixedUpdateAllStates();
        }
    }
    public bool IsBeingMoved {
        get {
            return isBeingMoved;
        }
        set {
            isBeingMoved = value;
        }
    }
    public bool IsAttacking {
        get {
            return isAttacking;
        }
        set {
            isAttacking = value;
        }
    }

    public void AnimateInDir(float x, float y) {
      //  playerAnimator.SetFloat("moveX", x);
       // playerAnimator.SetFloat("moveY", y);
    }
    public void AnimateMoving(bool isMoving) {
       // playerAnimator.SetBool("isMoving", isMoving);
    }

    public IEnumerator Attack() {
        //throw new System.NotImplementedException();
        yield return null;
    }

    // Start is called before the first frame update

    // Update is called once per frame


    public IdleNpcState IdleState { get { return idleState; } }
    public CombatState CombatState { get { return combatState; } } 
   
}
