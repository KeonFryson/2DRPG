using UnityEngine;

public class DamgeTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private PlayerController playerController;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private int HealthAmount = 10;
    [SerializeField] private int manaAmount = 10;
    [SerializeField] private int manaConsumeAmount = 10;

    [SerializeField] private bool damagePlayer = false;
    [SerializeField] private bool healthPlayer = false;
    [SerializeField] private bool addManaPlayer = false;
    [SerializeField] private bool consumeManaPlayer = false;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && damagePlayer)
        {
            Debug.Log("Player Health Before Damage: " + playerController.maxHealth);
            playerController.TakeDamge(damageAmount); // Reduce health by 10
            Debug.Log("Player Health After Damage: " + playerController.maxHealth);
        }

        if (collision.gameObject.CompareTag("Player") && healthPlayer)
        {
            Debug.Log("Player Health Before Heal: " + playerController.maxHealth);
            playerController.HealHeath(HealthAmount); // Increase health by 10
            Debug.Log("Player Health After Heal: " + playerController.maxHealth);
        }

        if (collision.gameObject.CompareTag("Player") && addManaPlayer)
        {
            Debug.Log("Player Mana Before Add: " + playerController.maxMana);
            playerController.RestoreMana(manaAmount); // Increase mana by 10
            Debug.Log("Player Mana After Add: " + playerController.maxMana);
        }

        if (collision.gameObject.CompareTag("Player") && consumeManaPlayer)
        {
            Debug.Log("Player Mana Before Consume: " + playerController.maxMana);
            playerController.ConsumeMana(manaConsumeAmount); // Decrease mana by 10
            Debug.Log("Player Mana After Consume: " + playerController.maxMana);
        }
    }
}
