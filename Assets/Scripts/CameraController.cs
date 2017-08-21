using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    // Use this for initialization
    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        Vector3 playerPos = Services.GameManager.player.controller.transform.position;
        Vector3 targetPos = new Vector3(
            playerPos.x + 5,
            transform.position.y,
            transform.position.z);
        Vector3 newPos = Vector3.Lerp(transform.position, targetPos, 0.05f);
        if ((newPos - transform.position).magnitude < 0.0001f)
        {
            newPos = targetPos;
        }
        transform.position = newPos;
    }
}
