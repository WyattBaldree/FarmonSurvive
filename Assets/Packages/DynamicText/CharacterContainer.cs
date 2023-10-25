using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterContainer : MonoBehaviour
{
    public enum AnimationStateEnum { none, wave, move, bounce, rainbow };

    public TextMeshProUGUI textMesh;

    public float readDelay = .2f;

    ///Returns the width of a text mesh.
    public float GetWidth()
    {
        //textMesh.wi

        return textMesh.preferredWidth;//.GetRenderedValues(false).x;
    }
}
