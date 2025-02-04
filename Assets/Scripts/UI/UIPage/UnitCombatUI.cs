using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class UnitCombatUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private Unit unit;
    [SerializeField] private UnitStats stats;
    [SerializeField] private GameObject unitBase;
    [SerializeField] private Vector3 unitOffest;

    public event Action<UnitCombatUI> onUnitClicked,
           onPointerEnter, onPointerExit;
    public void Awake()
    {
        this.unit = GetComponent<Unit>();
        this.stats = GetComponent<UnitStats>();
        ActivateBase(false);
        //  unitBase.transform.position = unit.transform.position + unitOffest;
    }
    public void ActivateBase(bool isActive)
    {
        if (isActive)
        {
            unitBase.SetActive(true);
        }
        else
        {
            unitBase.SetActive(false);
        }
    }
    public void ChangeBaseColor(Color baseColor)
    {
        SpriteRenderer baseSprite = unitBase.GetComponent<SpriteRenderer>();
        if (baseSprite == null)
        {
            return;
        }
        baseSprite.color = baseColor;
    }

    public void SelectUnitForTurn()
    {
        SpriteRenderer baseSprite = unitBase.GetComponent<SpriteRenderer>();
        if (baseSprite == null)
        {
            return;
        }
        baseSprite.color = Color.white;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (onUnitClicked != null)
        {
            onUnitClicked.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
    private void OnMouseExit()
    {
        if (onPointerExit != null)
        {
            onPointerExit.Invoke(this);
        }
    }
    private void OnMouseOver()
    {
        if (onPointerEnter != null)
        {
            onPointerEnter.Invoke(this);
        }
    }
    public Unit Unit { get { return unit; } }
    public UnitStats Stats { get { return stats; } }
    public GameObject UnitBase { get { return unitBase; } }
}
