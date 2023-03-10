using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screevil : Scrimp
{
    public override void DistributeEvolvePerks()
    {
        base.DistributeEvolvePerks();
        PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFrenzy() });
    }
}
