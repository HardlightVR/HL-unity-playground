using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NullSpace.SDK.Demos
{
	[Serializable]
	public class ExplosionEvent : UnityEvent<Explosion.ExplosionInfo> { }

	public class Explosion : MonoBehaviour
	{
		public bool ShouldExplodeImmediately;
		public int TimesExploded = 0;

		public enum DetonationType
		{
			None
					= 1 << 0,
			Script
					= 1 << 1,
			Physics
					= 1 << 2,
			ChainReaction
					= 1 << 3,
			Fire
					= 1 << 4,
			Bullet
					= 1 << 5,
			SpecificContact
					= 1 << 6
		}
		[EnumFlag]
		public DetonationType ExplosionCause = DetonationType.None;

		public delegate void StartExplosionDelegate();
		public StartExplosionDelegate startExplosionDelegate;

		public delegate void PostExplosionDelegate();
		public PostExplosionDelegate postExplosionDelegate;

		public LayerMask BlastableLayers;

		[Header("Explosion Attributes")]
		public GameObject alternateExplosionPoint;
		public float BlastRadius = 4;
		public float BlastForce = 25;
		public Vector2 ClampBlastDamage = new Vector2(0, 1000);
		public float DetonateAtImpactMagnitude = 12;
		public bool NegatedByLineOfSight = false;
		public float explosionVisualDelay = .25f;
		public Vector3 addedExplosionVector = Vector3.up / 4;

		[Header("Explosion Visual FX")]
		public bool CreateExplosionVFX = true;
		[ConditionalHide("CreateExplosionVFX", true)]
		public GameObject ExplosionPrefab;
		[ConditionalHide("CreateExplosionVFX", true)]
		public Vector3 ExplosionScale = Vector3.one;

		[Header("Destroyable Field")]
		public bool TriggerOwnDestroyable = true;
		public Destroyable DetonateSelf;

		[Header("Destroyable Field")]
		public float ShockwaveSize = 0;

		[Header("Explosion Audio")]
		public bool CreateAudioSFX = true;

		[ConditionalHide("CreateAudioSFX", true)]
		public AudioSource source;

		[ConditionalHide("CreateAudioSFX", true)]
		public string sfxName = "Explosions/dynamite boom";

		[ConditionalHide("CreateAudioSFX", true)]
		public float sfxVolume = .5f;

		[Header("Explosion Audio")]
		public bool HapticsWhenExplosionHit = true;

		[ConditionalHide("HapticsWhenExplosionHit", true)]
		public string hapticSequenceName = "Haptics/pain_short";
		private HapticSequence explosionSequence = new HapticSequence();

		[Header("Force Kinematic Rigidbody")]
		public bool RemoveKinematicAttribute = false;

		public class ExplosionInfo
		{
			public Explosion source;
			public Explodable affected;
			public float dist;
			public Vector2 ClampBlastDamage = new Vector2(0, 1000);
			public Vector3 explosionCenter;
			public Vector3 affectedPosition;
			public Vector3 forceMultipliers;
			public HapticSequence explosionSequence;

			public ExplosionInfo(Explosion src, Explodable afflicted, Vector3 explosionOrigination, Vector3 explosionMultiplier)
			{
				source = src;
				affected = afflicted;
				affectedPosition = affected.transform.position;
				explosionCenter = explosionOrigination;
				dist = Vector3.Distance(explosionCenter, affected.transform.position);
				forceMultipliers = explosionMultiplier;
			}
			public ExplosionInfo(Explosion src, Vector3 affectedPoint, Vector3 explosionOrigination, Vector3 explosionMultiplier)
			{
				source = src;
				explosionCenter = explosionOrigination;
				affectedPosition = affectedPoint;
				dist = Vector3.Distance(explosionCenter, affectedPoint);
				forceMultipliers = explosionMultiplier;
			}

			public float BlastStrength()
			{
				if (affected != null)
				{
					//Debug.Log("Evaluating Blast Strength: " + source.BlastRadius + "\n" + (dist + affected.MinSphereRadius / 2) + "\t\t" + affected.name);
					//Total distance is 13. If the target is 4 units away, we want them to take 9 damage.
					return Mathf.Clamp(source.BlastRadius - (dist - affected.MinSphereRadius / 2), ClampBlastDamage.x, ClampBlastDamage.y);
				}
				return Mathf.Clamp(source.BlastRadius - (dist), ClampBlastDamage.x, ClampBlastDamage.y);
			}

			public override string ToString()
			{
				string output = "Explosion [" + source.name + "] applied to [" + affected.name + "]\t" + Mathf.Round(dist * 100) / 100 + " units apart\nCenter: " + explosionCenter + "\t\tBase Force: " + forceMultipliers + "\n";
				return output + base.ToString();
			}
		}

		private Coroutine ExplosionStart;
		private bool Exploding = false;

		void Start()
		{
			explosionSequence.LoadFromAsset(hapticSequenceName);
			if (CreateAudioSFX)
			{
				if (source == null)
				{
					source = GetComponent<AudioSource>();
					if (source == null)
					{
						source = gameObject.AddComponent<AudioSource>();
					}
				}

				source.clip = AudioManager.Inst.FindOrLoadAudioClip(sfxName);
				source.volume = sfxVolume;
				source.enabled = true;
			}
		}

		void Update()
		{
			if (ShouldExplodeImmediately)
			{
				ShouldExplodeImmediately = false;
				TimesExploded = 0;
				Explode();
			}
		}

		public void Explode()
		{
			if (!Exploding && TimesExploded == 0)
			{
				ExplosionStart = StartCoroutine(StartExplosion());
			}
			else
			{
				//Debug.LogError("Attempting multiple explosions on object [" + name + "].\n\tNot yet implemented\n");
			}
		}

		private void ProcessExplosion(Vector3 blastPoint)
		{
			bool success = false;
			//Debug.DrawLine(transform.position, transform.position + Vector3.up * BlastRadius, Color.red, 5.0f);
			//Debug.DrawLine(transform.position, transform.position + Vector3.down * BlastRadius, Color.red, 5.0f);
			//Debug.DrawLine(transform.position, transform.position + Vector3.right * BlastRadius, Color.red, 5.0f);
			//Debug.DrawLine(transform.position, transform.position + Vector3.left * BlastRadius, Color.red, 5.0f);
			//Debug.DrawLine(transform.position, transform.position + Vector3.forward * BlastRadius, Color.red, 5.0f);
			//Debug.DrawLine(transform.position, transform.position + Vector3.back * BlastRadius, Color.red, 5.0f);

			Collider[] Blast = Physics.OverlapSphere(blastPoint, BlastRadius, BlastableLayers);
			if (Blast != null)
			{
				//Debug.Log("Blast Area: " + Blast.Length + "\n");
				for (int i = 0; i < Blast.Length; i++)
				{
					Explodable affected = Blast[i].GetComponent<Explodable>();

					if (affected && BlastRadius > 0)
					{
						//Debug.Log("Blast Area: " + Blast.Length + "\n" + affected.gameObject.name + "  " + name, this);
						if (affected.gameObject != gameObject)
						{
							float range = Vector3.Distance(blastPoint, Blast[i].gameObject.transform.position);

							//A multiplier for objects being knocked around
							float damage = Mathf.Clamp(1 - ((range - affected.MinSphereRadius) / BlastRadius), .25f, 1);
							//Debug.Log("Radius [" + BlastRadius + "]  Dist [" + range + "] bonusRadius [" + affected.MinSphereRadius + "]\n");

							Vector3 startPoint = (alternateExplosionPoint == null) ? transform.position : alternateExplosionPoint.transform.position;

							ExplosionInfo info = new ExplosionInfo(this, affected, startPoint, new Vector3(1, 1.25f, 1) * damage * BlastForce / 4);
							info.ClampBlastDamage = ClampBlastDamage;
							info.explosionSequence = explosionSequence;
							//Debug.Log(info.ToString() + "\n");
							success = affected.RecieveExplosion(info);

							if (!success)
							{
								Debug.Log("Failed to apply explosion\n" + info.ToString() + "\n");
							}
						}
					}
				}
			}

			if (ShockwaveSize > 0)
			{
				ProcessShockwave(blastPoint);
			}
		}
		private void ProcessShockwave(Vector3 blastPoint)
		{
			throw new System.NotImplementedException();

			//if (HardlightSuit.Instance != null)
			//{
			//	float range = Vector3.Distance(PlayerBody.Instance.transform.position, blastPoint);

			//	ExplosionInfo info = new ExplosionInfo(this, PlayerBody.Instance.transform.position, blastPoint, Vector3.one);
			//	info.ClampBlastDamage = ClampBlastDamage;
			//	PlayerBody.Instance.HitShockwave(info);

			//}
		}
		private void CreateExplosionVisual()
		{
			GameObject explosionGO = Instantiate(ExplosionPrefab);
			explosionGO.transform.position = transform.position;
			explosionGO.transform.localScale = ExplosionScale;

			LayerMask groundLayers = LayerMaskHelper.GroundOrDefaultLayer;
			RaycastHit hit;
			if (Physics.Raycast(transform.position + Vector3.up * .5f, Vector3.down, out hit, 2.5f, groundLayers))
			{
				Debug.DrawLine(transform.position + Vector3.up * .5f, hit.point, Color.blue, 5.0f);
				Transform ground = explosionGO.transform.Find("BurntGround");
				if (ground != null)
				{
					//Debug.Log("Grounding the burnt ground\n\t" + (ground.transform.position.y - hit.point.y));
					ground.transform.position -= Vector3.up * (ground.transform.position.y - hit.point.y - .06f);
					//ground.transform.position.Set(ground.transform.position.x, hit.point.y, ground.transform.position.z);
				}
			}
			else
			{
				Debug.DrawLine(transform.position + Vector3.up * .5f, transform.position - Vector3.up * 2.5f + Vector3.left * .1f, Color.red, 5.0f);
				explosionGO.transform.position -= Vector3.up * .75f;

				//Horrible code.
				Transform ground = explosionGO.transform.Find("BurntGround");
				if (ground != null)
				{
					ground.gameObject.SetActive(false);
				}
			}
		}

		void BulletHit(Vector3 point)
		{
			if (HasFlag(DetonationType.Bullet))
			{
				Explode();
			}
		}

		//We might want to have a 'delay before explode' and a 'particle effect delay'
		//Delay before explode - for Half Life style burning barrels to signal the detonation.
		//Particle effect delay - Don't apply force until appropriate time.
		IEnumerator StartExplosion()
		{
			//Debug.Log(name + " has started exploding!\n");
			Exploding = true;
			if (TimesExploded == 0)
			{
				PlayAudioSource();

				TimesExploded++;

				if (CreateExplosionVFX && ExplosionPrefab != null)
				{
					CreateExplosionVisual();
				}

				yield return new WaitForSeconds(explosionVisualDelay);

				TryTriggerOwnDestroyable();

				//Execute the explosion code?
				if (alternateExplosionPoint != null)
				{
					ProcessExplosion(alternateExplosionPoint.transform.position);
				}
				else
				{
					ProcessExplosion(transform.position);
				}
				StartCoroutine(EndExplosion());
			}
		}

		private void PlayAudioSource()
		{
			if (source != null)
			{
				source.volume = .75f;
				source.pitch = UnityEngine.Random.Range(.9f, 1.1f);
				source.Play();
			}
		}

		private void TryTriggerOwnDestroyable()
		{
			if (TriggerOwnDestroyable)
			{
				TryFindOwnDestroyable();

				if (DetonateSelf != null)
				{
					DetonateSelf.DestroyIt(new Destroyable.DestructionInfo(DetonateSelf, Destroyable.DestroyedBy.ForceDestroy, Vector3.up));
				}
			}
		}

		private void TryFindOwnDestroyable()
		{
			if (DetonateSelf == null)
			{
				DetonateSelf = GetComponent<Destroyable>();
			}
		}

		IEnumerator EndExplosion()
		{
			//Debug.Log("End Explosion\n");
			yield return new WaitForSeconds(0.0f);
			if (postExplosionDelegate != null)
			{
				postExplosionDelegate.Invoke();
			}
			Exploding = false;
		}

		public bool DrawExplosionGizmos = true;
		void OnDrawGizmos()
		{
			if (DrawExplosionGizmos)
			{
				Color newRed = new Color(.95f, .0f, .0f, .35f);

#if UNITY_EDITOR
				if (Selection.activeGameObject == gameObject)
					newRed = new Color(.95f, .02f, .02f, 1.0f);
#endif
				DebugExtension.DrawCircle(transform.position, Vector3.up, newRed, BlastRadius);
				DebugExtension.DrawCircle(transform.position, Vector3.right, newRed, BlastRadius);
				DebugExtension.DrawCircle(transform.position, Vector3.forward, newRed, BlastRadius);
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if (HasFlag(DetonationType.Physics))
			{
				float collisionMag = collision.relativeVelocity.magnitude;
				//Debug.Log("Collided " + collision.other.gameObject.name + "\n[" + collisionMag + "] [" + DetonateAtImpactMagnitude + "]", collision.other.gameObject);
				if (collisionMag > DetonateAtImpactMagnitude)
				{
					Explode();
				}
			}
		}

		#region Flags
		public bool HasFlag(DetonationType checkHaveID)
		{
			return (ExplosionCause & checkHaveID) == checkHaveID;
		}

		public static bool HasFlag(DetonationType baseID, DetonationType checkHaveID)
		{
			return (baseID & checkHaveID) == checkHaveID;
		}

		public DetonationType SetFlag(DetonationType baseID, DetonationType added)
		{
			return baseID | added;
		}

		public DetonationType UnsetFlag(DetonationType baseID, DetonationType removed)
		{
			return baseID & (~removed);
		}

		public DetonationType ToogleFlag(DetonationType baseID, DetonationType toggleID)
		{
			return baseID ^ toggleID;
		}
		#endregion
	}
}