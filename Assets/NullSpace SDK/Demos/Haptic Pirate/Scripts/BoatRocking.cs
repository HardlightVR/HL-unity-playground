using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatRocking : MonoBehaviour
{
	[Header("Rolling Waves")]
	public bool RollWithWaves = false;
	public Vector2 xRange = new Vector2(-1, 1);
	public AnimationCurve xPosCurve;
	public float xTimerRate = .1f;
	[SerializeField]
	private float xTimer;

	[Header("Port Starboard Tilting")]
	public bool Tilting = false;
	public Vector2 zRange = new Vector2(-1, 1);
	public AnimationCurve zPosCurve;
	public float zTimerRate = .1f;
	[SerializeField]
	private float zTimer;

	[Header("Float Simulation")]
	public bool FloatSimulation = false;
	public Vector2 yRange = new Vector2(-.25f, .25f);
	public AnimationCurve yPosCurve;
	public float yTimerRate = .1f;

	[SerializeField]
	private float yTimer;

	void Update()
	{
		if (FloatSimulation)
		{
			yTimer += Time.deltaTime * yTimerRate;
			if (yTimer >= 1)
			{
				yTimer -= Mathf.Floor(yTimer);
			}
			float yPos = Mathf.Lerp(yRange.x, yRange.y, yPosCurve.Evaluate(yTimer));
			transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
		}

		if (RollWithWaves || Tilting)
		{
			Vector3 newEulerAngles = transform.rotation.eulerAngles;
			bool dirtyEuler = false;
			float xRot = newEulerAngles.x;
			float zRot = newEulerAngles.z;
			if (RollWithWaves)
			{
				xTimer += Time.deltaTime * xTimerRate;
				if (xTimer >= 1)
				{
					xTimer -= Mathf.Floor(xTimer);
				}
				xRot = Mathf.Lerp(xRange.x, xRange.y, xPosCurve.Evaluate(xTimer));
				dirtyEuler = true;
			}

			if (Tilting)
			{
				zTimer += Time.deltaTime * zTimerRate;
				if (zTimer >= 1)
				{
					zTimer -= Mathf.Floor(zTimer);
				}
				zRot = Mathf.Lerp(zRange.x, zRange.y, zPosCurve.Evaluate(zTimer));
				dirtyEuler = true;
			}

			if (dirtyEuler)
			{
				newEulerAngles = new Vector3(xRot, newEulerAngles.y, zRot);
				transform.rotation = Quaternion.Euler(newEulerAngles);
			}
		}
	}
}
