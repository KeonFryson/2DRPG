using UnityEngine;

public class HitBox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Sword sword;

    void Start()
    {
        sword = GetComponentInParent<Sword>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        sword.OnHit(collision);
    }
}
