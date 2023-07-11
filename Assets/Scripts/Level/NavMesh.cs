using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class NavMesh : MonoBehaviour
{
    public static NavMesh instance;

    public LevelController levelController;

    [SerializeField, Range(0f, 0.4f)]
    private float borderSize = 0.1f;


    public bool showRaycasts;
    public bool showHits;
    public bool showLinks;
    public bool showBoxes;

    public Path debugPath = null;

    public Transform debugTransform1 = null;
    public Transform debugTransform2 = null;


    GridSpace[,,] _gridSpaceMatrix = new GridSpace[1,1,1];

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one EnemyController");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void OnValidate()
    {
        GenerateNavMesh();
    }

    public void GenerateNavMesh()
    {
        levelController.Initialize();
        FillGrid();
        GenerateBlockLinks();
    }

    public GridSpace GetGridSpaceArray(Vector3Int vInt)
    {
        return _gridSpaceMatrix[vInt.x, vInt.y, vInt.z];
    }

    void SetGridSpaceArray(Vector3Int vInt, GridSpace gridSpace)
    {
        _gridSpaceMatrix[vInt.x, vInt.y, vInt.z] = gridSpace;
    }

    private void FillGrid()
    {
        _gridSpaceMatrix = new GridSpace[levelController.levelSize.x, levelController.levelSize.y, levelController.levelSize.z];

        for (int i = 0; i < levelController.levelSize.x; i++)
        {
            for (int j = 0; j < levelController.levelSize.y; j++)
            {
                for (int k = 0; k < levelController.levelSize.z; k++)
                {
                    Vector3Int position = new Vector3Int(i, j, k);
                    _gridSpaceMatrix[i, j, k] = new GridSpace(position, levelController.gridSize, borderSize);
                }
            }
        }
    }

    private void GenerateBlockLinks()
    {
        for (int i = 0; i < levelController.levelSize.x; i++)
        {
            for (int j = 0; j < levelController.levelSize.y; j++)
            {
                for (int k = 0; k < levelController.levelSize.z; k++)
                {
                    GenerateBlockLink(i, j, k);
                }
            }
        }
    }

    public delegate float HeuristicDelegate(GridSpace location);

    public Path GetPath()
    {
        GridSpace startingSpace = GetGridSpaceArray(H.Vector3ToGridPosition(debugTransform1.position, levelController.gridSize));
        GridSpace endingSpace = GetGridSpaceArray(H.Vector3ToGridPosition(debugTransform2.position, levelController.gridSize));

        return GetPath(startingSpace, endingSpace, 0, false, (x) => { return Vector3.Distance(x.Center, endingSpace.Center); });
    }

    public Path GetPath(GridSpace startingPosition, GridSpace targetLocation, int jumpAbility, bool flying, HeuristicDelegate heuristic)
    {
        // a sorted list contating spaces that still need to be checked sorted by thier heuristic
        List<GridSpace> openSet = new List<GridSpace>();

        openSet.Add(startingPosition);
        //GridSpace d = (GridSpace)(openSet.GetByIndex(0));

        //cameFrom[n] returns the GridSpace that led to n
        Dictionary<GridSpace, BlockLink> cameFrom = new Dictionary<GridSpace, BlockLink>();

        Dictionary<GridSpace, float> gScore = new Dictionary<GridSpace, float>();

        gScore.Add(startingPosition, 0);

        Dictionary<GridSpace, float> fScore = new Dictionary<GridSpace, float>();

        fScore.Add(startingPosition, heuristic(startingPosition));

        while(openSet.Count > 0)
        {
            GridSpace current = openSet[0];

            if(current == targetLocation)
            {
                // goal reached reconstruct path
                Path path = new Path();

                BlockLink previousLink = null;

                while(current != startingPosition)
                {
                    BlockLink currentLink = cameFrom[current];

                    GridSpace currentGridSpace = currentLink.ToGridSpace;

                    BlockLink inputBlockLink = currentLink;

                    path.AddNode(new PathNode(currentGridSpace, inputBlockLink, previousLink));

                    previousLink = currentLink;
                    current = cameFrom[current].FromGridSpace;
                }

                path.AddNode(new PathNode(current, null, previousLink));

                return path;
            }

            openSet.RemoveAt(0);
            foreach(BlockLink link in current.BlockLinks)
            {
                bool jump = false;
                //Check if this link is crossable by this farmon.
                if (!link.walkable && !flying)
                {
                    //Check if the farmon can jump over this link.
                    if (link.jumpable && jumpAbility > 0 ||
                        link.doubleJumpable && jumpAbility > 1)
                    {
                        //thisLink is jumpable
                        jump = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                GridSpace neighbor = link.ToGridSpace;

                // tentative_gScore is the distance from start to the neighbor through current
                float tentativeGScore = gScore[current] + 1;// Vector3.Distance(current.Center, neighbor.Center);
                if (jump)
                {
                    //Slightly increase the score of jumps to prevent jump spam.
                    tentativeGScore += .1f;
                }

                float neighborGScore = gScore.TryGetValue(neighbor, out float value) ? value : float.PositiveInfinity;

                if (tentativeGScore < neighborGScore)
                {
                    // This path to neighbor is better than any previous one. Record it!
                    cameFrom[neighbor] = link;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + heuristic(neighbor);

                    GridSpace alreadyInList = openSet.Find((x) => (x == neighbor));
                    if (alreadyInList == null)
                    {
                        openSet.Add(neighbor);
                    }

                    //sort
                    openSet.Sort((x, y) => 
                    { 
                        return fScore[x].CompareTo(fScore[y]); 
                    });
                }
            }
        }

        return new Path();
    }

    private void GenerateBlockLink(int i, int j, int k)
    {
        GridSpace currentGridSpace = _gridSpaceMatrix[i, j, k];
        currentGridSpace.BlockLinks.Clear();

        // Check all 4 directions at all heights
        // Could only go up until a block is detected above.

        //Get links that allow farmon to go sideways and jump up.
        for (int y = j; y < _gridSpaceMatrix.GetLength(1); y++)
        {
            //Check if there is a block above that would prevent the jump.
            if (y != j && levelController.BlockArray[i, y, k] != null)
            {
                // There is a cieling here so don't make any more links upward
                break;
            }
            else
            {
                //North
                if (k + 1 < _gridSpaceMatrix.GetLength(2))
                {
                    GridSpace gridSpaceNorth = _gridSpaceMatrix[i, y, k + 1];

                    if (currentGridSpace.NorthValid && gridSpaceNorth.SouthValid)
                    {
                        currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceNorth, currentGridSpace.HitNorth, gridSpaceNorth.HitSouth, (gridSpaceNorth.HitSouth.point.y - currentGridSpace.HitNorth.point.y)));
                    }
                }

                //East
                if (i + 1 < _gridSpaceMatrix.GetLength(0))
                {
                    GridSpace gridSpaceEast = _gridSpaceMatrix[i + 1, y, k];

                    if (currentGridSpace.EastValid && gridSpaceEast.WestValid)
                    {
                        currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceEast, currentGridSpace.HitEast, gridSpaceEast.HitWest, (gridSpaceEast.HitWest.point.y - currentGridSpace.HitEast.point.y)));
                    }
                }

                //South
                if (k - 1 >= 0)
                {
                    GridSpace gridSpaceSouth = _gridSpaceMatrix[i, y, k - 1];

                    if (currentGridSpace.SouthValid && gridSpaceSouth.NorthValid)
                    {
                        currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceSouth, currentGridSpace.HitSouth, gridSpaceSouth.HitNorth, (gridSpaceSouth.HitNorth.point.y - currentGridSpace.HitSouth.point.y)));
                    }
                }

                //West
                if (i - 1 >= 0)
                {
                    GridSpace gridSpaceWest = _gridSpaceMatrix[i - 1, y, k];
                    if (currentGridSpace.WestValid && gridSpaceWest.EastValid)
                    {
                        currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceWest, currentGridSpace.HitWest, gridSpaceWest.HitEast, (gridSpaceWest.HitEast.point.y - currentGridSpace.HitWest.point.y)));
                    }
                }
            } 
        }

        //Get links that allow farmon to jump down from ledges

        //North
        if(k + 1 < _gridSpaceMatrix.GetLength(2))
        {
            for (int y = j - 1; y >= 0; y--)
            {
                GridSpace gridSpaceNorth = _gridSpaceMatrix[i, y, k + 1];
                if (currentGridSpace.NorthValid && gridSpaceNorth.SouthValid)
                {
                    currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceNorth, currentGridSpace.HitNorth, gridSpaceNorth.HitSouth, (gridSpaceNorth.HitSouth.point.y - currentGridSpace.HitNorth.point.y)));
                    break;
                }
            }
        }

        //East
        if (i + 1 < _gridSpaceMatrix.GetLength(0))
        {
            for (int y = j - 1; y >= 0; y--)
            {
                GridSpace gridSpaceEast = _gridSpaceMatrix[i + 1, y, k];

                if (currentGridSpace.EastValid && gridSpaceEast.WestValid)
                {
                    currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceEast, currentGridSpace.HitEast, gridSpaceEast.HitWest, (gridSpaceEast.HitWest.point.y - currentGridSpace.HitEast.point.y)));
                }
            }
        }

        //South
        if (k - 1 >= 0)
        {
            for (int y = j - 1; y >= 0; y--)
            {
                GridSpace gridSpaceSouth = _gridSpaceMatrix[i, y, k - 1];

                if (currentGridSpace.SouthValid && gridSpaceSouth.NorthValid)
                {
                    currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceSouth, currentGridSpace.HitSouth, gridSpaceSouth.HitNorth, (gridSpaceSouth.HitNorth.point.y - currentGridSpace.HitSouth.point.y)));
                }
            }
        }

        //West
        if (i - 1 >= 0)
        {
            for (int y = j - 1; y >= 0; y--)
            {
                GridSpace gridSpaceWest = _gridSpaceMatrix[i - 1, y, k];
                if (currentGridSpace.WestValid && gridSpaceWest.EastValid)
                {
                    currentGridSpace.BlockLinks.Add(new BlockLink(currentGridSpace, gridSpaceWest, currentGridSpace.HitWest, gridSpaceWest.HitEast, (gridSpaceWest.HitEast.point.y - currentGridSpace.HitWest.point.y)));
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        for(int i = 0; i < levelController.levelSize.x; i++)
        {
            for (int j = 0; j < levelController.levelSize.y; j++)
            {
                for (int k = 0; k < levelController.levelSize.z; k++)
                {
                    if (showRaycasts)
                    {
                        Gizmos.color = new Color(1, 0, 0, 1f);
                        Gizmos.DrawLine(_gridSpaceMatrix[i, j, k].NorthRaycastPosition, _gridSpaceMatrix[i, j, k].NorthRaycastPosition + levelController.gridSize * Vector3.down);
                        Gizmos.color = new Color(1, 1, 0, 1f);
                        Gizmos.DrawLine(_gridSpaceMatrix[i, j, k].EastRaycastPosition, _gridSpaceMatrix[i, j, k].EastRaycastPosition + levelController.gridSize * Vector3.down);
                        Gizmos.color = new Color(0, 1, 0, 1f);
                        Gizmos.DrawLine(_gridSpaceMatrix[i, j, k].SouthRaycastPosition, _gridSpaceMatrix[i, j, k].SouthRaycastPosition + levelController.gridSize * Vector3.down);
                        Gizmos.color = new Color(0, 0, 1, 1f);
                        Gizmos.DrawLine(_gridSpaceMatrix[i, j, k].WestRaycastPosition, _gridSpaceMatrix[i, j, k].WestRaycastPosition + levelController.gridSize * Vector3.down);
                    }

                    if (showLinks)
                    {
                        foreach(BlockLink link in _gridSpaceMatrix[i, j, k].BlockLinks)
                        {
                            if (link.walkable)
                            {
                                Gizmos.color = Color.black;
                            }
                            else if (link.jumpable)
                            {
                                Gizmos.color = Color.green;
                            }
                            else
                            {
                                Gizmos.color = Color.red;
                            }

                            Vector3 offset = Vector3.Cross(Vector3.up, link.ToHit.point - link.FromHit.point).normalized * .1f;
                            Gizmos.DrawLine(link.FromHit.point + offset, link.ToHit.point + offset);
                        }
                    }

                    if (showHits)
                    {
                        Gizmos.color = new Color(1, 0, 0, 1f);
                        if (_gridSpaceMatrix[i, j, k].NorthValid) Gizmos.DrawSphere(_gridSpaceMatrix[i, j, k].HitNorth.point, .1f);
                        Gizmos.color = new Color(1, 1, 0, 1f);
                        if (_gridSpaceMatrix[i, j, k].EastValid) Gizmos.DrawSphere(_gridSpaceMatrix[i, j, k].HitEast.point, .1f);
                        Gizmos.color = new Color(0, 1, 0, 1f);
                        if (_gridSpaceMatrix[i, j, k].SouthValid) Gizmos.DrawSphere(_gridSpaceMatrix[i, j, k].HitSouth.point, .1f);
                        Gizmos.color = new Color(0, 0, 1, 1f);
                        if (_gridSpaceMatrix[i, j, k].WestValid) Gizmos.DrawSphere(_gridSpaceMatrix[i, j, k].HitWest.point, .1f);

                    }
                    
                    if(showBoxes)
                    {
                        Gizmos.color = new Color(i / (float)levelController.levelSize.x, j / (float)levelController.levelSize.y, k / (float)levelController.levelSize.y, 0.5f);
                        Gizmos.DrawWireCube(_gridSpaceMatrix[i, j, k].Center, Vector3.one * levelController.gridSize);
                        Gizmos.DrawCube(_gridSpaceMatrix[i, j, k].Center, Vector3.one * (levelController.gridSize * (1 - borderSize)));
                    }
                }
            }
        }

        if (debugPath != null)
        {
            for (int i = 0; i < debugPath.nodeList.Count - 1; i++)
            {
                float per = (float)i / (float)debugPath.nodeList.Count;
                Gizmos.color = new Color(per, 1 - per, 0, 1);
                Gizmos.DrawLine(debugPath.nodeList[i].GridSpace.Center, debugPath.nodeList[i + 1].GridSpace.Center);
            }
        }

    }
}

public class BlockLink
{
    public GridSpace FromGridSpace { get; private set; }
    public GridSpace ToGridSpace { get; private set; }

    public RaycastHit FromHit { get; private set; }
    public RaycastHit ToHit { get; private set; }

    public float HeightDifference { get; private set; }

    public bool walkable = false;
    public bool jumpable = false;
    public bool doubleJumpable = false;

    public BlockLink(GridSpace fromGridSpace, GridSpace toGridSpace, RaycastHit fromHit, RaycastHit toHit, float heightDifference)
    {
        FromGridSpace = fromGridSpace;
        ToGridSpace = toGridSpace;
        FromHit = fromHit;
        ToHit = toHit;
        HeightDifference = heightDifference;

        walkable = HeightDifference < LevelController.Instance.gridSize / 5;
        jumpable = HeightDifference < LevelController.Instance.gridSize * 1.25f;
        doubleJumpable = HeightDifference < LevelController.Instance.gridSize * 2.25f;
    }
}

public class GridSpace
{
    public bool NorthValid;
    public bool EastValid;
    public bool SouthValid;
    public bool WestValid;
    public bool CenterValid;

    public RaycastHit HitNorth;
    public RaycastHit HitEast;
    public RaycastHit HitSouth;
    public RaycastHit HitWest;
    public RaycastHit HitCenter;

    public Vector3Int Position { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public Vector3 Center { get; private set; }
    public float GridSize { get; private set; }
    public float BorderSize { get; private set; }

    public Vector3 NorthRaycastPosition { get; private set; }
    public Vector3 EastRaycastPosition { get; private set; }
    public Vector3 SouthRaycastPosition { get; private set; }
    public Vector3 WestRaycastPosition { get; private set; }
    public Vector3 CenterRaycastPosition { get; private set; }

    public List<BlockLink> BlockLinks = new List<BlockLink>();

    public GridSpace(Vector3Int position, float gridSize, float borderSize)
    {
        Position = position;
        WorldPosition = new Vector3(position.x * gridSize, position.y * gridSize, position.z * gridSize);
        Center = WorldPosition + (gridSize / 2) * Vector3.one;
        GridSize = gridSize;
        BorderSize = borderSize;
        GeneratePoleHeights();
    }

    const float backFaceForgiveDistance = 0.01f;

    private void EdgeRaycast(Vector3 rayCastPosition, out bool valid, out RaycastHit hit)
    {
        Physics.queriesHitBackfaces = true;

        RaycastHit[] RaycastArray = Physics.RaycastAll(rayCastPosition, Vector3.down, GridSize, LayerMask.GetMask("Default"));
        System.Array.Sort(RaycastArray, (x, y) => x.distance.CompareTo(y.distance));

        Physics.queriesHitBackfaces = false;

        if (RaycastArray.Length < 1)
        {
            valid = false;
            hit = new RaycastHit();
            return;
        }

        //if the first hit is a backface this is a nonvalid cast
        if (Vector3.Dot(RaycastArray[0].normal, Vector3.down) > 0)
        {
            //If the first hit is a backface, then we are inside geometry
            valid = false;
            hit = new RaycastHit();
            return;
        }

        if (RaycastArray.Length < 2)
        {
            valid = true;
            hit = RaycastArray[0];
            return;
        }

        //now check if the second hit was a backface and within a small threshold of the first hit.
        if (Vector3.Dot(RaycastArray[1].normal, Vector3.down) > 0 &&
            Vector3.Distance(RaycastArray[0].point, RaycastArray[1].point) < backFaceForgiveDistance)
        {
            //If the second hit is a backface AND within a small distance of the first hit, then we are probably inside geometry
            valid = false;
            hit = new RaycastHit();
            return;
        }

        valid = true;
        hit = RaycastArray[0];
    }

    private void GeneratePoleHeights()
    {
        float halfSize = GridSize / 2;

        Vector3 positionFloat = new Vector3(Position.x, Position.y, Position.z);
        float StartingHeight = .98f;

        NorthRaycastPosition = WorldPosition + StartingHeight * GridSize * Vector3.up + (halfSize * Vector3.right) + (GridSize * Vector3.forward) - (GridSize * BorderSize * Vector3.forward);
        EastRaycastPosition = WorldPosition + StartingHeight * GridSize * Vector3.up + (GridSize * Vector3.right) + (halfSize * Vector3.forward) - (GridSize * BorderSize * Vector3.right);
        SouthRaycastPosition = WorldPosition + StartingHeight * GridSize * Vector3.up + (halfSize * Vector3.right) + (GridSize * BorderSize * Vector3.forward);
        WestRaycastPosition = WorldPosition + StartingHeight * GridSize * Vector3.up + (halfSize * Vector3.forward) + (GridSize * BorderSize * Vector3.right);
        CenterRaycastPosition = WorldPosition + StartingHeight * GridSize * Vector3.up + (halfSize * Vector3.forward) + (halfSize * Vector3.right);

        EdgeRaycast(NorthRaycastPosition,out NorthValid, out HitNorth);
        EdgeRaycast(EastRaycastPosition, out EastValid, out HitEast);
        EdgeRaycast(SouthRaycastPosition, out SouthValid, out HitSouth);
        EdgeRaycast(WestRaycastPosition, out WestValid, out HitWest);
        EdgeRaycast(CenterRaycastPosition, out CenterValid, out HitCenter);
    }
}

public class Path
{
    public List<PathNode> nodeList = new List<PathNode>();

    public void AddNode(PathNode newNode)
    {
        nodeList.Insert(0, newNode);
    }

    public PathNode PopNode()
    {
        PathNode poppedNode = nodeList[0];
        nodeList.RemoveAt(0);

        return poppedNode;
    }

    public PathNode PeekNode()
    {
        if(nodeList.Count > 0)
        {
            return nodeList[0];
        }

        return null;
    }
}

public class PathNode
{
    public GridSpace GridSpace;
    public BlockLink InputBlockLink, OutputBlockLink;
    public PathNode(GridSpace gridSpace, BlockLink inputBlockLink, BlockLink outputBlockLink)
    {
        GridSpace = gridSpace;
        InputBlockLink = inputBlockLink;
        OutputBlockLink = outputBlockLink;
    }
}

[CustomEditor(typeof(NavMesh), true)]
public class NavMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        NavMesh navMesh = (NavMesh)target;

        if (false && GUILayout.Button("Floor To Grid"))
        {

            float gridInverse = 1 / navMesh.levelController.gridSize;

            foreach (Transform t in navMesh.transform)
            {
                float newX = Mathf.Floor(t.position.x * gridInverse) / gridInverse;
                float newY = Mathf.Floor(t.position.y * gridInverse) / gridInverse;
                float newZ = Mathf.Floor(t.position.z * gridInverse) / gridInverse;

                t.position = new Vector3(newX, newY, newZ);
            }
        }

        if(GUILayout.Button("Generate Path"))
        {
            navMesh.debugPath = navMesh.GetPath();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
