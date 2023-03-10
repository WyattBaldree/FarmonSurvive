using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perk : IComparable<Perk>
{
    // Static
    public static Dictionary<string, Perk> perkDictionary = new Dictionary<string, Perk>();

    public static void CreatePerks()
    {
        perkDictionary.Clear();

        //Create a list containing an instance of each of the classes that inherits from Perk.
        //Then interate through that list and call the Add method on each.
        foreach(Perk perk in H.GetEnumerableOfType<Perk>())
        {
            perk.Add();
        }
    }

    // Instance
    public string PerkName = "Generic Perk";
    public string[] PerkDescription = { "Generic description." };
    public int MaxLevel = 1;

    public void Add()
    {
        perkDictionary.Add(PerkName, this);
    }

    public int CompareTo(Perk other)
    {
        return String.Compare(ToString(), other.ToString());
    }
}

//Generic perks
public class PerkJump : Perk
{
    public PerkJump()
    {
        PerkName = "Jump";
        PerkDescription = new string[]{
            "Jump up 1 block high ledges!",
            "Jump up 2 block high ledges!"};
        MaxLevel = 2;
    }
}

public class PerkFrenzy : Perk
{
    public PerkFrenzy()
    {
        PerkName = "Frenzy";
        PerkDescription = new string[]{
            "Attack 20% faster.",
            "Attack 40% faster.",
            "Attack 60% faster."};
        MaxLevel = 3;
    }
}

public class PerkSecretStash : Perk
{
    public PerkSecretStash()
    {
        PerkName = "Secret Stash";
        PerkDescription = new string[]{ "Equip a second item." };
        MaxLevel = 1;
    }
}

public class PerkFlameAspect : Perk
{
    public PerkFlameAspect()
    {
        PerkName = "Flame Aspect";
        PerkDescription = new string[] { "Deal more fire damage but take additional damage from water attacks." };
        MaxLevel = 1;
    }
}

public class PerkThickHide : Perk
{
    public PerkThickHide()
    {
        PerkName = "Thick Hide";
        PerkDescription = new string[]{
            "Health is increased by 20%.",
            "Health is increased by 40%.",
            "Health is increased by 60%."};
        MaxLevel = 3;
    }
}

public class PerkResurrection : Perk
{
    public PerkResurrection()
    {
        PerkName = "Resurrection";
        PerkDescription = new string[]{
            "Revive with one health once per battle.",
            "Revive with half health once per battle.",
            "Revive with full health once per battle."};
        MaxLevel = 3;
    }
}

//Scrimp
public class PerkFiendFire : Perk
{
    public PerkFiendFire()
    {
        PerkName = "Fiend Fire";
        PerkDescription = new string[]{
            "Scrimp's fireballs ignite targets. Fire damage scales with <color.blue>Focus<default>.",
            "Scrimp's fireballs ignite targets for a long time. Fire damage scales with <color.blue>Focus<default>.",
            "Scrimp's fireballs ignite targets for a long time. Flames can spread to other nearby farmon. Fire damage scales with <color.blue>Focus<default>."};
        MaxLevel = 3;
    }
}

public class PerkForked : Perk
{
    public PerkForked()
    {
        PerkName = "Forked";
        PerkDescription = new string[]{
            "Scrimp fires 2 projectiles.",
            "Scrimp fires 3 projectiles.",
            "Scrimp fires 3 projectiles that home in on targets. If all 3 projectiles hit the same farmon, they take increased damage."};
        MaxLevel = 3;
    }
}
