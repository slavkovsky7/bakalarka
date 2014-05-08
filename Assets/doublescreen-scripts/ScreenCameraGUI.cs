// Author: Vis Gravis
// Description: This script is attached to Screen Camera game object and controls rendering of scene on touch screen. 
//              Only scene objects from Screen layer are rendered throught this camera.


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenCameraGUI : MonoBehaviour 
{
	Texture2D m_controlTexture;
	bool m_enableBlack;

	public float SceneScale = 1;
	// Use this for initialization
	void Start () 
	{
		//GetComponent<Skybox>().material.shader = Shader.Find("RenderFX/Skybox Rotation");
		// create small black texture, will be used as black background on user request
		m_controlTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
		m_controlTexture.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 1.0f));
		m_controlTexture.Apply();
		m_enableBlack = false;
		
		// change material shader for all objects in scene
		// add clip shader to each renderable object, this will clip parts of objects that are not over touch screen in projectgor view

		// set dimension ratio of Screen Camera for all objects that need it
		SetScreenCameraRatios();

		Shader clipShader = Shader.Find("Custom/ClipShader");
		GameObject scene = GameObject.Find("Scene");

		List<GameObject> objects = new List<GameObject>();
		if (clipShader != null && scene != null)
		{
			objects.Add(scene);
			while (objects.Count > 0)
			{
				GameObject obj = objects[0];
				objects.RemoveAt(0);
				
				// find material and set new shader
				if (obj.renderer != null && obj.renderer.material != null)
				{
					obj.renderer.material.shader = clipShader;
					obj.renderer.material.SetFloat("_minX", -8.888f);
					obj.renderer.material.SetFloat("_maxX", 8.888f);
					obj.renderer.material.SetFloat("_minZ", -5f);
					obj.renderer.material.SetFloat("_maxZ", 5f);
					obj.renderer.material.SetFloat("_clip", 1f);
				}
				
				// add children nodes
				foreach (Transform trans in obj.transform)
					objects.Add(trans.gameObject);
			}
		}
		

	}

	// Update is called once per frame
	void Update () 
	{
		// set dimension ratio of Screen Camera for all objects that need it
		// for now, it is not necessary to call it each frame
		//SetScreenCameraRatios();
		//RotateSkybox();
		// press B to enable / disable black Screen Camera
		if (Input.GetKeyUp(KeyCode.B))
			m_enableBlack = !m_enableBlack;
	}
	
	void OnGUI()
	{
		// if requested, render black texture over whole Screen Camera
		if (m_enableBlack)
			GUI.DrawTexture(new Rect(0, 0, camera.pixelWidth,camera.pixelHeight), m_controlTexture, ScaleMode.ScaleAndCrop, true, 0.0F);
	}

	private 

	void SetScreenCameraRatios()
	{
		// each frame, set propper aspect ratio of screen camera to each game object that needs it
		float screen_camera_ratio = camera.pixelRect.width / camera.pixelRect.height;
		
		// set ratio to calibration plane
		GameObject calib_plane = GameObject.Find("Calibration Plane");
		if (calib_plane != null)
			calib_plane.transform.localScale = new Vector3(screen_camera_ratio*SceneScale, 1*SceneScale, 1*SceneScale);
		//GameObject space_plane = GameObject.Find("SpacePlane");
		//if (space_plane != null )
			//space_plane.transform.localScale = new Vector3(screen_camera_ratio*SceneScale, 1*SceneScale, 1*SceneScale);
		// set ratio in clip shaders for each object in scene
		// clip shader is used to clip all object that are not over screen in projector view
		List<GameObject> objects = new List<GameObject>();
		objects.Add(GameObject.Find("Scene"));	
		while (objects.Count > 0)
		{
			GameObject obj = objects[0];
			objects.RemoveAt(0);
			
			if (obj != null && obj.renderer != null && obj.renderer.material != null)
			{
				obj.renderer.material.SetFloat("_minX", -5 * screen_camera_ratio);
				obj.renderer.material.SetFloat("_maxX", 5 * screen_camera_ratio);
			}
					
			// add child objects if processing subobjects
			if (obj != null && obj.transform != null)
			{
				foreach (Transform child in obj.transform)
					objects.Add(child.gameObject);
			}
		}
	}
}
