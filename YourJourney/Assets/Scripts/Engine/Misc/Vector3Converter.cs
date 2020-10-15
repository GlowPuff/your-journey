using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Vector3Converter : JsonConverter
{
	public override bool CanWrite => true;
	public override bool CanRead => true;
	public override bool CanConvert( Type objectType )
	{
		return objectType == typeof( Vector3 );
	}

	public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
	{
		//V3 v3 = (V3)reader.Value;
		//return new Vector3( v3.x, v3.y, v3.z );
		//return new Vector3( JToken.Load( reader ).ToString() );
		JToken jt = JToken.Load( reader );
		return jt.ToObject( typeof( Vector3 ) );
	}

	public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
	{
		Vector3 v3 = (Vector3)value;

		JObject o = new JObject();
		o.Add( new JProperty( "x", v3.x ) );
		o.Add( new JProperty( "y", v3.y ) );
		o.Add( new JProperty( "z", v3.z ) );
		o.WriteTo( writer );
	}
}
