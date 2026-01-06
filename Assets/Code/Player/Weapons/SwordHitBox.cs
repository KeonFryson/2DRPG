using UnityEngine;

public class HitBox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Sword sword;
    [SerializeField] bool isDebug = false;

    void Start()
    {
        sword = GetComponentInParent<Sword>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDebug)
            Debug.Log($"HitBox collision detected with: {collision.name}, Tag: {collision.tag}");
        sword.OnHit(collision);
    }
}
