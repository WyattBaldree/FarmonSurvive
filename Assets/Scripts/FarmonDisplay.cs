using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FarmonDisplay : MonoBehaviour
{
    public FarmonHud DisplayFarmonHud;

    public Camera DisplayCamera;

    public GameObject LightFlash;

    public RenderTexture RenderTextureMaster;

    public RenderTexture RenderTexture;

    public UnityEvent<FarmonDisplay> Destroyed;

    public SpriteRenderer Ground;

    void Awake()
    {
        RenderTexture = new RenderTexture(RenderTextureMaster);
        RenderTexture.Create();

        DisplayCamera.targetTexture = RenderTexture;
    }

    private void OnDestroy()
    {
        RenderTexture.Release();
        Destroyed.Invoke(this);
    }

    public void Flash()
    {
        LightFlash.SetActive(true);
    }
}
