using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.UI {
    public class ItemActionMenu : MonoBehaviour {
        [SerializeField] private GameObject buttonPrefab;
        /*---------------------------------------------------------------------
        *  Method AddButon(string name, Action onClickAction)
        *
        *  Purpose: adds buttons to item action menu 
        *           formats it and adds listener for mouse clicks
        *
        *   Parameters: string name = name of button 
        *               Action onClickAction = what to do when clicked
        *
        *  Returns: none
        *-------------------------------------------------------------------*/
        public void AddButon(string name, Action onClickAction) {
            GameObject button = Instantiate(buttonPrefab, transform);
            button.GetComponent<Button>().onClick.AddListener(() => onClickAction());
            button.GetComponentInChildren<TMPro.TMP_Text>().text = name;
        }

        public void Toggle(bool val) {
            if (val == true)
                RemoveOldButtons();
            gameObject.SetActive(val);
        }

        public void RemoveOldButtons() {
            foreach (Transform transformChildObjects in transform) {
                Destroy(transformChildObjects.gameObject);
            }
        }
    }
}
