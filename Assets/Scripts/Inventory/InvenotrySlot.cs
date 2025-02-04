using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvenotrySlot : MonoBehaviour, IDropHandler, IPointerClickHandler 
    {

    [SerializeField] private Image image;
    [SerializeField] private Color selectedColor, defualtColor;
    public event Action<InvenotrySlot> onPointClick;

    public void Awkae() {
        Deselect();
    }
    public void Select() {
        image.color = selectedColor;
    }
    public void Deselect() {
        image.color = defualtColor;
    }
    public void OnDrop(PointerEventData eventData) {
        if(transform.childCount == 0) {
            InvenItem invenItem = eventData.pointerDrag.GetComponent<InvenItem>();
            Debug.Log("dropped on " + gameObject.name);
            invenItem.parentAfterDrag = transform;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(onPointClick != null) {
            Debug.Log("test " + gameObject.name);
           // GameObject parentIndex = gameObject.GetComponentInParent
            onPointClick.Invoke(this);
        }

    }
}
