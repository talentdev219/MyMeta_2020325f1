using UnityEngine;
using Mapbox.Directions;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using Mapbox.Unity;
using UnityEngine.UI;
using System;
using FullSerializer;
using Unity.IO;
using UnityEngine.Serialization;
using Proyecto26;

namespace Mapbox.Examples
{
	public class AstronautDirections : MonoBehaviour
	{
		AbstractMap _map;
		Directions.Directions _directions;
		Action<List<Vector3>> callback;
		public bool isPlayer = false;

		public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
		private string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
		public static fsSerializer serializer = new fsSerializer();
		public static EngagementWallData engagementWallData = new EngagementWallData();

		void Awake()
		{
			_directions = MapboxAccess.Instance.Directions;
		}

		public void Query(Action<List<Vector3>> vecs, Transform start, Transform end, AbstractMap map)
		{
			if (callback == null)
				callback = vecs;

			_map = map;

			var wp = new Vector2d[2];
			wp[0] = start.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			wp[1] = end.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
			_directionResource.Steps = true;
			_directions.Query(_directionResource, HandleDirectionsResponse);
		}

		void HandleDirectionsResponse(DirectionsResponse response)
		{
			if (null == response.Routes || response.Routes.Count < 1)
			{
				return;
			}

			var dat = new List<Vector3>();
			foreach (var point in response.Routes[0].Geometry)
			{
				dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
				if (isPlayer)
				{
					PlayerPrefs.SetFloat("loc0", (float)point.x);
					PlayerPrefs.SetFloat("loc1", (float)point.y);

					RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
					{
						fsData walltdata = fsJsonParser.Parse(response.Text);
						serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

						ApplicationsPlayerOnlineData applicationsPlayerOnlineData = new ApplicationsPlayerOnlineData();
						applicationsPlayerOnlineData.loc0 = (float)point.x;
						applicationsPlayerOnlineData.loc1 = (float)point.y;
						applicationsPlayerOnlineData.state = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].online.state;

						RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/"+ PlayerPrefs.GetInt("application_code") + "/online.json", applicationsPlayerOnlineData).Catch(error =>
						{
							Debug.Log(error);
						});
					});
				}
				
			}

			callback(dat);
		}
	}
}
