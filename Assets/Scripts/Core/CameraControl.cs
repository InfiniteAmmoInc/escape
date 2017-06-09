using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public new Camera camera;
    public float smoothTime = 0.3f;
    public float lookAhead = 0f;
    public float playerVelocitySpeed = 1f;
    Player player;
    Vector3 playerVelocity;
    Vector3 cameraVelocity;

    void Start()
    {
        player = Object.FindObjectOfType<Player>();
    }

    void Update()
    {
        if (player != null)
        {
            //playerVelocity = Vector3.Lerp(playerVelocity, player.GetComponent<Mover>().velocity, Time.deltaTime * playerVelocitySpeed);
            var targetPosition = player.transform.position + playerVelocity * lookAhead;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref cameraVelocity, smoothTime);
            /*
            float distance = (targetPosition - transform.position).magnitude;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * followSpeed * distance);
            */
        }
    }
}
