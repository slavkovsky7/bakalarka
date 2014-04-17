// Author: Vis Gravis
// Description: This script performs manual and semi-automatic calibration (finding FOV and camera position, rotation) of Projector Camera. 
//              Projector Camera renders scene on projector part of window, under it is rendered stream from webcam.


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;


public class ManualCameraCalibration : MonoBehaviour 
{
	
	private List<Vector2> m_calib_samples = new List<Vector2>();
	private List<Vector2> m_calibration_2D_points = new List<Vector2>();
	private List<Vector3> m_calibration_3D_points = new List<Vector3>();
	private GameObject m_calib_plane;
	private GameObject m_calib_sphere;
	private GameObject m_scene;
	private bool m_calibrating = false;
	private String m_status = "Press C for calibration...";
	
	// Use this for initialization
	void Start () 
	{		
		// get calibration objects for later use
		m_calib_plane = GameObject.Find("Calibration Plane");
		m_calib_sphere = GameObject.Find("Calibration Sphere");
		m_scene = GameObject.Find("Scene");
		
		// prepare calibration mark vectors that will be used for semi-automatic calibration
		int calib_grid_size = 2;
		for (int i = 0; i < calib_grid_size; i++)
		{
			for (int j = 0; j < calib_grid_size; j++)
			{
				m_calib_samples.Add(new Vector2(i/(float)(calib_grid_size-1), j/(float)(calib_grid_size-1)));
			}
		}
		
		// try to load file with camera properties
		try
        {
			using (System.IO.StreamReader file = new System.IO.StreamReader(Application.dataPath + @"/camera.txt"))
        	{
				float x = (float)Convert.ToDouble(file.ReadLine());
				float y = (float)Convert.ToDouble(file.ReadLine());
				float z = (float)Convert.ToDouble(file.ReadLine());
				camera.transform.position = new Vector3(x,y,z);
				
				float a = (float)Convert.ToDouble(file.ReadLine());
				float b = (float)Convert.ToDouble(file.ReadLine());
				float c = (float)Convert.ToDouble(file.ReadLine());
				camera.transform.eulerAngles = new Vector3(a, b, c);
				
				camera.fieldOfView = (float)Convert.ToDouble(file.ReadLine());
        	}
        }
        catch (Exception ex)
        {
			m_status = ex.Message;
        }
	}
	
	// Update is called once per frame
	void Update ()
	{
		// enable/disable calibration by pressing C key
		if (Input.GetKeyUp(KeyCode.C))
		{
			m_calibrating = !m_calibrating;
		}
		if (!m_calibrating)
		{
			m_calib_plane.SetActive(false);
			m_calib_sphere.SetActive(false);
			m_scene.SetActive(true);
			m_calibration_2D_points.Clear();
			m_calibration_3D_points.Clear();
			m_status = "Press C for calibration...";
			return;
		}
		
		// if calibrating, hide scene, show only calibration plane & sphere
		m_calib_plane.SetActive(true);
		m_calib_sphere.SetActive(true);
		m_scene.SetActive(false);
		
		// show calibration marker to be clicked somewhere inside
		int calib_index = m_calibration_3D_points.Count;
		if (calib_index < m_calib_samples.Count)
			m_status = String.Format("Click on green spot in projector view, spots {0}/{1}", calib_index + 1, m_calib_samples.Count);
		
		// get boundaries of calibration plane
		Bounds calib_plane_bounds = m_calib_plane.GetComponent<MeshRenderer>().bounds;
		if (m_calib_samples.Count > m_calibration_3D_points.Count)
			m_calib_sphere.transform.position = new Vector3(calib_plane_bounds.min.x + m_calib_samples[calib_index].x*(calib_plane_bounds.max.x - calib_plane_bounds.min.x),
															0,
															calib_plane_bounds.min.z + m_calib_samples[calib_index].y*(calib_plane_bounds.max.z - calib_plane_bounds.min.z));
		
		// SEMI_AUTOMATIC CALIBRATION OF CAMERA BY CLICKING ON GREEN MARKS IN PROJECTOR VIEW
		if (Input.GetMouseButtonUp(0))
		{
			if (m_calibration_3D_points.Count < m_calib_samples.Count)
				m_calibration_3D_points.Add(m_calib_sphere.transform.position);
			
			// get mouse position inside projector view
			GameObject projector_camera = GameObject.Find("Projector Camera");
			Rect rect = projector_camera.camera.pixelRect;
			if (m_calibration_2D_points.Count < m_calib_samples.Count)
				m_calibration_2D_points.Add(new Vector2(Input.mousePosition.x - rect.x, rect.height - (Input.mousePosition.y - rect.y)));
			
			if (m_calibration_3D_points.Count >= m_calib_samples.Count)
			{				
				CalibrateCamera(projector_camera, m_calibration_3D_points, m_calibration_2D_points);				
			}
		}
		
		// MANUAL TRANSFORMATION OF CAMERA, PRESSING KEYS FOR CAMERA TRANSFORMATION
		// rotate
		if (Input.GetKey(KeyCode.LeftShift))
		{
			// get intersection of dir vector and xz plane
			Vector3 origin = new Vector3(0f, 0f, 0f);
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				camera.transform.RotateAround(origin, Vector3.up, -0.3f);
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				camera.transform.RotateAround(origin, Vector3.up, 0.3f);
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				camera.transform.RotateAround(origin, Vector3.right, -0.3f);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				camera.transform.RotateAround(origin, Vector3.right, 0.3f);
			}
			if (Input.GetKey(KeyCode.S))
			{
				camera.transform.RotateAround(origin, Vector3.forward, -0.3f);
			}
			if (Input.GetKey(KeyCode.W))
			{
				camera.transform.RotateAround(origin, Vector3.forward, 0.3f);
			}
		}
		
