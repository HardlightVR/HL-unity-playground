using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class Gemstone : MoneyProjectile
	{
		//Assign random color
		[Header("Gemstone Variables")]
		public Renderer MyRenderer;
		public ParticleSystem particles;
		public TrailRenderer trailRend;
		public int GemColorIndex = 0;
		public Color[] GemColors;

		protected override void Start()
		{
			GemColorIndex = Random.Range(0, GemColors.Length);

			if (particles != null)
			{
				ParticleSystem.MainModule main = particles.main;
				main.startColor = GemColors[Mathf.Clamp(GemColorIndex, 0, GemColors.Length)];
			}
			else
			{
				Debug.LogError("Particles for gemstone " + name + " is not assigned\n");
			}

			if (trailRend != null)
			{
				trailRend.startColor = GemColors[Mathf.Clamp(GemColorIndex, 0, GemColors.Length)];
				trailRend.endColor = GemColors[Mathf.Clamp(GemColorIndex, 0, GemColors.Length)];
			}
			else
			{
				Debug.LogError("Trailrender for gemstone " + name + " is not assigned\n");
			}


			if (particles != null)
			{
				MyRenderer.material.color = GemColors[Mathf.Clamp(GemColorIndex, 0, GemColors.Length)];
			}
			else
			{
				Debug.LogError("Renderer for gemstone " + name + " is not assigned\n");
			}
		}
	}
}