using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Interactable : MonoBehaviour {
    public string promptMessage;
    protected bool isActive = false;
    protected GameObject interactedWith;

    // will be called from our player
    public void BaseInteract(GameObject interactWith) {
        Interact(interactWith);
        this.interactedWith = interactWith;
    }

    protected virtual void Interact(GameObject interactWith) {

    }
    public bool IsActive {
        get {
            return isActive;
        }
        set {
            isActive = value;
        }
             
    }
    public void Activate() {
        isActive = true;
    }
    public virtual void Deactivate() { isActive = false;}

}
