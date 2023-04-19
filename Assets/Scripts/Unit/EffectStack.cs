using System.Collections.Generic;
using UnityEngine.Events;

public class EffectList
{
	public int Stun;
    public int Frost;
    public int Shock;
    public int Burn;
	public int TortorrentShield;

	List<Effect> effectList = new List<Effect>();

	public UnityEvent EffectUpdatedEvent = new UnityEvent();

	public void UpdateEffects(float deltaTime)
	{
		for (int i = effectList.Count - 1; i >= 0; i--)
		{
			bool shouldDestroy = effectList[i].UpdateEffect(deltaTime);

            if (shouldDestroy)
            {
				RemoveEffect(effectList[i]);
            }
		}
	}

	int GetEffectMax(string effectName)
	{
		int max = 0;
		for (int i = effectList.Count - 1; i >= 0; i--)
		{
			if (effectList[i].effectName == effectName)
			{
				if (effectList[i].magnitude > max) max = effectList[i].magnitude;
			}
		}

		return max;
	}

	public void AddEffect(Effect effect)
	{
		effectList.Add(effect);

		SetStatus(effect.effectName, GetEffectMax(effect.effectName));
	}

	public void RemoveEffect(Effect effect)
	{
		effectList.RemoveAll((x) => x == effect);

		SetStatus(effect.effectName, GetEffectMax(effect.effectName));
	}

	void SetStatus(string effectName, int magnitude)
	{
		switch (effectName)
		{
			case "stun":
				Stun = magnitude;
				break;
			case "frost":
				Frost = magnitude;
				break;
			case "shock":
				Shock = magnitude;
				break;
			case "burn":
				Burn = magnitude;
				break;
			case "tortorrentShield":
				TortorrentShield = magnitude;
				break;
		}
		EffectUpdatedEvent.Invoke();
	}

	public void RemoveAllEffectWithName(string effectName)
	{
		effectList.RemoveAll((x) => x.effectName == effectName);

		SetStatus(effectName, GetEffectMax(effectName));
	}

	public void RemoveAllEffects()
	{
		effectList.Clear();
		Stun = 0;
		Frost = 0;
		Shock = 0;
		Burn = 0;
		TortorrentShield = 0;
		EffectUpdatedEvent.Invoke();
	}
}

public class Effect
{
	public int magnitude;
	public string effectName;

	string[] effectArray = new string[3];

	bool permanent = false;
	float duration;

	float currentDuration;

	public Effect(string effectText)
	{
		effectArray = effectText.Split(";");

		effectName = effectArray[0];
		magnitude = int.Parse(effectArray[1]);
		duration = float.Parse(effectArray[2]);

		if (duration == -1)
		{
			permanent = true;
		}

		currentDuration = 0;
	}

	public bool UpdateEffect(float deltaTime)
	{
		if (permanent) return false;
		this.currentDuration += deltaTime;
		if (currentDuration > duration)
		{
			return true;
		}

		return false;
	}
}
