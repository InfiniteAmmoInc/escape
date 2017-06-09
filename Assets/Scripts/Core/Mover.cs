using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour
{
    // walk/run/hop along ground
    // jump/fly/float
    public Vector3 gravity;
    public float jumpForce;
    public bool applyGravity = true;
    public float terminalDownY = -15f;
    public float flyMaxSpeed, flyAcceleration;
    public float slideAcceleration;
    public float bouncePower;
    public bool onGround { get; private set; }
    
    SphereCollider sphereCollider;
    [HideInInspector]
    public Vector3 velocity;
    Vector3 upNormal = Vector3.up;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    public void Jump()
    {
        velocity = jumpForce * upNormal;
    }

    void Update()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        if (applyGravity)
            velocity += gravity * Time.deltaTime;

        Debug.LogWarning("velocity.y: " + velocity.y);
        if (velocity.y < terminalDownY)
            velocity.y = terminalDownY;

        CastMove(velocity * Time.deltaTime);
    }

    void CastMove(Vector3 move)
    {
        if (move != Vector3.zero)
        {
            onGround = false;

            var pos = transform.position;
            var dir = move.normalized;
            var totalDistance = move.magnitude;
            float distanceRemaining = totalDistance;
            const float step = .01f;
            bool collided = false;
            Collider[] colliders = null;

            while (distanceRemaining > 0f)
            {
                float useDistance = step;
                if (distanceRemaining < step)
                    useDistance = distanceRemaining;

                var newPos = pos + useDistance * dir;
                distanceRemaining -= useDistance;
                colliders = Physics.OverlapSphere(newPos, sphereCollider.radius, LayerMask.GetMask("Obstruction"));
                if (colliders != null && colliders.Length > 0)
                {
                    collided = true;
                    break;
                }
                else
                {
                    pos = newPos;
                }
            }

            transform.position = pos;

            if (collided)
            {
                RaycastHit raycastHit;
                Ray ray = new Ray(transform.position, Vector3.down);
                if (Physics.Raycast(ray, out raycastHit, sphereCollider.radius * 2f, LayerMask.GetMask("Obstruction")))
                {
                    if (raycastHit.normal.y > .5f)
                    {
                        onGround = true;
                    }
                    else if (raycastHit.normal.y > 0f)
                    {
                        // slide
                        velocity += raycastHit.normal.x * slideAcceleration * Time.deltaTime * Vector3.right;
                    }
                }
                else
                {
                    velocity = -velocity * bouncePower;
                }
            }
        }
    }
}
