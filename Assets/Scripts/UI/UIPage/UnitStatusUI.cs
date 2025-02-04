using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
public class UnitStatusUI : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private HealthBar manaBar;
    private float curHealth;
    private float curMana;
    private Unit curUnit;
    public void Activate(bool isActive) {
        if (isActive) {
            this.gameObject.SetActive(true);
        }
        else {
            this.gameObject.SetActive(false);
        }
    }
    public void Damage(float damageNum) {
        if(curUnit == null) {
            return;
        }
        if (curUnit.Stats == null) {
            return;
        }
        healthBar.SetSlider(curUnit.Stats.CurHealth);
        
    }

    public void Heal(float healNum) {
        if (curUnit == null) {
            return;
        }
        if (curUnit.Stats == null) {
            return;
        }
        healthBar.SetSlider(curUnit.Stats.CurHealth);
    }

    public void SetHealth() {
        if (curUnit == null) {
            return;
        }
        if (curUnit.Stats == null) {
            return;
        }
        healthBar.SetSliderMax(curUnit.Stats.Maxhealth);
        healthBar.SetSlider(curUnit.Stats.CurHealth);
        
    }
    public void SetMana() {
        if (curUnit == null) {
            return;
        }
        if (curUnit.Stats == null) {
            return;
        }
        manaBar.SetSliderMax(curUnit.Stats.MaxMP);
        manaBar.SetSlider(curUnit.Stats.CurMP);
    }
    public void AssignUnit(Unit unit) {
        curUnit = unit;
        SetHealth();
        SetMana();
    }
    public Image ProfileImage { get { return profileImage; } }

    public HealthBar HealthBar { get {  return healthBar; } }
    public HealthBar ManaBar { get { return manaBar; } }
    public Unit CurUnit { get { return curUnit; } set { curUnit = value; } }
    
}
