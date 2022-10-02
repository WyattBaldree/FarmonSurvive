using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static GameObject emptyGameObject = new GameObject();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public static Projectile MakeFreeShot(Vector3 position)
    {
        GameObject newGameObject = Instantiate(ProjectileController.instance.defaultProjectilePrefab, position, Quaternion.identity);
        Projectile newProjectile = newGameObject.GetComponent<Projectile>();
        

        return newProjectile;


    }

    private void Start()
    {

    }


}