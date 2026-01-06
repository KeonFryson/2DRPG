using UnityEngine;

public class Dummy : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject damagePopupPrefab;
    private new void Start()
    {
        base.Start();
        moveSpeed = 0f; // Dummy does not move
        maxHealth = 10000;
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    private new void Update()
    {
       
        base.Update();
        currentHealth = maxHealth;
    }

    public override void TakeDamage(int damage)
    {
        Debug.Log("Dummy took damage: " + damage);
        GameObject popup = Instantiate(
            damagePopupPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        popup.GetComponent<DamagePopup>().Setup(damage);
    }
}
