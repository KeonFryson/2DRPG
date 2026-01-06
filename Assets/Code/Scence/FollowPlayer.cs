using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FollowPlayer : MonoBehaviour
{
    private CinemachineCamera vcam;
    private CinemachineConfiner2D confiner;

    void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        AssignFollowTarget();
        AssignCollider();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignFollowTarget();
        AssignCollider();
    }

    void AssignFollowTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;
        }
    }

    void AssignCollider()
    {
        GameObject colliderObject = GameObject.FindGameObjectWithTag("CameraCollider");
        if (colliderObject != null)
        {
            confiner.BoundingShape2D = colliderObject.GetComponent<PolygonCollider2D>();
        }
    }
}
