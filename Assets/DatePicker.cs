using UnityEngine;
using System.Collections;
using System;

public class DatePicker
{

	public static string[] months = new string[] { "jan", "feb", "mar", "apr" , "may" , "jun", "jul" , "aug" , "sep", "oct", "nov", "dec"};
	public static string[] longerMonths = new string[] { "jan", "mar", "may", "july", "aug", "oct", "dec" };

	private static bool containsValue(string[] arr, string value){
		foreach ( string str in arr)
			if ( value == str)
				return true;
		return false;
	}

	private static int GetCorrectDay(int day , string month, int year )
	{
		if (day < 1){
			return 1;
		}
		int maxDay = (month == "feb") ? (year % 4 == 0 ? 29 : 28) : containsValue(longerMonths, month ) ? 31 : 30;
		if ( day > maxDay )
			return maxDay;
		return day;
	}


	private Rect rectangle;
	private SinglePicker monthPicker;
	private SinglePicker dayPicker;
	private SinglePicker yearPicker;

	public DatePicker( Rect rectangle )
	{
		float w3 = rectangle.width / 3; 
		this.rectangle = rectangle;
		this.monthPicker	= new SinglePicker( new Rect( 0		,0 ,w3 ,rectangle.height), months );
		this.dayPicker 		= new SinglePicker( new Rect( w3 	,0 ,w3 ,rectangle.height), null   );
		this.yearPicker 	= new SinglePicker( new Rect( 2*w3 	,0 ,w3 ,rectangle.height), null	  );

		this.monthPicker.setCurrentIndex(0);
		this.dayPicker.setCurrentIndex(1);
		this.yearPicker.setCurrentIndex(1970);
	}

	public void onGui(){
		GUI.BeginGroup( rectangle );
			monthPicker.onGui();
			dayPicker.onGui();
			dayPicker.setCurrentIndex( GetCorrectDay( dayPicker.getCurrentIndex() ,months[monthPicker.getCurrentIndex()],yearPicker.getCurrentIndex() ) );
			yearPicker.onGui();
		GUI.EndGroup();
	}

	private class SinglePicker
	{
		private string[] content;
		private Rect rectangle;
		private String labelText; 
		private int currentIndex;
		private float lastClick;

		private KeyButton plusButton;
		private KeyButton minusButton;

		private int clickCount = 0;
		private float clickInterval = DEFAULT_INTERVAL;
		private const float DEFAULT_INTERVAL = 0.2f;

		public SinglePicker( Rect rectangle , string[] content)
		{
			this.rectangle = rectangle;
			this.currentIndex = 0;
			this.content = content;
			this.lastClick = Time.time;


			float h3 = rectangle.height/3;
			this.plusButton = new KeyButton(new Rect(0,0, rectangle.width, h3), "+", KeyCode.None);
			this.plusButton.action += delegate { addCurrentIndex(1); };
			this.plusButton.releaseAction += delegate { resetInterval(); };

			this.minusButton = new KeyButton(new Rect(0, rectangle.height - h3, rectangle.width, h3 ), "-" , KeyCode.None);
			this.minusButton.action += delegate { addCurrentIndex(-1); };
			this.minusButton.releaseAction += delegate {resetInterval(); };

		}

		public void setCurrentIndex( int index ) { 
			currentIndex = index;
		}

		public int getCurrentIndex( ) { return currentIndex; }

		private void addCurrentIndex( int n ){
			if (Time.time - lastClick > clickInterval){
				if ( content != null ) {
					int tmpCurrentIndex = currentIndex + n;
					if (tmpCurrentIndex >= 0 && tmpCurrentIndex < content.Length  ){
						currentIndex = tmpCurrentIndex;
					}
				}else{
					currentIndex += n;
				}
				lastClick = Time.time;
				clickCount++;
				if (clickCount/5 > 0){
					clickInterval = DEFAULT_INTERVAL  / ( float) (  clickCount/3 );
				}
			}
		}

		private void resetInterval(){
			clickCount = 0;
			clickInterval = DEFAULT_INTERVAL;
		}

		public void onGui(){
			float h = rectangle.height;
			float w = rectangle.width; 
			if (content != null )
			{
				labelText = content[currentIndex].ToString();
			}else{
				labelText = currentIndex.ToString();
			}
			GUI.BeginGroup( rectangle );

			GUI.Box (new Rect (0,0,w,h), "");

			plusButton.Perform();
			plusButton.PerformUpdate();

			minusButton.Perform();
			minusButton.PerformUpdate();

			GUIStyle style = new GUIStyle();
			style.fontSize = 20;
			style.normal.textColor = GUI.skin.label.normal.textColor;
			style.alignment = TextAnchor.MiddleCenter;
			GUI.Label(new Rect(0, h/3, w, (h/3) ), labelText, style);

			GUI.EndGroup();
		}
	}
}


