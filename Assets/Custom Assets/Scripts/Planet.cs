
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bakalarka;
using System;

public class Planet : MonoBehaviour {
	
	
	public const float EARTH_TICKS_PER_HOUR = 2.062971f; 
	public static Planet selectedPlanet = null;
	public List<Planet> childObjects = new List<Planet>();

	class PlanetPosition {
		public float angle;
		public float speed;
		public PlanetPosition( float angle , float speed ){
			this.angle = angle;
			this.speed = speed;
		}
	}
	
	class PlanetPositions
	{
		public PlanetPosition[] positions = null;
		private Ellipse ellipse = null;
		private double area = 0;
		private double period = 0;
		private double periodTime = 0;

		public PlanetPositions(Ellipse ellipse , double gravityParam )
		{
			this.ellipse = ellipse;
			this.period = (int)ellipse.getPeriod( gravityParam );
			this.area = ellipse.getAreaVelocity(gravityParam);
			double minSpeed = ellipse.getAngularVelocity( 0, area );
			this.positions = getPositionsArray( 0 , minSpeed, 360.0, getTimeModifier() );
		}

		private double getTimeModifier (){
			double timeModifier = 1.0;
			if (period > 100000 ){
				timeModifier = period / 100000;
			}
			return timeModifier;
		}
		
		
		internal double getTicksPerHour()
		{
			//return EARTH_TICKS_PER_HOUR;
			return (double)positions.Length / period;
		}
		
		private PlanetPosition[] getPositionsArray( double startAngle , double startSpeed, double endAngle , double timeModifier)
		{
			List<PlanetPosition> positionsList = new List<PlanetPosition>();
			double totalAngle = startAngle;
			double currentSpeed = startSpeed;
			positionsList.Add( new PlanetPosition( (float)totalAngle, (float)currentSpeed ) );
			while (totalAngle < endAngle)
			{
				totalAngle += currentSpeed*timeModifier;
				positionsList.Add( new PlanetPosition( (float) totalAngle, (float)currentSpeed ) );
				currentSpeed = ellipse.getAngularVelocity((float)totalAngle, (float)area); 
			}
			return positionsList.ToArray ();
		}
		
		public PlanetPosition getPlanetaryPosition(int hour , int second , float interval )
		{
			int start_hour = hour % (int)period;
			int end_hour =  ( hour + 1 )% (int)period;
			PlanetPosition start = getPlanetaryPosition(start_hour);
			PlanetPosition end = getPlanetaryPosition(end_hour);
			double endAngle = end.angle;
			//znamena ze sme islo z poslednej hodiny do hodiny 0 
			if ( start.angle > endAngle ){
				endAngle = 360.0 + endAngle;
			}
			PlanetPosition[] positionsPerSecond = getPositionsArray (start.angle, start.speed, endAngle, 1.0f / interval );
			int index = (int)((float)second * ((float)positionsPerSecond.Length / interval ));
			return positionsPerSecond[index];
		}
		
		public PlanetPosition getPlanetaryPosition(int hour )
		{
			hour = hour % (int)period;
			int index = (int) ( (float)(hour) * getTicksPerHour() );
			index  = (int)((float)index / getTimeModifier() );
			return positions[index];
		}
		
		public int findIndexPlanetPosition(double angle){
			angle = angle % 360.0;
			//kasleme na binarne hladanie
			for (int i = 0 ; i < positions.Length ; i++){	
				if ( positions[i].angle >= angle ) {
					return i;
				}
			}
			throw new System.Exception("Invalid angle provided for findIndexPlanetPosition or index was not found");
		}
		
		public double ticksToHour(double tick){
			return ( tick / getTicksPerHour() ) ;
		}
		/*
		public float hourToTicks(float tick){
			return tick / getTicksPerHour();
		}*/
	}
	
	
	
	public double RotateSpeed = 0;
	public double RotateAngle = 0;
	public double DayLength  = 0;
	public int DayCounter  =0;
	public float Tilt = 0;
	private Vector3 TiltVector = Vector3.up;
	public float Inclination = 0;
	
	public float Eccentricity = 0;
	public double OrbitalSpeed = 0;
	public double OrbitalAngle = 0;
	public double GravitParam = 0;
	public float Velocity = 0;
	public double AverageVelocity = 0;
	public int YearCounter = 0;
	public double Period = 0; 
	public float Perimeter = 0;
	public Planet parentObject = null;
	public double Area;
	
