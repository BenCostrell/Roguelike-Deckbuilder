using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    private Vector2 basePos;
    private bool initialized;
    [SerializeField]
    private Vector3 offset;
    // Use this for initialization
    void Start()
    {
        
    }

    public void InitCamera()
    {
        mainCamera = GetComponent<Camera>();
        transform.position = new Vector3(
            Services.MapManager.width / 2, 
            Services.MapManager.height / 2, 
            transform.position.z) + offset;
        basePos = transform.position;
        //Vector3 playerPos = Services.GameManager.player.controller.transform.position;
        //transform.position = new Vector3(
        //    playerPos.x + basePos.x,
        //    playerPos.y + basePos.y,
        //    transform.position.z);
        //initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        //if(Services.GameManager.player.controller != null && initialized)
        //    MoveTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        Vector3 playerPos = Services.GameManager.player.controller.transform.position;
        Vector3 targetPos = new Vector3(
            playerPos.x + basePos.x,
            playerPos.y + basePos.y,
            transform.position.z);
        Vector3 newPos = Vector3.Lerp(transform.position, targetPos, 0.05f);
        if ((newPos - transform.position).magnitude < 0.0001f)
        {
            newPos = targetPos;
        }
        transform.position = newPos;
    }
}
