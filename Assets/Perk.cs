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
    public string PerkDescription = "Generic description.";
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
        PerkDescription = "Jump up 1 block high ledges!";
        MaxLevel = 2;
    }
}

public class PerkFrenzy : Perk
{
    public PerkFrenzy()
    {
        PerkName = "Frenzy";
        PerkDescription = "Attack 20% faster per level.";
        MaxLevel = 3;
    }
}

public class PerkSecretStash : Perk
{
    public PerkSecretStash()
    {
        PerkName = "Secret Stash";
        PerkDescription = "Equip a second item.";
        MaxLevel = 3;
    }
}

public class PerkForked : Perk
{
    public PerkForked()
    {
        PerkName = "Forked";
        PerkDescription = "Launch an additional projectile on attack.";
        MaxLevel = 3;
    }
}

public class PerkFlameAspect : Perk
{
    public PerkFlameAspect()
    {
        PerkName = "Flame Aspect";
        PerkDescription = "Deal more fire damage but take additional damage from water attacks.";
        MaxLevel = 1;
    }
}

public class PerkThickHide : Perk
{
    public PerkThickHide()
    {
        PerkName = "Thick Hide";
        PerkDescription = "20% more health per level.";
        MaxLevel = 3;
    }
}

//Scrimp
public class PerkFiendFire : Perk
{
    public PerkFiendFire()
    {
        PerkName = "Fiend Fire";
        PerkDescription = "Scrimp's fireballs ignite targets. Fire damage scales with <color.blue>Focus<default>.";
        MaxLevel = 1;
    }
}

public class PerkNapalm : Perk
{
    public PerkNapalm()
    {
        PerkName = "Napalm";
        PerkDescription = "Scrimp's fire lasts 50% longer.";
        MaxLevel = 1;
    }
}
