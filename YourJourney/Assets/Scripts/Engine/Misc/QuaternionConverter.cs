using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class QuaternionConverter : JsonConverter
{
	public override bool CanWrite => true;
	public override bool CanRead => true;
	public override bool CanConvert( Type objectType )
	{
		return objectType == typeof( Quaternion );
	}

	public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
	{
		//V4 v4 = (V4)reader.Value;
		//return new Quaternion( v4.w, v4.x, v4.y, v4.z );
		JToken jt = JToken.Load( reader );
		return jt.ToObject( typeof( Quaternion ) );
	}

	public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
	{
		Quaternion v4 = (Quaternion)value;

		JObject o = new JObject();
		o.Add( new JProperty( "w", v4.x ) );
		o.Add( new JProperty( "x", v4.x ) );
		o.Add( new JProperty( "y", v4.y ) );
		o.Add( new JProperty( "z", v4.z ) );
		o.WriteTo( writer );
	}
}
