using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public Transform rig;
    public Transform rigOffset;
    public SpriteRenderer spriteRenderer;
    public GameObject prefabTrail;
    public GameObject prefabLoopBoost;

    public Sprite spriteFloat;
    public Sprite spriteFly;
    public Sprite spriteFlyBreak;
    public Sprite spriteGround;

    public bool doCheckFlyNodesForLoop;
    public bool doLoopBoost;
    public bool requireAverageAngleForLoop;

    public float burstSpeed;
    public float loopBoostSpeed;
    public float loopThresholdAngleMin = 10f;
    public float loopThresholdAngleMax = 20f;
    public float angleToLoop = 360f;
    public float flyMaxSpeedDeceleration;
    public float downFlyMaxSpeedAcceleration;
    public float downExtraFlyMaxSpeed;
    public float maxFlyTime;
    public float flyTimeRechargedPoint;
    public float flyBurnSpeed = 1f, flyRechargeSpeed = 1f;
    public float flyReleaseSpeedMultiplier = .5f;
    public float flyReleaseTimePenalty = .2f;

    public float maxGlideXSpeed = 1f;
    public float glideAcceleration = 1f;
    public float glideDeceleration;
    public float glideDownAccel;

    public class FlyNode
    {
        public FlyNode(Vector3 point)
        {
            this.point = point;
        }
        public Vector3 point;
        public bool markedAsLoop;
        public float angleToNext;
    }

    List<FlyNode> flyNodes = new List<FlyNode>();

    float flyTime;

    GameObject trailInstance;

    public enum State
    {
        None,
        Flying,
    }
    State state;
    Mover mover;
    bool holdingFly, holdingGlide;
    float originalTrailWidth;
    float currentFlyMaxSpeed;

    void Awake()
    {
        mover = GetComponent<Mover>();

        flyTime = maxFlyTime;
        currentFlyMaxSpeed = mover.flyMaxSpeed;
    }

    void Update()
    {
        UpdateInput();
    }

    void UpdateInput()
    {
        if (mover.onGround)
        {
            DetachTrail();
            holdingFly = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CanFly())
            {
                holdingFly = true;

                if (mover.onGround)
                {
                    currentFlyMaxSpeed = burstSpeed;
                    var dir = (Global.GetWorldPosition(Input.mousePosition) - transform.position).normalized;
                    mover.velocity = dir * currentFlyMaxSpeed;
                }
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (CanGlide())
            {
                holdingGlide = true;
            }
        }
        if (holdingFly && !Input.GetMouseButton(0))
        {
            holdingFly = false;
            if (mover.velocity.y > 0f)
            {
                mover.velocity.y *= flyReleaseSpeedMultiplier;
                flyTime -= flyReleaseTimePenalty;
                RefreshFlySprite();
            }
        }
        if (holdingGlide && !Input.GetMouseButton(0))
        {
            holdingGlide = false;
        }
        if (holdingFly && CanFly())
        {
            flyTime -= Time.deltaTime * flyBurnSpeed;
            RefreshFlySprite();

            if (!trailInstance)
                CreateTrail();

            mover.applyGravity = false;
            var diff = (Global.GetWorldPosition(Input.mousePosition) - transform.position);
            var dir = diff.normalized;

            if (diff.sqrMagnitude < 1f)
            {
                dir = Vector3.zero;
            }
            
            currentFlyMaxSpeed = Mathf.MoveTowards(currentFlyMaxSpeed, mover.flyMaxSpeed, Time.deltaTime * flyMaxSpeedDeceleration);

            if (dir.y < 0f)
            {
                float newMaxSpeed = mover.flyMaxSpeed + Mathf.Abs(dir.y) * downExtraFlyMaxSpeed;
                if (newMaxSpeed > currentFlyMaxSpeed)
                {
                    currentFlyMaxSpeed = Mathf.MoveTowards(currentFlyMaxSpeed, newMaxSpeed, Time.deltaTime * downFlyMaxSpeedAcceleration);
                }
            }

            if (dir != Vector3.zero)
                mover.velocity = Vector3.MoveTowards(mover.velocity, dir * currentFlyMaxSpeed, Time.deltaTime * mover.flyAcceleration);
            
            if (mover.velocity != Vector3.zero)
                rig.transform.up = mover.velocity.normalized;
            
            if (Mathf.Sign(mover.velocity.x) != Mathf.Sign(rig.transform.localScale.x))
            {
                var lscale = rig.transform.localScale;
                lscale.x *= -1f;
                rig.transform.localScale = lscale;
            }

            spriteRenderer.sprite = spriteFly;
            rigOffset.localPosition = Vector3.zero;

            if (trailInstance)
            {
                trailInstance.GetComponent<TrailRenderer>().widthMultiplier = originalTrailWidth * (flyTime / maxFlyTime);
            }

            flyNodes.Add(new FlyNode(transform.position));
            if (doCheckFlyNodesForLoop)
                CheckFlyNodesForLoop();
        }
        else
        {
            flyNodes.Clear();

            holdingFly = false;
            DetachTrail();

            mover.applyGravity = true;
            
            RefreshFlySprite();

            rigOffset.localPosition = Vector3.zero;

            if (holdingGlide && CanGlide())
            {
                var dir = (Global.GetWorldPosition(Input.mousePosition) - transform.position).normalized;
                mover.velocity.x = Mathf.MoveTowards(mover.velocity.x, dir.x * maxGlideXSpeed, Time.deltaTime * glideAcceleration);
                if (Vector3.Dot(dir, Vector3.down) > .9f)
                {
                    mover.velocity.y = Mathf.MoveTowards(mover.velocity.y, mover.terminalDownY, Time.deltaTime * glideDownAccel);
                }
            }
            else
            {
                mover.velocity.x = Mathf.MoveTowards(mover.velocity.x, 0f, Time.deltaTime * glideDeceleration);
            }
        }

        if (mover.onGround)
        {
            spriteRenderer.sprite = spriteGround;
            rigOffset.localPosition = Vector3.up * .5f;
            rig.transform.up = Vector3.up;
            flyTime = maxFlyTime;
        }
    }

    bool CanFly()
    {
        return (flyTime > 0f);
    }

    bool CanStartFly()
    {
        return mover.onGround && CanFly();
    }

    bool CanGlide()
    {
        return !CanStartFly() && !mover.onGround && !holdingFly;
    }

    void CreateTrail()
    {
        trailInstance = (GameObject)Instantiate(prefabTrail);
        trailInstance.transform.parent = this.transform;
        trailInstance.transform.localPosition = new Vector3(0f, 0f, .5f);
        originalTrailWidth = trailInstance.GetComponent<TrailRenderer>().widthMultiplier;
    }

    void RefreshFlySprite()
    {
        if (flyTime > 0f)
        {
            if (holdingFly)
            {
                spriteRenderer.sprite = spriteFly;
            }
            else
            {
                spriteRenderer.sprite = spriteFlyBreak;
                rig.transform.up = Vector3.up;
            }
        }
        else
        {
            spriteRenderer.sprite = spriteFloat;
            rig.transform.up = Vector3.up;
        }
    }

    void DetachTrail()
    {
        if (trailInstance != null)
        {
            trailInstance.transform.parent = null;
            trailInstance.AddComponent<TrailShrink>();
            Destroy(trailInstance, 15f);
            trailInstance = null;
        }
    }

    void CheckFlyNodesForLoop()
    {
        if (doCheckFlyNodesForLoop)
        {
            bool foundLoop = false;
            int loopEndIndex = 0;
            float totalAngle = 0f;
            for (int i = 1; i < flyNodes.Count-1; i++)
            {
                if (!flyNodes[i].markedAsLoop)
                {
                    var diff = flyNodes[i+1].point - flyNodes[i].point;
                    var diff0 = flyNodes[i].point - flyNodes[i-1].point;
                    float angleBetween = Vector3.Angle(diff0.normalized, diff.normalized);
                    var cross = Vector3.Cross(diff0.normalized, diff.normalized);
                    if (cross.z < 0f)
                        angleBetween = -angleBetween;
                    
                    //if (Mathf.Abs(angleBetween) < loopThresholdAngle)
                    if (false)
                    {
                        totalAngle = 0f;
                    }
                    else
                    {
                        
                        totalAngle += angleBetween;
                        //Debug.LogWarning("i: " + i + " angleBetween: " + angleBetween + " totalAngle: " + totalAngle);

                        flyNodes[i].angleToNext = angleBetween;
                        if (Mathf.Abs(totalAngle) > angleToLoop)
                        {
                            Debug.LogWarning("foundLoop!");
                            foundLoop = true;
                            loopEndIndex = i;
                            break;
                        }
                    }
                }
            }

            if (foundLoop)
            {
                float remainingAngle = totalAngle;
                Vector3 totalPoint = Vector3.zero;
                int count = 0;
                int otherIndex = 0;
                float averageAngle = 0f;
                for (int i = loopEndIndex; i >= 0; i --)
                {
                    remainingAngle -= flyNodes[i].angleToNext * Mathf.Sign(totalAngle);
                    flyNodes[i].markedAsLoop = true;
                    totalPoint += flyNodes[i].point;
                    count++;
                    averageAngle += flyNodes[i].angleToNext;
                    if ((Mathf.Sign(totalAngle) > 0f && remainingAngle < 0f) || (Mathf.Sign(totalAngle) < 0f && remainingAngle > 0f))
                    {
                        otherIndex = i;
                        // we're done!
                        break;
                    }
                }

                averageAngle /= (float)count;

                Debug.LogError("averageAngle: " + averageAngle + " time: " + Time.time);
                if (!requireAverageAngleForLoop || (Mathf.Abs(averageAngle) > loopThresholdAngleMin && Mathf.Abs(averageAngle) < loopThresholdAngleMax))
                {
                    Vector3 averagePoint = totalPoint / (float)count;
                    //averagePoint = (averagePoint * .25f + transform.position * .75f);

                    LoopBoost(flyNodes[loopEndIndex].point, averagePoint);
                }

                flyNodes.Clear();

            }
        }
    }

    void LoopBoost(Vector3 endPoint, Vector3 averagePoint)
    {
        if (doLoopBoost)
        {
            flyNodes.Clear();
            flyTime = maxFlyTime;
            //DetachTrail();
            currentFlyMaxSpeed = loopBoostSpeed;
            mover.velocity = mover.velocity.normalized * currentFlyMaxSpeed;
            //holdingFly = false;
            Instantiate(prefabLoopBoost, endPoint, Quaternion.identity);
            //Instantiate(prefabLoopBoost, averagePoint, Quaternion.identity);
            var go = (GameObject)Instantiate(prefabLoopBoost, transform.position, Quaternion.identity);
            go.transform.parent = transform;
        }
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < flyNodes.Count; i++)
        {
            Gizmos.color = Color.black;
            if (flyNodes[i].markedAsLoop)
                Gizmos.color = Color.yellow * .5f + Color.red * .5f;
            Gizmos.DrawWireSphere(flyNodes[i].point, .25f);
        }
    }
}
