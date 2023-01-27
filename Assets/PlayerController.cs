using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    List<Farmon> selectionList = new List<Farmon>();

    public static PlayerController instance;

    public RectTransform selectionRect;

    private bool selecting = false;

    private Vector2 selectionCorner1, selectionCorner2;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one PlayerController");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }
    private void Update()
    {
        Canvas canvas = selectionRect.GetComponentInParent<Canvas>();
        CanvasScaler canvasScaler = selectionRect.GetComponentInParent<CanvasScaler>();

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 referenceResolutionSize = new Vector2(canvasScaler.referenceResolution.x, canvasScaler.referenceResolution.x * 9f / 16f);

        if (Input.GetMouseButtonDown(0))
        {
            selecting = true;

            selectionCorner1 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        if (selecting)
        {
            selectionCorner2 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            Vector2 selectionCorner1Converted = new Vector2(selectionCorner1.x / screenSize.x * referenceResolutionSize.x,
                                                            selectionCorner1.y / screenSize.y * referenceResolutionSize.y);

            Vector2 selectionCorner2Converted = new Vector2(selectionCorner2.x / screenSize.x * referenceResolutionSize.x,
                                                            selectionCorner2.y / screenSize.y * referenceResolutionSize.y);

            Vector2 bottomLeftCorner = new Vector2(Mathf.Min(selectionCorner1Converted.x, selectionCorner2Converted.x),
                                                   Mathf.Min(selectionCorner1Converted.y, selectionCorner2Converted.y));

            Vector2 topRightCorner = new Vector2(Mathf.Max(selectionCorner1Converted.x, selectionCorner2Converted.x),
                                                 Mathf.Max(selectionCorner1Converted.y, selectionCorner2Converted.y));

            selectionRect.anchoredPosition = bottomLeftCorner;
            selectionRect.sizeDelta = topRightCorner - bottomLeftCorner;
            selectionRect.gameObject.SetActive(true);
        }
        else
        {
            selectionRect.gameObject.SetActive(false);
        }

        if(Input.GetMouseButtonUp(0))
        {
            selecting = false;

            // start by deselecting everything
            foreach (Farmon f in selectionList)
            {
                if(f) f.Deselect();
            }
            selectionList.Clear();

            //update selection
            if (Vector2.Distance(selectionCorner1, selectionCorner2) < 10)
            {
                // consider this a click instead of a drag select
                // select unit under cursor

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                bool hitFarmon = Physics.Raycast(ray, out RaycastHit hitInfo, 100, LayerMask.GetMask("Farmon"));

                if (hitFarmon)
                {
                    //Select the hit farmon
                    Farmon f = hitInfo.transform.GetComponentInParent<Farmon>();
                    
                    f.Select();
                    selectionList.Add(f);
                }
            }
            else
            {
                // consider this a drag select

                foreach(Farmon f in Farmon.farmonList)
                {
                    Vector3 farmonScreenPosition = Camera.main.WorldToScreenPoint(f.transform.position);

                    Vector2 bottomLeftCorner = new Vector2(Mathf.Min(selectionCorner1.x, selectionCorner2.x),
                                                           Mathf.Min(selectionCorner1.y, selectionCorner2.y));

                    Vector2 topRightCorner = new Vector2(Mathf.Max(selectionCorner1.x, selectionCorner2.x),
                                                         Mathf.Max(selectionCorner1.y, selectionCorner2.y));

                    if (farmonScreenPosition.x >= bottomLeftCorner.x &&
                        farmonScreenPosition.x <= topRightCorner.x &&
                        farmonScreenPosition.y >= bottomLeftCorner.y &&
                        farmonScreenPosition.y <= topRightCorner.y)
                    {
                        // Add this farmon to the selection.

                        f.Select();
                        selectionList.Add(f);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            bool hitFarmon = Physics.Raycast(ray, out RaycastHit hitInfo, 100, LayerMask.GetMask("Farmon"));

            if (hitFarmon)
            {
                //Select the hit farmon
                Farmon f = hitInfo.transform.GetComponentInParent<Farmon>();

                if(f.team == Farmon.TeamEnum.player)
                {
                    foreach (Farmon selectedFarmon in selectionList)
                    {
                        selectedFarmon.EnterDefendState(f);
                    }
                }
                else
                {
                    foreach (Farmon selectedFarmon in selectionList)
                    {
                        selectedFarmon.EnterAttackState(f);
                    }
                }
            }
        }
    }
}
