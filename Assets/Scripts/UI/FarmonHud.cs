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

    GameObject tortorrentShield;

    private void Update()
    {
        if (tortorrentShield == null && TargetFarmon.EffectList.TortorrentShield > 0)
        {
            tortorrentShield = Instantiate(FarmonController.instance.EffectTortorrentShieldPrefab,transform.position, transform.rotation, effectsParent);
        }
        
        if(tortorrentShield != null && TargetFarmon.EffectList.TortorrentShield == 0)
        {
            Destroy(tortorrentShield);
            tortorrentShield = null;
        }
    }


}
