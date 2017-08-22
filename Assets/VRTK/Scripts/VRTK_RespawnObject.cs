using System;
using UnityEngine;
using System.Collections;

namespace VRTK
{
	public class VRTK_RespawnObject : MonoBehaviour
	{
		//Position
		[SerializeField]
		private Vector3 resetPosition;
		//Rotation
		[SerializeField]
		private Quaternion resetRotation;
		//Scale
		[SerializeField]
		private Vector3 resetScale;

		//private float ResetAtDistFromSpawn;
		private float ResetAtTotalFlatDistFromBounds = 13;
		private bool disabledGrab;
		private bool disabledUse;
		public VRTK_InteractableObject myself;
		public Rigidbody myRB;

		private enum RespawnObjectState { Valid, Despawning, Waiting, Respawning }
		private RespawnObjectState myState = RespawnObjectState.Valid;

		public Coroutine DespawningCorout;
		public Coroutine RespawningCorout;
		public bool Despawning = false;
		public bool Respawning = false;
		public int FramesToGrow = 15;

		#region Start & Update
		void Start()
		{
			myself = GetComponent<VRTK_InteractableObject>();
			myRB = myself.GetComponent<Rigidbody>();

			UpdateResetPosition();
			Despawn(0.0f, 0.0f);
		}
		void Update()
		{
			if (CheckDespawn())
			{
				StartDespawn(3.0f, 1.0f);
			}
		}
		public void UpdateResetPosition()
		{
			resetPosition = myself.transform.position;
			resetRotation = myself.transform.rotation;
			resetScale = myself.transform.localScale;
		}
		#endregion

		void ClearRespawnOrDespawnProgress()
		{
			if (DespawningCorout != null)
			{
				StopCoroutine(DespawningCorout);
			}
			if (RespawningCorout != null)
			{
				StopCoroutine(RespawningCorout);
			}
			myState = RespawnObjectState.Valid;
		}

		#region Despawn
		bool CheckDespawn()
		{
			if (myState == RespawnObjectState.Valid)
			{
				//Turn off myself.
				Valve.VR.HmdQuad_t quad = new Valve.VR.HmdQuad_t();
				SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref quad);

				//Debug.DrawLine(GetPoint(quad.vCorners0), Vector3.one, Color.red, 5.0f);
				//Debug.DrawLine(GetPoint(quad.vCorners1), Vector3.one, Color.green, 5.0f);
				//Debug.DrawLine(GetPoint(quad.vCorners2), Vector3.one, Color.grey, 5.0f);
				//Debug.DrawLine(GetPoint(quad.vCorners3), Vector3.one, Color.yellow, 5.0f);
				bool[] FarFromCorner = new bool[4];
				float flatDistAway = 0;
				flatDistAway += FlatDistFrom(GetPoint(quad.vCorners0));
				flatDistAway += FlatDistFrom(GetPoint(quad.vCorners1));
				flatDistAway += FlatDistFrom(GetPoint(quad.vCorners2));
				flatDistAway += FlatDistFrom(GetPoint(quad.vCorners3));
				//failCount += Convert.ToInt32(FlatDistFrom(GetPoint(quad.vCorners0)) > ResetAtDistFromBounds);
				//failCount += Convert.ToInt32(FlatDistFrom(GetPoint(quad.vCorners1)) > ResetAtDistFromBounds);
				//failCount += Convert.ToInt32(FlatDistFrom(GetPoint(quad.vCorners2)) > ResetAtDistFromBounds);
				//failCount += Convert.ToInt32(FlatDistFrom(GetPoint(quad.vCorners3)) > ResetAtDistFromBounds);

				//Debug.Log(failCount + "\n");
				if (flatDistAway >= 13)
				{
					return true;
				}
			}
			return false;
		}

		public void StartDespawn(float delayDespawn, float respawnWaitTime)
		{
			ClearRespawnOrDespawnProgress();
			if (myState == RespawnObjectState.Valid)
			{
				if (myself.IsGrabbed())
				{
					myself.Ungrabbed(myself.GetGrabbingObject());
				}
				myState = RespawnObjectState.Despawning;
				//Shrink myself out of existence
				//Display a POOF particle.

				DespawningCorout = StartCoroutine(Despawn(delayDespawn, respawnWaitTime));
			}
		}

		IEnumerator Despawn(float delayDespawn, float respawnWaitTime)
		{
			yield return new WaitForSeconds(delayDespawn);

			disabledGrab = myself.isGrabbable;
			disabledUse = myself.isUsable;
			myself.transform.position = Vector3.one * 1000;
			myself.transform.localScale = Vector3.zero;
			myself.transform.rotation = Quaternion.identity;

			yield return new WaitForSeconds(respawnWaitTime);
			myState = RespawnObjectState.Waiting;
			StartRespawn();
		}
		#endregion

		#region Respawn
		public void StartRespawn()
		{
			RespawningCorout = StartCoroutine(Respawn());
		}

		IEnumerator Respawn()
		{
			if (myState == RespawnObjectState.Waiting)
			{
				myState = RespawnObjectState.Respawning;

				//Store previous state
				disabledGrab = myself.isGrabbable;
				disabledUse = myself.isUsable;

				if (myRB != null)
				{
					myRB.velocity = Vector3.zero;
					myRB.angularVelocity = Vector3.zero;
					myRB.useGravity = false;
				}

				//Turn off our interaction types.
				myself.isGrabbable = false;
				myself.isUsable = false;

				//Result is to lock self while we grow.
				myself.transform.localRotation = resetRotation;
				myself.transform.position = resetPosition;
				//Grow over N frames.
				for (int i = 0; i < FramesToGrow; i++)
				{
					float percentage = (float)i / FramesToGrow;
					//myself.transform.position = resetPosition - Vector3.up * resetScale.y / 2 * ((float)i) / frameCount;
					myself.transform.localScale = Vector3.Lerp(Vector3.zero, resetScale, percentage);
					//myself.transform.position = resetPosition - (Vector3.up * resetScale.y / 3) + (Vector3.up * resetScale.y) * percentage;
					yield return null;
				}
				//Relocate myself, rescale myself, set rotation
				myself.transform.position = resetPosition;
				myself.transform.localScale = resetScale;

				RestoreInteractions(disabledGrab, disabledUse);
				myState = RespawnObjectState.Valid;

				if (myRB != null)
				{
					myRB.useGravity = true;
				}
			}
		}

		public void RestoreInteractions(bool grabWasDisabled, bool useWasDisabled)
		{
			//Debug.Log("Restored Interaction\n");
			//Restore usable state.
			myself.isGrabbable = grabWasDisabled;
			myself.isUsable = useWasDisabled;
		}
		#endregion

		#region Helper
		float FlatDistFrom(Vector3 point)
		{
			point.y = 0;
			Vector3 myselfFlatPos = myself.transform.position;
			myselfFlatPos.y = 0;
			return Vector3.Distance(point, myselfFlatPos);
		}

		Vector3 GetPoint(Valve.VR.HmdVector3_t hmdVec)
		{
			return new Vector3(hmdVec.v0, hmdVec.v1, hmdVec.v2);
		}

		#endregion
	}
}
