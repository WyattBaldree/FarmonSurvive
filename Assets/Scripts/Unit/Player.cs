using Assets.Scripts.States;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Player instance;

    [SerializeField]
    Transform cameraRig;

    [SerializeField]
    float cameraSpeed = 3;

    [SerializeField]
    float cameraSpeedHyper = 6;

    [SerializeField]
    float cameraRotationSpeed = 15;

    [SerializeField]
    float cameraRotationSpeedHyper = 30;

    [SerializeField]
    float cameraZoomSpeed = 2;

    [SerializeField]
    float cameraZoomSpeedHyper = 3;

    [SerializeField]
    float cameraMinDistance = 20;

    [SerializeField]
    float cameraMaxDistance = 60;

    bool hyper = false;

    protected void Awake()
    {
        Assert.IsNull(instance, "There should only ever be one player.");
        instance = this;
    }

    protected void OnDestroy()
    {
        instance = null;
    }

    protected void Start()
    {
    }

    protected void OnDrawGizmos()
    {
    }

    private void Update()
    {
        hyper = Input.GetKey(KeyCode.LeftShift);

        PlayerMovement();

        PlayerZoom();

        PlayerRotate();
    }

    private void PlayerRotate()
    {
        float rotationInput = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            rotationInput = 1;
        }
        else if(Input.GetKey(KeyCode.E))
        {
            rotationInput = -1;
        }


        float finalSpeed = hyper ? cameraRotationSpeedHyper : cameraRotationSpeed;

        cameraRig.transform.eulerAngles = cameraRig.transform.eulerAngles + new Vector3(0, rotationInput * finalSpeed * Time.deltaTime, 0);
    }

    private void PlayerZoom()
    {
        float zoomInput = -Input.mouseScrollDelta.y;

        float currentCameraDistance = Vector3.Distance(Camera.main.transform.position, transform.position);

        float finalSpeed = hyper ? cameraZoomSpeedHyper : cameraZoomSpeed;

        currentCameraDistance += zoomInput * finalSpeed;

        if (currentCameraDistance < cameraMinDistance)
        {
            currentCameraDistance = cameraMinDistance;
        }

        if (currentCameraDistance > cameraMaxDistance)
        {
            currentCameraDistance = cameraMaxDistance;
        }

        Camera.main.transform.localPosition = -currentCameraDistance * Vector3.forward;
    }

    public void PlayerMovement()
    {
        Vector3 cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += cameraForward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= cameraForward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += cameraRight;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveDirection -= cameraRight;
        }

        if (moveDirection != Vector3.zero)
        {
            float finalSpeed = hyper ? cameraSpeedHyper : cameraSpeed;

            transform.Translate(moveDirection * finalSpeed * Time.deltaTime);
        }

        
    }
}