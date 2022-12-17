using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightList : MonoBehaviour
{
    [SerializeField]
    List<Highlight> _highlightList = new List<Highlight>();

    SpriteRenderer rend;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        UpdateHighlight();
    }

    public Highlight AddHighlight( Color color, int priority )
    {
        Highlight newHighlight = new Highlight(color, priority);
        int index = _highlightList.BinarySearch(newHighlight);
        if (index < 0) index = ~index;
        _highlightList.Insert(index, newHighlight);
        UpdateHighlight();
        return newHighlight;
    }

    public void RemoveHighlight( Highlight highlight )
    {
        _highlightList.Remove(highlight);
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (_highlightList.Count <= 0)
        {
            rend.material.SetFloat("_OutlineWidth", 0f);
        }
        else
        {
            Highlight topHighlight = _highlightList[0];
            rend.material.SetFloat("_OutlineWidth", 1f);
            rend.material.SetColor("_OutlineColor", topHighlight.Color);
        }
    }
}

[Serializable]
public class Highlight : IComparable<Highlight>
{
    [SerializeField]
    public int Priority;

    [SerializeField]
    public Color Color;

    public Highlight(Color color, int priority)
    {
        Priority = priority;
        Color = color;
    }

    public int CompareTo(Highlight other)
    {
        return Priority.CompareTo(other.Priority);
    }
}
