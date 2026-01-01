
using UnityEngine;

public class Sword : MonoBehaviour
{
    [Header("Sword Settings")]
    [SerializeField] private int damage = 25;

    public void OnHit(Collider2D collision)
    {
        Debug.Log("Sword hit: " + collision.name);

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
