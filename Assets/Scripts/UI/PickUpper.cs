using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpper : MonoBehaviour
{
    public static PickUpper instance;

    public PickUp currentPickUp = null;
    public DropPoint currentDropPoint = null;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void Grab(PickUp pickUp)
    {
        
        ForceDrop();
        pickUp.Grab();
        currentPickUp = pickUp;
        currentPickUp.transform.SetParent(currentPickUp.GetComponentInParent<Canvas>().transform);
        currentPickUp.transform.SetAsLastSibling();
    }

    public void ForceDrop()
    {
        if (currentPickUp)
        {
            currentPickUp.ReturnToDropPoint();
            currentPickUp = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!currentPickUp)
            {
                return;
            }

            if (currentDropPoint && !currentDropPoint.Locked && !currentDropPoint.currentPickUp)
            {
                currentPickUp.SetDropPoint(currentDropPoint);
            }
            else
            {
                currentPickUp.ReturnToDropPoint();
            }

            currentPickUp = null;
        }
    }

    private void LateUpdate()
    {
        if (currentPickUp)
        {
            currentPickUp.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        }
    }
    public void EnterDropPoint(DropPoint dropPoint)
    {
        currentDropPoint = dropPoint;
    }

    public void ExitDropPoint(DropPoint dropPoint)
    {

        if (currentDropPoint == dropPoint)
        {
            currentDropPoint = null;
        }

    }
}
