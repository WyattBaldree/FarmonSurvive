using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraController : MonoBehaviour
{
    public Material underwaterShaderMaterial;

    private float submergence = 0;
    //used to determine how under water the camera is.
    [SerializeField] float cameraHeight = .5f;

    private void FixedUpdate()
    {
        submergence = H.SubmergenceCheck(transform.position, cameraHeight);
    }

    

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (submergence >= .5 )
        {
            Graphics.Blit(source, destination, underwaterShaderMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
