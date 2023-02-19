using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarGui : Bar
{
    public Image barSpriteRenderer;
    public Image barDelayedSpriteRenderer;

    protected override void UpdateBarGraphics(float percent)
    {
        barSpriteRenderer.rectTransform.localScale = new Vector2(percent, 1);
    }

    protected override void UpdateDelayedBarGraphics(float percent)
    {
        barDelayedSpriteRenderer.rectTransform.localScale = new Vector2(percent, 1);
    }
}
