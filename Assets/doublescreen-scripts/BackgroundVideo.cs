// Author: Vis Gravis
// Description: This script is attached to Background Video game object and is responsible for playing video from webcam inside GUI texture of Background Video game object.
//              Background Video game object is rendered using Projector Background Camera and is set in Background Video layer.


using UnityEngine;
using System.Collections;

public class BackgroundVideo : MonoBehaviour 
{
	private GUITexture m_videoGUItex;
	public static WebCamTexture m_webCamTexture;
	
	void Awake()
	{
		// set basic transformation for background video game object
		transform.position = Vector3.zero;
		transform.localScale = Vector3.zero;
		
		// check for presence of some webcam
		if (WebCamTexture.devices.Length < 1)
			return;
		
		// get the attached GUITexture & set vide from webcam inside that texture
		m_videoGUItex = this.GetComponent<GUITexture>(); 
		m_webCamTexture = new WebCamTexture();
		m_videoGUItex.texture = m_webCamTexture;
		m_webCamTexture.Play();
		
		// set dimension ratio of GUI texture to be equal to dimension ratio of video from webcam
		GameObject backgroundCam = GameObject.Find("Projector Background Camera");
		float ratio = 1;
		if (m_webCamTexture.height > 0) 
			ratio = m_webCamTexture.width / (float)m_webCamTexture.height;
		float videoWidth = backgroundCam.camera.pixelWidth;
		float videoHeight = videoWidth / ratio;
		if (videoHeight < backgroundCam.camera.pixelHeight)
		{
			videoHeight = backgroundCam.camera.pixelHeight;
			videoWidth = ratio * videoHeight;
		}
		m_videoGUItex.pixelInset = new Rect(backgroundCam.camera.pixelWidth / 2 - videoWidth / 2, backgroundCam.camera.pixelHeight / 2 - videoHeight / 2, videoWidth, videoHeight);
	}
	
	// Use this for initialization
	void Start () 
	{
		if (m_webCamTexture != null)
			m_webCamTexture.Play();
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	void OnGUI()
	{
	}
}
