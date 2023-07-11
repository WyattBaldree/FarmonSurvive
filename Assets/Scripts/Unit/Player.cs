using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static int farmonPerTeam = 5;

    public string SaveName = "";
    public int StoryProgress = 0;
    public uint[] FarmonSquadSaveIds = new uint[farmonPerTeam];

    List<Farmon> selectionList = new List<Farmon>();

    public static Player instance;

    public RectTransform selectionRect;

    private bool selecting = false;

    private Vector2 selectionCorner1, selectionCorner2;

    public Farmon.TeamEnum playerTeam = Farmon.TeamEnum.team1;

    // List of loadedFarmonMap keys
    public List<uint> LoadedFarmon = new List<uint>();

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
        SaveController.LoadPlayer();
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


    public List<Farmon> GetFarmon()
    {
        List<Farmon> newList = new List<Farmon>();
        foreach(uint id in LoadedFarmon)
        {
            newList.Add(Farmon.loadedFarmonMap[id]);
        }

        return newList;
    }

    private void Update()
    {
        hyper = Input.GetKey(KeyCode.LeftShift);

        PlayerMovement();

        PlayerZoom();

        PlayerRotate();

        SelectionUpdate();
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

    private void SelectionUpdate()
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

        if (Input.GetMouseButtonUp(0))
        {
            selecting = false;

            // start by deselecting everything
            foreach (Farmon f in selectionList)
            {
                if (f) f.Deselect();
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

                foreach (Farmon f in Farmon.farmonList)
                {
                    if (f.team != playerTeam) continue;

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
                Farmon targetedFarmon = hitInfo.transform.GetComponentInParent<Farmon>();

                if (targetedFarmon.team == playerTeam)
                {
                    foreach (Farmon selectedFarmon in selectionList)
                    {
                        selectedFarmon.mainBattleState = new NewProtectState(selectedFarmon, targetedFarmon.loadedFarmonMapId);
                        selectedFarmon.SetState(selectedFarmon.mainBattleState);
                    }
                }
                else
                {
                    foreach (Farmon selectedFarmon in selectionList)
                    {
                        selectedFarmon.mainBattleState = new NewAttackState(selectedFarmon, targetedFarmon.loadedFarmonMapId);
                        selectedFarmon.SetState(selectedFarmon.mainBattleState);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if(selectionList.Count > 0 && selectionList[0]) StatsScreen.instance.TargetUnit = selectionList[0];
        }
    }
}