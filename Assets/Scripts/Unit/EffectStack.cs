using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EffectList
{
	public EffectType Stun = new EffectType("stun", EffectType.StackingTypeEnum.max);
    public EffectType Frost = new EffectType("frost", EffectType.StackingTypeEnum.max);
	public EffectType Shock = new EffectType("shock", EffectType.StackingTypeEnum.max);
	public EffectType Burn = new EffectType("burn", EffectType.StackingTypeEnum.max);
	public EffectType TortorrentShield = new EffectType("tortorrentShield", EffectType.StackingTypeEnum.stacking);

	public UnityEvent EffectUpdatedEvent = new UnityEvent();

	List<EffectType> effectTypeList = new List<EffectType>();

	public void Initialize()
    {
		effectTypeList.Add(Stun);
		effectTypeList.Add(Frost);
		effectTypeList.Add(Shock);
		effectTypeList.Add(Burn);
		effectTypeList.Add(TortorrentShield);
	}

	public void UpdateEffects(float deltaTime)
	{
		Stun.Update(deltaTime);
		Frost.Update(deltaTime);
		Shock.Update(deltaTime);
		Burn.Update(deltaTime);
		TortorrentShield.Update(deltaTime);
	}

	public void RemoveAllEffects()
	{
		Stun.Clear();
		Frost.Clear();
		Shock.Clear();
		Burn.Clear();
		TortorrentShield.Clear();
	}

	public void PrintOutEffects()
    {
		foreach(EffectType et in effectTypeList)
        {
			et.PrintOutEffects();
        }
    }
}

public class EffectType
{
	public enum StackingTypeEnum
    {
		stacking,
		max
    }

	string Name;

	private int _value;

	public int Value
    {
        get => _value;
    }

	public StackingTypeEnum StackingType;

	public EffectType(string name, StackingTypeEnum stackingType)
    {
		Name = name;
		StackingType = stackingType;
    }

	public Effect AddEffect(float duration, int magnitude)
    {
		Effect newEffect = new Effect(Name, duration, magnitude);
		effectList.Add(newEffect);
		CalculateValue();

		effectList.Sort();

		return newEffect;
	}

	public void RemoveEffect(Effect effect)
	{
		effectList.RemoveAll((x) => x == effect);

		CalculateValue();
	}

	public void Clear()
    {
		effectList.Clear();
		CalculateValue();
    }

	/// <summary>
	/// Subtract the amount specified from the magnitudes of each effect in the effect list.
	/// Useful in cases were you need to subtract from the magnitude of a max type effect.
	/// For example, when damaging none-stacking overhealth, you want to "damage" the magnitude of all the overhealth effects.
	/// </summary>
	/// <param name="amount">The amount to subract.</param>
	/// <returns>The left over amount or 0 if there is none.</returns>
	public int SubtractFromAll(int amount)
	{
		if (amount == 0) return 0;

		int remainder = Math.Max(0, amount - Value);

		foreach (Effect e in effectList)
		{
			e.Magnitude = Math.Max(0, e.Magnitude - amount);
		}

		CalculateValue();

		return remainder;
	}

	/// <summary>
	/// Subtract the amount specified from the magnitudes in the effect list one after another until the specifed amount is depeleted.
	/// Useful in cases were you need to subtract from the overall magnitude of a stacking type effect.
	/// For example, when damaging shields, you want to "damage" the magnitude of the TOTAL shield effect.
	/// </summary>
	/// <param name="amount">The amount to subract.</param>
	/// <returns>The left over amount or 0 if there is none.</returns>
	public int SubtractFromTotal(int amount)
    {
		if (amount == 0) return 0;

		int remainder = Math.Max(0, amount - Value);

		int amountRemaining = amount;

		foreach (Effect e in effectList)
        {
			if(e.Magnitude >= amountRemaining)
            {
				e.Magnitude = e.Magnitude - amountRemaining;
				amountRemaining = 0;
				break;
            }
            else
            {
				amountRemaining -= e.Magnitude;
				e.Magnitude = 0;
            }
        }

		CalculateValue();

		return remainder;
	}

	public void Update(float deltaTime)
    {
		List<Effect> removeList = new List<Effect>();
		foreach(Effect e in effectList)
        {
			bool durationExpired = e.UpdateEffect(deltaTime);
			bool magnitudeZeroOrLess = e.Magnitude <= 0;
			if (durationExpired || magnitudeZeroOrLess) removeList.Add(e);
		}

		foreach(Effect e in removeList)
        {
			RemoveEffect(e);
        }
    }

	private void CalculateValue()
    {
		//count up all of the magnitudes based on the StackingType then set the final value.
		int val = 0;
		foreach (Effect e in effectList)
		{
            switch (StackingType)
            {
                case StackingTypeEnum.stacking:
					val += e.Magnitude;
                    break;
                case StackingTypeEnum.max:
					val = Math.Max(val, e.Magnitude);
                    break;
            }
        }

		_value = val;
    }

	public void PrintOutEffects()
    {
		foreach(Effect e in effectList)
        {
			e.Print();
        }
    }

    List<Effect> effectList = new List<Effect>();
}

public class Effect : IComparable<Effect>
{
	public int Magnitude;
	public string EffectName;

	bool permanent = false;
	public float Duration;

	float currentDuration;

	public Effect(string effectName, float duration, int magnitude)
	{
		EffectName = effectName;
		Magnitude = magnitude;
		Duration = duration;

		if (Duration == -1)
		{
			permanent = true;
		}

		currentDuration = 0;
	}

	public bool UpdateEffect(float deltaTime)
	{
		if (permanent) return false;
		this.currentDuration += deltaTime;
		if (currentDuration > Duration)
		{
			return true;
		}

		return false;
	}

    public int CompareTo(Effect obj)
    {
		return this.Magnitude - obj.Magnitude;
    }

    internal void Print()
    {
		Debug.Log(EffectName.PadRight(20) + " : M" + Magnitude.ToString("0000") + " : D" + Duration.ToString("00.00"));
    }
}
