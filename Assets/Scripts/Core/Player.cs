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
    public Sprite spriteGround;

    public float maxFlyTime;
    public float flyTimeRechargedPoint;
    public float flyBurnSpeed = 1f, flyRechargeSpeed = 1f;

    public float maxGlideXSpeed = 1f;
    public float glideAcceleration = 1f;

    public enum FlyTimeMode
    {
        CanFly,
        Recharging,
    }
    FlyTimeMode flyTimeMode;
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

    void Awake()
    {
        mover = GetComponent<Mover>();

        flyTime = maxFlyTime;
    }

    void Update()
    {
        UpdateInput();
    }

    void UpdateInput()
    {
        switch (flyTimeMode)
        {
            case FlyTimeMode.CanFly:
                break;
            case FlyTimeMode.Recharging:
                if (mover.onGround)
                    flyTime = maxFlyTime;
                //else
                //    flyTime += Time.deltaTime * flyRechargeSpeed;

                //if (flyTime >= maxFlyTime)
                //{
                //    flyTime = maxFlyTime;
                //    flyTimeMode = FlyTimeMode.CanFly;
                //}
                break;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CanFly())
            {
                holdingFly = true;
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
        }
        if (holdingGlide && !Input.GetMouseButton(0))
        {
            holdingGlide = false;
        }
        if (holdingFly && CanFly())
        {
            flyTimeMode = FlyTimeMode.CanFly;
            flyTime -= Time.deltaTime * flyBurnSpeed;

            if (!trailInstance)
                CreateTrail();

            float currentMaxFlySpeed = mover.flyMaxSpeed;

            mover.applyGravity = false;
            var dir = (Global.GetWorldPosition(Input.mousePosition) - transform.position).normalized;
            /*
            float dot = 1f;
            if (mover.velocity != Vector3.zero && dir != Vector3.zero)
            {
                dot = Vector3.Dot(dir.normalized, mover.velocity.normalized);
                Debug.LogWarning("dot: " + dot);
                currentMaxFlySpeed *= (1f-dot) + 1f;
            }
            */

            mover.velocity = Vector3.MoveTowards(mover.velocity, dir * currentMaxFlySpeed, mover.flyAcceleration);
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
            flyTimeMode = FlyTimeMode.Recharging;
            DetachTrail();

            mover.applyGravity = true;
            rig.transform.up = Vector3.up;

            spriteRenderer.sprite = spriteFloat;
            rigOffset.localPosition = Vector3.zero;

            if (holdingGlide && CanGlide())
            {
                var dir = (Global.GetWorldPosition(Input.mousePosition) - transform.position).normalized;
                float x = mover.velocity.x;
                x = Mathf.MoveTowards(x, dir.x * maxGlideXSpeed, Time.deltaTime * glideAcceleration);
                mover.velocity.x = x;
            }
        }

        if (mover.onGround)
        {
            spriteRenderer.sprite = spriteGround;
            rigOffset.localPosition = Vector3.up * .5f;
        }
    }

    bool CanFly()
    {
        return (flyTimeMode == FlyTimeMode.CanFly && flyTime > 0f) || (flyTimeMode == FlyTimeMode.Recharging && flyTime >= flyTimeRechargedPoint);
    }

    bool CanGlide()
    {
        return !CanFly() && !mover.onGround;
    }

    void CreateTrail()
    {
        trailInstance = (GameObject)Instantiate(prefabTrail);
        trailInstance.transform.parent = this.transform;
        trailInstance.transform.localPosition = new Vector3(0f, 0f, .5f);
        originalTrailWidth = trailInstance.GetComponent<TrailRenderer>().widthMultiplier;
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
}
