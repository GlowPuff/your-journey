using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TileConverter : JsonConverter
{
	public override bool CanWrite => false;
	public override bool CanRead => true;
	public override bool CanConvert( Type objectType )
	{
		return objectType == typeof( ITile );
	}
	public override void WriteJson( JsonWriter writer,
			object value, JsonSerializer serializer )
	{
		throw new InvalidOperationException( "Use default serialization." );
	}

	public override object ReadJson( JsonReader reader,
			Type objectType, object existingValue,
			JsonSerializer serializer )
	{
		var jsonObject = JArray.Load( reader );
		var tile = default( ITile );
		List<ITile> tileObserver = new List<ITile>();

		foreach ( var item in jsonObject )
		{
			switch ( item["tileType"].Value<int>() )
			{
				case 0:
					tile = item.ToObject<HexTile>();
					break;
				case 1:
					tile = item.ToObject<BattleTile>();
					break;
				case 2:
					tile = item.ToObject<SquareTile>();
					break;
			}
			tileObserver.Add( tile );
		}

		return tileObserver;
	}
}