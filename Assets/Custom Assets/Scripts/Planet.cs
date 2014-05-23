
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bakalarka;
using System;

public class Planet : MonoBehaviour {

	public static Planet selectedPlanet = null;
	public List<Planet> childObjects = new List<Planet>();
	


	public double RotateSpeed = 0;
	public double RotateAngle = 0;
	public double DayLength  = 0;
	public int DayCounter  =0;
	public float Tilt = 0;
	private Vector3 TiltVector = Vector3.up;
	public float Inclination = 0;
	
	public float Eccentricity = 0;
	private double OrbitalSpeed = 0;
	private double OrbitalAngle = 0;
	public double GravitParam = 0;
	private float Velocity = 0;
	private double AverageVelocity = 0;
	private int YearCounter = 0;
	public double Period = 0; 
	private float Perimeter = 0;
	public Planet parentObject = null;
	private double Area;
	
	public Vector3 U = new Vector3(1,0,0);
	public Vector3 V = new Vector3(0,0,1);
	
	private Ellipse ellipse = null;
	private Quaternion originalRotation;
	
	public bool isGlobalTime = false;
	public Light sunLight;
	private double initialHour = 0;
	private LineRenderer renderer;
	
	private void rotateLight()
	{
		if (sunLight != null){
			Vector3 direction = (this.transform.localPosition - parentObject.transform.localPosition).normalized;
			sunLight.transform.transform.rotation = Quaternion.LookRotation(direction);
			GameObject scene = GameObject.Find("Scene");
			sunLight.transform.Rotate ( scene.transform.rotation.eulerAngles );
		}
	}

	public float getApsisDistance(){
		return ellipse.getApsisDistance();
	}


	GUIText guiText;
	
	void InitPlanet()
	{
		//Font
		GameObject text = new GameObject("GuiText." + this.name);
		text.layer = 13;
		guiText = (GUIText)text.AddComponent(typeof(GUIText));
		guiText.fontSize = isMoon ? 11 : 13;
		guiText.text = this.name;


		//Ostatok
		originalRotation = this.transform.localRotation;
		U = (Quaternion.Euler( 0, 0,-Inclination) * U ).normalized;
		
		float distance = (this.transform.position - parentObject.transform.position ).magnitude;
		ellipse = new Ellipse( U , V , distance , Eccentricity );
		if (Period > 0 )
		{
			GravitParam = ellipse.getGravitationParameter(Period);
		}else{
			Period = ellipse.getPeriod(GravitParam);
		}


		Perimeter = ellipse.getPerimeter();
		Area =  ellipse.getAreaVelocity(GravitParam);
		
		OrbitalSpeed = ellipse.getAngularVelocity(OrbitalAngle, Area);
		Velocity = ellipse.getVelocity(OrbitalAngle,Area).magnitude;
		
		AverageVelocity = ellipse.getAverageOrbitalSpeed(GravitParam);
		
		
		//hourPositions = new PlanetPositions (ellipse, GravitParam);
		
		
		if (DayLength > 0)
		{
			double f =   Period / DayLength ;
			//TOTO JE URCITE zle
			//double f =  hourPositions.getTicksPerHour() * DayLength;
			RotateSpeed =   360.0f  / f ; 
		}
		
		
		if (OrbitalAngle > 0){
			initialHour = CurrentTime;
			
		}

		if (isMoon){
			Tilt += parentObject.Tilt;
		}
	
		setPlanetTilt();
		Sun.AddPlanet(this);
		
		//Init Renderere
		renderer = this.gameObject.AddComponent<LineRenderer>();
		renderer.useWorldSpace = true;
		renderer.material= new Material(Shader.Find("Particles/Additive"));		

		if (this.name == "Io" || this.name == "Ganymede" || this.name == "Calisto" || this.name == "Europa" )
		{
			Debug.Log("Hook");
		}
		if (isMoon){
			parentObject.AddMoon(this);
		}
	}
	

	private void AddMoon(Planet moon)
	{
		int i = 0;
		foreach (Planet p in childObjects ){
			if (moon.getApsisDistance() < p.getApsisDistance()){
				break;
			}
			i++;
		}
		childObjects.Insert(i, moon);
	}

	
	private void setPlanetTilt()
	{
		Quaternion quat = Quaternion.AngleAxis( Tilt, Vector3.forward);
		TiltVector = quat * TiltVector;
		//this.transform.rotation = Quaternion.Euler( new Vector3(0,0, Tilt));
	}
	
