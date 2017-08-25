
namespace NullSpace.SDK
{
	using System;
	using UnityEngine;

	public static class AreaFlagExtensions
	{
		public static AreaFlag Mirror(this AreaFlag lhs)
		{
			return (AreaFlag)RotateLeft((uint)lhs, 16);
		}
		private static uint RotateLeft(uint x, byte n)
		{
			return ((x << n) | (x >> (32 - n)));
		}
	}
}