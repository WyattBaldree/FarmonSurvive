using UnityEngine;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void Start()
    {

    }


}