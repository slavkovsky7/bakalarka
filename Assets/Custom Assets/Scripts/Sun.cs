using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class Sun : MonoBehaviour {

	public static float TimeConstant = 0;
	public float TimeConstantCurrent = 0;
	//private float timeAccelaration = 0;
	private bool isPaused = false;
	private float lastTimeConstantCurrent = 0;

	string[] pole = new string[]{"jan", "feb", "march"};
	private static List<Planet> planets = new List<Planet>();

	private bool initialDateSet = false;
	private double lastDateHours = -1;


	//GUI
	public static DatePicker datePicker = null;
	private KeyButton plusButton;
	private KeyButton minusButton;
	private String TimeButtonString = "Pause";
	public Camera screenCam;
	public Camera projectorCam;
	private Rect guiRect;

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
	private bool followingPlanet = false;


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
		//Debug.Log("a =" + a);
		float accelaration = znamienko * ( a * Mathf.Pow(10, t)* Mathf.Pow((float)tmpX , 2.0f));
		if (accelaration < 50.0f )
		{
			x = tmpX;
		}

		if ( Math.Abs( TimeConstantCurrent + accelaration ) < 300.0f)
		{
			TimeConstantCurrent += accelaration;
		}

		///Debug.Log( "e^" + x + " = accelaration = "  + accelaration);
	}

	public static void AddPlanet(Planet planet){
		if (planet.isMoon ){
			planets.Add(planet);
		}else{
			int i = 0;
			foreach (Planet p in planets ){
				if (planet.getApsisDistance() < p.getApsisDistance() || p.isSun || p.isMoon ){
					break;
				}
				i++;
			}
			planets.Insert(i, planet);
		}
	}

	void Start () 
	{
		//guiRect = 
		plusButton = new KeyButton(new Rect(10,Screen.height - 115,50,50), "+", KeyCode.KeypadPlus );
		minusButton = new KeyButton(new Rect(10,Screen.height - 60,50,50), "-", KeyCode.KeypadMinus );
		datePicker = new DatePicker(new Rect(200, Screen.height - 115, 300, 100 ) );

		TimeConstantCurrent = GetDefaultTimeConstant();
		TimeConstant = GetDefaultTimeConstant();
		plusButton.action  += delegate { addTime(+1.0f); } ;
		plusButton.releaseAction += delegate{ x = 0; };

		minusButton.action += delegate { addTime(-1.0f); } ;
		minusButton.releaseAction += delegate{ x = 0; };
		projectorCam.enabled = false;

	}
	
	public void setPlanetsDate(DateTime date )
	{
		double dateHours = DatePicker.getDateInHours(  date );
		if (dateHours != lastDateHours)
		{
			//Debug.Log("Date set");
			foreach ( Planet planet in Sun.planets ){
				planet.CurrentTime = dateHours;
				planet.SetDate(dateHours);
			}
		}
		lastDateHours = dateHours;
	}


	void Update () 
	{
		//Woo
		if (projectorCam.enabled == false){
			projectorCam.enabled = true;
		}
		TimeConstant = TimeConstantCurrent;
		if (plusButton != null && minusButton != null) {
			plusButton.PerformUpdate ();
			minusButton.PerformUpdate ();
		}
		if ( !initialDateSet){
			setPlanetsDate(DateTime.Now);
			initialDateSet = true;
		}
		datePicker.onUpdate();

		if (Sun.TimeConstant == 0){
			setPlanetsDate( datePicker.getDate() );
		}
		RenderSettings.haloStrength = 0.5f * TouchControl.ScaleFactor;
		//halo.range = 500 * TouchControl.ScaleFactor;

		if ( followingPlanet ){
			setPositionToPlanet();
		}
	}

	public float hSliderValue = 0.0f;

	private Rect createGuiRect( int x, int y, int w, int h){
		Rect result = new Rect(0, 100, 300, 30);
		TouchControl.GuiRectangles.Add(result);
		return result;
	}

	private void OnGUI(){

		if (!isPaused) {
			plusButton.PerformOnGui ();
			minusButton.PerformOnGui ();
		}
		if ( GUI.Button( new Rect(70,Screen.height - 60, 100, 50) ,  TimeButtonString ) ) {
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

		if (!isPaused) {
			if (GUI.Button (new Rect (70, Screen.height - 115, 100, 50), "Reset Time")) {
					TimeConstantCurrent = GetDefaultTimeConstant ();
			}
		}
	
		if ( GUI.Button( new Rect(Screen.width/2 - 170,10, 85, 40) ,  "Reset View" ) ) {
			TouchControl.ResetView();
			followingPlanet = false;
		}

		if (followingPlanet){
			if ( GUI.Button( new Rect(Screen.width/2 - 80,10, 70, 40) ,  "Unlock" ) ) {
				followingPlanet = false;
			}
		}

		int i = 0;
		foreach (Planet planet in planets) {
			int bWidth = 70;
			if ( !planet.isMoon && GUI.Button( new Rect  ( i*bWidth, 10 , bWidth, 40 ) , planet.name ) ){
				followingPlanet = true;
				Planet.selectedPlanet = planet;
			}

			if (Planet.selectedPlanet != null){
				Planet planetWithMoons = Planet.selectedPlanet.isMoon ? Planet.selectedPlanet.parentObject : Planet.selectedPlanet; 
				if (planetWithMoons == planet){
					int j = 0;
					foreach ( Planet moon in  planet.childObjects){
						j++;
						if ( GUI.Button( new Rect  ( i*bWidth, 10 + j*35 , bWidth - 7, 35 ) , moon.name ) ) {
							followingPlanet = true;
							Planet.selectedPlanet = moon;
						}
					}
				}
			}
			i++;
		}
	
		datePicker.onGui();
	}


	public static float GetDefaultTimeConstant()
	{
		return (0.02f * Planet.EARTH_TICKS_PER_HOUR) / 3600.0f;
	}

	public void setPositionToPlanet(){
		GameObject sceneObjects = GameObject.Find("Scene");
		if (Planet.selectedPlanet != null){
			Debug.Log("setPositionToPlanet = " + Planet.selectedPlanet.transform.localPosition);
			sceneObjects.transform.position = -(TouchControl.ScaleFactor) *  Planet.selectedPlanet.transform.localPosition ;
		}
	}
}


