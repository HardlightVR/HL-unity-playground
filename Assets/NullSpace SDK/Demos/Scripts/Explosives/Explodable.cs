using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace NullSpace.SDK.Demos
{
	public class Explodable : MonoBehaviour
	{
		//This enum is for defining how this object responds TO an explosion.
		//Does it call some specific script, play haptics, destroy itself or start another explosion.

		public enum ExplosionResponse
		{
			None
					= 1 << 0,
			Rigidbody
					= 1 << 1,
			Haptics
					= 1 << 2,
			Fireworks
					= 1 << 3,
			ChainReaction
					= 1 << 4,
			Destroyable
					= 1 << 5,
			CallScript
					= 1 << 6,
			SpecificContact
					= 1 << 7
		}
		[EnumFlag]
		public ExplosionResponse ResponseType = ExplosionResponse.None;

		public Rigidbody rb;
		public float MinSphereRadius = .5f;
		public float ChainReactionDelay = .25f;

		public ExplosionEvent WhenExploded;

		public delegate void ExplodedDelegate(Explosion.ExplosionInfo info);
		public ExplodedDelegate HitByExplosionDelegate;

		void Start()
		{
			if (HasFlag(ExplosionResponse.Rigidbody) && rb == null)
			{
				rb = GetComponent<Rigidbody>();
				if (rb == null)
				{
					Debug.LogError("Explodable Object [" + name + "] is marked as a Rigidbody Explodable but does not have a rigidbody assigned\n\tDefaulting and removing the rigidbody explodable flag.\n");
					ResponseType = UnsetFlag(ResponseType, ExplosionResponse.Rigidbody);
				}
			}
		}

		public bool RecieveExplosion(Explosion.ExplosionInfo info)
		{
			if (HasFlag(ExplosionResponse.None))
			{
				return false;
			}

			Vector3 direction = transform.position - info.explosionCenter;
			Vector3 blast = Vector3.Scale(direction.normalized, info.forceMultipliers);

			//Debug.Log("I have recieved an explosion\t" + name + "\n\tForce: " + forceMultipliers);
			if (HasFlag(ExplosionResponse.Rigidbody))
			{
				if (rb != null)
				{
					if (rb.isKinematic && info.source.RemoveKinematicAttribute)
					{
						rb.isKinematic = false;
						rb.useGravity = true;
					}
					//Debug.Log("Applying force to " + name + "\n");
					rb.AddForce(blast, ForceMode.Impulse);
				}
			}
			if (HasFlag(ExplosionResponse.Haptics))
			{
				if (info.explosionSequence != null)
				{
					var Where = HardlightSuit.Find().FindNearestFlag(info.explosionCenter, 5);
					HardlightSuit.Find().EmanatingHit(Where, info.explosionSequence, .25f, 8);
				}
			}
			if (HasFlag(ExplosionResponse.Fireworks))
			{
				//Create a visual effect at the point of impact?
			}
			if (HasFlag(ExplosionResponse.ChainReaction))
			{
				StartCoroutine(StartChainReaction(info));
			}
			if (HasFlag(ExplosionResponse.Destroyable))
			{
				Destroyable breakable = GetComponent<Destroyable>();
				if (breakable != null)
				{
					//Debug.Log(info.BlastStrength() + "  \n" + blast + "\n");
					breakable.DestroyableHit(info.BlastStrength(), blast);
				}
			}
			if (HasFlag(ExplosionResponse.CallScript))
			{
				if (WhenExploded != null)
				{
					WhenExploded.Invoke(info);
				}
				//Delegate to call
				if (HitByExplosionDelegate != null)
				{
					HitByExplosionDelegate.Invoke(info);
				}
			}

			return true;
		}

		IEnumerator StartChainReaction(Explosion.ExplosionInfo info)
		{
			//Look for our own explosion and get things rolling!
			Explosion exp = GetComponent<Explosion>();
			if (exp != null && exp != info.source && exp.TimesExploded <= 0)
			{
				//Have a delegate for starting a chain reaction explosive. Maybe move this to Explosion.cs

				//Delay for visuals
				yield return new WaitForSeconds(ChainReactionDelay);

				UnsetFlag(ResponseType, ExplosionResponse.ChainReaction);

				//Chain Reaction has begun!
				//Debug.Log("Chain Raction on " + exp.name + " caused by " + name + "\n");
				exp.Explode();
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if (HasFlag(ExplosionResponse.SpecificContact))
			{
				Explosion exp = collision.collider.GetComponent<Explosion>();
				if (exp != null)
				{
					if (exp.HasFlag(Explosion.DetonationType.SpecificContact))
					{
						exp.Explode();
					}
				}
			}
		}

		#region Flags
		public bool HasFlag(ExplosionResponse checkHaveID)
		{
			return (ResponseType & checkHaveID) == checkHaveID;
		}

		public static bool HasFlag(ExplosionResponse baseID, ExplosionResponse checkHaveID)
		{
			return (baseID & checkHaveID) == checkHaveID;
		}

		public void SetFlag(ExplosionResponse added, bool targetValue)
		{
			//If we have the flag XOR we want it
			if (HasFlag(ResponseType, added) ^ targetValue)
			{
				//Toggle it
				ResponseType = ToogleFlag(ResponseType, added);
			}
		}

		public ExplosionResponse SetFlag(ExplosionResponse baseID, ExplosionResponse added)
		{
			return baseID | added;
		}

		public ExplosionResponse UnsetFlag(ExplosionResponse baseID, ExplosionResponse removed)
		{
			return baseID & (~removed);
		}

		public ExplosionResponse ToogleFlag(ExplosionResponse baseID, ExplosionResponse toggleID)
		{
			return baseID ^ toggleID;
		}
		#endregion
	}
}