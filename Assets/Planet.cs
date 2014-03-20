using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bakalarka;

public class Planet : MonoBehaviour {
	public GameObject parentObject = null;

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
		PlanetPosition[] positions = null;
		Ellipse ellipse = null;
		float area = 0;
		int period = 0;
		
		public PlanetPositions(Ellipse ellipse , float gravityParam )
		{
			this.ellipse = ellipse;
			period = (int)ellipse.getPeriod( gravityParam );
			float minSpeed = ellipse.getMinimumSpeed( gravityParam );
			area = ellipse.getAreaVelocity(0, minSpeed);
			positions = getPositionsArray( 0 , minSpeed, 360.0f, 1.0f );
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
		
		public PlanetPosition[] getPlanetaryPosition(int hour , int second )
		{
			int start_hour = hour % period;
			int end_hour =  ( hour + 1 )% period;
			PlanetPosition start = getPlanetaryPosition(start_hour);
			PlanetPosition end = getPlanetaryPosition(end_hour);
			PlanetPosition[] positionsPerSecond = getPositionsArray (start.angle, start.speed, end.angle, 1.0f / 3600.0f);
			return positionsPerSecond;
		}
		
		public PlanetPosition getPlanetaryPosition(int hour )
		{
			hour = hour % period;
			int index = (int) ( (float)(hour) * ( (float)(positions.Length) / (float)(period) ) );
			return positions[index];
		}
	}




	public float rotateSpeed = 50;
	public float Eccentricity = 0;
	public float Speed = 0;
	public float Angle = 0;
	public float GravitParam = 0;
	private Ellipse ellipse = null;
	private float area;
	public float Velocity = 0;
	public float AverageVelocity = 0;
	public int Counter = 0;
	public float Period = 0; 
	public float Perimeter = 0;

	void Start () {
		Vector3 U = new Vector3(1,0,0);
		Vector3 V = new Vector3(0,0,1);
		float distance = (this.transform.position - parentObject.transform.position ).magnitude;
		ellipse = new Ellipse( U , V , distance , Eccentricity );
		if (Period > 0 )
		{
			GravitParam = ellipse.getGravitationParameter(Period);
		}else{
			Period = ellipse.getPeriod(GravitParam);
		}
		Perimeter = ellipse.getPerimeter();
		Speed = ellipse.getMinimumSpeed(GravitParam);
		area = ellipse.getAreaVelocity(0, Speed);
		AverageVelocity = ellipse.getAverageOrbitalSpeed(GravitParam);

	//	PlanetPositions positions = new PlanetPositions (ellipse, GravitParam);
		//PlanetPosition position = positions.getPlanetaryPosition (startHour);

		//Speed = position.speed;
		//Angle = position.angle;

		//PlanetPosition[] positionsPerSecond = positions.getPlanetaryPosition(3000, 70);
	}

	private Vector3 lastPost = new Vector3();
	bool lastPosSet = false;


	public int HourCount = 0;
	void FixedUpdate () {
		Debug.Log("Update time :" + Time.deltaTime);

		HourCount++;
		Angle += (Speed  * Sun.TimeConstant);
		Counter = (int) (Angle / 360);

		this.transform.Rotate(Vector3.up, 1);

		Vector3 center = parentObject.transform.position; 
		Vector3 pos =  ( center - ellipse.getF1() )  + ellipse.getPosition(Angle);
		this.gameObject.transform.position = pos;

		Velocity = ellipse.toVelocity(Speed);
		
		if (lastPosSet)
		{
			//Debug.DrawLine(pos ,lastPost, Color.red, 1000 );
		}
		lastPosSet = true;
		lastPost = pos;

		Speed = ellipse.getAngularSpeed(Angle, area );
		ellipse.drawAroundPoint(  ( center - ellipse.getF1() ) );
	}
}
