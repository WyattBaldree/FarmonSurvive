using UnityEngine;
using UnityEngine.Assertions;

public class Projectile : MonoBehaviour
{

    /*public static Projectile MakeFreeShot(Vector3 position, Vector3 velocity, int damage, Sprite sprite)
    {
        GameObject newGameObject = Instantiate(ProjectileController.instance.defaultProjectilePrefab, position, Quaternion.identity);
        Projectile newProjectile = newGameObject.GetComponent<Projectile>();

        newProjectile.rigidBody.velocity = velocity;

        newProjectile.damage = damage;

        newProjectile.spriteRenderer.sprite = sprite;

        return newProjectile;
    }*/

    public Rigidbody2D rigidBody;

    public int damage = 1;

    public SpriteRenderer spriteRenderer;

    public bool destroyOnHit = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if(enemy)
        {
            enemy.ChangeHeath(-damage);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError( "Projectile collided with something that it was not supposed to.", this );
        }
    }

    private void Update()
    {
        if(Vector3.Distance(transform.position, Player.instance.transform.position) > 50)
        {
            Destroy(gameObject);
        }
    }


}