using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class LayerMaskHelper : MonoBehaviour
	{
		public static LayerMask GroundLayer
		{
			get { return ((1 << 4) | (1 << 17)); }
		}
		public static LayerMask GroundOrDefaultLayer
		{
			get { return ((1 << 0) | (1 << 4) | (1 << 17)); }
		}
		public static LayerMask PhysicalObjects
		{
			get { return ((1 << 8) | (1 << 9) | (1 << 10) | (1 << 12) | (1 << 15)); }
		}
	}
}