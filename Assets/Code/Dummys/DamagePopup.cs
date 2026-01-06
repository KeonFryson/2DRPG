using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 1.5f;
    public float lifetime = 1f;

    private TMP_Text text;
    private Vector3 moveDir;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
         
        moveDir = new Vector3(Random.Range(-0.3f, 0.3f), 1f, 0f);

    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += moveDir * floatSpeed * Time.deltaTime;
    }
}
