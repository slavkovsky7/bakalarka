using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class Sun : MonoBehaviour {

	public static double TimeConstant = 0;
	public double TimeConstantCurrent = 0;
	//private float timeAccelaration = 0;
	private bool isPaused = false;
	private double lastTimeConstantCurrent = 0;

	string[] pole = new string[]{"jan", "feb", "march"};
	private static List<Planet> planets = new List<Planet>();
	public static Planet SunAsPlanet = null; 

	private bool initialDateSet = false;
	private double lastDateHours = -1;

	public static bool DrawOrbits = true;

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
		if ( Math.Abs( (Sun.TimeConstant / GetDefaultTimeConstant() ) ) < 60.00f ){			
			a *= 0.00005f;
		}else if ( Math.Abs(Sun.TimeConstant) < 1.00f ){			
			a *= 0.005f;
		}

		timeAccelaration += a;
		//TimeConstantCurrent += timeAccelaration;
		double tmp = TimeConstantCurrent + timeAccelaration;
		if (Math.Abs(TimeConstantCurrent) < 300 ){
			TimeConstantCurrent = tmp;
		}
	}

	//private float x = 0;
	private bool followingPlanet = false;

	public static void AddPlanet(Planet planet){

		if (planet.isMoon ){
			planets.Add(planet);
		}else if (planet.isSun){
			planets.Insert(0, planet);
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
				planet.SetCurrenTime( dateHours );
				planet.SetDate(dateHours);
			}
		}
		lastDateHours = dateHours;
	}


	public void RotateSkybox()
	{
		GameObject sceneObjects = GameObject.Find("Scene");
		Quaternion rot = Quaternion.Euler( new Vector3(-90+sceneObjects.transform.eulerAngles.x,
		                                               sceneObjects.transform.eulerAngles.y,
		                                               sceneObjects.transform.eulerAngles.z));
		Matrix4x4 m = Matrix4x4.TRS (Vector3.zero, rot , new Vector3(1,1,1) );
		RenderSettings.skybox.SetMatrix ("_Rotation", m);
	}

	void Update() 
	{
		RotateSkybox();
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
		
		if ( followingPlanet && easePositions != null ){
			GameObject sceneObjects = GameObject.Find("Scene");
			//sceneObjects.transform.position = - TouchControl.ScaleFactor * Planet.selectedPlanet.transform.localPosition;


			Vector3 tmpPos = -(TouchControl.ScaleFactor) * lastFollowedPosition; ;
			if (!unlocking){
				tmpPos.y += TouchControl.OriginalY;
			}else{
				tmpPos.y = lastFollowedPosition.y;
			}
			tmpPos = Quaternion.Euler ( sceneObjects.transform.localRotation.eulerAngles) * tmpPos;
			sceneObjects.transform.position = tmpPos;

		}

		//kreslime to tu lebo ak by sme to kreslili v Planet.Update tak matica sa zmeni najpr v planetach potom tu.
		// Potom pri vysokych rychlostiach to vyzera nahovno
		foreach ( Planet p in planets){
			p.DrawOrbit(DrawOrbits);
		}
	}

	//TOTO je problem
	private Rect createGuiRect( int x, int y, int w, int h){
		Rect result = new Rect(x, y, w, h);
		if ( !TouchControl.GuiRectangles.Contains(result)){
			TouchControl.GuiRectangles.Add(result);
		}
		return result;
	}

	private const int TIME_BTN_ID = 1;
	private const int RESET_TIME_BTN_ID = 2;
	private const int RESET_VIEW_BTN_ID = 3;
	private const int EXIT_BTN_ID = 4;
	private const int UNLOCK_BTN_ID = 5;
	private const int PLANET_BTNS = 100;
	private const int ORBITS_BTN_ID = 6;
	private const int MOONS_BTNS = 200;

	private String drawOrbitsBtnText = "Orbits On";
	private String btnMouseSwitchText = TouchControl.IgnoreMouse ? "Mouse Off": "Mouse on" ;


	bool showInfo = false;
	private void showPlanetInfo(){
		int w = 300;
		int h = 300;
		int x = Screen.width / 2 - h - 10, y = Screen.height / 2 - w / 2;
		if (Planet.selectedPlanet != null){
			if (showInfo){
				GUI.Box(  new Rect(x,y,w,h) , "");
				string text = " - blablabla asdasd \n - blablabla blablabla \n - blablabla "; 

				GUIStyle style = new GUIStyle();
				style.fontSize = 20;
				style.normal.textColor = GUI.skin.label.normal.textColor;

				GUI.Label( new Rect(x + 10, y + 35 , w , h) , text, style);
			}
			if ( KeyButton.Button(new Rect(x,y,w,30), Planet.selectedPlanet.name ,432321) ){
				showInfo = !showInfo;
			}
		}
	}


	private void OnGUI(){

		showPlanetInfo();

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
				DatePicker.TimeRunning = false;
			}else {
				isPaused = false;
				TimeConstantCurrent = lastTimeConstantCurrent;
				TimeConstant = lastTimeConstantCurrent;
				lastTimeConstantCurrent = 0;
				TimeButtonString = "Pause";
				DatePicker.TimeRunning = true;
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
		if ( KeyButton.Button( createGuiRect(Screen.width/2 - 170,55, 85, 40) ,  drawOrbitsBtnText , ORBITS_BTN_ID ) ) {
			if (!DrawOrbits){
				drawOrbitsBtnText = "Orbits On";
				DrawOrbits = true;
			}else{
				DrawOrbits = false;
				drawOrbitsBtnText = "Orbits Off";
			}
		}
		
		if ( GUI.Button( createGuiRect(Screen.width/2 - 2*90, Screen.height - 50 , 85, 40) ,  btnMouseSwitchText ) ) {
			if (TouchControl.IgnoreMouse){
				TouchControl.IgnoreMouse = false;
				btnMouseSwitchText = "Mouse On";
			}else{
				TouchControl.IgnoreMouse = true;
				btnMouseSwitchText = "Mouse Off";
			}
		}

		if ( KeyButton.Button( createGuiRect(Screen.width/2 - 90, Screen.height - 50 , 85, 40) ,  "Exit" , EXIT_BTN_ID) ) {
			Application.Quit();
		}

		//BBTouchableButton.Instantiate(  );
		//Treba dorobit unloack ease
		if (followingPlanet){
			if ( KeyButton.Button( createGuiRect(Screen.width/2 - 80,10, 70, 40) ,  "Unlock" , UNLOCK_BTN_ID) ) {
				//followingPlanet = false;
				//GameObject sceneObjects = GameObject.Find("Scene");
				//Vector3 pos = sceneObjects.transform.position;
				//This is it
				//sceneObjects.transform.position = new Vector3 ( pos.x, TouchControl.OriginalY, pos.z ) ;
				resetEase();
				//followedPlanet = null;
				unlocking = true;
			}
		}
		GUIStyle style = new GUIStyle();
		style.fontSize = 50;
		style.normal.textColor = GUI.skin.label.normal.textColor;
		GUI.Label(createGuiRect(530, Screen.height - 90, 100, 100 ), getTimeModifierAsString(), style );


		//Planet buttons
		int bWidth = 80;
		if ( KeyButton.Button( createGuiRect  ( 0, 10 , bWidth, 40 ) , "Sun" , PLANET_BTNS ) ){
			setEaseToObject(SunAsPlanet);
		}
		int i = 1;
		foreach (Planet planet in planets) {

			if ( !planet.isMoon && KeyButton.Button( createGuiRect  ( i*bWidth, 10 , bWidth, 40 ) , planet.name, PLANET_BTNS+i  ) ){
				setEaseToObject(planet);
			}

			if (Planet.selectedPlanet != null){
				Planet planetWithMoons = Planet.selectedPlanet.isMoon ? Planet.selectedPlanet.parentObject : Planet.selectedPlanet; 
				if (planetWithMoons == planet){
					int j = 0;
					foreach ( Planet moon in  planet.childObjects){
						j++;
						if ( KeyButton.Button(  createGuiRect( i*bWidth, 20 + j*30 , bWidth , 30 ) , moon.name, MOONS_BTNS + PLANET_BTNS*i + j) ) {
							setEaseToObject(moon);
						}
					}
				}
			}
			i++;
		}

		datePicker.onGui();
	}

	public static double GetDefaultTimeConstant()
	{
		//return (0.02f * Planet.EARTH_TICKS_PER_HOUR) / 3600.0f;
		return 1.0/50.0/3600;
	}
	private String getTimeModifierAsString(){
		double timeModifier = Sun.TimeConstant / GetDefaultTimeConstant();
		string perWhat = "";
		bool minus = timeModifier < 0;
		timeModifier = Math.Abs(timeModifier);
		double showNumber = Math.Abs ( timeModifier );
		if (  timeModifier /  ( 3600 * 24 * 365 ) > 1  ) {
			perWhat = "y";
			showNumber =  timeModifier /  ( 3600 * 24 * 365 );
		}else if (timeModifier /  ( 3600 * 24 * 30  ) > 1  ) {
			perWhat = "mon";
			showNumber = timeModifier /  ( 3600 * 24 * 30  );
		}else if (timeModifier /  ( 3600 * 24  ) > 1  ) {
			perWhat = "d"; 
			showNumber = timeModifier /  ( 3600 * 24  );
		}else if (timeModifier /  ( 3600 ) > 1  ) {
			perWhat = "h"; 
			showNumber = timeModifier /  ( 3600 );
		}else if (timeModifier /  ( 60 ) > 1  ) {
			perWhat = "min";
			showNumber = timeModifier /  ( 60 );
		}else {
			perWhat = "s";
		}
		if (minus){
			showNumber *= -1;
		}
		return Math.Round(showNumber, 2) + perWhat+"/s";
	}

	//**************************Easing**************************

	private List<Vector3> easePositions = null;
	private List<float> easeZooms = null;
	Vector3 lastFollowedPosition =  Vector3.zero;
	private Planet followedPlanet = null;
	private bool unlocking = false;

	private void setEaseToObject(Planet p ){
		if ( followedPlanet != p )
		{
			followingPlanet = true;
			Planet.selectedPlanet = p;
			followedPlanet = p;
			resetEase();
		}
	}



	static List<Vector3> getEasePositions(Vector3 start, Vector3 end, double duration)
	{
		List<Vector3> result = new List<Vector3>();
		double magnitute = Math.Floor ( (end - start).magnitude );
		Vector3 direction = (end - start).normalized;
		
		double currentTime  = 0;
		while (currentTime < duration)
		{
			double ease = easeInOutQuad(currentTime, 0, magnitute, duration);
			result.Add(start + (float)ease * direction );
			currentTime += 1;
			
		}
		return result;
	}

	static List<float> getEaseZooms(int duration){
		duration = duration / 2;
		double currentTime  = 0;
		List<float> tmpResult = new List<float>();
		while (currentTime < duration)
		{
			double ease = easeInOutQuad(currentTime, 1.0, TouchControl.ScaleFactor - 1.0, duration);
			tmpResult.Insert(0, (float)ease );
			currentTime += 1;
		}
		List<float> result = new List<float> ( tmpResult );
		for (int i = tmpResult.Count - 1 ; i >= 0; i-- ){
			result.Add( tmpResult[i] ); 
		}
		return result;
	}


	static double easeInOutQuad(double t, double b, double c, double d)
	{
		t /= d/2;
		if (t < 1) return c/2*t*t + b;
		t--;
		return -c/2 * (t*(t-2) - 1) + b;
	}


	 private void resetEase(){
		easePositions = null;
		easeZooms = null;
	}

	public void FixedUpdate(){   
		if ( followingPlanet ){
			setPositionToPlanet();
		}
	}

	public void setPositionToPlanet(){
		GameObject sceneObjects = GameObject.Find("Scene");
		if (easePositions == null){
			if (unlocking){
				int duration = (int)( Math.Abs(TouchControl.OriginalY - sceneObjects.transform.position.y ) + 10 ) / 3 ;
				Vector3 futurePosition  =followedPlanet.getFuturePosition(duration) ;
				futurePosition.y = TouchControl.OriginalY;
				Vector3 start = followedPlanet.transform.localPosition;
				start.y = sceneObjects.transform.position.y;
				easePositions = getEasePositions( start , futurePosition, duration);
			}else{
				float y = -(sceneObjects.transform.position.y - TouchControl.OriginalY ) / TouchControl.ScaleFactor; 
				Vector3 start = new Vector3(-sceneObjects.transform.position.x/TouchControl.ScaleFactor, y ,-sceneObjects.transform.position.z/TouchControl.ScaleFactor);
				Quaternion quat = Quaternion.AngleAxis ( -sceneObjects.transform.eulerAngles.y, Vector3.up );
				start = quat*start;

				int duration = 100;
				double easeLastDistance = (start - followedPlanet.transform.localPosition ).magnitude;
				bool useZoom = easeLastDistance > 1000 && TouchControl.ScaleFactor > 5;
				int totalDuration = useZoom ? duration*2 : duration ;
				Vector3 futurePosition  =followedPlanet.getFuturePosition(totalDuration) ;
				easePositions = getEasePositions(start, futurePosition, duration);

				if (useZoom){
					easeZooms = getEaseZooms( duration );
					for (int i = 0; i < easeZooms.Count / 2; i++){
						easePositions.Insert(0,  easePositions[0] );
						easePositions.Add(  easePositions[easePositions.Count - 1] );
					}
					for (int i = 0 ; i < duration; i++){
						easeZooms.Insert( duration / 2 , easeZooms[duration / 2]);
					}
				}
			}
		}

		if (followedPlanet != null){
			//Debug.Log("setPositionToPlanet = " + Planet.selectedPlanet.transform.localPosition);
			//Vector3 tmpPos =  -(TouchControl.ScaleFactor) *  Planet.selectedPlanet.transform.localPosition;
			Vector3 planetPos = followedPlanet.transform.localPosition;
			Vector3 tmpPos = ( easePositions.Count == 0 ) ? planetPos : easePositions[ 0 ];

			if (easePositions.Count > 0){
				easePositions.RemoveAt( 0 );
				if (easeZooms != null ){
					TouchControl.zoom( easeZooms[0] );
					easeZooms.RemoveAt(0);
				}
			}

			if (unlocking && easePositions.Count == 0){
				followedPlanet = null;
				followingPlanet = false;
				unlocking = false;
			}

			lastFollowedPosition = tmpPos;
		}
	}
}


