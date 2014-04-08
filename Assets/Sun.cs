using UnityEngine;
using System.Collections;
using System;



public class Sun : MonoBehaviour {

	public static float TimeConstant = 0  ;
	public float TimeConstantCurrent = TimeConstant;
	private float timeAccelaration = 0;



	public void addTime(float n){
		timeAccelaration += n;
		//TimeConstantCurrent += timeAccelaration;
		TimeConstantCurrent +=timeAccelaration;
	}



	private KeyButton plusButton = new KeyButton(new Rect(70,10,50,50), "+", KeyCode.KeypadPlus );
	private KeyButton minusButton = new KeyButton(new Rect(10,10,50,50), "-", KeyCode.KeypadMinus );

	string[] pole = new string[]{"jan", "feb", "march"};
	public static DatePicker DatePicker = null;
	
	void Start () 
	{
		TimeConstant = TimeConstantCurrent;
		plusButton.action  += delegate { addTime( 0.001f); } ;
		plusButton.releaseAction += delegate{ timeAccelaration = 0; };

		minusButton.action += delegate { addTime(-0.001f); } ;
		minusButton.releaseAction += delegate{ timeAccelaration = 0; };

		DatePicker = new DatePicker(new Rect(100,100, 300, 100 ) );
	}
	
	void Update () 
	{
		TimeConstant = TimeConstantCurrent;
		plusButton.PerformUpdate();
		minusButton.PerformUpdate();
	}

	void OnGUI(){
		plusButton.Perform();
		minusButton.Perform();
		DatePicker.onGui();
	}
}
