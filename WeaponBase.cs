//Contract
public abstract class WeaponBase
{
    public abstract float GetDamage();
    public abstract float GetRange();
    public abstract float GetTimeBetweenAttacks();
    public abstract string GetWeaponName();
}

//concrete Base Weapon class
class BroadswordWeapon : WeaponBase
{
    protected string WeaponType = "Broadsword";
    protected float TimeBetweenAttacks = 1.0f;
    protected float AttackDamage = 5.0f;
    protected float Range = 2.0f;

    public override float GetDamage()
    {
        return AttackDamage;
    }

    public override float GetRange()
    {
        return Range;
    }

    public override float GetTimeBetweenAttacks()
    {
        return TimeBetweenAttacks;
    }

    public override string GetWeaponName()
    {
        return WeaponType;
    }
}

class MaceWeapon : WeaponBase
{
    protected string WeaponType = "Mace";
    protected float TimeBetweenAttacks = 1.0f;
    protected float AttackDamage = 8.0f;
    protected float Range = 1.5f;

    public override float GetDamage()
    {
        return AttackDamage;
    }

    public override float GetRange()
    {
        return Range;
    }

    public override float GetTimeBetweenAttacks()
    {
        return TimeBetweenAttacks;
    }

    public override string GetWeaponName()
    {
        return WeaponType;
    }
}

//decorator base
public abstract class SwordDecorator : WeaponBase
{
    protected WeaponBase BaseWeapon;

    public SwordDecorator(WeaponBase weaponBase)
    {
        BaseWeapon = weaponBase;
    }

    public override float GetDamage()
    {
        return BaseWeapon.GetDamage();
    }

    public override float GetRange()
    {
        return BaseWeapon.GetRange();
    }

    public override float GetTimeBetweenAttacks()
    {
        return BaseWeapon.GetTimeBetweenAttacks();
    }

    public override string GetWeaponName()
    {
        return BaseWeapon.GetWeaponName();
    }
}

public class FlamingDec : SwordDecorator
{
    float RangeModifier;
    float DamageModifier = 2.0f;
    float AttackSpeedModifier;
    string WeaponNameModifier = "Flaming ";

    public FlamingDec(WeaponBase weaponBase) : base(weaponBase)
    {

    }

    public override float GetDamage()
    {
        return BaseWeapon.GetDamage() + DamageModifier;
    }

    public override float GetRange()
    {
        return BaseWeapon.GetRange() + RangeModifier;
    }

    public override float GetTimeBetweenAttacks()
    {
        return BaseWeapon.GetTimeBetweenAttacks() + AttackSpeedModifier;
    }

    public override string GetWeaponName()
    {
        return WeaponNameModifier + BaseWeapon.GetWeaponName();
    }
}

public class ElectricDec : SwordDecorator
{
    float RangeModifier;
    float DamageModifier = 2.0f;
    float AttackSpeedModifier = -0.5f;
    string WeaponNameModifier = "Electric ";

    public ElectricDec(WeaponBase weaponBase) : base(weaponBase)
    {

    }

    public override float GetDamage()
    {
        return BaseWeapon.GetDamage() + DamageModifier;
    }

    public override float GetRange()
    {
        return BaseWeapon.GetRange() + RangeModifier;
    }

    public override float GetTimeBetweenAttacks()
    {
        return BaseWeapon.GetTimeBetweenAttacks() + AttackSpeedModifier;
    }

    public override string GetWeaponName()
    {
        return WeaponNameModifier + BaseWeapon.GetWeaponName();
    }
}

public class FastDec : SwordDecorator
{
    float RangeModifier;
    float DamageModifier;
    float AttackSpeedModifier = -0.5f;
    string WeaponNameModifier = "Fast ";

    public FastDec(WeaponBase weaponBase) : base(weaponBase)
    {

    }

    public override float GetDamage()
    {
        return BaseWeapon.GetDamage() + DamageModifier;
    }

    public override float GetRange()
    {
        return BaseWeapon.GetRange() + RangeModifier;
    }

    public override float GetTimeBetweenAttacks()
    {
        return BaseWeapon.GetTimeBetweenAttacks() + AttackSpeedModifier;
    }

    public override string GetWeaponName()
    {
        return WeaponNameModifier + BaseWeapon.GetWeaponName();
    }
}
