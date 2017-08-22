using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class CashPack : MoneyProjectile
	{
		[Header("Cash Pack Properties")]
		public GameObject CleanPack;
		public GameObject MessyPack;

		protected override void Start()
		{
			DurabilityRemaining = 2;
			CleanPack.SetActive(true);
			MessyPack.SetActive(false);
			base.Start();
		}
		public override void DamageMoney()
		{
			CleanPack.SetActive(false);
			MessyPack.SetActive(true);

			base.DamageMoney();
		}
	}
}