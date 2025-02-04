using Inventory.Model;
using System;
using UnityEngine;
/*---------------------------------------------------------------------
*  Class: CharacterStats
*
*  Purpose: Record the statistics of a Unit or player
*-------------------------------------------------------------------*/
public class UnitStats : MonoBehaviour
{
    [SerializeField] private UnitStatsSO stats;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private HealthBar manaBar;
    private int tempSpeed, tempStrength, tempMagic, tempDurability, tempVitality, tempInteligence, tempCharisma;
    public event Action<UnitStats, float> onDamage, onHeal, onAddMana, onUseMana;
    private Unit unit;
    private int movesFrozen, curPosionDamage;

    private void Start()
    {
        stats.curHealth = stats.maxHealth;
        stats.curMP = stats.maxMP;
        stats.burnChance = 0;
        stats.freezeChance = 0;
        stats.posionChance = 0;
        stats.shockChance = 0;
        SetHealth();
        SetMana();
        healthBar.gameObject.SetActive(false);
        manaBar.gameObject.SetActive(false);
        unit = GetComponent<Unit>();

    }

    public void Damage(float damageNum)
    {
        if (stats.curHealth - damageNum <= 0)
        {
            stats.curHealth = 0;
        }
        else
        {
            stats.curHealth -= damageNum;
        }


        healthBar.SetSlider(stats.curHealth);

        if (onDamage != null)
        {
            onDamage.Invoke(this, damageNum);
        }
    }
    public UnitStatsData GetUnitStatData()
    {
        UnitStatsData curStats = new UnitStatsData(this);
        return curStats;
    }
    public UnitStatsData GetStatusInflictedStats()
    {
        UnitStatsData curStats = new UnitStatsData(this);
        if (stats.isBurned)
        {
            curStats.speed = (int)(curStats.speed / 2);
            curStats.charisma = (int)(curStats.charisma / 2);
            curStats.strength = (int)(curStats.strength / 1.5);
            curStats.durability = (int)(curStats.durability / 2);

        }
        if (stats.isShocked)
        {
            curStats.speed = (int)(curStats.speed * 1.5);
            curStats.charisma = (int)(curStats.charisma / 1.5);
            curStats.durability = (int)(curStats.durability / 2.5);

        }
        return curStats;
    }
    public void ClearStatusEffects()
    {
        Burn(false);
        Freeze(false);
        Posion(false);
        Shock(false);
    }
    /*---------------------------------------------------------------------
     *  Method: BurnUnit()
     *
     *  Purpose: Burns this unit by applying BurnUnit
     *           amount of damage until the curHeath is 1 
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void BurnUnit()
    {
        if (unit == null)
        {
            return;
        }
        if (unit.Stats == null)
        {
            return;
        }

        float burnDamage = ((int)unit.Stats.Maxhealth / 10);
        if (CurHealth > 1)
        {
            if (CurHealth - burnDamage <= 0)
            {
                burnDamage = CurHealth - 1;
            }
            unit.Stats.Damage(burnDamage);
            // plasy hit icon if in comabat
            if (unit.InCombat && unit.GridCombatSystem != null)
            {
                unit.StartCoroutine(unit.PlayAnimation("Attack", () =>
                {
                    String hitText = "Burned -" + (int)burnDamage;
                    unit.GridCombatSystem.PLayHitIcon(unit, hitText, () => { });
                }));
            }
        }
    }
    /*---------------------------------------------------------------------
     *  Method: ShockUnit()
     *
     *  Purpose: Shocks this unit by having a 1 in 4  chance of skipping thier turn    
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ShockUnit()
    {
        if (unit == null)
        {
            return;
        }
        if (unit.Stats == null)
        {
            return;
        }
        if (MakeRoll(1, 4))
        {
            // plasy hit icon if in comabat
            if (unit.InCombat && unit.GridCombatSystem != null)
            {
                unit.StartCoroutine(unit.PlayAnimation("Attack", () =>
                {
                    String hitText = "Shocked";
                    unit.GridCombatSystem.PLayHitIcon(unit, hitText, () =>
                    {
                        unit.GridCombatSystem.CompleteTurn(2);
                        unit.GridCombatSystem.AttackComplete = true;
                    });
                }));

            }
        }
    }
    /*---------------------------------------------------------------------
     *  Method: PosionUnit()
     *
     *  Purpose: Posions this unit by applying curPoisionDamage
     *           amount of damage until the curHea;th is 1 
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void PosionUnit()
    {
        if (unit == null)
        {
            return;
        }
        if (unit.Stats == null)
        {
            return;
        }
        float posionDamage = ((int)unit.Stats.Maxhealth / 15);
        if (CurHealth > 1)
        {
            if (CurHealth - posionDamage <= 0)
            {
                posionDamage = CurHealth - 1;
            }
            Damage(posionDamage);
            // plasy hit icon if in comabat
            if (unit.InCombat && unit.GridCombatSystem != null)
            {
                unit.StartCoroutine(unit.PlayAnimation("Attack", () =>
                {
                    String hitText = "Posioned -" + posionDamage;
                    unit.GridCombatSystem.PLayHitIcon(unit, hitText, () => { });
                }));

            }
        }
    }
    /*---------------------------------------------------------------------
     *  Method: FreezeUnit()
     *
     *  Purpose: Freezes this unit by skipping thier turn for a 3 turns     
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void FreezeUnit()
    {
        if (unit == null)
        {
            return;
        }
        if (unit.Stats == null)
        {
            return;
        }
        if (movesFrozen > 3)
        {
            if (MakeRoll(1, 10))
            {
                IsFrozen = false;
                if (unit.InCombat && unit.GridCombatSystem != null)
                {
                    // skips turns 3 times 
                    String hitText = "Unfrozen";
                    unit.StartCoroutine(unit.PlayAnimation("Attack", () =>
                    {

                        unit.GridCombatSystem.PLayHitIcon(unit, hitText, () => { });
                    }));
                }
            }
            // plasy hit icon if in comabat
            else if (unit.InCombat && unit.GridCombatSystem != null)
            {
                // skips turns 3 times 
                String hitText = "Frozen";
                unit.StartCoroutine(unit.PlayAnimation("Attack", () =>
                {

                    unit.GridCombatSystem.PLayHitIcon(unit, hitText, () =>
                    {
                        unit.GridCombatSystem.CompleteTurn(2);
                        unit.GridCombatSystem.AttackComplete = true;
                    });
                }));


            }
        }
        else
        {
            movesFrozen = 0;
            IsFrozen = false;
        }
    }
    public void Burn(bool val)
    {
        if (stats != null)
        {
            if (stats.isBurned == val)
            {
                return;
            }
            if (val == true)
            {
                ClearStatusEffects();
            }
            stats.curStatusEffect = UnitStatsSO.StausEffect.Burned;
            stats.isBurned = val;

        }
    }
    public void Shock(bool val)
    {
        if (stats != null)
        {
            stats.isShocked = val;
        }
        stats.curStatusEffect = UnitStatsSO.StausEffect.Shocked;
    }
    public void Freeze(bool val)
    {
        if (stats != null)
        {

            if (stats.isFrozen == val)
            {
                return;
            }
            if (val == true)
            {
                ClearStatusEffects();
            }
            stats.curStatusEffect = UnitStatsSO.StausEffect.Frozen;
            stats.isFrozen = val;
            movesFrozen = 0;

        }
    }
    public void Posion(bool val)
    {
        if (stats != null)
        {
            if (stats.isPosioned == val)
            {
                return;
            }
            if (val == true)
            {
                ClearStatusEffects();
            }
            stats.curStatusEffect = UnitStatsSO.StausEffect.Posioned;
            stats.isPosioned = val;

        }
    }

    public void Heal(float healNum)
    {
        float amountToHeal = (stats.maxHealth - stats.curHealth);
        if (healNum + stats.curHealth >= stats.maxHealth)
        {
            stats.curHealth = stats.maxHealth;
        }
        else
        {
            stats.curHealth += healNum;
        }


        healthBar.SetSlider(stats.curHealth);

        if (onHeal != null)
        {
            onHeal.Invoke(this, healNum);
        }
    }

    public void AddMana(float manaToAdd)
    {
        float amountToHeal = (stats.maxHealth - stats.curHealth);
        if (manaToAdd + stats.curMP >= stats.maxMP)
        {
            stats.curMP = stats.maxMP;
        }
        else
        {
            stats.curMP += manaToAdd;
        }

        if (manaBar != null)
        {
            manaBar.SetSlider(stats.curMP);
        }
        if (onAddMana != null)
        {
            onAddMana.Invoke(this, manaToAdd);
        }
    }

    public void UseMana(float manaCost)
    {
        if (stats.curMP - manaCost <= 0)
        {
            stats.curMP = 0;
        }
        else
        {
            stats.curMP -= manaCost;
        }
        if (manaBar != null)
        {
            manaBar.SetSlider(stats.curMP);
        }
        if (onUseMana != null)
        {
            onUseMana.Invoke(this, manaCost);
        }
    }
    public void SetHealth()
    {
        if (healthBar == null)
        {
            return;
        }
        healthBar.SetSliderMax(stats.maxHealth);
        healthBar.SetSlider(stats.curHealth);

    }
    public void SetMana()
    {
        if (manaBar == null)
        {
            return;
        }
        manaBar.SetSliderMax(stats.maxMP);
        manaBar.SetSlider(stats.curMP);
    }
    public void LoadUnitStatsData(UnitStatsData curData)
    {
        MaxMoveDistance = curData.maxMoveDistance;
        MaxAttackDistance = curData.maxAttackDistance;
        Speed = curData.speed;
        Strength = curData.strength;
        Magic = curData.magic;
        Durability = curData.durability;
        //ProfileImage = curData.profileImage;
        Intelligence = curData.intelligence;
        Vitality = curData.vitality;
        Charisma = curData.charisma;
        CurHealth = curData.curHealth;
        Maxhealth = curData.maxHealth;
        MaxMP = curData.maxMP;
        CurMP = curData.curMP;
        MaxExp = curData.maxExp;
        CurExp = curData.curExp;
        Level = curData.level;
        Money = curData.money;
        MaxWeight = curData.maxWeight;
        CurWeight = curData.curWeight;
        CaptureUnitRate = curData.captureUnitRate;
        BurnChance = curData.burnChance;
        FreezeChance = curData.freezeChance;
        PosionChance = curData.posionChance;
        ShockChance = curData.shockChance;
        BurnResistance = curData.burnResistance;
        FreezeResistance = curData.freezeResistance;
        PosionResistance = curData.posionResistance;
        ShockResistance = curData.shockResistance;
        IsBurned = curData.isBurned;
        IsFrozen = curData.isFrozen;
        CharmRate = curData.charmRate;
        IntimidateRate = curData.intimidateRate;
        TrickRate = curData.trickRate;
    }
    public bool MakeRoll(float hitChnce, int max)
    {
        int moveRoll = UnityEngine.Random.Range(1, max);
        if (moveRoll <= hitChnce)
        {
            return true;
        }
        return false;
    }

    public int MaxMoveDistance
    {
        get { return stats.maxMoveDistance; }
        set { stats.maxMoveDistance = value; }
    }
    public int MaxAttackDistance
    {
        get { return stats.maxAttackDistance; }
        set { stats.maxAttackDistance = value; }
    }

    public int Speed
    {
        get { return stats.speed; }
        set { stats.speed = value; }
    }

    public int Strength
    {
        get { return stats.strength; }
        set { stats.strength = value; }
    }

    public int Magic
    {
        get { return stats.magic; }
        set { stats.magic = value; }
    }

    public int Durability
    {
        get { return stats.durability; }
        set { stats.durability = value; }
    }
    public Sprite ProfileImage
    {
        get
        {
            return stats.profileImage;
        }
        set
        {

            stats.profileImage = value;
        }
    }

    public int Intelligence
    {
        get { return stats.intelligence; }
        set { stats.intelligence = value; }
    }

    public int Vitality
    {
        get { return stats.vitality; }
        set { stats.vitality = value; }
    }

    public int Charisma
    {
        get { return stats.charisma; }
        set { stats.charisma = value; }
    }

    public float CurHealth
    {
        get { return stats.curHealth; }
        set { stats.curHealth = value; }
    }

    public float Maxhealth
    {
        get { return stats.maxHealth; }
        set { stats.maxHealth = value; }
    }

    public float MaxMP
    {
        get { return stats.maxMP; }
        set { stats.maxMP = value; }
    }

    public float CurMP
    {
        get { return stats.curMP; }
        set { stats.curMP = value; }
    }

    public float MaxExp
    {
        get { return stats.maxExp; }
        set { stats.maxExp = value; }
    }

    public float CurExp
    {
        get { return stats.curExp; }
        set { stats.curExp = value; }
    }

    public int Level
    {
        get { return stats.level; }
        set { stats.level = value; }
    }

    public int Money { get { return stats.money; } set { stats.money = value; } }

    public float MaxWeight { get { return stats.maxWeight; } set { stats.maxWeight = value; } }
    public float CurWeight { get { return stats.curWeight; } set { stats.curWeight = value; } }

    public int CaptureUnitRate { get { return stats.captureUnitRate; } set { stats.captureUnitRate = value; } }
    public float BurnChance { get { return stats.burnChance; } set { stats.burnChance = value; } }
    public float FreezeChance { get { return stats.freezeChance; } set { stats.freezeChance = value; } }
    public float PosionChance { get { return stats.posionChance; } set { stats.posionChance = value; } }
    public float ShockChance { get { return stats.shockChance; } set { stats.shockChance = value; } }
    public float BurnResistance { get { return stats.burnResistance; } set { stats.burnResistance = value; } }
    public float FreezeResistance { get { return stats.freezeResistance; } set { stats.freezeResistance = value; } }
    public float ShockResistance { get { return stats.shockResistance; } set { stats.shockResistance = value; } }
    public float PosionResistance { get { return stats.posionResistance; } set { stats.posionResistance = value; } }
    public bool IsBurned { get { return stats.isBurned; } set { stats.isBurned = value; } }
    public bool IsFrozen { get { return stats.isFrozen; } set { stats.isFrozen = value; } }
    public bool IsShocked { get { return stats.isShocked; } set { stats.isShocked = value; } }
    public bool IsPosioned { get { return stats.isPosioned; } set { stats.isPosioned = value; } }
    public int CharmRate { get { return stats.charmRate; } set { stats.charmRate = value; } }
    public int IntimidateRate { get { return stats.intelligence; } set { stats.intelligence = value; } }
    public int TrickRate { get { return stats.trickRate; } set { stats.trickRate = value; } }
    public UnitStatsSO.StausEffect CurStatusEffect { get { return stats.curStatusEffect; } }
    public ModifierData ComboModifer { get { return stats.comboModifier; } }
    public MoveSO SpecialMove { get { return stats.specialMove; } }

    public HealthBar HealthBar
    {
        get
        {
            return healthBar;
        }
        set
        {
            healthBar = value;
        }
    }
    public HealthBar ManaBar
    {
        get
        {
            return manaBar;
        }
        set
        {
            manaBar = value;
        }
    }

}
