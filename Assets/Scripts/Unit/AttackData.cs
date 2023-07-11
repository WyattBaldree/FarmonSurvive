using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AttackData is a collection of information that is passed along with attacks. It contains information like damage, knockback, whether the attack can be dodged, etc.
/// </summary>
public class AttackData
{
    public int Damage;
    public float Knockback;
    public bool Undodgeable;
    public AudioClip InitialSound;
    public AudioClip HitSound;
    public AttackData(int damage = 10, float knockback = 5f, bool undodgeable = false, AudioClip initialSound = null, AudioClip hitSound = null)
    {
        Damage = damage;
        Knockback = knockback;
        Undodgeable = undodgeable;
        InitialSound = initialSound;
        HitSound = hitSound;
    }
}
