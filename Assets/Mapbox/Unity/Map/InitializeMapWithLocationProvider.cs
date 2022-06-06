namespace Mapbox.Unity.Map
{
	using System.Collections;
	using Mapbox.Unity.Location;
	using UnityEngine;

	public class InitializeMapWithLocationProvider : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		public Utils.Vector2d startpoint;

		ILocationProvider _locationProvider;
    
		private void Awake()
		{
			_map.InitializeOnStart = false;
			if (PlayerPrefs.GetFloat("loc0") == 0) PlayerPrefs.SetFloat("loc0", (float)startpoint.x);
			if (PlayerPrefs.GetFloat("loc1") == 0) PlayerPrefs.SetFloat("loc1", (float)startpoint.y);
		}

		protected virtual IEnumerator Start()
		{
			yield return null;
			_locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
			_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
		}

		void LocationProvider_OnLocationUpdated(Unity.Location.Location location)
		{
			_locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
			Utils.Vector2d oldlocation = new Utils.Vector2d((double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1"));
			if (oldlocation.x == 0) _map.Initialize(startpoint, (int)GameManager.moveZoom);
			if (oldlocation.x != 0) _map.Initialize(oldlocation, (int)GameManager.moveZoom);
		}
	}
}
