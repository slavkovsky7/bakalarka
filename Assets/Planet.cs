using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bakalarka;

public class Planet : MonoBehaviour {



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
			this.positions = getPositionsArray( 0 , minSpeed, 360.0f, 1.0f );
		}

		public float getTicksPerHour()
		{
			//TOTO JE HODNOTA PRE ZEM, NEJAKE BLBOSTI TAM VYCHADZAJU KED JE to pre kazdu planetu 
			return 2.062894f;
			//return ((float)positions.Length )  / period;
		}

		private PlanetPosition[] getPositionsArray( float startAngle , float startSpeed, float endAngle , float timeModifier)
		{
			List<PlanetPosition> positionsList = new List<PlanetPosition>();
			float totalAngle = startAngle;
			float currentSpeed = startSpeed;
			positionsList.Add( new PlanetPosition(totalAngle, currentSpeed ) );
			while (totalAngle < endAngle)
			{
				totalAngle += currentSpeed*timeModifier;
				positionsList.Add( new PlanetPosition(totalAngle, currentSpeed ) );
				currentSpeed = ellipse.getAngularVelocity(totalAngle, area); 
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
			return positions[index];
		}
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
	public GameObject parentObject = null;
	public float Area;

	public Vector3 U = new Vector3(1,0,0);
	public Vector3 V = new Vector3(0,0,1);
	
	private Ellipse ellipse = null;
	private PlanetPositions hourPositions;
	private Quaternion originalRotation;

	public bool isGlobalTime = false;

	void InitPlanet()
	{

		originalRotation = this.transform.rotation;
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

		OrbitalSpeed = ellipse.getAngularVelocity(0, Area);
		Velocity = ellipse.getVelocity(0,Area).magnitude;
	
		AverageVelocity = ellipse.getAverageOrbitalSpeed(GravitParam);
		
	
		hourPositions = new PlanetPositions (ellipse, GravitParam);

		if (DayLength > 0)
		{
			//TOTO JE TIEZ ZLE
			//float f =  (float)hourPositions.positions.Length / ( Period / DayLength ) ;
			float f =  hourPositions.getTicksPerHour() * DayLength;
			RotateSpeed =   360.0f  / f ; 
		}

		Quaternion quat = Quaternion.AngleAxis( Tilt, Vector3.forward);
		TiltVector = quat * TiltVector;
		this.transform.Rotate( new Vector3(0,0, Tilt));

		Sun.Planets.Add(this);
	}

	public static float getMaxTime(){
		return DatePicker.YEAR_COUNT * DatePicker.EARTH_PERIOD;
	}
	
	public void SetDate( double hours ){

		int years = (int) ( hours / Period );
		int hour = (int)hours;  
		int minute = (int)((hours - (float)hour) * 60.0f ) ;
		SetDate(years, hour, minute);
		CurrentTime = hours + 30*Math2f.SEC_TO_HOUR;
		CurrentTick = CurrentTime * hourPositions.getTicksPerHour();
	}

	public void SetDate( int years , int hour , int minute )
	{   
		//PlanetPosition position = hourPositions.getPlanetaryPosition (hour);	
		//OrbitalSpeed = position.speed;

		//OrbitalAngle = ( 360.0f* (float)years) +position.angle;
		PlanetPosition pos = (minute > 0) ? hourPositions.getPlanetaryPosition( hour, minute, 60.0f ) : hourPositions.getPlanetaryPosition (hour);
		OrbitalAngle = ( 360.0f* (float)years) + pos.angle;
		OrbitalSpeed = pos.speed;

		transform.rotation = originalRotation;

		RotateAngle = RotateSpeed * ( ( hour + minute * Math2f.MIN_TO_HOUR ) * hourPositions.getTicksPerHour() ); 
		this.transform.Rotate( new Vector3(0, RotateAngle, 0));
	}

	void Start () {
		InitPlanet();
		//SetDate(CurrentHour, CurrentMinute );
		Debug.Log( hourPositions.positions.Length + " "  + this.gameObject.name );
		Debug.Log( hourPositions.getTicksPerHour() + " "  + this.gameObject.name );
	}
	

	public double CurrentTick = 0;
	public double CurrentTime = 0;

	public void Advance()
	{
		if (RotateSpeed > 0)
		{
			float speed =  RotateSpeed * (float)Sun.TimeConstant;
			RotateAngle += speed ;
			DayCounter  =  ( int) ( RotateAngle / 360.0f );
			this.transform.Rotate( new Vector3(0,speed, 0));
			//Debug.DrawLine(this.transform.position - TiltVector*50, this.transform.position + TiltVector*50, Color.green);
		}



		OrbitalAngle += (OrbitalSpeed  * (float)Sun.TimeConstant);
		YearCounter = (int) (OrbitalAngle / 360);

		Vector3 center = parentObject.transform.position; 
		Vector3 pos =  ( center - ellipse.getF1() )  + ellipse.getPosition(OrbitalAngle);
		this.gameObject.transform.position = pos;

		Velocity = ellipse.getVelocity(OrbitalAngle, Area).magnitude;

		OrbitalSpeed = ellipse.getAngularVelocity(OrbitalAngle, Area );
		ellipse.drawAroundPoint(  ( center - ellipse.getF1() ) );
		CurrentTick += Sun.TimeConstant * 1;
		CurrentTime = CurrentTick / hourPositions.getTicksPerHour();
		if ( CurrentTime < 0 ){
			CurrentTime = getMaxTime() - CurrentTime;
			CurrentTick = CurrentTime * hourPositions.getTicksPerHour();
		}
		if ( isGlobalTime && Sun.TimeConstant != 0){
			Sun.DatePicker.setDate(CurrentTime);
		}
	}

	void FixedUpdate () {

		if ( Inclination > 0 )
		{
			Debug.DrawLine(Vector3.zero,  U * 1000 , Color.red);
			Debug.DrawLine(Vector3.zero, V * 1000 , Color.red);
		}
		Advance();
	}
}
