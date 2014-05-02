
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bakalarka;

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
		private float area = 0;
		private float period = 0;
		public PlanetPositions(Ellipse ellipse , float gravityParam )
		{
			this.ellipse = ellipse;
			this.period = (int)ellipse.getPeriod( gravityParam );
			this.area = ellipse.getAreaVelocity(gravityParam);
			float minSpeed = ellipse.getAngularVelocity( 0, area );
			float timeModifier = 1.0f;
			if (period > 100000f ){
				timeModifier = period / 100000f;
			}
			this.positions = getPositionsArray( 0 , minSpeed, 360.0f, timeModifier );
		}
		
		
		
		public float getTicksPerHour()
		{
			return EARTH_TICKS_PER_HOUR;
			//return ((float)positions.Length )  / period;
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
			PlanetPosition[] positionsPerSecond = getPositionsArray (start.angle, start.speed, end.angle, 1.0f / interval );
			int index = (int)((float)second * ((float)positionsPerSecond.Length / interval ));
			return positionsPerSecond[index];
		}
		
		public PlanetPosition getPlanetaryPosition(int hour )
		{
			hour = hour % (int)period;
			int index = (int) ( (float)(hour) * getTicksPerHour() );
			if (period > 100000f ){
				float timeModifier = period / 100000f;
				index  = (int)((float)index / timeModifier ); 
			}
			return positions[index];
		}
		
		public int findIndexPlanetPosition(float angle){
			angle = angle % 360.0f;
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
	
	
	
	public float RotateSpeed = 0;
	public float RotateAngle = 0;
	public float DayLength  = 0;
	public int DayCounter  =0;
	public float Tilt = 0;
	private Vector3 TiltVector = Vector3.up;
	public float Inclination = 0;
	
	public float Eccentricity = 0;
	public float OrbitalSpeed = 0;
	public float OrbitalAngle = 0;
	public float GravitParam = 0;
	public float Velocity = 0;
	public float AverageVelocity = 0;
	public int YearCounter = 0;
	public float Period = 0; 
	public float Perimeter = 0;
	public Planet parentObject = null;
	public float Area;
	
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
			GravitParam = ellipse.getGravitationParameter(Period);
		}else{
			Period = ellipse.getPeriod(GravitParam);
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
			float f =  hourPositions.getTicksPerHour() * DayLength;
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
		int hour = (int)hours;  
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
		PlanetPosition pos = (minute > 0) ? hourPositions.getPlanetaryPosition( hour, minute, 60.0f ) : hourPositions.getPlanetaryPosition (hour);
		OrbitalAngle = ( 360.0f* (float)years) + pos.angle;
		OrbitalSpeed = pos.speed;
		
		transform.localRotation = originalRotation;
		
		RotateAngle = RotateSpeed * ( ( hour + minute * Math2f.MIN_TO_HOUR ) * hourPositions.getTicksPerHour() ); 
		
		this.transform.Rotate( new Vector3(0, 0, Tilt), Space.Self );
		this.transform.Rotate( new Vector3(0, RotateAngle, 0),Space.Self );
	}
	
	void Start () {
		if (isSun)
			return;
		InitPlanet();
		//SetDate(CurrentHour, CurrentMinute );
		//Debug.Log( hourPositions.positions.Length + " "  + this.gameObject.name );
		//Debug.Log( hourPositions.getTicksPerHour() + " "  + this.gameObject.name );
	}	
	
	
	public double CurrentTick = 0;
	public double CurrentTime = 0;
	public bool isSun = false;
	public bool isMoon = false;

	public void Advance()
	{
		if (RotateSpeed > 0)
		{
			float speed =  RotateSpeed * (float)Sun.TimeConstant;
			RotateAngle += speed ;
			DayCounter  =  ( int) ( RotateAngle / 360.0f );
			this.transform.Rotate( new Vector3(0,speed, 0), Space.Self);
			//Debug.DrawLine(this.transform.position - TiltVector*50, this.transform.position + TiltVector*50, Color.green);
		}
		
		if (isSun)
			return;
		
		Vector3 center = parentObject.transform.localPosition; 
		ellipse.drawAroundPoint(   center - ellipse.getF1() ,  TouchControl.ScaleFactor,0, renderer,  this.transform, this == selectedPlanet);
		OrbitalAngle += (OrbitalSpeed  * (float)Sun.TimeConstant);
		YearCounter = (int) (OrbitalAngle / 360);
		
		
		Vector3 tmpPos = ellipse.getPosition (OrbitalAngle, 1.0f,0);
		Vector3 pos =  ( center - ellipse.getF1() )  + tmpPos;
		//Debug.DrawLine(pos, this.transform.position, Color.red, 1000); 
		this.gameObject.transform.localPosition = pos;
		
		Velocity = ellipse.getVelocity(OrbitalAngle, Area).magnitude;
		OrbitalSpeed = ellipse.getAngularVelocity(OrbitalAngle, Area );
		
		CurrentTick += Sun.TimeConstant * 1;
		//toot je problem. TOto je kratsie ako OrbitalAngle  , tedxa pri 30.0111 mam len 0....01 time pricom by mal byt
		CurrentTime = CurrentTick / hourPositions.getTicksPerHour();
		
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
