﻿namespace Mapbox.Unity.Map
{
	using System.Collections;
	using Mapbox.Unity.Location;
	using UnityEngine;

	public class UpdateMapWithLocationProvider : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		ILocationProvider _locationProvider;
		Vector3 _targetPosition;
		bool _isMapInitialized = false;

		/// <summary>
		/// The time taken to move from the start to finish positions
		/// </summary>
		public float timeTakenDuringLerp = 1f;

		private bool _isLerping;

		private Vector3 _startPosition;
		private Vector3 _endPosition;

		private Utils.Vector2d _startLatLong;
		private Utils.Vector2d _endLatlong;

		private float _timeStartedLerping;

		private void Awake()
		{
			_map.InitializeOnStart = false;
		}

		IEnumerator Start()
		{
			yield return null;
			_locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
			_locationProvider.OnLocationUpdated += LocationProvider_OnFirstLocationUpdate;
		}

		void LocationProvider_OnFirstLocationUpdate(Unity.Location.Location location)
		{
			_locationProvider.OnLocationUpdated -= LocationProvider_OnFirstLocationUpdate;
			_map.OnInitialized += () =>
			{
				_isMapInitialized = true;
				_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
			};
			_map.Initialize(location.LatitudeLongitude, _map.AbsoluteZoom);
		}

		void LocationProvider_OnLocationUpdated(Unity.Location.Location location)
		{
			if (_isMapInitialized && location.IsLocationUpdated)
			{
				StartLerping(location);
			}
		}

		/// <summary>
		/// Called to begin the linear interpolation
		/// </summary>
		void StartLerping(Unity.Location.Location location)
		{
			_isLerping = true;
			_timeStartedLerping = Time.time;
			timeTakenDuringLerp = Time.deltaTime;

			_startLatLong = _map.CenterLatitudeLongitude;
			_endLatlong = location.LatitudeLongitude;
			_startPosition = _map.GeoToWorldPosition(_startLatLong, false);
			_endPosition = _map.GeoToWorldPosition(_endLatlong, false);
		}

		void LateUpdate()
		{
			if (_isMapInitialized && _isLerping)
			{
				float timeSinceStarted = Time.time - _timeStartedLerping;
				float percentageComplete = timeSinceStarted / timeTakenDuringLerp;

				_startPosition = _map.GeoToWorldPosition(_startLatLong, false);
				_endPosition = _map.GeoToWorldPosition(_endLatlong, false);
				var position = Vector3.Lerp(_startPosition, _endPosition, percentageComplete);
				var latLong = _map.WorldToGeoPosition(position);
				_map.UpdateMap(latLong, _map.Zoom);

				//When we've completed the lerp, we set _isLerping to false
				if (percentageComplete >= 1.0f)
				{
					_isLerping = false;

				}
			}
		}
	}
}