namespace Mapbox.Unity.Map
{
	using System;
	using UnityEngine;
	using Mapbox.Unity.Utilities;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine.Rendering.PostProcessing;
	using UnityEngine.UI;
	using Mapbox.Unity.Map;
	using UnityEngine.SceneManagement;
	using Mapbox.Examples;
	using Mapbox.Unity.Location;
	using Mapbox.Utils;
	using Proyecto26;
	using FullSerializer;

	[Serializable]
	public class MapLocationOptions : MapboxDataProperty
	{
		[Geocode]
		[Tooltip("The coordinates to build a map around")]
		public string latitudeLongitude = "0,0";
		[Range(3, 17)]
		[Tooltip("The zoom level of the map")]
		public float zoom = 16.0f;

		//TODO : Add Coordinate conversion class. 
		[NonSerialized]
		public MapCoordinateSystemType coordinateSystemType = MapCoordinateSystemType.WebMercator;
	}
}
