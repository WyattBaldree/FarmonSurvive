using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perk
{
    // Static
    public static Dictionary<string, Perk> perkDictionary;

    public static void CreatePerks()
    {
        perkDictionary.Clear();

        new PerkJump().Add();
    }

    // Instance
    public string PerkName = "Generic Perk";

    public void Add()
    {
        perkDictionary.Add(PerkName, this);
    }
}

public class PerkJump : Perk
{
    public PerkJump()
    {
        PerkName = "Jump";
    }
}

public class PerkFiendFire : Perk
{
    public PerkFiendFire()
    {
        PerkName = "FiendFire";
    }
}
