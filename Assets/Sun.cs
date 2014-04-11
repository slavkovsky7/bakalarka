using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class Sun : MonoBehaviour {

	public static float TimeConstant = 0  ;
	public float TimeConstantCurrent = TimeConstant;
	private float timeAccelaration = 0;
	private String TimeButtonString = "Pause";

	private bool isPaused = false;
	private float lastTimeConstantCurrent = 0;

	private KeyButton plusButton = new KeyButton(new Rect(70,10,50,50), "+", KeyCode.KeypadPlus );
	private KeyButton minusButton = new KeyButton(new Rect(10,10,50,50), "-", KeyCode.KeypadMinus );
	
	string[] pole = new string[]{"jan", "feb", "march"};
	public static DatePicker DatePicker = null;
	public static List<Planet> Planets = new List<Planet>();




	public void addTime(float n){
		timeAccelaration += n;
		//TimeConstantCurrent += timeAccelaration;
		TimeConstantCurrent +=timeAccelaration;
	}
	

	void Start () 
	{
		TimeConstant = TimeConstantCurrent;
		plusButton.action  += delegate { addTime( 0.001f); } ;
		plusButton.releaseAction += delegate{ timeAccelaration = 0; };

		minusButton.action += delegate { addTime(-0.001f); } ;
		minusButton.releaseAction += delegate{ timeAccelaration = 0; };

		DatePicker = new DatePicker(new Rect(100,100, 300, 100 ) );
	}

	/*
	private float x = 0;

	public void addTime(float znamienko ){
		float tmpX = x + 1.0f;
		//TimeConstantCurrent += timeAccelaration;
		//float accelaration = znamienko * ( 0.000001f*Mathf.Pow( (float)x ,2.
		float t  = tmpX / 500;

		float accelaration = znamienko * (0.000001f* Mathf.Pow(10, t)* Mathf.Pow((float)tmpX , 2.0f));
		if (accelaration < 50.0f )
		{
			x = tmpX;
		}

		if (TimeConstantCurrent < 300.0f)
		{
			TimeConstantCurrent += accelaration;
		}

		Debug.Log( "e^" + x + " = accelaration = "  + accelaration);
	}
	

	void Start () 
	{
		TimeConstant = TimeConstantCurrent;
		plusButton.action  += delegate { addTime(+1.0f); } ;
		plusButton.releaseAction += delegate{ x = 0; };

		minusButton.action += delegate { addTime(-1.0f); } ;
		minusButton.releaseAction += delegate{ x = 0; };

		DatePicker = new DatePicker(new Rect(100,100, 300, 100 ) );
	}
	 */

	void Update () 
	{
		TimeConstant = TimeConstantCurrent;
		plusButton.PerformUpdate();
		minusButton.PerformUpdate();
	}



	void OnGUI(){
		plusButton.Perform();
		minusButton.Perform();
		if ( GUI.Button( new Rect(100,210, 100, 50) ,  TimeButtonString ) ) {
			if (!isPaused ){
				isPaused = true;
				lastTimeConstantCurrent = TimeConstant;
				TimeConstantCurrent = 0;
				TimeConstant = 0;
				TimeButtonString = "Resume";
			}else {
				isPaused = false;
				TimeConstantCurrent = lastTimeConstantCurrent;
				TimeConstant = lastTimeConstantCurrent;
				lastTimeConstantCurrent = 0;
				TimeButtonString = "Pause";
			}
		}

		if ( GUI.Button( new Rect(210,210, 100, 50) ,  "Reset Time" ) ) {
			TimeConstantCurrent = 1.146052e-05f;
		}
		DatePicker.onGui();
	}
}
