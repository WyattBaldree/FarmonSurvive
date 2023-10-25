using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpFarmon : PickUp
{
    FarmonDisplay farmonDisplay;

    [SerializeField]
    RawImage rawImage;

    public Farmon farmonInstance;


    public void Initialize(Sprite farmonSprite)
    {
        farmonDisplay = FarmonDisplayController.instance.GetFarmonDisplay();

        farmonDisplay.DisplayFarmonHud.spriteRenderer.sprite = farmonSprite;

        rawImage.texture = farmonDisplay.RenderTexture;

        StartCoroutine(FarmonIdleCoroutine());
    }

    public void Initialize(Farmon newInstance)
    {
        farmonInstance = newInstance;
        Initialize(farmonInstance.Hud.spriteRenderer.sprite);
    }

    public void SetFarmonInstance(Farmon newInstance)
    {
        farmonInstance = newInstance;

        farmonDisplay.DisplayFarmonHud.spriteRenderer.sprite = farmonInstance.Hud.spriteRenderer.sprite;
    }

    private void OnDestroy()
    {
        Destroy(farmonDisplay.gameObject);
    }

    IEnumerator FarmonIdleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(6f + Random.value * 20f);

            switch (Mathf.RoundToInt(Random.value * 1))
            {
                case 0:
                    farmonDisplay.DisplayFarmonHud.SpriteEffects.JumpJoyAnimation();
                    break;
                case 1:
                    farmonDisplay.DisplayFarmonHud.SpriteEffects.EatAnimation();
                    break;
                case 2:
                    farmonDisplay.DisplayFarmonHud.SpriteEffects.SneezeAnimation();
                    break;
            }
        }
    }
}
