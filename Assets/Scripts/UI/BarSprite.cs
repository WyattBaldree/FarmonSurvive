using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarSprite : Bar
{
    public SpriteRenderer barSpriteRenderer;
    public SpriteRenderer barDelayedSpriteRenderer;

    protected override void UpdateBarGraphics(float percent)
    {
        barSpriteRenderer.transform.localScale = new Vector2(percent, 1);
    }

    protected override void UpdateDelayedBarGraphics(float percent)
    {
        barDelayedSpriteRenderer.transform.localScale = new Vector2(percent, 1);
    }
}
