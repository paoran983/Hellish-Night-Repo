using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour {
    [SerializeField] private PlayerInput playerInput;
    private InputAction move;
    private InputAction isInteract;
    private InputAction isInventory;
    private InputAction continueDialouge;
    private InputAction takeAllLoot;
    private InputAction back;
    private InputAction submit;
    private bool isInventoryMenuOpen;
    private bool isIneracting, isContinue;
    private bool didTakeAllLoot;
    private bool isBack;
    private Vector2 noveVector;
    [SerializeField] private PlayerController controller;
    private void Awake() {
        controller = GameObject.Find("Player").GetComponentInChildren<PlayerController>();
        isInventoryMenuOpen = false;
        isIneracting = false;
        didTakeAllLoot = false;
        isBack = false;
    }

    private void OnEnable() {
        move = playerInput.actions["Move"];
        move.Enable();
        isInteract = playerInput.actions["Interact"];
        isInteract.Enable();
        isInteract.performed += Interact;
        isInteract.canceled += Interact;
        isInventory = playerInput.actions["InventoryMenu"];
        isInventory.performed += InventoryMenu;
        isInventory.Enable();
        continueDialouge = playerInput.actions["Submit"];
        continueDialouge.performed += ContinueDialoge;
        continueDialouge.canceled += ContinueDialoge;
        continueDialouge.Enable();
        takeAllLoot = playerInput.actions["TakeAllLoot"];
        takeAllLoot.performed += HandleTakeAllLoot;
        takeAllLoot.canceled += HandleTakeAllLoot;
        takeAllLoot.Enable();
        back = playerInput.actions["Back"];
        back.performed += HandleBack;
        back.canceled += HandleBack;
        back.Enable();
        submit = playerInput.actions["Submit"];
        submit.Enable();

    }

    private void HandleBack(InputAction.CallbackContext context) {
        if (controller.CurState == controller.CombatState) {
            if (context.performed) {
                if (isBack == false) {
                    isBack = true;
                }

                //isBack = false;
            }
            else if (context.canceled) {
                isBack = false;
            }
        }
    }

    private void HandleTakeAllLoot(InputAction.CallbackContext context) {
        if (controller.CurState == controller.InteractingState) {
            if (context.performed) {
                didTakeAllLoot = true;
            }
            else if (context.canceled) {
                didTakeAllLoot = false;
            }
        }
    }

    private void ContinueDialoge(InputAction.CallbackContext context) {
        if (context.performed) {
            isContinue = true;
        }
        else if (context.canceled) {
            isContinue = false;
        }

    }

    private void InventoryMenu(InputAction.CallbackContext context) {

        if (context.performed) {
            if (isIneracting) {
                isInventoryMenuOpen = false;
            }
            else {
                isInventoryMenuOpen = !isInventoryMenuOpen;
            }

        }
    }

    private void Interact(InputAction.CallbackContext context) {
        if (context.performed) {
            if (controller.PlayerInteract.CheckForInteractable() != null) {
                isIneracting = !isIneracting;
                //Debug.Log("interacting...........");
            }
            else {
                isIneracting = false;
            }
        }
    }

    private void OnDisable() {
        move.Disable();
        //isInventory.Disable();
        isInteract.Disable();
    }
    public Vector2 Move {
        get {
            return move.ReadValue<Vector2>();
        }
    }
    public Vector2 MoveNormalized {

        get {
            int x = 0;
            int y = 0;
            if (Move.x == 0) {
                x = 0;
            }
            if (Move.x < 0) {
                x = -1;
            }
            if (Move.x > 0) {
                x = 1;
            }
            if (Move.y == 0) {
                y = 0;
            }
            if (Move.y < 0) {
                y = -1;
            }
            if (Move.y > 0) {
                y = 1;
            }
            return new Vector2(x, y);
        }
    }
    public InputAction MoveAction { get { return move; } }
    public InputAction SubmitAction { get { return submit; } }

    public bool IsInteracting {
        get {
            bool res = isIneracting;
            isContinue = false;
            return res;
        }
        set { isIneracting = value; }
    }
    public bool IsInventoryMenuOpen {
        get {
            bool res = isInventoryMenuOpen;
            isContinue = false;
            return res;
        }
    }
    public bool IsContinue {
        get {
            bool res = isContinue;
            isContinue = false;
            return res;
        }
    }
    public bool TakeAllLoot {
        get { return didTakeAllLoot; }
    }
    public bool IsBack {
        get { return isBack; }
    }
    public PlayerInput PlayerInput { get { return playerInput; } }
}
