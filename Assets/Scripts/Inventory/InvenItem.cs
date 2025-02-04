using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Inventory.Model;
using TMPro;
using System;

public class InvenItem : MonoBehaviour,IBeginDragHandler, IEndDragHandler,IDragHandler
{
    public Image image;
    public Transform parentAfterDrag;
    public ItemSO item;
    public int count = 1;
    public TMP_Text countText;
    public event Action<InvenotrySlot> onBeginDragItem,onEndDragItem;

    public void Start() {
       InitialiseItem(item);

        
    }
    public void InitialiseItem(ItemSO item) {
        image.sprite = item.GetImage();
    }
    public void incrementCount(int amountToAdd) {
        count += amountToAdd;
    }
    public void RefreshCount() {
        countText.text = count.ToString();
    }
    public void OnBeginDrag(PointerEventData eventData) {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        if (onBeginDragItem != null) {
            Debug.Log("test drag " + gameObject.name);
            // GameObject parentIndex = gameObject.GetComponentInParent
            onBeginDragItem.Invoke(parentAfterDrag.GetComponent<InvenotrySlot>());
        }

    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        if (onEndDragItem != null) {
            Debug.Log("test end drag " + gameObject.name);
            // GameObject parentIndex = gameObject.GetComponentInParent
            onEndDragItem.Invoke(parentAfterDrag.GetComponent<InvenotrySlot>());
        }
    }

 
}
