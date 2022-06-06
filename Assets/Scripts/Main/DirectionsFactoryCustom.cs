namespace Mapbox.Unity.MeshGeneration.Factories
{
	using UnityEngine;
	using Mapbox.Directions;
	using System.Collections.Generic;
	using System.Linq;
	using Mapbox.Unity.Map;
	using Data;
	using Modifiers;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using System.Collections;
    using System;
    using Mapbox.CheapRulerCs;
	using UnityEngine.UI;
	using Proyecto26;
	using FullSerializer;
    using Mapbox.Examples;
    using Mapbox.Unity.Location;

    public class DirectionsFactoryCustom : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		MeshModifier[] MeshModifiers;
		[SerializeField]
		Material _material;

		[SerializeField]
		public Transform[] _waypoints;
		private List<Vector3> _cachedWaypoints;

		[SerializeField]
		[Range(1, 10)]
		private float UpdateFrequency = 2;

		private Directions _directions;
		private int _counter;

		GameObject _directionsGO;
		private bool _recalculateNext;
		GameObject player;

		public GasController gasController;
		double[] start_location = new double[2];
		double[] end_location = new double[2];
		double distance;

		bool isLoading = false;

		protected virtual void Awake()
		{
			if (_map == null)
			{
				_map = FindObjectOfType<AbstractMap>();
			}
			_directions = MapboxAccess.Instance.Directions;
		}

		public void Start()
		{
			player = GameObject.FindGameObjectWithTag("Player");
			_cachedWaypoints = new List<Vector3>(_waypoints.Length);
			foreach (var item in _waypoints)
			{
				_cachedWaypoints.Add(item.position);
			}
			_recalculateNext = false;

			foreach (var modifier in MeshModifiers)
			{
				modifier.Initialize();
			}
		}

		public void Query(Vector2d start, Vector2d end)
		{
			//gasController.shadowScener.StartPart(1);
			_waypoints[0].transform.position = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(start);
			_waypoints[1].transform.position = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(end);

			var wp = new Vector2d[2];

			wp[0] = start;
			wp[1] = end;

			var ruler = new CheapRuler(Convert.ToDouble(wp[0][0]), CheapRulerUnits.Miles);
			start_location[0] = Convert.ToDouble(wp[0][0]); start_location[1] = Convert.ToDouble(wp[0][1]);
			end_location[0] = Convert.ToDouble(wp[1][0]); end_location[1] = Convert.ToDouble(wp[1][1]);

			distance = ruler.Distance(start_location, end_location);

			StartCoroutine(MoveToTargetCoroutine(wp));
		}

		IEnumerator MoveToTargetCoroutine(Vector2d[] wp)
		{
			yield return new WaitForEndOfFrame();
			float startzoom = _map.Zoom;
			float multiplyer = 1;
			if (startzoom > 6)
            {
                for (float i = startzoom; i > 6; i -= 0.1f)
                {
                    yield return new WaitForSeconds(0.007f * multiplyer);
                    if (multiplyer < 3)
                    {
                        multiplyer += 0.1f;
                    }
                    _map.UpdateMap(wp[1], i);

                }
            }
			if (startzoom <= 6)
            {
				_map.UpdateMap(wp[1], startzoom);
			}
            var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
            _directionResource.Steps = true;
            _directions.Query(_directionResource, HandleDirectionsResponse);
			_waypoints[1].transform.position = new Vector3(0, 0, 0);
		}

		void HandleDirectionsResponse(DirectionsResponse response)
		{
			if (response == null || null == response.Routes || response.Routes.Count < 1)
			{
				return;
			}

			var meshData = new MeshData();
			var dat = new List<Vector3>();
			foreach (var point in response.Routes[0].Geometry)
			{
				dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
			}

			var feat = new VectorFeatureUnity();
			feat.Points.Add(dat);

			foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
			{
				mod.Run(feat, meshData, _map.WorldRelativeScale);
			}

			CreateGameObject(meshData);
		}

		GameObject CreateGameObject(MeshData data)
		{
			if (_directionsGO != null)
			{
				Destroy(_directionsGO);
			}
			_directionsGO = new GameObject("direction waypoint entity");
			var mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
			mesh.subMeshCount = data.Triangles.Count;

			mesh.SetVertices(data.Vertices);
			_counter = data.Triangles.Count;
			for (int i = 0; i < _counter; i++)
			{
				var triangle = data.Triangles[i];
				mesh.SetTriangles(triangle, i);
			}

			_counter = data.UV.Count;
			for (int i = 0; i < _counter; i++)
			{
				var uv = data.UV[i];
				mesh.SetUVs(i, uv);
			}

			mesh.RecalculateNormals();
			_directionsGO.AddComponent<MeshRenderer>().material = _material;
			return _directionsGO;
		}
	}

}

