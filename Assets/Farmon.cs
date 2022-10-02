using System.Collections;
using UnityEngine;

public class Farmon : Unit
{
    [SerializeField]
    private GameObject fireBallPrefab;

    private void Start()
    {
        targetDistance = transform.lossyScale.x / 2 + Player.instance.transform.lossyScale.x/2;

        StartCoroutine(ShootFireBalls());
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();

        MoveWithPlayer();
    }

    void MoveWithPlayer()
    {
        myRigidBody.velocity += Player.instance.myRigidBody.velocity * .9f;
    }

    private void OnDrawGizmos()
    {
        if(EnemyController.instance && EnemyController.instance.ClosestEnemy) Debug.DrawLine(transform.position, EnemyController.instance.ClosestEnemy.transform.position);
    }

    private IEnumerator ShootFireBalls()
    {
        while (true)
        {
            Enemy targetEnemy = EnemyController.instance.ClosestEnemy;

            if (targetEnemy)
            {
                Projectile fireBall = Instantiate(fireBallPrefab, transform.position, transform.rotation).GetComponent<Projectile>();

                fireBall.rigidBody.velocity = targetEnemy.GetUnitVectorToMe(transform.position) * 5f;

                yield return new WaitForSeconds(4);
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}