using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.NewLocation
{
	public class SuitLocation : HapticLocationNode
	{
		public AreaFlag MyLocation;
		public SuitLocation(AreaFlag Location)
		{
			MyLocation = Location;
		}
		public override int Where()
		{
			return (int)MyLocation;
		}
		public override HapticLocationType GetHapticType()
		{
			return HapticLocationType.Suit;
		}
		public override AreaFlag ConvertToAreaFlag()
		{
			return MyLocation;
		}
		public override string ToString()
		{
			return MyLocation.ToArray().ToString();
			//return MyLocation.ToStringIncludedAreas();
			//return MyLocation.ToStringIncludedAreas();
		}
	}
}