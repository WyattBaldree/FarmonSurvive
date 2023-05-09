using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmonHud : MonoBehaviour
{
    Farmon _targetFarmon;
    public Farmon TargetFarmon
    {
        set
        {
            _targetFarmon = value;
            HealthBar.SetFarmon(value);
        }
        get => _targetFarmon;
    }

    public GameObject SpriteQuad;
    public Animator Animator;
    public PositionQuad PositionQuad;
    public AudioSource AudioSource;
    public Transform ScalingObjectsParent;
    public TMPro.TextMeshPro debugText;
    public Transform effectsParent;
    public HealthBar HealthBar;
    public SpriteEffects SpriteEffects;
    public HighlightList HighlightList;

    GameObject tortorrentShield;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = SpriteQuad.GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!TargetFarmon) return;

        if (tortorrentShield == null && TargetFarmon.EffectList.TortorrentShield.Value > 0)
        {
            tortorrentShield = Instantiate(FarmonController.instance.EffectTortorrentShieldPrefab,transform.position, transform.rotation, effectsParent);
        }
        
        if(tortorrentShield != null && TargetFarmon.EffectList.TortorrentShield.Value == 0)
        {
            Destroy(tortorrentShield);
            tortorrentShield = null;
        }
    }

    internal void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
