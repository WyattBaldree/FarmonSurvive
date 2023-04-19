using Assets.Scripts.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    Farmon _targetFarmon;

    [SerializeField]
    RectTransform backgroundRect;
    [SerializeField]
    RectTransform healthRect;
    [SerializeField]
    RectTransform healthDelayedRect;
    [SerializeField]
    RectTransform armorRect;
    [SerializeField]
    RectTransform armorDelayedRect;
    [SerializeField]
    RectTransform redRect;

    int delayedHealth, delayedArmor, redHealth;

    Timer healDelayTimer = new Timer(), 
          damageDelayTimer = new Timer(), 
          delayedHealthTimer = new Timer(),
          delayedArmorTimer = new Timer(), 
          delayedRedTimer = new Timer();

    public void SetFarmon(Farmon targetFarmon)
    {
        _targetFarmon = targetFarmon;
        delayedHealth = targetFarmon.GetHealth();
        delayedArmor = targetFarmon.GetArmor();

        delayedHealthTimer.autoReset = true;
        delayedHealthTimer.SetTime(.1f);

        delayedArmorTimer.autoReset = true;
        delayedArmorTimer.SetTime(.1f);

        delayedRedTimer.autoReset = true;
        delayedRedTimer.SetTime(.1f);

        _targetFarmon.DamageEvent.AddListener(OnDamage);
        _targetFarmon.HealEvent.AddListener(OnHeal);
    }

    private void OnDamage()
    {
        damageDelayTimer.SetTime(1);
    }

    public void OnHeal()
    {
        healDelayTimer.SetTime(0.5f);
    }

    private void Update()
    {
        if (!_targetFarmon) return;

        int health = _targetFarmon.GetHealth();
        int armor = _targetFarmon.GetArmor();
        int total = health + armor;

        float currentHealthAllSources = total;
        float maxHealthAllSources = _targetFarmon.MaxHealth + armor;

        //Get the percent of the bar that will be filled by health and armor
        float healthPercent = health / maxHealthAllSources;
        float armorPercent = armor / maxHealthAllSources;

        float totalPercent = currentHealthAllSources / maxHealthAllSources;

        float redHealthPercent = (redHealth - total) / maxHealthAllSources;

        float healthDelayedPercent =  (health - delayedHealth) / maxHealthAllSources;
        float armorDelayedPercent = (armor - delayedArmor) / maxHealthAllSources;

        float horizontalOffset = 0f;

        //Background
        backgroundRect.localScale = new Vector2(1, 1);

        //Health
        healthRect.localPosition = new Vector2(horizontalOffset, 0);
        healthRect.localScale = new Vector2(healthPercent - healthDelayedPercent, 1);
        horizontalOffset += healthRect.rect.width * healthRect.localScale.x;

        //DelayedHealth
        healthDelayedRect.localPosition = new Vector2(horizontalOffset, 0);
        healthDelayedRect.localScale = new Vector2(healthDelayedPercent, 1);
        horizontalOffset += healthDelayedRect.rect.width * healthDelayedRect.localScale.x;

        //Armor
        armorRect.localPosition = new Vector2(horizontalOffset, 0);
        armorRect.localScale = new Vector2(armorPercent - armorDelayedPercent, 1);
        horizontalOffset += armorRect.rect.width * armorRect.localScale.x;

        //DelayedArmor
        armorDelayedRect.localPosition = new Vector2(horizontalOffset, 0);
        armorDelayedRect.localScale = new Vector2(armorDelayedPercent, 1);
        horizontalOffset += armorDelayedRect.rect.width * armorDelayedRect.localScale.x;

        //RedHealth
        redRect.localPosition = new Vector2(horizontalOffset, 0);
        redRect.localScale = new Vector2(redHealthPercent, 1);

        //healDelayTimer().Tick

        // Now update everything
        if(delayedHealth >= health)
        {
            delayedHealth = health;
        }
        else
        {
            if (delayedHealthTimer.Tick(Time.deltaTime))
            {
                delayedHealth += 1;
            }
        }

        if (delayedArmor >= armor)
        {
            delayedArmor = armor;
        }
        else
        {
            if (delayedArmorTimer.Tick(Time.deltaTime))
            {
                delayedArmor += 1;
            }
        }

        if (redHealth <= total)
        {
            redHealth = total;
        }
        else
        {
            if (delayedRedTimer.Tick(Time.deltaTime))
            {
                redHealth -= 1;
            }
        }
    }
}
