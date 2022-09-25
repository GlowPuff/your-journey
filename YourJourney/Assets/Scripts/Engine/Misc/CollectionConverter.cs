using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CollectionConverter : JsonConverter
{
	public override bool CanWrite => true;
	public override bool CanRead => true;
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(int);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Collection collection = (Collection)value;
		int id = collection.ID;
		JToken t = JToken.FromObject(id);
		t.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var jValue = new JValue(reader.Value);
		Collection collection = Collection.FromID(jValue.Value<int>());

		return collection;
	}
}
