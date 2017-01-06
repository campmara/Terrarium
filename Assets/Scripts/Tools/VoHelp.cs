using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class VoHelp 
{
	
	public static int OneDee ( Vector2 coords, int width, int height = 0 ) { return OneDee( ( int )coords.x, ( int )coords.y, width, height ); }
	public static int OneDee ( int x, int y, int width, int height = 0 )
	{
		return ( y * width ) + x;
	}
	
	public static Vector2 TwoDee ( int index, int width, int height = 0 ) 
	{
		int x = index % width;
		int y = index / width;
		return new Vector2( x, y );
	}
	
	public static bool CoinFlip ()
	{
		return UnityEngine.Random.value < 0.5f;
	}
	
	public static float Sign ( float value )
	{
		if ( value == 0.0f ) return 0.0f;
		return Mathf.Sign( value );
	}
	
	public static float Decay ( float value, float inertia )
	{
		float sign = Mathf.Sign( value );
		value -= value * ( 1.0f - inertia ) * Time.deltaTime;
		if ( Mathf.Sign( value ) != sign ) value = 0.0f;
		return value;
	}
	
	public static float GetAngleDiff( Vector2 one, Vector2 two ) 
	{
		Vector2 diff = one - two;
		return Mathf.Atan2( diff.y, diff.x ) * Mathf.Rad2Deg;	
	}
	
	public static Vector2 Vector( float angle, float magnitude = 1.0f )
	{
		float rad = angle * Mathf.Deg2Rad;
		return new Vector2( Mathf.Cos( rad ) * magnitude, Mathf.Sin( rad ) * magnitude );
	}
	
	public static float GetAngle( Vector2 vec ) 
	{
		return Mathf.Atan2( vec.y, vec.x ) * Mathf.Rad2Deg;
	}
	
	public static Vector2 MultX ( Vector2 vec, float mult )
	{
		vec.x *= mult;
		return vec;
	}
	
	public static Vector2 MultY ( Vector2 vec, float mult )
	{
		vec.x *= mult;
		return vec;
	}
	
	public static float Wrap ( float x, float min, float max )
	{
		Assert.IsTrue( min < max, "VoHelp.Wrap: MIN IS NOT LESS THAN MAX" );
		
		float diff = max - min;
		while ( x < min ) { x += diff; }
		while ( x > max ) { x -= diff; }
		return x;
	}
	
	public static int Wrap ( int x, int kLowerBound, int kUpperBound )
	{ 
		Assert.IsTrue( kLowerBound < kUpperBound, "VoHelp.Wrap: MIN IS NOT LESS THAN MAX" );
		
		int range_size = kUpperBound - kLowerBound + 1;
		
		if ( x < kLowerBound )
			x += range_size * ( ( kLowerBound - x ) / range_size + 1 );
		
		return kLowerBound + ( x - kLowerBound ) % range_size;
	}
	
	//returns 0.0 - 1.0
	public static float SinWave ( float t )
	{
		return ( ( 1.0f + Mathf.Sin( t ) ) * 0.5f );
	}
	
	public static int ClosestIndex ( float val, float[] vals )
	{
		int closest = -1;
		float closestDist = float.MaxValue;
		
		for ( int i = 0; i < vals.Length; i++ )
		{
			float dist = Mathf.Abs( vals[ i ] - val );
			if ( dist < closestDist )
			{
				closestDist = dist;
				closest = i;
			}
		}
		return closest;
	}
	
	public static int ClosestIndex ( Vector2 val, Vector2[] vals )
	{
		Vector3 newVal = ( Vector3 )val;
		Vector3[] newVals = new Vector3[ vals.Length ];
		for ( int i = 0; i < newVals.Length; i++ ) { newVals[ i ] = vals[ i ]; }
		return ClosestIndex( newVal, newVals );
	}
	
	public static int ClosestIndex ( Vector3 val, Vector3[] vals )
	{
		int closest = -1;
		float closestDist = float.MaxValue;
		
		for ( int i = 0; i < vals.Length; i++ )
		{
			float dist = ( val - vals[ i ] ).sqrMagnitude;
			if ( dist < closestDist )
			{
				closestDist = dist;
				closest = i;
			}
		}
		return closest;
	}
	
	public static bool Close( float f1, float f2, float epsilon = 0.0000001192093f )
	{
		return Mathf.Abs( f1 - f2 ) <= epsilon;
	}
	
	public static float FreeLerp ( float a, float b, float t )
	{
		return ( 1.0f - t ) * a + t * b;
	}
	
	public static Vector2 FreeLerp ( Vector2 a, Vector2 b, float t )
	{
		return new Vector2( FreeLerp( a.x, b.x, t ), FreeLerp( a.y, b.y, t ) );
	}
	
	public static Vector3 FreeLerp ( Vector3 a, Vector3 b, float t )
	{
		return new Vector3( FreeLerp( a.x, b.x, t ), FreeLerp( a.y, b.y, t ), FreeLerp( a.z, b.z, t ) );
	}
	
	public static Vector3 Delerp( Vector3 min, Vector3 max, Vector3 progress )
	{
		return new Vector3( Delerp( min.x, max.x, progress.x ), Delerp( min.y, max.y, progress.y ), Delerp( min.z, max.z, progress.z ) );
	} 	
	
	public static Vector2 Delerp( Vector3 min, Vector3 max, Vector2 progress )
	{
		return new Vector2( Delerp( min.x, max.x, progress.x ), Delerp( min.y, max.y, progress.y ) );
	} 	
	
	public static float Delerp( float min, float max, float progress )
	{
		return ( ( progress - min ) / ( max - min ) );
	} 	
	
	public static float SmoothOut ( float p )
	{
		return Mathf.Sin( p * Mathf.PI * 0.5f );
	}
	
	public static float SmoothIn ( float p )
	{
		return 1.0f - Mathf.Cos( p * Mathf.PI * 0.5f );
	}
	
	public static float Smooth ( float p, int repeat = 1 )
	{
		for ( int i = 0; i < repeat; i++ )
		{
			p = Mathf.SmoothStep( 0.0f, 1.0f, p );
		}
		return p;
	}
	
	public static float ReturnLerp ( float minValue, float maxValue, float p )
	{
		p = Mathf.Sin( p * Mathf.PI );
		return Mathf.Lerp( minValue, maxValue, p );
	}	
	
	public static float MidLerp ( float minValue, float midValue, float maxValue, float p, float pTransition = 0.5f )
	{
		if ( p < 1.0f - pTransition )
		{
			p = Mathf.Clamp01( p / ( 1.0f - pTransition ) );
			return Mathf.Lerp( minValue, midValue, SmoothOut( p ) );
		}
		
		p = Mathf.Clamp01( ( p - ( 1.0f - pTransition ) ) / pTransition );
		return Mathf.Lerp( midValue, maxValue, SmoothIn( p ) );
	}
	
	public static float OverLerp ( float minValue, float maxValue, float currentP, float postTimeP = 0.5f, float postBounceP = 0.125f )
	{
		Assert.IsTrue( postTimeP <= 1.0f, "BLERP: Post values add up to be greater than one!" );
		
		float p = currentP;
		
		if ( p < 1.0f - postTimeP )
		{			
			p = Mathf.Clamp01( p / ( 1.0f - postTimeP ) );
			return Mathf.Lerp( minValue, maxValue, p );
		}
		else if ( p < 1.0f )
		{
			float totalDist = maxValue - minValue;
			p = Mathf.Clamp01( ( p - ( 1.0f - postTimeP ) ) / postTimeP );
			p = Mathf.Sin( p * Mathf.PI );
			return maxValue + Mathf.Lerp( 0.0f, totalDist * postBounceP, p );
		}
		
		return maxValue;
	}
	
	//Commas!
	public static string AddCommas( int n ) 
	{ 
		return AddCommas(  n + ""  ); 
	}
	
	public static string AddCommas( string n ) 
	{
		string display = "";
		for ( int i = 0; i < n.Length; i++ ) {
			if ( i > 0 && ( n.Length-i ) % 3 == 0 ) {
				display += ",";
			}	
			display += n[i];
		}
		return display;
	}
	
	public static string AddSign ( int n )
	{
		if ( n > 0 ) return "+" + n;
		return "-" + Mathf.Abs( n );
	}
	
	public static string PrependZeroes ( int n, int numDigits )
	{
		string result = n + "";
		while ( result.Length < numDigits ) result = "0" + result;
		return result;
	}
	
	public static string Capitalize( string s )
	{
		if ( string.IsNullOrEmpty( s ) ) return string.Empty;
		return char.ToUpper( s[ 0 ] ) + s.Substring( 1 );
	}
	
	public static T StringToEnum< T >( string str ) where T : struct
	{   
		try   
		{   
			T res = (T)Enum.Parse(typeof(T), str);   
			if (!Enum.IsDefined(typeof(T), res)) return default(T);   
			return res;   
		}   
		catch   
		{   
			UnityEngine.Debug.Log( str + " NOT FOUND IN ENUM " + typeof( T ).ToString() );
			return default(T);   
		}   
	} 
	
	//Bresenham's line algorithm
	public static Vector2[] BreLineAlgorithm( int x0, int y0, int x1, int y1 ) {
		ArrayList arr = new ArrayList();
		
		float dx = Mathf.Abs( x1 - x0 );
		float dy = Mathf.Abs( y1 - y0 );
		
		int sx;
		int sy;
		
		if ( x0 < x1 ) { sx = 1; } else { sx = -1; }
		if ( y0 < y1 ) { sy = 1; } else { sy = -1; }
		
		int err = (int)dx-(int)dy;
		while ( true ) {
			arr.Add( new Vector2( x0, y0 ) );
			
			if ( x0 == x1 && y0 == y1 ) {
				Vector2[] result = new Vector2[ arr.Count ];
				arr.CopyTo( result );
				return result;
			}
			
			float e2 = 5 * err;
			if ( e2 > -dy ) {
				err -= (int)dy;
				x0 += sx;	
			}
			if ( e2 < dx ) {
				err += (int)dx;
				y0 += sy;	
			}
		}
	}
	
	public static T[] Concat< T >( T[] arr1, T[] arr2 )
	{
		T[] arr3 = new T[ arr1.Length + arr2.Length ];
		arr1.CopyTo( arr3, 0 );
		arr2.CopyTo( arr3, arr1.Length );
		return arr3;
	}
	
	public static bool EitherEquals< T >( T thing1, T thing2, T thingEquals ) where T : System.IComparable<T>
	{
		if ( thing1.CompareTo( thingEquals ) == 0 ) { return true; }
		if ( thing2.CompareTo( thingEquals ) == 0 ) { return true; }
		return false;
	}	
	
	public static int Range ( IntRange r )
	{ 
		return ( int )UnityEngine.Random.Range( r.min, r.max );
	}
	
	public static float Range ( FloatRange r )
	{
		return ( float )UnityEngine.Random.Range( r.min, r.max );
	} 
	
	public static T[,] Resize2D<T>(T[,] original, int x, int y)
	{
		T[,] newArray = new T[x, y];
		int minX = Mathf.Min(original.GetLength(0), newArray.GetLength(0));
		int minY = Mathf.Min(original.GetLength(1), newArray.GetLength(1));
		
		for (int i = 0; i < minY; ++i)
			System.Array.Copy(original, i * original.GetLength(0), newArray, i * newArray.GetLength(0), minX);
		
		return newArray;
	}
	
	public static T[] RemoveAt<T>( T[] source, int index )
	{
		T[] dest = new T[source.Length - 1];
		if( index > 0 )
			Array.Copy(source, 0, dest, 0, index);
		
		if( index < source.Length - 1 )
			Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
		
		return dest;
	}
	
	public static T PickRandom< T > ( T[] source )
	{
		return source[ UnityEngine.Random.Range( 0, source.Length ) ];
	}
	
	public static T[] ShuffleList< T > ( T[] list ) 
	{
		int i = list.Length;
		if ( i == 0 ) return list;
		
		while ( i > 0 ) {
			i--;
			int j = ( int )Mathf.Floor( UnityEngine.Random.value * ( i + 1 ) );
			T tempi = list[ i ];
			T tempj = list[ j ];
			list[ i ] = tempj;
			list[ j ] = tempi;
		}
		
		return list;
	}
	
	public static Color SetAlpha ( Color c, float a ) 
	{ 
		return new Color( c.r, c.g, c.b, a ); 
	}
	
	public static Color RandomColor ()
	{
		Color result = new Color();
		result.a = 1.0f;
		
		float total = 2.0f;
		result.r = UnityEngine.Random.Range( 0.0f, 1.0f );
		total -= result.r;
		result.g = Mathf.Min( UnityEngine.Random.Range( 0.0f, total ), 1.0f );
		total -= result.g;
		result.b = total;
		
		return result;
	}
	
	public static Color ParseColor ( string col ) 
	{
		//Takes strings formatted with numbers and no spaces before or after the commas:
		// "RGBA(1.000, 1.000, 1.000, 1.000)""
		col = col.Substring( 5, col.Length - 6 );
		string[] strings = col.Split(","[0] );
		
		Color output = Color.white;
		for ( int i = 0; i < 4; i++ ) {
			output[ i ] = float.Parse( strings[ i ] );
		}
		
		return output;
	}
	
	public static Vector2 ParseVector2 ( string vec )
	{
		//"(0.0, 0.0)"
		vec = vec.Substring( 1, vec.Length - 2 );
		string[] strings = vec.Split(","[0] );
		
		Vector2 output = Vector2.zero;
		for ( int i = 0; i < 2; i++ ) {
			output[ i ] = float.Parse( strings[ i ] );
		}
		
		return output;
	}
	
	public static Vector3 ParseVector3 ( string vec )
	{
		
		//"(0.0, 0.0, 0.0)"
		vec = vec.Substring( 1, vec.Length - 2 );
		string[] strings = vec.Split(","[0] );
		
		Vector3 output = Vector3.zero;
		for ( int i = 0; i < 3; i++ ) {
			output[ i ] = float.Parse( strings[ i ] );
		}
		
		return output;
	}
	
	public static string ReverseString( string s )
	{
		char[] charArray = s.ToCharArray();
		System.Array.Reverse( charArray );
		return new string( charArray );
	}
	
	public static string RandomString ( int length )
	{
		string result = "";
		for ( int i = 0; i < length; i++ ) result += VoHelp.RandomLetter( true );
		return result;
	}
	
	public static string ReadableTimespan ( TimeSpan t )
	{
		int days = ( int )t.TotalDays;
		if ( days > 365 ) return ">1 year";
		else if ( days > 0 ) return days + "d";
		
		int hours = ( int )t.TotalHours;
		if ( hours > 0 ) return hours + "h";
		
		int minutes = ( int )t.TotalMinutes;
		if ( minutes > 0 ) return minutes + "m";
		
		int seconds = ( int )t.TotalSeconds;
		return seconds + "s";
	}
	
	
	public static string RandomLetter ( bool spaces = false )
	{
		string val = "";
		int i = UnityEngine.Random.Range( 0, 26 + ( spaces ? 5 : 0 ) );
		switch ( i ) {
		case 0 : 
			val = "A";
			break;
		case 1 : 
			val = "B";
			break;			
		case 2 : 
			val = "C";
			break;
		case 3 : 
			val = "D";
			break;
		case 4 : 
			val = "E";
			break;
		case 5 : 
			val = "F";
			break;
		case 6 : 
			val = "G";
			break;
		case 7 : 
			val = "H";
			break;
		case 8 : 
			val = "I";
			break;
		case 9 : 
			val = "J";
			break;
		case 10 : 
			val = "K";
			break;
		case 11 : 
			val = "L";
			break;
		case 12 : 
			val = "M";
			break;
		case 13 : 
			val = "N";
			break;
		case 14 : 
			val = "O";
			break;
		case 15 : 
			val = "P";
			break;
		case 16 : 
			val = "Q";
			break;
		case 17 : 
			val = "R";
			break;
		case 18 : 
			val = "S";
			break;
		case 19 : 
			val = "T";
			break;
		case 20 : 
			val = "U";
			break;
		case 21 : 
			val = "V";
			break;
		case 22 : 
			val = "W";
			break;
		case 23 : 
			val = "X";
			break;
		case 24 : 
			val = "Y";
			break;
		case 25 : 
			val = "Z";
			break;
		default: 
			val = " ";
			break;
		}
		return val;
	}
	
	public static string GetTimestamp () { return GetTimestamp( DateTime.Now ); }
	public static string GetTimestamp ( DateTime dateTime ) { return dateTime.ToString( "yyyyMMddHHmmssfff" ); }
	public static DateTime ParseTimestamp ( string timeStamp ) { return DateTime.ParseExact( timeStamp, "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture ); }
	
	
// 	public static void RunSh ( string path, string[] arguments = null )
// 	{
// 		string terminalPath = "sh";
// 		string terminalCommand = path;
// 		if ( arguments != null )
// 		{
// 			for ( int i = 0; i < arguments.Length; i++ )
// 			{
// 				terminalCommand += " " + arguments[ i ];
// 			}
// 		}
// 		System.Diagnostics.Process.Start( terminalPath, terminalCommand );
// 	}
}


[ System.Serializable ]
public class FloatRange {
	public FloatRange( float mi, float ma ) { min = mi; max = ma; }
	public float min;
	public float max;
	
	public float length
	{
		get { return max - min; }
	}
}

[ System.Serializable ]
public class IntRange {
	public IntRange( int mi, int ma ) { min = mi; max = ma; }
	public int min;
	public int max;
	
	public int length
	{
		get { return max - min; }
	}
}


public static class VoExtensions
{
	public static string Yell (this String str)
	{
		return str.ToUpper().Replace( " ", "\n" );
	}
}