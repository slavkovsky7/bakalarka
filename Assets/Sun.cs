using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour {

	public static float TimeConstant = 1.0f  ;
	// Use this for initialization
	public float TimeConstantCurrent = TimeConstant;

	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		TimeConstant = TimeConstantCurrent;
	}
}
