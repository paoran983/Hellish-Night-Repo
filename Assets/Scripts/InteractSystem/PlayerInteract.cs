using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    private Interactable curInteracting;
    [SerializeField] private float interactRange;
    [SerializeField]  private bool isFound;
    [SerializeField] private CircleCollider2D circleCollider;
    private void Awake() {
        playerInputManager = GetComponent<PlayerInputManager>();
    }
    public void ActivateInteract() {
        Collider2D[] colliderList = Physics2D.OverlapCircleAll(transform.position, interactRange);
        isFound = false;
        foreach (Collider2D collider in colliderList) {
            //Debug.Log(collider.gameObject.name);
            if (collider.GetComponent<Interactable>() != null) {
                isFound = true;
                
                // gets first interactable available if possible
                if (curInteracting == null) {

                    curInteracting = collider.GetComponent<Interactable>();
                   // Debug.Log(" --- interacting with " + curInteracting.gameObject.name);
                }
                // gets the nearest interacble if possible
                else if (Vector3.Distance(collider.GetComponent<Interactable>().transform.position, transform.position) <
                    Vector3.Distance(curInteracting.transform.position, transform.position)) {

                    curInteracting = collider.GetComponent<Interactable>();
                    //Debug.Log(" --- interacting with " + curInteracting.gameObject.name);
                }

            }
            
        }
        if (isFound==false) {
            curInteracting = null;
        }
    }
    public Interactable CheckForInteractable() {
        Collider2D[] colliderList = Physics2D.OverlapCircleAll(transform.position, interactRange);
        isFound = false;
        foreach (Collider2D collider in colliderList) {
            //Debug.Log(collider.gameObject.name);
            if (collider.GetComponent<Interactable>() != null) {
                isFound = true;

                // gets first interactable available if possible
                if (curInteracting == null) {

                    curInteracting = collider.GetComponent<Interactable>();
                    // Debug.Log(" --- interacting with " + curInteracting.gameObject.name);
                }
                // gets the nearest interacble if possible
                else if (Vector3.Distance(collider.GetComponent<Interactable>().transform.position, transform.position) <
                    Vector3.Distance(curInteracting.transform.position, transform.position)) {

                    curInteracting = collider.GetComponent<Interactable>();
                    //Debug.Log(" --- interacting with " + curInteracting.gameObject.name);
                }

            }

        }
        if (isFound == false) {
            curInteracting = null;
        }
        return curInteracting;
    }

    public void StartInteraction() {
        if(curInteracting!= null) {
            curInteracting.BaseInteract(this.gameObject);
        }
    }
    public void Deactivate() {
        if (curInteracting != null) {
           
            curInteracting.Deactivate();
        }
    }
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
    //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position,interactRange);
    }

    public Interactable CurInteracting { get { return curInteracting; } }
}
