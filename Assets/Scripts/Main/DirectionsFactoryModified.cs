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

	public class DirectionsFactoryModified : MonoBehaviour
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
		protected virtual void Awake()
		{
			if (_map == null)
			{
				_map = FindObjectOfType<AbstractMap>();
			}
			_directions = MapboxAccess.Instance.Directions;
			_map.OnInitialized += Query;
			_map.OnUpdated += Query;
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

			StartCoroutine(QueryTimer());
			Query();
			if (player.activeSelf) _waypoints[0].transform.position = player.transform.position;
		}

		protected virtual void OnDestroy()
		{
			_map.OnInitialized -= Query;
			_map.OnUpdated -= Query;
		}

		void Query()
		{
			var count = _waypoints.Length;
			var wp = new Vector2d[count];
			wp[1] = _waypoints[1].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			double[] newend_location = new double[2];
			newend_location[0] = Convert.ToDouble(wp[1][0]); newend_location[1] = Convert.ToDouble(wp[1][1]);
			if (newend_location[0] != end_location[0] && newend_location[1] != end_location[1])
            {
				for (int i = 0; i < count; i++)
				{
					wp[i] = _waypoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);

					var ruler = new CheapRuler(Convert.ToDouble(wp[0][0]), CheapRulerUnits.Miles);
					start_location[0] = Convert.ToDouble(wp[0][0]); start_location[1] = Convert.ToDouble(wp[0][1]);
					end_location[0] = Convert.ToDouble(wp[1][0]); end_location[1] = Convert.ToDouble(wp[1][1]);

					distance = ruler.Distance(start_location, end_location);
					_waypoints[1].GetComponentInChildren<TextMesh>().text = distance.ToString("0.0") + " miles\n" + wp[1][0].ToString("0.00") + ", " + wp[1][1].ToString("0.00");
					gasController.gameManager.LoadingScreen.SetActive(false);
				}
				StartCoroutine(RoadCreateDelay(wp));
			}
			
		}

		IEnumerator RoadCreateDelay(Vector2d[] wp)
        {
			yield return new WaitForSeconds(0.5f);

			float timetotravel = (float)distance / (GasController.speed_persecond * 60);
			gasController.b_redtravel_time.text = timetotravel.ToString("0") + " minutes";

			if (distance > GasController.gasvolume * (GasController.gasvalue / 100))
            {
				gasController.redbutton_textlavel.text = "NOT ENOUGH ENERGY";
				gasController.b_redtravel.GetComponent<Image>().color = gasController.redbutton_color[0];
				gasController.b_redtravel.GetComponent<Button>().onClick.RemoveAllListeners();
				gasController.b_redtravel.GetComponent<Button>().onClick.AddListener(() => { gasController.noGasToTravel.SetActive(true); });
			}

			if (distance <= GasController.gasvolume * (GasController.gasvalue / 100))
			{
				gasController.redbutton_textlavel.text = "TRAVEL HERE";
				gasController.b_redtravel.GetComponent<Image>().color = gasController.redbutton_color[1];
				gasController.b_redtravel.GetComponent<Button>().onClick.RemoveAllListeners();
				gasController.b_redtravel.GetComponent<Button>().onClick.AddListener(() => { gasController.letstravelPage.SetActive(true);});

				gasController.t_letstravel_distance.text = distance.ToString("0") + " miles";
				gasController.t_letstravel_time.text = timetotravel.ToString("0") + " min";
				gasController.t_letstravel_speed.text = (GasController.speed_persecond * 60 * 60).ToString("0") + " miles per hour";
				gasController.t_letstravel_gas.text = ((100 * distance) / GasController.gasvolume).ToString("0") + "%";
				gasController.t_letstravel_location.text = "MOVING TO\n<color=#30C5FF>" + end_location[0].ToString("0.00") + ", " + end_location[1].ToString("0.00") + "</color>";
				gasController.b_letstravel_back.onClick.RemoveAllListeners();
				gasController.b_letstravel_back.onClick.AddListener(() => {
					gasController.letstravelPage.SetActive(false);
				});
				gasController.b_letstravel_start.onClick.RemoveAllListeners();
				gasController.b_letstravel_start.onClick.AddListener(() => {
					ApplicationsPlayerTravelData new_profile_travel = new ApplicationsPlayerTravelData();
					new_profile_travel.dateStart = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Epoch timestamp
					new_profile_travel.dateEnd = (int)(new_profile_travel.dateStart + (float)distance / (GasController.speed_persecond)); //Epoch timestamp
					new_profile_travel.distance = (float)distance;
					new_profile_travel.gasStart = (GasController.gasvalue * GasController.gasvolume) / 100; //gas amount in miles
					new_profile_travel.isTravel = 1;
					new_profile_travel.speed = GasController.speed_persecond;

					if (start_location[0].ToString().Contains('.'))
                    {
						new_profile_travel.loc0 = start_location[0].ToString().Split('.')[0] + "," + start_location[0].ToString().Split('.')[1] + "," + start_location[1].ToString().Split('.')[0] + "," + start_location[1].ToString().Split('.')[1];
						new_profile_travel.loc1 = end_location[0].ToString().Split('.')[0] + "," + end_location[0].ToString().Split('.')[1] + "," + end_location[1].ToString().Split('.')[0] + "," + end_location[1].ToString().Split('.')[1];
					}
					if (!start_location[0].ToString().Contains('.'))
					{
						new_profile_travel.loc0 = start_location[0] + "," + start_location[1];
						new_profile_travel.loc1 = end_location[0] + "," + end_location[1];
					}
					

					RestClient.Put(GameManager.DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/travel.json", new_profile_travel).Then(result => 
					{
						gasController.b_travel.GetComponent<Button>().onClick.RemoveAllListeners();
						gasController.b_travel.GetComponent<Button>().onClick.AddListener(() => { gasController.GetTravelData(true); });

						gasController.travelStartedPage.SetActive(true);
						gasController.b_travelstarted_ok.onClick.RemoveAllListeners();
						gasController.b_travelstarted_ok.onClick.AddListener(() =>
                        {
							gasController.travelStartedPage.SetActive(false);
							gasController.GetTravelData(true);
						});

						gasController.b_travelpage_close.onClick.RemoveAllListeners();
						gasController.b_travelpage_close.onClick.AddListener(() => {
							gasController.panMovement.enabled = true;
							gasController.MainPage.SetActive(true);
							gasController.TrackJourneyPage.SetActive(false);
						});
						
						gasController.MainPage.SetActive(true);
						gasController.setupWaypointsPage.SetActive(false);
						if (!gasController.map.GetComponent<Mapbox.Examples.QuadTreeCameraMovement>().enabled) gasController.map.GetComponent<Mapbox.Examples.QuadTreeCameraMovement>().enabled = true;
						GasController.isSetuppingTravel = false;

						gasController.gameManager.toggler.TurnOn();
						if (gasController.b_leanToggle.interactable)
						{
							gasController.toggle_colors[0] = gasController.leantoggle_color.color;
							gasController.leantoggle_color.color = gasController.toggle_colors[1];
						}
						gasController.b_leanToggle.interactable = false;

						GameObject.FindGameObjectWithTag("Player").SetActive(false);
						StartCoroutine(gasController.LateTravelDataUpdate());
						Destroy(gameObject);
						StopCoroutine(RoadCreateDelay(wp));
						gasController.gameManager.LoadingScreen.SetActive(false);
						Destroy(gasController.DireactionsCustom);
						Destroy(GameObject.Find("direction waypoint entity"));
						Destroy(GameObject.Find("bubble"));

					});
				});
			}

			if (!Input.GetMouseButton(0) && Input.touchCount == 0)
			{
				var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
				_directionResource.Steps = true;
				_directions.Query(_directionResource, HandleDirectionsResponse);
			}
        }

		public IEnumerator QueryTimer()
		{
			while (true)
			{
				yield return new WaitForSeconds(UpdateFrequency);
				for (int i = 0; i < _waypoints.Length; i++)
				{
					if (_waypoints[i].position != _cachedWaypoints[i])
					{
						_recalculateNext = true;
						_cachedWaypoints[i] = _waypoints[i].position;
					}
				}

				if (_recalculateNext)
				{
					Query();
					_recalculateNext = false;
				}
			}
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

