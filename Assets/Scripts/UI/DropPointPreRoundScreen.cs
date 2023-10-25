using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropPointPreRoundScreen : DropPoint
{
    [SerializeField]
    Image BackgroundImage;

    [SerializeField]
    Image RingImage;

    [SerializeField]
    Transform scrollingBackgroundMask;

    [SerializeField]
    float maskGrowSpeed = 2;

    [SerializeField]
    Color LockedRingColor;

    [SerializeField]
    Color LockedBackgroundColor;

    [SerializeField]
    Color UnlockedRingColor;

    [SerializeField]
    Color UnlockedBackgroundColor;

    [SerializeField]
    Color GreenRingColor;

    public void Enable()
    {
        BackgroundImage.color = UnlockedBackgroundColor;
        RingImage.color = UnlockedRingColor;
        Locked = false;
    }

    public void Disable()
    {
        BackgroundImage.color = LockedBackgroundColor;
        RingImage.color = LockedRingColor;
        Locked = true;
    }

    private void Update()
    {
        Vector3 newScale;
        if (currentPickUp && !CurrentPickUpGrabbed)
        {
            newScale = Vector3.one * Mathf.Lerp(scrollingBackgroundMask.localScale.x, 1, Time.deltaTime * maskGrowSpeed);
        }
        else
        {
            newScale = Vector3.one * Mathf.Lerp(scrollingBackgroundMask.localScale.x, 0, Time.deltaTime * maskGrowSpeed);
        }
        scrollingBackgroundMask.localScale = newScale;
    }

    public void LockIn()
    {
        BackgroundImage.color = UnlockedBackgroundColor;
        RingImage.color = GreenRingColor;
        Locked = true;
    }
}
