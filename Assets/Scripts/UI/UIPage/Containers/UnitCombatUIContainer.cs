using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UnitCombatUIContainer : MonoBehaviour
{
    [SerializeField] private List<UnitCombatUI> unitsCombatUI;
    private List<Unit> units;
    [SerializeField] private Color enemyColor;
    [SerializeField] private Color friendlyColor;
    [SerializeField] private TMP_Text hitChanceText;
    public event Action<UnitCombatUI> onUnitClicked,
       onPointerEnter, onPointerExit;
    private void Start() {

        hitChanceText = GetComponent<TMP_Text>();
        Activate(false);
    }
    public void InitateForCombat(List<Unit> unitList) {
        this.units = unitList;
        unitsCombatUI= new List<UnitCombatUI>();
        foreach (Unit unit in units) {
            if (unit.UnitCombatUI != null) {
                unit.UnitCombatUI.onPointerEnter += HandleUnitPointerEnter;
                unit.UnitCombatUI.onPointerExit += HandleUnitPointerExit;
                unit.UnitCombatUI.onUnitClicked += HandleUnitPointerClick;
                unitsCombatUI.Add(unit.UnitCombatUI);
            }
        }
    }
    public void Activate(bool isActive) {
        if(isActive) {
            gameObject.SetActive(true);
        }
        else {
            gameObject.SetActive(false);
        }
    }

    private void HandleUnitPointerClick(UnitCombatUI uI) {
        if(onUnitClicked != null) {
            onUnitClicked.Invoke(uI);
        }
    }

    private void HandleUnitPointerExit(UnitCombatUI uI) {
        if(onPointerExit != null) {
            onPointerExit.Invoke(uI);
        }
    }

    private void HandleUnitPointerEnter(UnitCombatUI uI) {
        if (onPointerEnter != null) {
            onPointerEnter.Invoke(uI);
        }
    }
    

    public List<UnitCombatUI> UnitsList { get { return unitsCombatUI; } set { unitsCombatUI = value; } }
    public TMP_Text HitChanceText { get { return hitChanceText; } set { hitChanceText = value; } }


}
