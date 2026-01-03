
using UnityEngine;

public class Sword : MonoBehaviour
{
    [Header("Sword Settings")]
     
    [SerializeField] private ItemData swordItemData;
    public void OnHit(Collider2D collision)
    {
 

        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Sword hit: " + collision.name);
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(swordItemData.damage);
            }
        }
    }
}