	public Vector3 U = new Vector3(1,0,0);
	public Vector3 V = new Vector3(0,0,1);
	
	private Ellipse ellipse = null;
	private PlanetPositions hourPositions;
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


	/*public void setApoapsisDate(Date date)
	{
		///DatePicker.getDateInHours(date);
	}*/
	
	void InitPlanet()
	{
		originalRotation = this.transform.localRotation;
		U = (Quaternion.Euler( 0, 0,-Inclination) * U ).normalized;
		
		float distance = (this.transform.position - parentObject.transform.position ).magnitude;
		ellipse = new Ellipse( U , V , distance , Eccentricity );
		if (Period > 0 )
		{
			Period = Math.Round(Period);
			GravitParam = ellipse.getGravitationParameter(Period);
		}else{
			Period = ellipse.getPeriod(GravitParam);
			Period = Math.Round(Period);
		}


		Perimeter = ellipse.getPerimeter();
		Area =  ellipse.getAreaVelocity(GravitParam);
		
		OrbitalSpeed = ellipse.getAngularVelocity(OrbitalAngle, Area);
		Velocity = ellipse.getVelocity(OrbitalAngle,Area).magnitude;
		
		AverageVelocity = ellipse.getAverageOrbitalSpeed(GravitParam);
		
		
		hourPositions = new PlanetPositions (ellipse, GravitParam);
		
		
		if (DayLength > 0)
		{
			//float f =  (float)hourPositions.positions.Length / ( Period / DayLength ) ;
			//TOTO JE URCITE zle
			double f =  hourPositions.getTicksPerHour() * DayLength;
			RotateSpeed =   360.0f  / f ; 
		}
		
		
		if (OrbitalAngle > 0){
			CurrentTick =  hourPositions.findIndexPlanetPosition(( OrbitalAngle % 360.0f)) ;
			CurrentTime = hourPositions.ticksToHour(CurrentTick);
			initialHour = CurrentTime;
			
		}
		setPlanetTilt();
		Sun.AddPlanet(this);
		
		//Init Renderere
		renderer = this.gameObject.AddComponent<LineRenderer>();
		renderer.useWorldSpace = true;
		renderer.material= new Material(Shader.Find("Particles/Additive"));		

		if (isMoon){
			parentObject.childObjects.Add(this);
		}
	}
	
	
	
	private void setPlanetTilt()
	{
		Quaternion quat = Quaternion.AngleAxis( Tilt, Vector3.forward);
		TiltVector = quat * TiltVector;
		this.transform.Rotate( new Vector3(0,0, Tilt), Space.Self);
	}
	
	public static float getMaxTime(){
		return DatePicker.YEAR_COUNT * DatePicker.EARTH_PERIOD;
	}
	
	public void SetDate( double hours ){
		
		hours += initialHour;
		int years = (int) ( hours / Period );
		int hour =  (int) ( hours );  
		int minute = (int)((hours - (float)hour) * 60.0f ) ;
		SetDate(years, hour, minute);
		CurrentTime = hours ;//+ 30*Math2f.SEC_TO_HOUR;
		CurrentTick = CurrentTime * hourPositions.getTicksPerHour();
	}
	
	void OnMouseDown()
	{
		selectedPlanet = this;
		//Debug.Log("clicked on the planet " + this.name);
	}
	
	public void SetDate( int years , int hour , int minute )
	{   
		//PlanetPosition position = hourPositions.getPlanetaryPosition (hour);	
		//OrbitalSpeed = position.speed;
		
		//OrbitalAngle = ( 360.0f* (float)years) +position.angle;
		if (this.name == "Mercury"){
			Debug.Log("Hook");
		}
		PlanetPosition pos = (minute > 0) ? hourPositions.getPlanetaryPosition( hour, minute, 60.0f ) : hourPositions.getPlanetaryPosition (hour);


		double a = hourPositions.positions[hourPositions.positions.Length -1].angle - 360;
		double d = ellipse.getAngularVelocity(a , Area ) ;
		double ostatok =  years*( a + d ) ;
		OrbitalAngle = ( 360.0f*years) + pos.angle + ostatok;
		OrbitalSpeed = pos.speed;
		
		transform.localRotation = originalRotation;
		
		RotateAngle = RotateSpeed * ( ( hour + minute * Math2f.MIN_TO_HOUR ) * hourPositions.getTicksPerHour() ); 
		
		this.transform.Rotate( new Vector3(0, 0, Tilt), Space.Self );
		this.transform.Rotate( new Vector3(0, (float)RotateAngle, 0),Space.Self );
	}
	