		// pan
		if (Input.GetKey(KeyCode.RightShift))
		{
			// get intersection of dir vector and xz plane
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				camera.transform.Translate(-0.03f, 0.0f, 0.0f, Space.Self);
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				camera.transform.Translate(0.03f, 0.0f, 0.0f, Space.Self);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				camera.transform.Translate(0.0f, -0.03f, 0.0f, Space.Self);
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				camera.transform.Translate(0.0f, 0.03f, 0.0f, Space.Self);
			}
		}
		
		// move
		if (Input.GetKey(KeyCode.LeftControl))
		{
			// get intersection of dir vector and xz plane
			if (Input.GetKey(KeyCode.DownArrow))
			{
				camera.transform.Translate(0.0f, 0.0f, -0.03f, Space.Self);
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				camera.transform.Translate(0.0f, 0.0f, 0.03f, Space.Self);
			}
		}
		
		// chenge fov
		if (Input.GetKeyUp(KeyCode.M))
		{
			camera.fieldOfView = camera.fieldOfView + 1;
		}
		if (Input.GetKeyUp(KeyCode.N))
		{
			camera.fieldOfView = camera.fieldOfView - 1;
		}
		
		// IN CALIBRATION MODE, PRESS X FOR SAVING BASIC CAMERA PARAMETERS INTO TEXT FILE 
		if (Input.GetKeyUp(KeyCode.X))
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.dataPath + @"/camera.txt"))
            {
				file.WriteLine(string.Format("{0}", camera.transform.position.x));
				file.WriteLine(string.Format("{0}", camera.transform.position.y));
				file.WriteLine(string.Format("{0}", camera.transform.position.z));
				file.WriteLine(string.Format("{0}", camera.transform.eulerAngles.x));
				file.WriteLine(string.Format("{0}", camera.transform.eulerAngles.y));
				file.WriteLine(string.Format("{0}", camera.transform.eulerAngles.z));
				file.WriteLine(string.Format("{0}", camera.fieldOfView));
            }
		}
	}
	
	void OnGUI()
	{
		if (m_calib_plane != null && m_calib_plane.activeSelf)
		{
			// render basic status in calibration mode
			string text = String.Format("Camera position: {0}, {1}, {2}", transform.position.x, transform.position.y, transform.position.z);
			GUI.Label(new Rect(5, 5, 350, 20), text);
			text = String.Format("Camera rotation: {0}, {1}, {2}", transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
			GUI.Label(new Rect(5, 25, 350, 20), text);
			text = String.Format("Status: {0}", m_status);
			GUI.Label(new Rect(5, 45, 350, 20), text);
		}
	}
	
	// function computes camera pose and fov from clicked calibration points 
	public double CalibrateCamera(GameObject cameraGO, List<Vector3> objectPoints, List<Vector2> imagePoints)
    {
		// prepare data for opencv calibrate camera function
		// prepare input object points
		Matrix<float> objectPointsCV = new Matrix<float>(objectPoints.Count, 3);
		Matrix<float> objectPoints2CV = new Matrix<float>(objectPoints.Count, 2);
		for (int i = 0; i < objectPoints.Count; i++)
		{
			objectPointsCV[i, 0] = objectPoints[i].x;
			objectPointsCV[i, 1] = objectPoints[i].y;
			objectPointsCV[i, 2] = objectPoints[i].z;
			objectPoints2CV[i, 0] = objectPoints[i].x;
			objectPoints2CV[i, 1] = objectPoints[i].z;
		}
		
		// prepare input image points
		Matrix<float> imagePointsCV = new Matrix<float>(imagePoints.Count, 2);
		for (int i = 0; i < imagePoints.Count; i++)
		{
			imagePointsCV[i, 0] = imagePoints[i].x;
			imagePointsCV[i, 1] = imagePoints[i].y;
		}
		
		// we have only one image from camera
		Matrix<int> pointsInImageCountsCV = new Matrix<int>(1, 1);
		pointsInImageCountsCV[0, 0] = objectPoints.Count;
		
		// image size in camera
		Size cameraSizeCV = new Size((int)cameraGO.camera.pixelRect.width, (int)cameraGO.camera.pixelRect.height);
		
		// prepare initial guess of intrinsic camera matrix
		IntrinsicCameraParameters intrinsicCV = new IntrinsicCameraParameters();
		intrinsicCV.IntrinsicMatrix.Data[0,0] = 0.5f * cameraGO.camera.pixelRect.height / Mathf.Tan(Mathf.PI * cameraGO.camera.fieldOfView / 360.0f);
		intrinsicCV.IntrinsicMatrix.Data[1,1] = intrinsicCV.IntrinsicMatrix.Data[0,0];
		intrinsicCV.IntrinsicMatrix.Data[2,2] = 1.0f;
		intrinsicCV.IntrinsicMatrix.Data[0,2] = cameraGO.camera.pixelRect.width / 2;
		intrinsicCV.IntrinsicMatrix.Data[1,2] = cameraGO.camera.pixelRect.height / 2;
		
		// matrix with output vectors
        Matrix<double> rotationVectorsCV = new Matrix<double>(1, 3);
        Matrix<double> translationVectorsCV = new Matrix<double>(1, 3);
		Matrix<double> homographyCV = new Matrix<double>(3, 3);
		
		double reprojectionError = -1;
		try
		{
			// compute intrinsic parameters + rotation & translation of camera
	    	reprojectionError = CvInvoke.cvCalibrateCamera2(
	                objectPointsCV.Ptr,
	                imagePointsCV.Ptr,
	                pointsInImageCountsCV.Ptr,
	                cameraSizeCV,
	                intrinsicCV.IntrinsicMatrix,
	                intrinsicCV.DistortionCoeffs,
	                rotationVectorsCV,
	                translationVectorsCV,
	                CALIB_TYPE.CV_CALIB_USE_INTRINSIC_GUESS);
			
			// second way for rotation & translation of camera, homography between two planes
			CvInvoke.cvFindHomography(objectPoints2CV.Ptr, imagePointsCV.Ptr, homographyCV, HOMOGRAPHY_METHOD.RANSAC, 5, IntPtr.Zero);
		}
		catch (Exception ex)
		{	
		    Debug.Log(ex.ToString());
			m_status = String.Format("Calibration error: {0}", ex.ToString());
			return -1;
		}
		
		// rotation & translation of camera from homography
		Matrix<double> rotH = new Matrix<double>(3, 3);
		Matrix<double> transH = new Matrix<double>(3, 1);
		Matrix<double> invintrinsic = new Matrix<double>(3, 3);
		CvInvoke.cvInvert(intrinsicCV.IntrinsicMatrix.Ptr, invintrinsic.Ptr, SOLVE_METHOD.CV_LU);
		CvInvoke.cvGEMM(invintrinsic.Ptr, homographyCV.Ptr, 1.0, IntPtr.Zero, 0.0, homographyCV.Ptr, GEMM_TYPE.CV_GEMM_DEFAULT);
		float norm1 = (float) CvInvoke.cvNorm(homographyCV.GetCol(0).Ptr, IntPtr.Zero, NORM_TYPE.CV_L2, IntPtr.Zero);
    	float norm2 = (float) CvInvoke.cvNorm(homographyCV.GetCol(1).Ptr, IntPtr.Zero, NORM_TYPE.CV_L2, IntPtr.Zero);
		float tnorm = (norm1 + norm2) / 2.0f;
		Matrix<double> temp = new Matrix<double>(3, 1);
		CvInvoke.cvNormalize(homographyCV.GetCol(0).Ptr, temp.Ptr, 1, 1, NORM_TYPE.CV_L2, IntPtr.Zero);
		rotH[0, 0] = temp[0, 0];
		rotH[1, 0] = temp[1, 0];
		rotH[2, 0] = temp[2, 0];
		CvInvoke.cvNormalize(homographyCV.GetCol(1).Ptr, temp.Ptr, 1, 1, NORM_TYPE.CV_L2, IntPtr.Zero);
		rotH[0, 2] = temp[0, 0];
		rotH[1, 2] = temp[1, 0];
		rotH[2, 2] = temp[2, 0];
		CvInvoke.cvCrossProduct(rotH.GetCol(2).Ptr, rotH.GetCol(0), temp);
		rotH[0, 1] = temp[0, 0];
		rotH[1, 1] = temp[1, 0];
		rotH[2, 1] = temp[2, 0];
		transH[0, 0] = homographyCV[0, 2] / tnorm;
		transH[1, 0] = homographyCV[1, 2] / tnorm;
		transH[2, 0] = homographyCV[2, 2] / tnorm;
		
		// get position of camera
		Matrix<double> rotation = new Matrix<double>(3, 3);
		Matrix<double> invrotation = new Matrix<double>(3, 3);
		Matrix<double> transposedTrans = new Matrix<double>(3, 1);
		Matrix<double> campos = new Matrix<double>(3, 1);
		CvInvoke.cvRodrigues2(rotationVectorsCV.Ptr, rotation.Ptr, IntPtr.Zero);
		CvInvoke.cvTranspose(translationVectorsCV.Ptr, transposedTrans.Ptr);
	    CvInvoke.cvTranspose(rotation.Ptr, invrotation.Ptr);
		CvInvoke.cvGEMM(invrotation.Ptr, (-1 * transposedTrans).Ptr, 1.0, IntPtr.Zero, 0.0, campos.Ptr, GEMM_TYPE.CV_GEMM_DEFAULT);
		cameraGO.transform.position = new Vector3((float) campos[0, 0],
												  (float) -campos[1, 0],
												  (float) campos[2, 0]);
				
		// set camera rotation
		Matrix<double> camera_view = new Matrix<double>(3, 1);
		camera_view[0, 0] = 0;
		camera_view[1, 0] = 0;
		camera_view[2, 0] = 1;
		Matrix<double> camera_up = new Matrix<double>(3, 1);
		camera_up[0, 0] = 0;
		camera_up[1, 0] = -1;
		camera_up[2, 0] = 0;
		Matrix<double> view = new Matrix<double>(3, 1);
		Matrix<double> up = new Matrix<double>(3, 1);
		CvInvoke.cvGEMM(invrotation.Ptr, camera_view.Ptr, 1.0, IntPtr.Zero, 0.0, view.Ptr, GEMM_TYPE.CV_GEMM_DEFAULT);
		CvInvoke.cvGEMM(invrotation.Ptr, camera_up.Ptr, 1.0, IntPtr.Zero, 0.0, up.Ptr, GEMM_TYPE.CV_GEMM_DEFAULT);
		cameraGO.transform.LookAt(new Vector3(cameraGO.transform.position.x + (float)view[0, 0],
											  cameraGO.transform.position.y - (float)view[1, 0],
											  cameraGO.transform.position.z + (float)view[2, 0]),
										  new Vector3((float)up[0, 0], (float)-up[1, 0], (float)up[2, 0]));
				
		// set FOV and near plane of camera
		float fx = (float)intrinsicCV.IntrinsicMatrix[0, 0];
		float fy = (float)intrinsicCV.IntrinsicMatrix[1, 1];
		float fovx = 2 * Mathf.Atan(cameraSizeCV.Width / (2 * fx)) * 180.0f / Mathf.PI;
		float fovy = 2 * Mathf.Atan(cameraSizeCV.Height / (2 * fy)) * 180.0f / Mathf.PI;
		cameraGO.camera.fieldOfView = fovy;
		m_status = "Camera calibrated!";
		
        return reprojectionError;
    }
}
