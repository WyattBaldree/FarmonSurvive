using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    public static RoundController Instance;

    [SerializeField]
    Animator RoundUIAnimator;

    public List<Farmon> FarmonTeam1;
    public List<Farmon> FarmonTeam2;

    bool startingRound = false;
    public bool RoundPlaying = false;

    // Load all farmon
    // wait for input
    // start round

    private void Start()
    {
        StartMatch();
    }

    public void StartMatch()
    {
        NavMesh.instance.GenerateNavMesh();

        SpawnPlayerTeam();
        SpawnEnemyTeam();

        for (int i = 0; i < (int)Mathf.Min(FarmonTeam1.Count, FarmonTeam2.Count); i++)
        {
            if (FarmonTeam1[i] && FarmonTeam2[i])
            {
                FarmonTeam1[i].attackTarget = FarmonTeam2[i];
                FarmonTeam2[i].attackTarget = FarmonTeam1[i];
            }
        }

        startingRound = true;
        RoundUIAnimator.Play("RoundUIRoundStart", 0);
    }


    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void SpawnPlayerTeam()
    {
        FarmonTeam1.Clear();

        foreach (uint i in Player.instance.FarmonSquadIds)
        {
            if(i != 0)
            {
                FarmonTeam1.Add(SaveController.LoadFarmonPlayer(i).GetComponent<Farmon>());
            }
        }

        foreach (Farmon farmon in FarmonTeam1)
        {
            farmon.team = Farmon.TeamEnum.team1;
        }

        float gridSize = LevelController.Instance.gridSize;

        if (FarmonTeam1[0])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(1, 1, 0), gridSize);
            FarmonTeam1[0].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (FarmonTeam1[1])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(0, 1, 1), gridSize);
            FarmonTeam1[1].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (FarmonTeam1[2])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(1, 1, 1), gridSize);
            FarmonTeam1[2].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        //if (FarmonTeam1[3])
        //{
        //    Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(2, 1, 1), gridSize);
        //    FarmonTeam1[3].transform.position = gridPosition + gridSize * Vector3.one / 2;
        //}
        //if (FarmonTeam1[4])
        //{
        //    Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(1, 1, 2), gridSize);
        //    FarmonTeam1[4].transform.position = gridPosition + gridSize * Vector3.one / 2;
        //}
    }

    private void SpawnEnemyTeam()
    {
        FarmonTeam2.Clear();

        FarmonTeam2.Add(SaveController.LoadFarmon("wave1enemy1").GetComponent<Farmon>());
        FarmonTeam2.Add(SaveController.LoadFarmon("wave1enemy2").GetComponent<Farmon>());
        FarmonTeam2.Add(SaveController.LoadFarmon("wave1enemy3").GetComponent<Farmon>());
        //FarmonTeam2.Add(SaveController.LoadFarmon("wave1enemy4").GetComponent<Farmon>());
        //FarmonTeam2.Add(SaveController.LoadFarmon("wave1enemy5").GetComponent<Farmon>());

        float gridSize = LevelController.Instance.gridSize;
        Vector3Int levelExtents = LevelController.Instance.levelSize - Vector3Int.one;

        foreach(Farmon farmon in FarmonTeam2)
        {
            farmon.team = Farmon.TeamEnum.team2;
        }

        if (FarmonTeam2.Count > 0 && FarmonTeam2[0])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 1, 1, levelExtents.z - 0), gridSize);
            FarmonTeam2[0].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (FarmonTeam2.Count > 1 && FarmonTeam2[1])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 0, 1, levelExtents.z - 1), gridSize);
            FarmonTeam2[1].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (FarmonTeam2.Count > 2 && FarmonTeam2[2])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 1, 1, levelExtents.z - 1), gridSize);
            FarmonTeam2[2].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        //if (FarmonTeam2.Count > 3 && FarmonTeam2[3])
        //{
        //    Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 2, 1, levelExtents.z - 1), gridSize);
        //    FarmonTeam2[3].transform.position = gridPosition + gridSize * Vector3.one / 2;
        //}
        //if (FarmonTeam2.Count > 4 && FarmonTeam2[4])
        //{
        //    Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 1, 1, levelExtents.z - 2), gridSize);
        //    FarmonTeam2[4].transform.position = gridPosition + gridSize * Vector3.one / 2;
        //}
    }

    private void Update()
    {
        if(startingRound && RoundUIAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            startingRound = false;
            RoundPlaying = true;
        }

        if (RoundPlaying)
        {
            // See if team 1 has lost
            bool allFarmonDead = true;
            foreach (Farmon f in FarmonTeam1)
            {
                if (f && !f.dead)
                {
                    allFarmonDead = false;
                }
            }

            if (allFarmonDead) // if all team 1 farmon are dead
            {
                //Team 2 wins
                RoundUIAnimator.Play("RoundUIRoundLose", 0);
                RoundPlaying = false;
                return;
            }

            // See if team 2 has lost
            allFarmonDead = true;
            foreach (Farmon f in FarmonTeam2)
            {
                if (f && !f.dead)
                {
                    allFarmonDead = false;
                }
            }

            if (allFarmonDead) // if all team 2 farmon are dead
            {
                //Team 1 wins
                RoundUIAnimator.Play("RoundUIRoundWin", 0);
                RoundPlaying = false;

                StartCoroutine(EndOfRoundCoroutine());
            }
        }
    }

    IEnumerator EndOfRoundCoroutine()
    {
        yield return new WaitForSeconds(1);

        EndOfRoundScreen.instance.Popup(this);
    }
}
