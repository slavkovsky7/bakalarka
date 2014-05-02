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

	float timeAccelaration = 0;
	public void addTime(float n){

		float a = n * 0.001f;
		if ( TimeConstantCurrent > -1.00f && TimeConstantCurrent < 1.00f ){
			a *= 0.01f;
		}

		timeAccelaration += a;
		//TimeConstantCurrent += timeAccelaration;
		TimeConstantCurrent +=timeAccelaration;
	}

	//private float x = 0;
	private bool followingPlanet = false;


	/*public void addTime(float znamienko ){
		float tmpX = x + 1.0f;
		//TimeConstantCurrent += timeAccelaration;
		//float accelaration = znamienko * ( 0.000001f*Mathf.Pow( (float)x ,2.
		float t  = tmpX / 500;
		float a = 0.0001f;
		if (znamienko < 0 && TimeConstantCurrent > -1.00f && TimeConstantCurrent < 1.00f ){
			a *= 0.01f;
		}
		float accelaration = znamienko * ( a * Mathf.Pow(10, t)* Mathf.Pow((float)tmpX , 2.0f));
		if (accelaration < 50.0f )
		{
			x = tmpX;
		}

		if ( Math.Abs( TimeConstantCurrent + accelaration ) < 300.0f)
		{
			TimeConstantCurrent += accelaration;
		}
	}*/

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
		plusButton = new KeyButton(createGuiRect(10,Screen.height - 115,50,50), "+", KeyCode.KeypadPlus );
		minusButton = new KeyButton(createGuiRect(10,Screen.height - 60,50,50), "-", KeyCode.KeypadMinus );
		datePicker = new DatePicker(createGuiRect(200, Screen.height - 115, 300, 100 ) );

		TimeConstantCurrent = GetDefaultTimeConstant();
		TimeConstant = GetDefaultTimeConstant();


		plusButton.action  += delegate { addTime(+1.0f); } ;
		plusButton.releaseAction += delegate{ timeAccelaration = 0; };

		minusButton.action += delegate { addTime(-1.0f); } ;
		minusButton.releaseAction += delegate{ timeAccelaration = 0; };
		
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
			//plusButton.PerformUpdate ();
			//minusButton.PerformUpdate ();
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


	private Rect createGuiRect( int x, int y, int w, int h){
		Rect result = new Rect(x, y, w, h);
		TouchControl.GuiRectangles.Add(result);
		return result;
	}

	private const int TIME_BTN_ID = 1;
	private const int RESET_TIME_BTN_ID = 2;
	private const int RESET_VIEW_BTN_ID = 3;
	private const int EXIT_BTN_ID = 4;
	private const int UNLOCK_BTN_ID = 5;
	private const int PLANET_BTNS = 100;
	private const int MOONS_BTNS = 200;


	private void OnGUI(){

		if (!isPaused) {
			plusButton.PerformOnGui ();
			minusButton.PerformOnGui ();
		}

		if ( KeyButton.Button(  createGuiRect(70,Screen.height - 60, 100, 50)  ,  TimeButtonString, TIME_BTN_ID ) ) {
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
			if (KeyButton.Button ( createGuiRect(70, Screen.height - 115, 100, 50), "Reset Time", RESET_TIME_BTN_ID)) {
					TimeConstantCurrent = GetDefaultTimeConstant ();
			}
		}
	
		if ( KeyButton.Button( createGuiRect(Screen.width/2 - 170,10, 85, 40) ,  "Reset View", RESET_VIEW_BTN_ID ) ) {
			TouchControl.ResetView();
			followingPlanet = false;
		}

		if ( KeyButton.Button( createGuiRect(Screen.width/2 - 90, Screen.height - 50 , 85, 40) ,  "Exit" , EXIT_BTN_ID) ) {
			Application.Quit();
		}

		//BBTouchableButton.Instantiate(  );

		if (followingPlanet){
			if ( KeyButton.Button( createGuiRect(Screen.width/2 - 80,10, 70, 40) ,  "Unlock" , UNLOCK_BTN_ID) ) {
				followingPlanet = false;
			}
		}

		int i = 0;
		foreach (Planet planet in planets) {
			int bWidth = 80;
			if ( !planet.isMoon && KeyButton.Button( createGuiRect  ( i*bWidth, 10 , bWidth, 40 ) , planet.name, PLANET_BTNS+i  ) ){
				followingPlanet = true;
				Planet.selectedPlanet = planet;
			}

			if (Planet.selectedPlanet != null){
				Planet planetWithMoons = Planet.selectedPlanet.isMoon ? Planet.selectedPlanet.parentObject : Planet.selectedPlanet; 
				if (planetWithMoons == planet){
					int j = 0;
					foreach ( Planet moon in  planet.childObjects){
						j++;
						if ( KeyButton.Button(  createGuiRect( i*bWidth, 20 + j*30 , bWidth , 30 ) , moon.name, MOONS_BTNS + PLANET_BTNS*i + j) ) {
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
			//Debug.Log("setPositionToPlanet = " + Planet.selectedPlanet.transform.localPosition);
			Vector3 tmpPos =  -(TouchControl.ScaleFactor) *  Planet.selectedPlanet.transform.localPosition;
			tmpPos.y += TouchControl.OriginalY;
			sceneObjects.transform.position =  Quaternion.Euler ( sceneObjects.transform.localRotation.eulerAngles) * tmpPos;
		}
	}
}


