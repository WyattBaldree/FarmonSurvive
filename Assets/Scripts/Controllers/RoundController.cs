using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    public static RoundController Instance;

    [SerializeField]
    Animator RoundUIAnimator;

    public List<uint> EnemyTeam1Ids;

    bool startingRound = false;
    public bool RoundPlaying = false;

    public bool spawnEnemyTeam = true;

    private void Start()
    {
        StartMatch();
    }

    public void StartMatch()
    {
        NavMesh.instance.GenerateNavMesh();

        SpawnPlayerTeam();
        SpawnEnemyTeam();

        List<Farmon> playerTeam = Player.instance.GetFarmon();
        List<Farmon> enemyTeam = GetEnemyTeam();

        for (int i = 0; i < (int)Mathf.Min(playerTeam.Count, EnemyTeam1Ids.Count); i++)
        {
            if (playerTeam[i] && enemyTeam[i])
            {
                playerTeam[i].attackTarget = enemyTeam[i].loadedFarmonMapId;
                enemyTeam[i].attackTarget = playerTeam[i].loadedFarmonMapId;

                playerTeam[i].mainState = new AttackState(playerTeam[i]);
                enemyTeam[i].mainState = new AttackState(enemyTeam[i]);

                playerTeam[i].SetState(playerTeam[i].mainState);
                enemyTeam[i].SetState(enemyTeam[i].mainState);
            }
        }

        startingRound = true;
        RoundUIAnimator.Play("RoundUIRoundStart", 0);
    }

    public List<Farmon> GetEnemyTeam()
    {
        List<Farmon> newList = new List<Farmon>();
        foreach (uint id in EnemyTeam1Ids)
        {
            newList.Add(Farmon.loadedFarmonMap[id]);
        }

        return newList;
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
        foreach (uint i in Player.instance.FarmonSquadSaveIds)
        {
            if(i != 0)
            {
                Farmon newFarmon = Farmon.ConstructFarmon(SaveController.LoadFarmonPlayer(i), true).GetComponent<Farmon>();
                newFarmon.team = Player.instance.playerTeam;
                newFarmon.Initialize();
            }
        }

        List<Farmon> playerTeam = Player.instance.GetFarmon();
        float gridSize = LevelController.Instance.gridSize;

        if (playerTeam[0])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(1, 1, 0), gridSize);
            playerTeam[0].transform.position = gridPosition + gridSize * Vector3.one / 2;
            playerTeam[0].team = Farmon.TeamEnum.team1;
        }

        if (playerTeam[1])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(0, 1, 1), gridSize);
            playerTeam[1].transform.position = gridPosition + gridSize * Vector3.one / 2;
            playerTeam[1].team = Farmon.TeamEnum.team1;
        }

        if (playerTeam[2])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(1, 1, 1), gridSize);
            playerTeam[2].transform.position = gridPosition + gridSize * Vector3.one / 2;
            playerTeam[2].team = Farmon.TeamEnum.team1;
        }
        if (playerTeam[3])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(2, 1, 1), gridSize);
            playerTeam[3].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (playerTeam[4])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(1, 1, 2), gridSize);
            playerTeam[4].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
    }

    private void SpawnEnemyTeam()
    {
        EnemyTeam1Ids.Clear();

        if (!spawnEnemyTeam) return;

        EnemyTeam1Ids.Add(Farmon.ConstructFarmon(SaveController.LoadFarmon("wave1enemy1")).GetComponent<Farmon>().loadedFarmonMapId);
        EnemyTeam1Ids.Add(Farmon.ConstructFarmon(SaveController.LoadFarmon("wave1enemy2")).GetComponent<Farmon>().loadedFarmonMapId);
        EnemyTeam1Ids.Add(Farmon.ConstructFarmon(SaveController.LoadFarmon("wave1enemy3")).GetComponent<Farmon>().loadedFarmonMapId);
        EnemyTeam1Ids.Add(Farmon.ConstructFarmon(SaveController.LoadFarmon("wave1enemy4")).GetComponent<Farmon>().loadedFarmonMapId);
        EnemyTeam1Ids.Add(Farmon.ConstructFarmon(SaveController.LoadFarmon("wave1enemy5")).GetComponent<Farmon>().loadedFarmonMapId);

        float gridSize = LevelController.Instance.gridSize;
        Vector3Int levelExtents = LevelController.Instance.levelSize - Vector3Int.one;

        List<Farmon> enemyTeam = GetEnemyTeam();

        foreach (Farmon farmon in enemyTeam)
        {
            farmon.team = Farmon.TeamEnum.team2;
            farmon.Initialize();
        }

        if (enemyTeam.Count > 0 && enemyTeam[0])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 1, 1, levelExtents.z - 0), gridSize);
            enemyTeam[0].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (enemyTeam.Count > 1 && enemyTeam[1])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 0, 1, levelExtents.z - 1), gridSize);
            enemyTeam[1].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (enemyTeam.Count > 2 && enemyTeam[2])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 1, 1, levelExtents.z - 1), gridSize);
            enemyTeam[2].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (enemyTeam.Count > 3 && enemyTeam[3])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 2, 1, levelExtents.z - 1), gridSize);
            enemyTeam[3].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
        if (enemyTeam.Count > 4 && enemyTeam[4])
        {
            Vector3 gridPosition = H.GridPositionToVector3(new Vector3Int(levelExtents.x - 1, 1, levelExtents.z - 2), gridSize);
            enemyTeam[4].transform.position = gridPosition + gridSize * Vector3.one / 2;
        }
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
            List<uint> playerTeam = Player.instance.LoadedFarmon;
            List<Farmon> enemyTeam = GetEnemyTeam();

            // See if team 1 has lost
            bool allFarmonDead = true;
            foreach (uint id in playerTeam)
            {
                Farmon farmon = Farmon.loadedFarmonMap[id];
                if (farmon && !farmon.dead)
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
            foreach (Farmon f in enemyTeam)
            {
                if (f && !f.dead)
                {
                    allFarmonDead = false;
                }
            }

            if (allFarmonDead && spawnEnemyTeam) // if all team 2 farmon are dead
            {
                GameController.SlowMo(13f, .175f);

                //Team 1 wins
                RoundUIAnimator.Play("RoundUIRoundWin", 0);
                RoundPlaying = false;

                StartCoroutine(EndOfRoundCoroutine());
            }
        }
    }

    IEnumerator EndOfRoundCoroutine()
    {
        yield return new WaitForSeconds(2.5f);

        EndOfRoundScreen.instance.Popup(this);
    }
}
