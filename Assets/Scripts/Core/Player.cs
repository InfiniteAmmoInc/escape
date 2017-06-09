using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public Transform rig;
    public SpriteRenderer spriteRenderer;
    public GameObject prefabTrail;

    public Sprite spriteFloat;
    public Sprite spriteFly;
    public Sprite spriteGround;

    public float maxFlyTime;
    public float flyTimeRechargedPoint;
    public float flyBurnSpeed = 1f, flyRechargeSpeed = 1f;

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
    bool holdingFly;

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
                else
                    flyTime += Time.deltaTime * flyRechargeSpeed;

                if (flyTime >= maxFlyTime)
                {
                    flyTime = maxFlyTime;
                    flyTimeMode = FlyTimeMode.CanFly;
                }
                break;
        }

        if (Input.GetMouseButtonDown(0) && CanFly())
        {
            holdingFly = true;
        }
        if (holdingFly && !Input.GetMouseButton(0))
        {
            holdingFly = false;
        }
        if (holdingFly && CanFly())
        {
            flyTimeMode = FlyTimeMode.CanFly;
            flyTime -= Time.deltaTime * flyBurnSpeed;

            if (!trailInstance)
                CreateTrail();

            mover.applyGravity = false;
            var dir = (Global.GetWorldPosition(Input.mousePosition) - transform.position).normalized;
            mover.velocity = Vector3.MoveTowards(mover.velocity, dir * mover.flyMaxSpeed, mover.flyAcceleration);
            if (mover.velocity != Vector3.zero)
                rig.transform.up = mover.velocity.normalized;
            if (Mathf.Sign(mover.velocity.x) != Mathf.Sign(rig.transform.localScale.x))
            {
                var lscale = rig.transform.localScale;
                lscale.x *= -1f;
                rig.transform.localScale = lscale;
            }

            spriteRenderer.sprite = spriteFly;
        }
        else
        {
            holdingFly = false;
            flyTimeMode = FlyTimeMode.Recharging;
            DetachTrail();

            mover.applyGravity = true;
            rig.transform.up = Vector3.up;

            spriteRenderer.sprite = spriteFloat;
        }

        if (mover.onGround)
        {
            spriteRenderer.sprite = spriteGround;
        }
    }

    bool CanFly()
    {
        return (flyTimeMode == FlyTimeMode.CanFly && flyTime > 0f) || (flyTimeMode == FlyTimeMode.Recharging && flyTime >= flyTimeRechargedPoint);
    }

    void CreateTrail()
    {
        trailInstance = (GameObject)Instantiate(prefabTrail);
        trailInstance.transform.parent = this.transform;
        trailInstance.transform.localPosition = new Vector3(0f, 0f, .5f);
    }

    void DetachTrail()
    {
        if (trailInstance != null)
        {
            trailInstance.transform.parent = null;
            Destroy(trailInstance, 15f);
            trailInstance = null;
        }
    }
}
