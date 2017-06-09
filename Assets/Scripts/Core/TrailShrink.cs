using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailShrink : MonoBehaviour
{
	TrailRenderer trailRenderer;
	public float duration = .5f;

	void Awake()
	{
		trailRenderer = GetComponent<TrailRenderer>();
	}

	void Update()
	{
		float startA = trailRenderer.startColor.a;
		startA = Mathf.MoveTowards(startA, 0f, Time.deltaTime * (1f/duration));
		trailRenderer.startColor = new Color(trailRenderer.startColor.r, trailRenderer.startColor.g, trailRenderer.startColor.b, startA);
		float endA = trailRenderer.endColor.a;
		endA = Mathf.MoveTowards(endA, 0f, Time.deltaTime * (1f/duration));
		trailRenderer.endColor = new Color(trailRenderer.endColor.r, trailRenderer.endColor.g, trailRenderer.endColor.b, endA);
		//trailRenderer.widthMultiplier = Mathf.MoveTowards(trailRenderer.widthMultiplier, 0f, Time.deltaTime * (1f/duration));
	}
}
