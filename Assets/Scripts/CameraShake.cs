using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
	private static float shakeDuration;
	
	[SerializeField] private float shakeAmount = 1f;
	[SerializeField] private float decreaseFactor = 0.2f;
	[SerializeField] private Vector3 originalPos;
	
	void OnEnable()
	{
		originalPos = transform.localPosition;
	}

	private void Update()
	{
		if (shakeDuration < 0) shakeDuration = 0f;
		transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount * shakeDuration;
		shakeDuration -= Time.deltaTime * decreaseFactor;
	}

	public static void Shake()
	{
		shakeDuration = 0.15f;
	}
}
