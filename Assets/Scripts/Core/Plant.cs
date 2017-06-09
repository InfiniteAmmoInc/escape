using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public Transform rigTransform;
    public Transform lean;
    public SpriteRenderer spriteRenderer;

    public float angleAmount = 30f;
    public float speed = 2f;

    public enum LeanType
    {
        Away,
        Towards,
    }

    public LeanType leanType;
    public float leanRadius = 100f;
    public float leanStrength = .5f;
    public float leanSpeed = 1f;

    
    float timer;

    static int numPlants;

    void Awake()
    {
        timer = Random.value * Mathf.PI * 2f;

        spriteRenderer.sortingOrder = numPlants;
        numPlants++;
    }

    void OnDestroy()
    {
        numPlants--;
    }

    void Update()
    {
        timer += speed * Time.deltaTime;
        rigTransform.localEulerAngles = Vector3.forward * Mathf.Sin(timer) * angleAmount;

        var diff = transform.position - Global.player.transform.position;

        if (leanType == LeanType.Towards)
            diff = Global.player.transform.position - transform.position;

        if (diff.sqrMagnitude < leanRadius * leanRadius)
        {
            float mag = diff.magnitude;
            float p = (leanRadius - mag) / leanRadius;
            var targetUp = diff.normalized * leanStrength + Vector3.up * (1f - leanStrength);
            lean.up = Vector3.Lerp(lean.up, targetUp * p + Vector3.up * (1f - p), Time.deltaTime * leanSpeed);
        }
        else
        {
            lean.up = Vector3.Lerp(lean.up, Vector3.up, Time.deltaTime * leanSpeed);
        }
    }
}
