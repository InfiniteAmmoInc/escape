using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public Transform rig;
    public Transform rigOffset;
    public SpriteRenderer spriteRenderer;
    public GameObject prefabTrail;

    public Sprite spriteFloat;
    public Sprite spriteFly;
    public Sprite spriteFlyBreak;
    public Sprite spriteGround;

    public float burstSpeed;
    public float flyMaxSpeedDeceleration;
    public float downFlyMaxSpeedAcceleration;
    public float downExtraFlyMaxSpeed;
    public float maxFlyTime;
    public float lastFlyFadeTime;
    public float flyTimeRechargedPoint;
    public float flyBurnSpeed = 1f, flyRechargeSpeed = 1f;
    public float flyReleaseSpeedMultiplier = .5f;
    public float flyReleaseTimePenalty = .2f;

    public float maxGlideXSpeed = 1f;
    public float glideAcceleration = 1f;
    public float glideDeceleration;
    public float glideDownAccel;

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
        }
        else
        {
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
        Debug.LogWarning("flyTime: " + flyTime);
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
            flyTime = 0f;
            trailInstance.transform.parent = null;
            var trailShrink = trailInstance.AddComponent<TrailShrink>();
            trailShrink.duration = lastFlyFadeTime;
            Destroy(trailInstance, trailShrink.duration);
            CancelInvoke("ResetFlyTime");
            Invoke("ResetFlyTime", lastFlyFadeTime);
            trailInstance = null;
        }
    }

    void ResetFlyTime()
    {
        if (trailInstance == null)
            flyTime = maxFlyTime;
    }
}
