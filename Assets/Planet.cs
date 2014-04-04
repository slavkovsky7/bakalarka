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
		Ellipse ellipse = null;
		float area = 0;
		float period = 0;
		
		public PlanetPositions(Ellipse ellipse , float gravityParam )
		{
			this.ellipse = ellipse;
			period = (int)ellipse.getPeriod( gravityParam );
			float minSpeed = ellipse.getMinimumSpeed( gravityParam );
			area = ellipse.getAreaVelocity(0, minSpeed);
			positions = getPositionsArray( 0 , minSpeed, 360.0f, 1.0f );
		}

		public float getTicksPerHour()
		{
			return (float)positions.Length / period;
		}

		private PlanetPosition[] getPositionsArray( float startAngle , float startSpeed, float endAngle , float timeModifier)
		{
			List<PlanetPosition> positionsList = new List<PlanetPosition>();
			float totalAngle = startAngle;
			float currentSpeed = startSpeed;
			while (totalAngle < endAngle)
			{
				totalAngle += currentSpeed*timeModifier;
				positionsList.Add( new PlanetPosition(totalAngle, currentSpeed ) );
				currentSpeed = ellipse.getAngularSpeed(totalAngle, area); 
			}
			return positionsList.ToArray ();
		}
		
		public PlanetPosition getPlanetaryPosition(int hour , int second )
		{
			int start_hour = hour % (int)period;
			int end_hour =  ( hour + 1 )% (int)period;
			PlanetPosition start = getPlanetaryPosition(start_hour);
			PlanetPosition end = getPlanetaryPosition(end_hour);
			PlanetPosition[] positionsPerSecond = getPositionsArray (start.angle, start.speed, end.angle, 1.0f / 3600.0f);
			int index = (int)((float)second * ((float)positionsPerSecond.Length / 3600.0f ));
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
	public int CurrentHour = 0;
	public int CurrentMinute = 0;
	public GameObject parentObject = null;

	public Vector3 U = new Vector3(1,0,0);
	public Vector3 V = new Vector3(0,0,1);


	
	private float area;
	private Ellipse ellipse = null;
	private PlanetPositions hourPositions;
	private Quaternion originalRotation;

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
		OrbitalSpeed = ellipse.getMinimumSpeed(GravitParam);
		area = ellipse.getAreaVelocity(0, OrbitalSpeed);
		AverageVelocity = ellipse.getAverageOrbitalSpeed(GravitParam);
		
	
		hourPositions = new PlanetPositions (ellipse, GravitParam);

		if (DayLength > 0)
		{
			//TOTO JE TIEZ ZLE
			float f =  (float)hourPositions.positions.Length / ( Period / DayLength ) ;
			RotateSpeed =   360.0f  / f ; 
		}

		Quaternion quat = Quaternion.AngleAxis( Tilt, Vector3.forward);
		TiltVector = quat * TiltVector;
		this.transform.Rotate( new Vector3(0,0, Tilt));

	}


	public void SetDate(int hour , int minute, int second )
	{   


		//PlanetPosition position = hourPositions.getPlanetaryPosition (hour);	
		//OrbitalSpeed = position.speed;
		int years = (int) ( (float)hour / Period );
		//OrbitalAngle = ( 360.0f* (float)years) +position.angle;

		minute = minute + second / 60;
		second = second % 60;
		int hours = hour + minute/60;
		minute = minute % 60;
		int seconds = minute*60 + second;

		PlanetPosition pos = (seconds > 0) ? hourPositions.getPlanetaryPosition( hours, seconds ) : hourPositions.getPlanetaryPosition (hours);
		OrbitalAngle = ( 360.0f* (float)years) + pos.angle;
		OrbitalSpeed = pos.speed;

		transform.rotation = originalRotation;

		RotateAngle = RotateSpeed * ( ( hours + seconds * Math2f.SEC_TO_HOUR ) * hourPositions.getTicksPerHour() ); 
		this.transform.Rotate( new Vector3(0, RotateAngle, 0));
	}

	void Start () {
		InitPlanet();
		SetDate(CurrentHour, CurrentMinute , 0);
		Debug.Log( " SemiMajor "  + ellipse.semi_major + this.gameObject.name );
		Debug.Log( hourPositions.positions.Length + " "  + this.gameObject.name );
		Debug.Log( hourPositions.getTicksPerHour() + " "  + this.gameObject.name );
		Debug.Log( hourPositions.positions.Length * Time.deltaTime + " "  + this.gameObject.name );
	}

	/*private Vector3 lastPost = new Vector3();
	bool lastPosSet = false;*/

	public int CurrentTick = 0;
	public float VelocitySum = 0;
	public float VelocityAverage = 0;


	public float SweepedArea = 0;

	public void Advance()
	{

		SweepedArea = ellipse.getAreaVelocity(OrbitalAngle, OrbitalSpeed); 


		CurrentTick++;
		if (RotateSpeed > 0)
		{
			float speed =  RotateSpeed * Sun.TimeConstant;
			RotateAngle += speed ;
			DayCounter  =  ( int) ( RotateAngle / 360.0f );
			this.transform.Rotate( new Vector3(0,speed, 0));
			Debug.DrawLine(this.transform.position - TiltVector*50, this.transform.position + TiltVector*50, Color.green);
		}


		OrbitalAngle += (OrbitalSpeed  * Sun.TimeConstant);
		YearCounter = (int) (OrbitalAngle / 360);

		Vector3 center = parentObject.transform.position; 
		Vector3 pos =  ( center - ellipse.getF1() )  + ellipse.getPosition(OrbitalAngle);
		this.gameObject.transform.position = pos;

		Velocity = ellipse.toVelocity(OrbitalSpeed);
		VelocitySum += Velocity;
		VelocityAverage = VelocitySum / (float)CurrentTick;

		/*if (lastPosSet) Debug.DrawLine(pos ,lastPost, Color.red, 1000 );
		lastPosSet = true;
		lastPost = pos;
		*/
		OrbitalSpeed = ellipse.getAngularSpeed(OrbitalAngle, area );
		ellipse.drawAroundPoint(  ( center - ellipse.getF1() ) );
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
