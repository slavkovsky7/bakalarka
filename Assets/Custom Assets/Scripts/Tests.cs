//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//using NUnit.Framework;
using System;
using Bakalarka;
using UnityEngine;
//using NUnit.Framework;

namespace SolarTests
{
/*	[TestFixture()]
	public class Tests
	{
		[Test()]
		public void TestFactorial ()
		{
			Assert.AreEqual ( Math2f.Factorial(5), 5*4*3*2*1 ); 
			Assert.AreEqual ( Math2f.Factorial(0) , 1);
			Assert.AreEqual ( Math2f.BinomialCoefficient(5,3), (5*4*3) / (3*2*1) );
			Assert.AreEqual ( Math2f.BinomialCoefficient(5,3), Math2f. BinomialCoefficient(5,2));
			Assert.AreEqual( Math2f.HalfBinomialCoefficient(2), (-1.0/8.0) );
			Assert.AreEqual( Math2f.HalfBinomialCoefficient(1), 0.5 );
			Assert.AreEqual( Math2f.HalfBinomialCoefficient(3), 0.0625 );
			Assert.AreEqual( Math2f.HalfBinomialCoefficient(0), 1 );
			Assert.True( Math.Abs ( new Ellipse(10,10).getPerimeter() -  62.832 ) < 0.02 ); 
			Assert.True( Math.Abs ( new Ellipse(10,1).getPerimeter() -  40.623 ) < 0.02 ); 
			Assert.True( Math.Abs ( new Ellipse(10,3).getPerimeter() -  43.859 ) < 0.02 ); 
		
			float sunGravityParam = 1327124.25f;

			Ellipse earth =	new Ellipse(new Vector3(1,0,0), new Vector3(0,1,0), 1520.0f, 0.0167f);
			Assert.True( Math.Abs ( earth.getAverageOrbitalSpeed(sunGravityParam) -  29.79 ) < 0.02 );

			float v = earth.getAverageOrbitalSpeed(sunGravityParam);
			float p = earth.getPerimeter();
			float period = earth.getPeriod(sunGravityParam);


			Assert.AreEqual( earth.getGravitationParameter(period), sunGravityParam );
		}

		public float getAngularVelocity(Ellipse ellipse, float angle , float area)
		{ 
			Vector3 v = getVelocity(ellipse, angle, area);
			Vector3 r = ellipse.getPosition(angle);
			float theta = Vector3.Angle(r, v)*Mathf.Deg2Rad;
			float w = (v.magnitude * Mathf.Sin(theta) )  / r.magnitude;
			return w;
		}

		
		public Vector3 getVelocity(Ellipse ellipse, float angle  , float area)
		{
			Vector3 r = ellipse.getPosition(angle) - ellipse.getF1();
			float vAnsSinTheta = (2*area) / r.magnitude;
			Vector3 velocityDir = ellipse.getVelocityDirection(angle);
			float theta = Vector3.Angle(r, velocityDir)*Mathf.Deg2Rad;
			float velocity =vAnsSinTheta / Mathf.Sin(theta);
			return velocityDir*velocity;
		}

		[Test()]
		public void TestMercury()
		{
			float sunGravityParam = 1327124.25f;
			Ellipse mercury = new Ellipse(new Vector3(1,0,0), new Vector3(0,1,0), 698.16f, 0.205630f);
			float mercuryMinimumSpeed = mercury.getMinimumSpeed(sunGravityParam);
			Assert.True( Math.Abs ( mercuryMinimumSpeed -  38.85f ) < 0.02 );
			float perihelion = (mercury.getF1() - mercury.getPosition(180) ) .magnitude;

			Vector3 minimalSpeed = new Vector3(0,mercuryMinimumSpeed, 0);

			float area =   ( 698.16f * mercuryMinimumSpeed ) / 2; 
			Console.WriteLine( "aphelion.v = " + getVelocity(mercury, 0, area) );
			Console.WriteLine( "aphelion.w = " + getAngularVelocity(mercury, 0, area) );
			Console.WriteLine( "90 = " + getVelocity(mercury, 90, area) );
			Console.WriteLine( "aphelion.v = " + getVelocity(mercury, 180, area) );
			Console.WriteLine( "aphelion.w = " + getAngularVelocity(mercury, 180, area ) );

			Console.WriteLine( "VelocityDiv = " + getVelocity(mercury, 180, area).magnitude /  getVelocity(mercury, 0, area).magnitude);
			Console.WriteLine( "AngularDiv = "  + getAngularVelocity(mercury, 180, area) /  getAngularVelocity(mercury, 0, area));
			Assert.True( (perihelion -  460.01) < 0.02 ); 
		}
	}*/
}

