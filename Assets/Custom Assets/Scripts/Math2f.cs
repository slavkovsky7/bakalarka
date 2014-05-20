//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace Bakalarka
{
	public class Math2f
	{
		public static double EULER =2.71828;

		public static int Factorial(int n)
		{
			int result = 1;
			for (int i = 1; i <= n ; i++) result *= i;
			return result;
		}

		public static int BinomialCoefficient(int n , int k)
		{
			if (n < k) return 0; 
			return Factorial(n) / ( Factorial(k)* Factorial(n - k) );
		}

		//http://math.stackexchange.com/questions/340124/binomial-coefficients-1-2-k
		public static float HalfBinomialCoefficient( int n )
		{
			if (n == 0) return 1.0f;
			float n1 = Mathf.Pow(-1, n - 1) / ( Mathf.Pow(2, 2*n - 1) * n );
			float n2 = BinomialCoefficient( 2*n - 2 , n -1 );
			return n1*n2;
		}

		public const float MIN_TO_HOUR = 0.0166666667f;
		public const float SEC_TO_HOUR = 0.000277777778f;
	}

	public class Ellipse
	{
		public const float SCALE_PARAM = 100000;

		public Vector3 U;
		public Vector3 V;
		public float e; //eccentricity
		
		public float semi_major;
		public float semi_minor;

		private float apsisDistance;

		public Vector3 getF1( )
		{
			float F = (float)Math.Sqrt( semi_major*semi_major  - semi_minor*semi_minor );
			Vector3 F1 = -F*U;
			return F1;
		}
		
		public Vector3 getF2( )
		{
			float F = (float)Math.Sqrt( semi_major*semi_major  - semi_minor*semi_minor );
			Vector3 F1 = F*U;
			return F1;
		}

		
		public Ellipse( float semi_minor , float semi_major )
		{
			this.U = new Vector3(1,0,0);
			this.V = new Vector3(0,1,0);
			this.semi_major = semi_major;
			this.semi_minor = semi_minor;
			this.e = (float) Math.Sqrt( semi_major*semi_major - semi_minor*semi_minor)/semi_major;
		}

		public Ellipse(Vector3 U, Vector3 V, float apsisDistance , float e)
		{
			this.U = U.normalized;
			this.V = V.normalized;
			this.e = e;
			this.semi_major = apsisDistance / ( 1 + e);
			this.semi_minor  = semi_major*Mathf.Sqrt( 1 - e*e );
			this.apsisDistance = apsisDistance;
		}

		public float getApsisDistance(){
			return apsisDistance;
		}

		public Vector3 getPosition(double angle ){
			return getPosition(angle, 1.0f, 0 );
		}
		
		public Vector3 getPosition(double angle, double scale , double rotation )
		{
			angle = Mathf.Deg2Rad * angle;
			Vector3 result = new Vector3();
			result.x =  (float) ( scale*semi_major * Math.Cos(angle)*U.x + scale*semi_minor * Math.Sin(angle)*V.x );
			result.y =  (float) ( scale*semi_major * Math.Cos(angle)*U.y + scale*semi_minor * Math.Sin(angle)*V.y );
			result.z =  (float) ( scale*semi_major * Math.Cos(angle)*U.z + scale*semi_minor * Math.Sin(angle)*V.z );

			result = Quaternion.AngleAxis ((float)rotation, new Vector3(0,1,0)) * result;
			return result;
		}

		public Vector3 getPosition2( double eccentricAnomally ){
			eccentricAnomally = eccentricAnomally* Mathf.Deg2Rad;
			double C = Math.Cos(eccentricAnomally);
			double S = Math.Sin(eccentricAnomally);
			
			double x = semi_major* (C - this.e); 
			double y = semi_minor*S;
			
			Vector3 result = new Vector3();
			result.x = (float)(U.x * x  + V.x*y);
			result.y = (float)(U.y * x  + V.y*y);
			result.z = (float)(U.z * x  + V.z*y);
			return result;	
		}
		
		public Vector3 getVelocityDirection( double angle )
		{
			angle = Mathf.Deg2Rad * angle;
			Vector3 v = new Vector3();
			v.x = (float) ( -semi_major * Math.Sin(angle)*U.x + semi_minor * Math.Cos(angle)*V.x );
			v.y = (float) ( -semi_major * Math.Sin(angle)*U.y + semi_minor * Math.Cos(angle)*V.y );
			v.z = (float) ( -semi_major * Math.Sin(angle)*U.z + semi_minor * Math.Cos(angle)*V.z );
			v.Normalize();
			return v;
		}
		
		public double getAreaVelocity(double gravitParam)
		{
			Vector3 aphelion = getPosition(0) - getF1();
			double minimumSpeed = getMinimumSpeed(gravitParam);
			double area = ( aphelion.magnitude * minimumSpeed ) / 2;
			return area;
		}



		public double getAngularVelocity(double angle , double area)
		{ 
			Vector3 v = getVelocity(angle, area);
			Vector3 r = getPosition(angle);
			double theta = Vector3.Angle(r, v)*Mathf.Deg2Rad;
			double w = (v.magnitude * Math.Sin(theta) )  / r.magnitude;
			return w;
		}

		public double getFullArea(double gravitParam){
			double area = getAreaVelocity(gravitParam);
			double sw = getAngularVelocity(0, area);
			double full = ( 360/sw ) * area;
			return full;
		}

		public Vector3 getVelocity(double angle  , double area)
		{
			Vector3 r = getPosition(angle) - getF1();
			double vAnsSinTheta = (2*area) / r.magnitude;
			Vector3 velocityDir = getVelocityDirection(angle);
			double theta = Vector3.Angle(r, velocityDir)*Mathf.Deg2Rad;
			float velocity = (float) ( vAnsSinTheta / Math.Sin(theta) );
			return velocityDir*velocity;
		}

		public double getMinimumSpeed(double gravitParameter)
		{
			double velocity =  Math.Sqrt( ((1 - e)*gravitParameter) / ( (1+e)*semi_major ) ); 
			return velocity;
			//float angularVelocity = velocity / semi_major ;
			//return angularVelocity;
		}


		public float getArea() {return Mathf.PI*semi_major*semi_minor;}
		
		public void drawAroundPoint( Vector3 point, float scale , float rotation )
		{
			for (int i = 0 ; i < 360 ; i++ )
			{
				Vector3 p1 = point + getPosition(i , scale,  rotation  );
				Vector3 p2 = point + getPosition( (i + 1) %  360 , scale, rotation );
				//Drawing.DrawLine(p1, p2, Color.red, 10, true);

				Debug.DrawLine(p1 , p2, Color.blue);
			}
		}
		
		public void drawAroundPoint( Vector3 point, LineRenderer renderer , Transform t, bool isSelected)
		{
			if (renderer != null)
			{
				renderer.SetVertexCount(361);
				if (isSelected){
					renderer.SetColors(new Color(1,0.3f,0.3f,0.3f), new Color(1,0.3f,0.3f,0.3f));
					renderer.SetWidth(12,12);
				}else{
					renderer.SetColors(new Color(1,1,1,0.3f), new Color(1,1,1,0.3f));
					renderer.SetWidth(7,7);
				}
				for (int i = 0 ; i <= 360 ; i++ )
				{
					Vector3 p1 = point + getPosition((double)i,  1.0f , 0 );
					//Vector3 p2 = point + getPosition( (i + 1) %  360 , scale, rotation );
					renderer.SetPosition(i, t.parent.transform.TransformPoint(  p1)  );			//Debug.DrawLine(p1 , p2, Color.blue);
				}
			}
		}


		public double getAverageOrbitalSpeed( double gravitParam ){return Math.Sqrt(gravitParam / semi_major);}

		public float getPerimeter()
		{
			float a = semi_major;
			float b = semi_minor;
			float result = Mathf.PI *( 3.0f*( a + b) - Mathf.Sqrt( (3.0f*a + b )*( a + 3.0f*b) ) );
			return result;
		}


		public double getPeriod(double gravitParam)
		{
			/*double v = getAverageOrbitalSpeed(gravitParam);
			double s = getPerimeter();
			double period = SCALE_PARAM*(s / (v*3600 ));
			return period;*/
			double result =  2*Math.PI*Math.Sqrt( Math.Pow( semi_major, 3.0 ) / gravitParam );
			return  (result*SCALE_PARAM) / 3600;
		}

		public double getGravitationParameter( double period )
		{
			/*period = period / SCALE_PARAM;
			double s = getPerimeter();
			double v = s / (period*3600);
			double result = semi_major * v*v;
			return result;*/
			double d = 3600 / SCALE_PARAM;
			double result =  (4*Math.PI*Math.PI*Math.Pow( semi_major, 3 ) ) / Math.Pow(period*d,2.0);
			return  result;
		}

		/*public double EccentricAnnomaly(double meanAnomally, double precision){
			double K = Math.PI / 180.0;
			int maxIter = 30;
			int i = 0 ;
			double delta = Math.Pow(10, -precision);
			double E, F;
			meanAnomally = meanAnomally/360;
			meanAnomally = 2*Math.PI*( meanAnomally - Math.Floor(meanAnomally));
			E = this.e < 0.8 ? meanAnomally : Math.PI;
			F = E - this.e*Math.Sin(meanAnomally) - meanAnomally;
			while ( Math.Abs(F) > delta && i < maxIter ){
				E = E - F/ ( 1 - this.e*Math.Cos(E));
				F = E - this.e* Math.Sin(E) - meanAnomally;
				i++;
			}
			E = E/K;
			return Math.Round(E*Math.Pow(10,precision))/ Math.Pow(10, precision);
		}*/
		

		public double EccentricAnnomaly(double meanAnomally, double precision){
			double M = meanAnomally*Mathf.Deg2Rad;
			int maxIter = 30;
			double delta = Math.Pow(10, -precision);

			double E0 = M + e * Math.Sin(M) * (1 + e * Math.Cos(M));
			double E1 = E0 - (E0 - e * Math.Sin(E0) - M) / ( 1 - e * Math.Cos(E0));
			int i = 0;
			while ( Math.Abs(E0 - E1) > delta && i < maxIter ){
				E0 = E1;
				E1 = E0 - (E0 - e * Math.Sin(E0) - M) / ( 1 - e * Math.Cos(E0));
				i++;
			}
			return E1 * Mathf.Rad2Deg;
		}
	}
}

