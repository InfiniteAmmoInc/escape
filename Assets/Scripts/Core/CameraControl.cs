using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public new Camera camera;
    public float followSpeed = 1f;
    public float lookAhead = 0f;
    public float playerVelocitySpeed = 1f;
    Player player;
    Vector3 playerVelocity;

    void Start()
    {
        player = Object.FindObjectOfType<Player>();
    }

    void Update()
    {
        if (player != null)
        {
            playerVelocity = Vector3.Lerp(playerVelocity, player.GetComponent<Mover>().velocity, Time.deltaTime * playerVelocitySpeed);
            var targetPosition = player.transform.position + playerVelocity * lookAhead;
            float distance = (targetPosition - transform.position).magnitude;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * followSpeed * distance);
        }
    }
}
