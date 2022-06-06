namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.Utilities;
	using System.Collections.Generic;

	public class SpawnOnMap : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		[Geocode]
		string[] _locationStrings;
		Vector2d[] _locations;

		[SerializeField]
		float _spawnScale = 100f;

		[SerializeField]
		GameObject _markerPrefab;

		List<GameObject> _spawnedObjects;

		public float ShowOnZoom;

		//спавним объекты
		void Start()
		{
			_locations = new Vector2d[_locationStrings.Length];
			_spawnedObjects = new List<GameObject>();
			for (int i = 0; i < _locationStrings.Length; i++)
			{
				var locationString = _locationStrings[i];												//_locationStrings - строка с шириной/долготой
				_locations[i] = Conversions.StringToLatLon(_locationStrings[i]);
				var instance = Instantiate(_markerPrefab);												//instance - создание префаба
				instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);        //_locations - координаты в сцене
				_markerPrefab.gameObject.tag = "diamond" + i;
				instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
				_spawnedObjects.Add(instance);
			}
		}

		//обновляем данные
		private void Update()
		{
			int count = _spawnedObjects.Count;
			for (int i = 0; i < count; i++)
			{
				var spawnedObject = _spawnedObjects[i];
				var location = _locations[i];
				spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
				spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
				if (_map.Zoom <= ShowOnZoom)
				{
					spawnedObject.SetActive(false);
				}

				if (_map.Zoom > ShowOnZoom)
				{
					spawnedObject.SetActive(true);
				}
			}
		}
	}
}
