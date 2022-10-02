using UnityEngine;

public class Farmon : Unit
{
    private void Start()
    {
        targetDistance = transform.lossyScale.x / 2 + Player.instance.transform.lossyScale.x/2;
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
}