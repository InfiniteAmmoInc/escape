using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailShrink : MonoBehaviour
{
	TrailRenderer trailRenderer;
	float speed = .5f;

	void Awake()
	{
		trailRenderer = GetComponent<TrailRenderer>();
	}

	void Update()
	{
		trailRenderer.widthMultiplier = Mathf.MoveTowards(trailRenderer.widthMultiplier, 0f, Time.deltaTime * speed);
	}
}
