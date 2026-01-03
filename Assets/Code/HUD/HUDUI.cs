using UnityEngine;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    private float HeathPercent;
    private float ManaPercent;
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        hudPanel = this.gameObject;

    }


    // Update is called once per frame
    void Update()
    {
        if (playerController != null)
        {
            // Calculate health percentage (current / max)
            HeathPercent = (float)playerController.GetCurrentHeath() / playerController.maxHealth;
            healthBar.value = HeathPercent;

            // Calculate mana percentage (current / max)
            ManaPercent = (float)playerController.GetCurrentMana() / playerController.maxMana;
            manaBar.value = ManaPercent;
        }
    }
}
