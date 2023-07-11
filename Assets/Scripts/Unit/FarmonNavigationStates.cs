using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class FollowPathState : StateMachineState
{
    Farmon farmon;
    Path path;

    Timer randomPathUpdateTimer = new Timer();

    bool jump = false;

    public FollowPathState(Farmon thisUnit)
    {
        farmon = thisUnit;
    }

    public override void Enter()
    {
        base.Enter();

        farmon.PathNodeReachedEvent.AddListener(NodeReached);
        farmon.GridSpaceChangedEvent.AddListener(UpdatePath);

        farmon.maxSpeed = farmon.GetMovementSpeed();

        randomPathUpdateTimer.SetTime(UnityEngine.Random.Range(.5f, 1f));

        UpdatePath();
    }

    public override void Exit()
    {
        base.Exit();

        farmon.PathNodeReachedEvent.RemoveListener(NodeReached);
        farmon.GridSpaceChangedEvent.RemoveListener(UpdatePath);
    }

    private void UpdatePath()
    {
        if (farmon.targetTransform)
        {
            Vector3 myPos = farmon.transform.position;
            Vector3 targetPos = farmon.targetTransform.position;
            float gridSize = LevelController.Instance.gridSize;

            Vector3 myRaycastPosition = myPos - new Vector3(myPos.x % gridSize, 0, myPos.z % gridSize) + new Vector3(gridSize / 2, 0, gridSize / 2);
            Vector3 targetRaycastPosition = targetPos - new Vector3(targetPos.x % gridSize, 0, targetPos.z % gridSize) + new Vector3(gridSize / 2, 0, gridSize / 2);

            Physics.Raycast(new Ray(myRaycastPosition, Vector3.down), out RaycastHit myHitInfo, 100f, LayerMask.GetMask("Default"));
            Physics.Raycast(new Ray(targetRaycastPosition, Vector3.down), out RaycastHit targetHitInfo, 100f, LayerMask.GetMask("Default"));

            Vector3 myPoint = myHitInfo.point + .1f * Vector3.up;
            Vector3 targetPoint = targetHitInfo.point + .1f * Vector3.up;

            GridSpace myGridSpace = NavMesh.instance.GetGridSpaceArray(H.Vector3ToGridPosition(myPoint, gridSize));
            GridSpace targetGridSpace = NavMesh.instance.GetGridSpaceArray(H.Vector3ToGridPosition(targetPoint, gridSize));

            farmon.perkList.TryGetValue(new PerkJump().PerkName, out int jumpAbility);
            path = NavMesh.instance.GetPath(myGridSpace, targetGridSpace, jumpAbility, farmon.Flying, (x) => { return Vector3.Distance(x.Center, targetGridSpace.Center); });

            jump = ShouldJumpForNextLink();
        }
        else
        {
            path = new Path();
        }
    }

    private void NodeReached(PathNode previousNode)
    {
        if (jump == true)
        {
            BlockLink nextLink = previousNode.OutputBlockLink;

            float jumpHeight = nextLink.HeightDifference / 2 + 1f;

            Vector3 centerOfBlock = nextLink.ToGridSpace.HitCenter.point;

            JumpState jumpState = new JumpState(farmon, farmon.transform.position, centerOfBlock + farmon.sphereCollider.radius * Vector3.up, jumpHeight);
            farmon.SetState(jumpState);

            //return since we have exited this state.
            return;
        }

        jump = ShouldJumpForNextLink();
    }

    private bool ShouldJumpForNextLink()
    {
        if (path != null && path.nodeList.Count > 0)
        {
            BlockLink nextLink = path.PeekNode().OutputBlockLink;

            if (nextLink != null && !nextLink.walkable && nextLink.jumpable)
            {
                return true;
            }
        }

        return false;
    }

    public override void Tick()
    {
        base.Tick();

        if (!farmon.targetTransform)
        {
            farmon.mainBattleState = new NewIdleState(farmon);
            farmon.SetState(farmon.mainBattleState);
            return;
        }

        farmon.maxSpeed = farmon.GetMovementSpeed();

        if (randomPathUpdateTimer.Tick(Time.deltaTime))
        {
            randomPathUpdateTimer.SetTime(UnityEngine.Random.Range(2f, 3f));
            UpdatePath();
        }

        farmon.FollowPath(path);
    }
}*/

public class WanderState : StateMachineState
{
    Farmon unit;

    public WanderState(Farmon thisUnit)
    {
        unit = thisUnit;
    }

    public override void Enter()
    {
        base.Enter();

        unit.maxSpeed = unit.GetMovementSpeed();
    }

    public override void Tick()
    {
        base.Tick();

        unit.MovementWander();
    }
}
