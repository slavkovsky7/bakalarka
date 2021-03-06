// Author: Vis Gravis
// Description: This script is attached to Screen Camera game object and controls input from touch screen and transforms objects in scene based on user input from touch screen.
//              tuioStarter game object and its static classes are used for getting input from touch screen.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TouchControl : MonoBehaviour 
{
	float m_ZoomDistance;
	float m_PrevAngle;
	Vector2 m_MovePrevPosition;
	bool mouseClicked = false;

	public static float OriginalY =0;
	public static float ScaleFactor = 1.0f;
	public static float RotationFactor = 0;
	public static List<Rect> GuiRectangles = new List<Rect>();
	public static bool IgnoreMouse = false;
	public float MoveMultiplier = 10.0f;

	private GameObject m_space_plane;

	class TouchData
	{
		public Vector2 vPrevPosition = new Vector2(0f, 0f);
		public Vector2 vVelocity = new Vector2(0f, 0f);
		public float fPrevTime = 0f;
		public bool bActive = false;
	};
	//private Dictionary<int, TouchData> vTouchDatas = new Dictionary<int, TouchData>(); 
	
	// Use this for initialization
	void Start () 
	{
		m_ZoomDistance = 0f;
		m_PrevAngle = -1f;
		m_MovePrevPosition = new Vector2(-10000, -10000);
		OriginalY = GameObject.Find("Scene").transform.position.y; 
		m_space_plane = GameObject.Find("SpacePlane");
	}
	
	// Update is called once per frame

	public static List<iPhoneTouch> getGoodIphoneTouches(){
		int iCount = iPhoneInput.touchCount;
		Camera cam = GameObject.Find("Screen Camera").camera;
		List<iPhoneTouch> goodIphoneTouches = new  List<iPhoneTouch>();
		for (int itouch = 0; itouch < iCount; itouch++)
		{
			iPhoneTouch touch = (iPhoneTouch)iPhoneInput.GetTouch(itouch);
			if (touch.position.x > 0.0f && touch.position.x < cam.pixelWidth &&
			    touch.position.y > 0.0f && touch.position.y < cam.pixelHeight)
			{
				goodIphoneTouches.Add(touch);
			}
		}
		return goodIphoneTouches;
	}

	void Update () 
	{
		List<iPhoneTouch> goodIphoneTouches = getGoodIphoneTouches();
	
		if (IgnoreMouse) {
			mouseClicked = false;
		}

		if ( Input.GetMouseButtonDown(0) && !IgnoreMouse ){
			mouseClicked = true;
		}
		if ( Input.GetMouseButtonUp(0) && !IgnoreMouse ){
			//Debug.Log("Mouse up");
			mouseClicked = false;
		}
		
		if (goodIphoneTouches.Count == 1 || mouseClicked )
		{
			// ----------------MOVING-----------------
			iPhoneTouch touch;
			Vector2 touchedPosition = Vector2.zero;
			if (goodIphoneTouches.Count == 1)
			{
				touch = (iPhoneTouch)goodIphoneTouches[0];
				touchedPosition = touch.position;
			}else{
				touchedPosition = Input.mousePosition;
			}

			
			Vector2 v = new Vector2( touchedPosition.x, Screen.height - touchedPosition.y);
			foreach ( Rect r in GuiRectangles){
				if (r.Contains(v)){
					return;
				}	
			}

			// initialization of movement
			if (m_MovePrevPosition.x == -10000 && m_MovePrevPosition.y == -10000)
			{
				//Debug.Log("initialization of movement");
				m_MovePrevPosition = new Vector2(touchedPosition.x, touchedPosition.y);
			}
			
			// move objects
			GameObject sceneObjects = GameObject.Find("Scene");
			if (sceneObjects != null)
			{
				float moveX = -1 * MoveMultiplier * (touchedPosition.x - m_MovePrevPosition.x);
				float moveY = -1 * MoveMultiplier * 0.5625f * (touchedPosition.y - m_MovePrevPosition.y);
				//Debug.Log("Moving ["+moveX+", "+moveY+"]");
				//Mozno naopak prehodit
				sceneObjects.transform.Translate(moveX, 0, moveY, Space.World);
				
				// check for out of bounds
				GameObject plane = GameObject.Find("Map Plane Screen");
				Bounds bounds = plane.GetComponent<MeshFilter>().mesh.bounds;
				Vector3 min = plane.transform.TransformPoint(bounds.min);
				Vector3 max = plane.transform.TransformPoint(bounds.max);
				if (min.x > 0 || max.x < 0 || min.z > 0 || max.z < 0)
				{
					// return to previous transformation
					//TODO::toto robi blbosti
					//if (!mouseClicked)
					//{
						//sceneObjects.transform.Translate(-moveX, 0, -moveY, Space.World);
					//}
				}
			}

			m_MovePrevPosition =  new Vector2(touchedPosition.x, touchedPosition.y);
		}
		else if (mouseClicked == false)
		{
			m_MovePrevPosition = new Vector2(-10000, -10000);
		}

		if (goodIphoneTouches.Count == 2 )
		{
			iPhoneTouch touch1 = (iPhoneTouch)goodIphoneTouches[0];
			iPhoneTouch touch2 = (iPhoneTouch)goodIphoneTouches[1];
			
			
			// ----------------ZOOMING-----------------
			// init zoom distance
			if (m_ZoomDistance == 0f)
				m_ZoomDistance = Mathf.Sqrt(Mathf.Pow(touch1.position.x - touch2.position.x, 2f) + Mathf.Pow(touch1.position.y - touch2.position.y, 2f));
			float scale = 1f;
			if (m_ZoomDistance > 0)
			{
				float dist = Mathf.Sqrt(Mathf.Pow(touch1.position.x - touch2.position.x, 2f) + Mathf.Pow(touch1.position.y - touch2.position.y, 2f));
				scale = dist / m_ZoomDistance;
				if (Mathf.Abs(scale - 1.0f) < 0.008f) scale = 1.0f;
				m_ZoomDistance = dist;
			}

			Vector3 rayPoint = new Vector3(0.5f * (touch1.position.x+touch2.position.x), 0.5f * (touch1.position.y+touch2.position.y), 0);
			zoom(rayPoint, scale);
			
			// ---------------ROTATING------------------
			if (m_PrevAngle == -1f)
			{
				Vector2 dir = new Vector2(touch2.position.x - touch1.position.x, touch2.position.y - touch1.position.y);
				dir.Normalize();
				m_PrevAngle = Mathf.Acos(dir.x);
				if (dir.y < 0) m_PrevAngle = 2*Mathf.PI - m_PrevAngle;
			}
			
			// compute new angle from line connecting two fingers
			Vector2 dir2 = new Vector2(touch2.position.x - touch1.position.x, touch2.position.y - touch1.position.y);
			dir2.Normalize();
			float angle = Mathf.Acos(dir2.x);
			if (dir2.y < 0) angle = 2*Mathf.PI - angle;
			float diff = angle - m_PrevAngle;
			m_PrevAngle = angle;
			
			// do not rotate small angles
			if (Mathf.Abs(diff) < 0.01f) diff = 0f;
			// if zooming, the rotate only with larger angles
			if (scale != 1.0f && Mathf.Abs(diff) < 0.18f) diff = 0f;
			rotate(rayPoint,  -360.0f*diff/2/Mathf.PI );
			//sceneObjects.transform.RotateAround(ray.origin, Vector3.up,);
		} else if ( !IgnoreMouse && Input.GetAxis("Mouse ScrollWheel") != 0 ){
			//Mouse Zooming
			float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
			Vector3 rayPoint = new Vector3( Input.mousePosition.x,  Input.mousePosition.y , 0);
			float scale = 1.0f + mouseScroll;
			//Debug.Log("Zoomed with mouse wheel");
			zoom(rayPoint, scale);

		}else if ( !IgnoreMouse &&  ( Input.GetKey( "q" ) || Input.GetKey("e") ) ){
			float angle = 0.5f;
			if (Input.GetKey("e"))
				angle *= -1;
			rotate( new Vector3( Input.mousePosition.x,  Input.mousePosition.y , 0), angle );
			//RotationFactor += angle;
		}else{
			m_ZoomDistance = 0f;
			m_PrevAngle = -1f;
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)){
        	Application.Quit();
    	}

		/*GameObject scene = GameObject.Find("Scene");
			if (scene != null) {
			m_space_plane.transform.rotation = GameObject.Find("Scene").transform.rotation;
		}*/
	}

	private static void rotate(Vector3 rayPoint , float angle)
	{
		GameObject screenCamera = GameObject.Find("Screen Camera");
		GameObject sceneObjects = GameObject.Find("Scene");
		if ( sceneObjects != null ) {
			Ray ray = screenCamera.camera.ScreenPointToRay(rayPoint);
			sceneObjects.transform.RotateAround(ray.origin, Vector3.up, angle);
		}
	}

	public static void zoom(float scale){
		GameObject sceneObjects = GameObject.Find("Scene");
		ScaleFactor = scale;
		sceneObjects.transform.localScale = new Vector3(scale, scale, scale);
	}

	private static void zoom(Vector3 rayPoint , float scale )
	{
		GameObject screenCamera = GameObject.Find("Screen Camera"); 
		GameObject sceneObjects = GameObject.Find("Scene");
		if (sceneObjects != null)
		{
			Ray ray = screenCamera.camera.ScreenPointToRay (rayPoint);
			if (Mathf.Abs(scale - 1.0f) < 0.008f) scale = 1.0f;
			
			float newScale = sceneObjects.transform.localScale.x + sceneObjects.transform.localScale.x * 0.3f * (scale - 1.0f);
			if (newScale < 0.1f || newScale > 20.0f) newScale = sceneObjects.transform.localScale.x;
	
			// centring to center of two fingers
			float moveX = -(ray.origin.x - sceneObjects.transform.localPosition.x) * (newScale - sceneObjects.transform.localScale.x) / sceneObjects.transform.localScale.x;
			float moveZ = -(ray.origin.z - sceneObjects.transform.localPosition.z) * (newScale - sceneObjects.transform.localScale.x) / sceneObjects.transform.localScale.x;
			ScaleFactor = newScale;
			sceneObjects.transform.localScale = new Vector3(newScale, newScale, newScale);
			sceneObjects.transform.Translate(moveX, 0, moveZ, Space.World);
		}
	}

	public static void ResetView(){
		GameObject sceneObjects = GameObject.Find("Scene");
		sceneObjects.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		sceneObjects.transform.position = new Vector3(0, OriginalY);
		sceneObjects.transform.rotation = Quaternion.Euler ( new Vector3(0,0,0) );
		ScaleFactor = 1.0f;
	}

	void OnGUI()
	{
	}
}
