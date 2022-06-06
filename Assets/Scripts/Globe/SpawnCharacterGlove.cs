namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies;
	using Mapbox.Unity.Map;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine.UI;
	using Mapbox.CheapRulerCs;
	using Mapbox.Unity.Location;
	using System;
	using Proyecto26;
	using FullSerializer;
	using Mapbox.Examples;

	public class SpawnCharacterGlove : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		public GameObject map;

		string[] _locations = new string[1];

		[SerializeField]
		float _spawnScale = 20f;

		[SerializeField]
		GameObject _markerPrefab;

		bool started = false;

		public string[] _locations_videos;
		int num_videos = 0;

		[SerializeField]
		GameObject[] characters;

		public void Spawn()
        {
			Debug.Log(_locations[0]);
			var location = Conversions.StringToLatLon(_locations[0]);
			var earthRadius = ((IGlobeTerrainLayer)_map.Terrain).EarthRadius;
			_markerPrefab.transform.position = Conversions.GeoToWorldGlobePosition(location, earthRadius);
			_markerPrefab.transform.localScale = Vector3.one * _spawnScale;
			_markerPrefab.transform.SetParent(map.transform);
		}

		void Start()
		{
			_locations[0] = PlayerPrefs.GetFloat("loc0") + "," + PlayerPrefs.GetFloat("loc1");
			StartCoroutine(SpawnOtherCharacter());
		}

        private void Update()
        {
        }

		IEnumerator SpawnOtherCharacter()
        {
			foreach (var locationString1 in _locations_videos)
			{
				characters[num_videos].SetActive(true);
				var location = Conversions.StringToLatLon(locationString1);
				var earthRadius = ((IGlobeTerrainLayer)_map.Terrain).EarthRadius;
				var instance = characters[num_videos];
				instance.transform.position = Conversions.GeoToWorldGlobePosition(location, earthRadius);
				instance.transform.localScale = Vector3.one * _spawnScale;
				instance.transform.SetParent(map.transform);
				num_videos++;
				yield return new WaitForSeconds(1);
			}
		}

    }
}