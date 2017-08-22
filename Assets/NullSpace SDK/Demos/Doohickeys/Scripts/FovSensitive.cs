using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NullSpace.SDK.Demos
{
	[Serializable]
	public class MyEvent : UnityEvent { }
	public class FovSensitive : MonoBehaviour
	{
		//[Serializable]
		//public class MyEvent : UnityEvent<string, GameObject> { }
		public UnityEvent OnGazeOver;
		public UnityEvent OnGazeExit;

		private Camera cameraWithFOV;
		public GameObject playerCamera;
		bool wasGazedUpon = false;
		bool gazedUpon = false;

		void Start()
		{
			//checkAgainst = VRMimic.Instance.VRCamera.GetComponent<Camera>();
			cameraWithFOV = playerCamera.GetComponent<Camera>();
		}

		void Update()
		{
			if (cameraWithFOV)
			{
				var viewportPoint = cameraWithFOV.WorldToViewportPoint(transform.position);
				//Debug.Log("Viewport Point: " + viewportPoint + "\n", this);

				if (viewportPoint.x > 0 && viewportPoint.y > 0 && viewportPoint.x < 1 && viewportPoint.y < 1)
				{
					gazedUpon = true;
				}
				else
				{
					gazedUpon = false;
				}

				CheckAndCallGazeEvents();
			}
		}

		private void CheckAndCallGazeEvents()
		{
			if (gazedUpon != wasGazedUpon)
			{
				if (gazedUpon)
				{
					OnGazeOver.Invoke();
					Debug.Log("OnGazeOver Invoked\n", this);
					wasGazedUpon = true;
				}
				else
				{
					Debug.Log("OnGazeExit Invoked\n", this);
					OnGazeExit.Invoke();
					wasGazedUpon = false;
				}
			}
		}
	}
}