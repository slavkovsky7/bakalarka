using UnityEngine;
using System.Collections;
using Bakalarka;

public class Planet : MonoBehaviour {
	public GameObject parentObject = null;
	public float rotateSpeed = 50;
	// Use this for initialization
	
	
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
	}

	private Vector3 lastPost = new Vector3();
	bool lastPosSet = false;



	void Update () {
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
