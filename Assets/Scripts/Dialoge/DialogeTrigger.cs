using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.InputSystem;

public class DialogeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private TextAsset inkText;
    [SerializeField] private bool playerInRange;
    [SerializeField] Collider2D hitbox;
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private DialougeManager dialougeManager;
    private bool isContinue;

    private void Awake() {
        playerInRange = false;
        icon.SetActive(false);
        hitbox=GetComponent<CircleCollider2D>();
        playerInputManager = GameObject.Find("Managers").GetComponentInChildren<PlayerInputManager>();
        dialougeManager = GameObject.Find("Managers").GetComponentInChildren<DialougeManager>();
    }
    private void Update() {
        if (playerInRange && dialougeManager.IsPlaying==false) {
            
            icon.SetActive (true);
            
        }
        else {
            //dialougeManager.ExitDialougeMode();
            icon.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        
       if (collision.tag == "Player" && hitbox.IsTouching(collision)) {
            playerInRange = true;
       }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag == "Player" && !hitbox.IsTouching(collision)) {
            playerInRange = false;
        }

    }
}
