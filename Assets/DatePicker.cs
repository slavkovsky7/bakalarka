using UnityEngine;
using System.Collections;
using System;

public class DatePicker
{

	public static float EARTH_PERIOD = 8766.0f; 
	public static int YEAR_COUNT = 196; 
	public static int START_YEAR = 1904; 


	public static string[] months = new string[] { "jan", "feb", "mar", "apr" , "may" , "jun", "jul" , "aug" , "sep", "oct", "nov", "dec"};
	public static string[] longerMonths = new string[] { "jan", "mar", "may", "july", "aug", "oct", "dec" };

	private static bool containsValue(string[] arr, string value){
		foreach ( string str in arr)
			if ( value == str)
				return true;
		return false;
	}

	private static void SetCorrectDay( SinglePicker dayPicker , SinglePicker monthPicker, SinglePicker yearPicker , int monthDiff)
	{
		int day = dayPicker.getCurrentIndex();
		if (day < 1){
			dayPicker.setCurrentIndex( GetMaxDay( months[monthPicker.getCurrentIndex()] , yearPicker.getCurrentIndex() ) );
		}
		int maxDay = GetMaxDay( months[monthPicker.getCurrentIndex()] , yearPicker.getCurrentIndex() );
		if ( day > maxDay ){
			if (monthDiff == 0)
			{
				dayPicker.setCurrentIndex(1);
			}else{
				dayPicker.setCurrentIndex(maxDay);
			}
		}
	}

	public static int GetMaxDay( string month, int year ){
		return (month == "feb") ? (year % 4 == 0 ? 29 : 28) : containsValue(longerMonths, month ) ? 31 : 30;
	}

	private Rect rectangle;
	private SinglePicker monthPicker;
	private SinglePicker dayPicker;
	private SinglePicker yearPicker;

	private SinglePicker hourPicker;
	private SinglePicker minutePicker;


	public static DateTime getEpoch()
	{ 
		return new DateTime(1904,1,1);
	}

	public DatePicker( Rect rectangle )
	{
		float w6 = rectangle.width / 6; 
		this.rectangle = rectangle;
		this.yearPicker 	= new SinglePicker( new Rect( 2*w6 	,0 ,w6 ,rectangle.height), null	  , null ).withInterval(START_YEAR, START_YEAR + YEAR_COUNT );
		this.monthPicker	= new SinglePicker( new Rect( 0		,0 ,w6 ,rectangle.height), months, null ).withInterval(0,months.Length);
		this.dayPicker 		= new SinglePicker( new Rect( w6 	,0 ,w6 ,rectangle.height), null  , null );

		this.hourPicker = new SinglePicker(	  new Rect( 3*w6 ,0 ,w6 ,rectangle.height), null , null).withInterval(0,24);
		this.minutePicker = new SinglePicker( new Rect( 4*w6 ,0 ,w6 ,rectangle.height), null , null ).withInterval(0,60);

		/*this.monthPicker.setCurrentIndex(0);
		this.dayPicker.setCurrentIndex(1);
		this.yearPicker.setCurrentIndex(1970);
		this.hourPicker.setCurrentIndex(0);
		this.minutePicker.setCurrentIndex(0);*/
		setDate( DateTime.Now );
	}

	public void setDate( DateTime date){
		this.monthPicker.setCurrentIndex(date.Month - 1);
		this.dayPicker.setCurrentIndex(date.Day);
		this.yearPicker.setCurrentIndex(date.Year);
		this.hourPicker.setCurrentIndex(date.Hour);
		this.minutePicker.setCurrentIndex(date.Minute);
	}

	public void setDate(double hours )
	{
		TimeSpan t = TimeSpan.FromHours( hours);
		setDate( getEpoch().AddSeconds(t.TotalSeconds) );
	}

	public double getDateInHours()
	{
		DateTime date = getDate();
		TimeSpan span= date.Subtract( getEpoch() ) ;
		return span.TotalHours;
	}

	public DateTime getDate()
	{
		DateTime time = new DateTime(yearPicker.getCurrentIndex(), 
		                             monthPicker.getCurrentIndex() + 1,
		                             dayPicker.getCurrentIndex(),
		                             hourPicker.getCurrentIndex(),
		                             minutePicker.getCurrentIndex() , 0, DateTimeKind.Utc);
		return time;
	}

	private double lastDateHours = -1;

	public void onGui(){
		GUI.BeginGroup( rectangle );
		GUI.Box ( new Rect(0,0, rectangle.width, rectangle.height) , "");

		int startMonth = monthPicker.getCurrentIndex();
		monthPicker.onGui();
		int diff = monthPicker.getCurrentIndex() - startMonth;
		dayPicker.onGui();
		SetCorrectDay( dayPicker ,monthPicker ,yearPicker, diff );
		yearPicker.onGui();
		hourPicker.onGui();
		minutePicker.onGui();
		if (Sun.TimeConstant == 0){
			double dateHours = Sun.DatePicker.getDateInHours();
			if (dateHours != lastDateHours)
			{
				Debug.Log("Date set");
				foreach ( Planet planet in Sun.Planets ){
					planet.CurrentTime = dateHours;
					planet.SetDate(dateHours);
				}
			}
			lastDateHours = dateHours;
		}

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

		private int intervalBegin;
		private int intervalEnd;
		private bool intervalSpecified = false;

		private SinglePicker parentPicker = null;

		public SinglePicker( Rect rectangle , string[] content, SinglePicker parentPicker)
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

			this.parentPicker = parentPicker;

		}

		public SinglePicker withInterval( int begin , int end ){
			this.intervalEnd = end;
			this.intervalBegin = begin;
			intervalSpecified = true;
			return this;
		}

		public void setCurrentIndex( int index ) { 
			bool doReturn = false;
			if (parentPicker != null ){

				if ( index == intervalEnd ){
					currentIndex = intervalBegin;
					parentPicker.addCurrentIndex(1);
					doReturn = true;
				}

				if ( index < intervalBegin ){
					currentIndex = intervalEnd - 1;
					parentPicker.addCurrentIndex(-1);
					doReturn = true;
				}
				if (doReturn){
					return;
				}
			}

			if (intervalSpecified )
			{
				if (index < intervalBegin) {
					currentIndex = intervalEnd - 1;
					return;
				}else if (index == intervalEnd ){
					currentIndex = intervalBegin;
					return;
				}
			}
			currentIndex = index;
			/*
			if ( ( intervalSpecified && index >= intervalBegin && index < intervalEnd ) || !intervalSpecified ){
				currentIndex = index;
			}*/
		}

		public int getCurrentIndex( ) { return currentIndex; }

		private void addCurrentIndex( int n ){
			if (Time.time - lastClick > clickInterval){
				setCurrentIndex( currentIndex + n);
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