	public static float getMaxTime(){
		return DatePicker.YEAR_COUNT * DatePicker.EARTH_PERIOD;
	}
	
	public void SetDate( double hours ){

		CurrentTime = hours;
		//RotateAngle = RotateSpeed * ( ( hour + minute * Math2f.MIN_TO_HOUR ) * hourPositions.getTicksPerHour() ); 
	}
	
	void OnMouseDown()
	{
		selectedPlanet = this;
		//Debug.Log("clicked on the planet " + this.name);
	}

	void Start () {
		if (isSun){
			Sun.SunAsPlanet = this;
			return;
		}
		InitPlanet();
  	}	
	
	public double CurrentTime = 0;
	public bool isSun = false;
	public bool isMoon = false;

	public void SetCurrenTime(double currentTime){
		CurrentTime = currentTime;
	}

	public void DrawOrbit(bool draw){
		if (draw){
			Vector3 center = parentObject.transform.localPosition; 
			ellipse.drawAroundPoint(   center - ellipse.getF2() , renderer,  this.transform, this == selectedPlanet);
		}else{
			renderer.SetVertexCount(0);
		}
	}

	public Vector3 getFuturePosition(int ticks){
		if (isSun){
			return Vector3.zero;
		}
		Vector3 center = parentObject.getFuturePosition(ticks); 
		double meanAnomally =  (( CurrentTime + Sun.TimeConstant*(double)ticks ) / Period ) * 360.0;
		return center + ellipse.getPosition2( ellipse.EccentricAnnomaly(meanAnomally, 5) ); 
	}

	public void Advance()
	{
		this.transform.localRotation = Quaternion.Euler( new Vector3(0,0, Tilt));
		if (DayLength > 0)
		{
			RotateAngle =  ((CurrentTime / DayLength) *360 ) % 360.0 ;
			if (this.name == "Earth"){
				Debug.Log("Hook");
			}
			//RotateAngle += 0.1 ;
			DayCounter  =  ( int) ( RotateAngle / 360.0f );
			//this.transform.localRotation = Quaternion.AngleAxis((float)RotateAngle, TiltVector);
			//this.transform.localEulerAngles = new Vector3(0,(float)RotateAngle, Tilt);
			this.transform.Rotate( new Vector3(0, (float) -RotateAngle, 0), Space.Self);
			//setPlanetTilt();
			Debug.DrawLine(this.transform.position - TiltVector*120, this.transform.position + TiltVector*120, Color.green);
		}
		
		if (!isSun){
			Vector3 center = parentObject.transform.localPosition; 
		
			//OrbitalAngle += (OrbitalSpeed  * (float)Sun.TimeConstant);
			YearCounter = (int) (OrbitalAngle / 360);


			//Vector3 tmpPos = ellipse.getPosition (OrbitalAngle, 1.0f,0);
			//Vector3 pos =  ( center  - ellipse.getF1() )  + tmpPos;

			double meanAnomally =  CurrentTime / Period * 360.0;
			Vector3 tmpPos = ellipse.getPosition2( ellipse.EccentricAnnomaly(meanAnomally, 5) ); 
			Vector3 pos =   center  + tmpPos;
			//Debug.DrawLine(pos, this.transform.position, Color.red, 1000); 
			this.gameObject.transform.localPosition = pos;
			
			//Velocity = ellipse.getVelocity(OrbitalAngle, Area).magnitude;
			//OrbitalSpeed = ellipse.getAngularVelocity(OrbitalAngle, Area );

			rotateLight();
		}

		//dat do metody
		CurrentTime += Sun.TimeConstant;


		float maxTime =  getMaxTime();
		if ( CurrentTime < 0 ){
			CurrentTime = maxTime + CurrentTime;;
		}  
		if ( CurrentTime > maxTime ){
			CurrentTime = CurrentTime % maxTime;
		}
		if ( isGlobalTime && Sun.TimeConstant != 0){
			Sun.datePicker.setDateToPickers(CurrentTime);
		}

	}
	
	void FixedUpdate () {
		if (guiText != null){
			if (Sun.ShowPlanetNames){
				guiText.enabled = true;
				Camera screenCam = GameObject.Find("Screen Camera").GetComponent<Camera>();
				Vector3 tmp = screenCam.WorldToViewportPoint(this.transform.position);
				guiText.transform.position = tmp;
			}else{
				guiText.enabled = false;
			}
		}
		Advance();
		//scaleByTouchScreen();
	}
}
