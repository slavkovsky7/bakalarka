// Author: Vis Gravis
// Description: This script is attached to Background Video Trans game object and is responsible for mixing static screenshot from webcam and live stream from webcam.
//              Background Video Trans game object is rendered using Projector Background Camera and is set in Background Video layer.


using UnityEngine;
using System.Collections;

public class BackgroundVideoTrans : MonoBehaviour 
{
	
	private GUITexture m_videoGUItex;
	
	// Use this for initialization
	void Start () 
	{
		transform.position = Vector3.zero;
		transform.localScale = Vector3.zero;
		
		// get the attached GUITexture
		m_videoGUItex = this.GetComponent<GUITexture>();
		
		// set dimension ratio of GUI texture to be equal to dimension ratio of video from webcam
		GameObject backgroundCam = GameObject.Find("Projector Background Camera");
		float ratio = 1;
		if (BackgroundVideo.m_webCamTexture.height > 0) 
			ratio = BackgroundVideo.m_webCamTexture.width / (float)BackgroundVideo.m_webCamTexture.height;
		float videoWidth = backgroundCam.camera.pixelWidth;
		float videoHeight = videoWidth / ratio;
		if (videoHeight < backgroundCam.camera.pixelHeight)
		{
			videoHeight = backgroundCam.camera.pixelHeight;
			videoWidth = ratio * videoHeight;
		}
		m_videoGUItex.pixelInset = new Rect(backgroundCam.camera.pixelWidth / 2 - videoWidth / 2, backgroundCam.camera.pixelHeight / 2 - videoHeight / 2, videoWidth, videoHeight);
	}
	
	// Update is called once per frame
	void Update () 
	{
		// use key P to take snapshot of video from webcam
		if (Input.GetKey(KeyCode.P))
		{
			// get snapshot from webcam
			Color32[] pixels = BackgroundVideo.m_webCamTexture.GetPixels32();
			// store it in GUI texture of Background Video Trans game object
			Texture2D tex = new Texture2D(BackgroundVideo.m_webCamTexture.width, BackgroundVideo.m_webCamTexture.height);
			tex.SetPixels32(pixels);
			tex.Apply();
			m_videoGUItex.texture = tex;
		}
		
	}
}
