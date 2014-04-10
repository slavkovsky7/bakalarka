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

		if ( GUI.Button( new Rect(210,210, 100, 50) ,  "Set Date" ) ) {

		}
		DatePicker.onGui();
	}
}
