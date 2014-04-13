using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class Sun : MonoBehaviour {

	public static float TimeConstant = 0;
	public float TimeConstantCurrent = 0;
	private float timeAccelaration = 0;
	private String TimeButtonString = "Pause";

	private bool isPaused = false;
	private float lastTimeConstantCurrent = 0;

	private KeyButton plusButton = new KeyButton(new Rect(70,10,50,50), "+", KeyCode.KeypadPlus );
	private KeyButton minusButton = new KeyButton(new Rect(10,10,50,50), "-", KeyCode.KeypadMinus );
	
	string[] pole = new string[]{"jan", "feb", "march"};
	public static DatePicker datePicker = null;
	public static List<Planet> Planets = new List<Planet>();

	private bool initialDateSet = false;
	private double lastDateHours = -1;
	private bool pauseButtonPressed = false;

	/*
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
*/

	private float x = 0;

	public void addTime(float znamienko ){
		float tmpX = x + 1.0f;
		//TimeConstantCurrent += timeAccelaration;
		//float accelaration = znamienko * ( 0.000001f*Mathf.Pow( (float)x ,2.
		float t  = tmpX / 500;
		float a = 0.0001f;
		if (znamienko < 0 && TimeConstantCurrent > -1.00f && TimeConstantCurrent < 1.00f ){
			//a = a*Mathf.Pow(10, -Mathf.Min( 2.0f , (float) ( (int) ( 1 / TimeConstantCurrent)) ));
			a *= 0.01f;
		}
		Debug.Log("a =" + a);
		float accelaration = znamienko * ( a * Mathf.Pow(10, t)* Mathf.Pow((float)tmpX , 2.0f));
		if (accelaration < 50.0f )
		{
			x = tmpX;
		}

		if ( Math.Abs( TimeConstantCurrent + accelaration ) < 300.0f)
		{
			TimeConstantCurrent += accelaration;
		}

		Debug.Log( "e^" + x + " = accelaration = "  + accelaration);
	}



	void Start () 
	{
		TimeConstantCurrent = GetDefaultTimeConstant();
		TimeConstant = GetDefaultTimeConstant();
		plusButton.action  += delegate { addTime(+1.0f); } ;
		plusButton.releaseAction += delegate{ x = 0; };

		minusButton.action += delegate { addTime(-1.0f); } ;
		minusButton.releaseAction += delegate{ x = 0; };

		datePicker = new DatePicker(new Rect(100,100, 300, 100 ) );
	}
	
	public void setPlanetsDate(DateTime date )
	{
		double dateHours = DatePicker.getDateInHours(  date );
		if (dateHours != lastDateHours)
		{
			Debug.Log("Date set");
			foreach ( Planet planet in Sun.Planets ){
				planet.CurrentTime = dateHours;
				planet.SetDate(dateHours);
			}
		}
		lastDateHours = dateHours;
	}


	void Update () 
	{
		TimeConstant = TimeConstantCurrent;
		plusButton.PerformUpdate();
		minusButton.PerformUpdate();

		if ( !initialDateSet){
			setPlanetsDate(DateTime.Now);
			initialDateSet = true;
		}


		if (Sun.TimeConstant == 0){
			setPlanetsDate( datePicker.getDate() );
		}
	}


	private 
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
			TimeConstantCurrent = GetDefaultTimeConstant();
		}
		datePicker.onGui();
	}

	public static float GetDefaultTimeConstant()
	{
		return (0.02f * Planet.EARTH_TICKS_PER_HOUR) / 3600.0f;
	}
}
