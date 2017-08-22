using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	[System.Serializable]
	public class DestructionEvent : UnityEvent<Destroyable.DestructionInfo> { }

	public class Destroyable : MonoBehaviour
	{
		public enum DestroyedBy
		{
			None
					= 1 << 0,
			Pressure
					= 1 << 1,
			Physics
					= 1 << 2,
			Explosives
					= 1 << 3,
			Fire
					= 1 << 4,
			Bullets
					= 1 << 5,
			ForceDestroy
					= 1 << 6
		}
		[Header("Note: Force Destroy always works")]

		[EnumFlag]/* [Header("What is allowed to destroy this")]*/
		public DestroyedBy CanBeDestroyedBy = DestroyedBy.None;
		public enum DestructionResponse
		{
			None
					= 1 << 0,
			CallScript
					= 1 << 1,
			CreatePieces
					= 1 << 2,
			RigidbodyPieces
					= 1 << 3,
			DisableSelf
					= 1 << 4,
			FaceDestruction
					= 1 << 5
		}
		[EnumFlag]
		[Header("When this is destroyed")]
		public DestructionResponse CalledOnDestroy =
			DestructionResponse.CreatePieces |
			DestructionResponse.RigidbodyPieces |
			DestructionResponse.DisableSelf |
			DestructionResponse.FaceDestruction;

		public UnityEvent WhenDestroyedCallback;
		//public delegate void DestroyedDelegate(Vector3 direction, bool physics = false, bool ForceDestroy = false, GameObject pieces = null);
		//public DestroyedDelegate DestroyedResponseDelegate;

		public GameObject DestroyedObject;
		public GameObject DestroyPhysics;
		private float Hits = 5;
		public Vector2 HitRange = new Vector2(2, 4);
		public float MagToBreak = 5;
		public Rigidbody myRB;
		public Collider myCol;
		public MeshRenderer myRend;
		public GameObject[] AdditionalDeactivation;
		public bool HasBeenDestroyed = false;

		public class DestructionInfo
		{
			public Destroyable Destroyed;
			public DestroyedBy Reason;
			public Vector3 direction;
			public bool usePhysics;
			public bool ForceDestroy;
			public GameObject destroyedPieces;

			public DestructionInfo(Destroyable destroyed, DestroyedBy reason = DestroyedBy.ForceDestroy, Vector3 dir = default(Vector3), bool physics = false, bool forceDestroy = true, GameObject piecesLeft = null)
			{
				Destroyed = destroyed;
				Reason = reason;
				direction = dir;
				ForceDestroy = forceDestroy;
				destroyedPieces = piecesLeft;
				Destroyed = destroyed;
				usePhysics = physics;
			}
		}

		public void Reset()
		{
			HasBeenDestroyed = false;
			Hits = Random.Range(HitRange.x, HitRange.y);
			SetOwnActivity(true);
		}

		void Start()
		{
			myCol = GetComponent<Collider>();
			myRend = GetComponent<MeshRenderer>();
			Hits = Random.Range(HitRange.x, HitRange.y);
		}

		void BulletHitNearby(Vector3 dir)
		{
			if (HasFlag(DestroyedBy.Bullets))
			{
				if (myRB != null)
				{
					dir = dir.normalized;
					Debug.DrawLine(transform.position, transform.position + dir * 50, Color.yellow, 3.5f);
					myRB.AddForce(dir * 3, ForceMode.VelocityChange);
					myRB.AddForce(Vector3.up * 1, ForceMode.VelocityChange);
				}

				DestroyableHit();
			}
		}

		void BulletHit(Vector3 point)
		{
			if (HasFlag(DestroyedBy.Bullets))
			{
				if (myRB != null)
				{
					Vector3 dir = transform.position - point;
					dir = dir.normalized;
					Debug.DrawLine(transform.position, transform.position + dir * 50, Color.yellow, 3.5f);
					myRB.AddForce(dir * 3, ForceMode.VelocityChange);
					myRB.AddForce(Vector3.up * 1, ForceMode.VelocityChange);
				}

				DestroyableHit();
			}
		}

		//The idea is if this object is affected by the Explodable class.
		//However the component parts don't break and go flying off.
		public void DestroyableHit(float damage = 1, Vector3 blastDir = default(Vector3))
		{
			if (HasFlag(DestroyedBy.Explosives))
			{
				Hits -= damage;

				float collisionMag = blastDir.magnitude;
				if (collisionMag > MagToBreak)
				{
					DestroyIt(new DestructionInfo(this, DestroyedBy.Explosives, blastDir.normalized * damage, myRB, false));
				}
				else if (Hits <= 0)
				{
					DestroyIt(new DestructionInfo(this, DestroyedBy.Explosives, blastDir.normalized * damage, myRB, false));
				}
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if (HasFlag(DestroyedBy.Physics))
			{
				float collisionMag = collision.relativeVelocity.magnitude;
				//Debug.Log("collisionMag: " + collisionMag + "\n" + name);
				if (collisionMag > MagToBreak)
				{
					DestroyIt(new DestructionInfo(this, DestroyedBy.Physics, collision.relativeVelocity.normalized, myRB, false));
				}
			}
			else
			{
				//Debug.Log("Collision no worky\n" + CanBeDestroyedBy.ToString() + "\n");
			}
		}

		public void DestroyIt(DestructionInfo info)
		{
			if (!HasFlag(DestroyedBy.None) || info.ForceDestroy)
			{
				bool CanCreatePieces = DestroyedObject != null && !HasBeenDestroyed;
				bool CallScript = HasFlag(DestructionResponse.CallScript);
				bool DisableSelf = HasFlag(DestructionResponse.DisableSelf);

				//Necessary for the CallScript delegate (which will handle for null pieces)
				GameObject destroyedPieces = null;

				#region Destroyed object replacement
				if (CanCreatePieces)
				{
					//Check the flags here to avoid doing them needlessly
					bool CreatePieces = HasFlag(DestructionResponse.CreatePieces);
					bool RigidbodyPieces = HasFlag(DestructionResponse.RigidbodyPieces);
					bool FaceDestruction = HasFlag(DestructionResponse.FaceDestruction);

					#region Create Pieces
					if (CreatePieces)
					{
						destroyedPieces = (GameObject)Instantiate(info.usePhysics ? DestroyPhysics : DestroyedObject, transform.position, transform.rotation);
						destroyedPieces.transform.localScale = transform.localScale;
						int g = 0;
						if (RigidbodyPieces)
						{
							foreach (Rigidbody childRB in destroyedPieces.transform.GetComponentsInChildren<Rigidbody>())
							{
								g++;
								//Debug.Log(g + "\n");
								childRB.AddForce(info.direction * 2, ForceMode.VelocityChange);
							}
						}
					}
					#endregion
					#region Face Destruction
					if (FaceDestruction)
					{
						Vector3 lookAt = transform.position + info.direction;
						lookAt.y = transform.position.y;
						info.direction.y = transform.position.y;

						if (CreatePieces)
						{
							if (destroyedPieces != null)
							{
								destroyedPieces.transform.LookAt(Vector3.Cross(info.direction, Vector3.up));
							}
						}
					}
					#endregion
				}
				#endregion
				#region Disabling Self
				if (DisableSelf && !HasBeenDestroyed)
				{
					SetOwnActivity(false);
				}
				#endregion
				#region Call Delegate Script
				if (CallScript && !HasBeenDestroyed)
				{
					WhenDestroyedCallback.Invoke();
				}
				#endregion
				HasBeenDestroyed = true;
			}
		}

		public void SetOwnActivity(bool isEnabled)
		{
			if (myRend)
				myRend.enabled = isEnabled;
			if (myCol)
				myCol.enabled = isEnabled;

			for (int i = 0; i < AdditionalDeactivation.Length; i++)
			{
				AdditionalDeactivation[i].SetActive(isEnabled);
			}

			foreach (Collider c in GetComponents<Collider>())
			{
				c.enabled = isEnabled;
			}

			if (myRB)
			{
				myRB.isKinematic = !isEnabled;
				myRB.velocity = Vector3.zero;
			}
			enabled = isEnabled;
		}

		public bool HasFlag(DestroyedBy checkHaveID)
		{
			return (CanBeDestroyedBy & checkHaveID) == checkHaveID;
		}

		public bool HasFlag(DestructionResponse checkHaveID)
		{
			return (CalledOnDestroy & checkHaveID) == checkHaveID;
		}
	}
}