	void Start () {
		if (isSun)
			return;
		if  ( this.name  == "Ganymede"){
			Debug.Log("pice");
		}
		InitPlanet();

		/*
		if ( this.name == "Mercury" ){
			double w1 =  ellipse.getAngularVelocity(0, Area);
			double w2 = ellipse.getAngularVelocity(0, (360/w1) * Area  );


			double n = 10;  
			Sun.TimeConstant = 1;
			do{
				Sun.TimeConstant = 1;
				Advance();
			}  while (360 -0.0001> OrbitalAngle );
			double angle = OrbitalAngle;
			double c = CurrentTime;
			double area1 = TotalArea;

			CurrentTick = 0;
			CurrentTime = 0;
			OrbitalAngle = 0;
			TotalArea = 0;
			double full = ellipse.getFullArea(GravitParam);
			do{
				Sun.TimeConstant *= 1.1;
				Advance();
			}  while ( c > CurrentTime  );
			double angle2 = OrbitalAngle;
			double area2 = TotalArea;

			
			//CIEL
			SetDate(CurrentTime);
			double angle1 = OrbitalAngle;
			if (Mathf.Abs( (float)angle - (float)angle1) > 1 ){
				Debug.Log("Shit");
			}
		}*/
  	}	
	//private double TotalArea = 0;

	public double CurrentTick = 0;
	public double CurrentTime = 0;
	public bool isSun = false;
	public bool isMoon = false;

	public void Advance()
	{
		if (RotateSpeed > 0)
		{
			double speed =  RotateSpeed * Sun.TimeConstant;
			RotateAngle += speed ;
			DayCounter  =  ( int) ( RotateAngle / 360.0f );
			this.transform.Rotate( new Vector3(0, (float)speed, 0), Space.Self);
			//Debug.DrawLine(this.transform.position - TiltVector*50, this.transform.position + TiltVector*50, Color.green);
		}
		
		if (isSun)
			return;
		
		Vector3 center = parentObject.transform.localPosition; 
		if (Sun.DrawOrbits){
			ellipse.drawAroundPoint(   center - ellipse.getF1() ,  TouchControl.ScaleFactor,0, renderer,  this.transform, this == selectedPlanet);
		}else{
			renderer.SetVertexCount(0);
		}

		Vector3 tmpPos = ellipse.getPosition (OrbitalAngle, 1.0f,0);
		Vector3 pos =  ( center - ellipse.getF1() )  + tmpPos;
		//Debug.DrawLine(pos, this.transform.position, Color.red, 1000); 
		this.gameObject.transform.localPosition = pos;
		
		Velocity = ellipse.getVelocity(OrbitalAngle, Area).magnitude;
		OrbitalSpeed = ellipse.getAngularVelocity(OrbitalAngle, Area )* Sun.TimeConstant;
		//TotalArea += Area*Sun.TimeConstant;
		OrbitalAngle += OrbitalSpeed;
		YearCounter = (int) (OrbitalAngle / 360);


		CurrentTick += Sun.TimeConstant;
		//toot je problem. TOto je kratsie ako OrbitalAngle  , tedxa pri 30.0111 mam len 0....01 time pricom by mal byt
		CurrentTime = CurrentTick / hourPositions.getTicksPerHour();
		//CurrentTime = Sun.GetDefaultTimeConstant()*TotalArea;

		//CurrentTime = TotalArea ;;/// ellipse.getFullArea(GravitParam) ;
		float maxTime =  getMaxTime();
		if ( CurrentTime < 0 ){
			CurrentTime = maxTime - CurrentTime;
			CurrentTick = CurrentTime * hourPositions.getTicksPerHour();
		}  
		if ( CurrentTime > maxTime ){
			CurrentTime = CurrentTime % maxTime;
		}
		if ( isGlobalTime && Sun.TimeConstant != 0){
			Sun.datePicker.setDateToPickers(CurrentTime);
		}
		
		rotateLight();
	}
	
	void FixedUpdate () {
		Advance();
		//scaleByTouchScreen();
	}
}
