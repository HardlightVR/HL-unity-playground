using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveCentral : MonoBehaviour 
{
	public static CurveCentral Instance;

	public AnimationCurve linear;
	public AnimationCurve alpha;
	public AnimationCurve beta;
	public AnimationCurve gamma;
	public AnimationCurve delta;
	public AnimationCurve zeta;

	void Awake() 
	{
		Instance = this;
	}
}
