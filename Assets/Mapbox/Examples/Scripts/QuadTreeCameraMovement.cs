namespace Mapbox.Examples
{
	using Mapbox.Unity.Map;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using System;
	using System.Collections.Generic;
	using UnityEngine.UI;

	public class QuadTreeCameraMovement : MonoBehaviour
	{
		[SerializeField]
		[Range(11, 20)]
		public float _panSpeed = 1.0f;

		[SerializeField]
		float _zoomSpeed = 0.25f;

		[SerializeField]
		float x_factor = 1.5f;

		[SerializeField]
		public Camera _referenceCamera;

		[SerializeField]
		AbstractMap _mapManager;

		[SerializeField]
		bool _useDegreeMethod;

		private Vector3 _origin;
		private Vector3 _mousePosition;
		private Vector3 _mousePositionPrevious;
		private bool _shouldDrag;
		private bool _isInitialized = false;
		private Plane _groundPlane = new Plane(Vector3.up, 0);
		private bool _dragStartedOnUI = false;

		private float zoomdelay_start = 0.2f;
		private float zoomdelay = 0f;
		public Text debugger;

		private bool IsPointerOverUIObject()
		{
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
			eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}

		void Awake()
		{
			if (null == _referenceCamera)
			{
				_referenceCamera = GetComponent<Camera>();
				if (null == _referenceCamera) { Debug.LogErrorFormat("{0}: reference camera not set", this.GetType().Name); }
			}
			_mapManager.OnInitialized += () =>
			{
				_isInitialized = true;
			};
		}

		public void Update()
		{
			if (zoomdelay > 0)
			{
				zoomdelay -= Time.deltaTime;
			}

			if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
			{
				_dragStartedOnUI = true;
			}

			if (Input.GetMouseButtonUp(0))
			{
				_dragStartedOnUI = false;
			}

			
		}


		private void LateUpdate()
		{
			if (!_isInitialized) { return; }

			if (!_dragStartedOnUI)
			{
				if (Input.touchSupported && Input.touchCount > 0)
				{
					HandleTouch();
				}
				else
				{
					HandleMouseAndKeyBoard();
				}
			}
		}

		void HandleMouseAndKeyBoard()
		{
			float scrollDelta = 0.0f;
			scrollDelta = Input.GetAxis("Mouse ScrollWheel");
			ZoomMapUsingTouchOrMouse(scrollDelta);

			float xMove = Input.GetAxis("Horizontal");
			float zMove = Input.GetAxis("Vertical");

			PanMapUsingKeyBoard(xMove, zMove);

			PanMapUsingTouchOrMouse();
		}

		void HandleTouch()
		{
			float zoomFactor = 0.0f;
			if (!IsPointerOverUIObject())
			{
				debugger.text = (Input.touchCount).ToString();
				switch (Input.touchCount)
				{
					case 1:
						{
							if (zoomdelay <= 0)
							{
								PanMapUsingTouchOrMouse();
							}
						}
						break;
					case 2:
						{
							Touch touchZero = Input.GetTouch(0);
							Touch touchOne = Input.GetTouch(1);

							Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
							Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

							float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
							float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

							zoomFactor = 0.01f * (touchDeltaMag - prevTouchDeltaMag);
							zoomdelay = zoomdelay_start;
						}
						ZoomMapUsingTouchOrMouse(zoomFactor);
						break;
					case 3:
						{
							zoomdelay = zoomdelay_start;
						}
						break;
					default:
						break;
				}
			}
			
		}

		void ZoomMapUsingTouchOrMouse(float zoomFactor)
		{
			if (!GasController.isSetuppingTravel)
			{
				var zoom = Mathf.Max(3f, Mathf.Min(_mapManager.Zoom + zoomFactor * _zoomSpeed, 17f)); //11-17
				if (Math.Abs(zoom - _mapManager.Zoom) > 0.0f)
				{
					_mapManager.UpdateMap(_mapManager.CenterLatitudeLongitude, zoom);
				}
			}
			
		}

		void PanMapUsingKeyBoard(float xMove, float zMove)
		{
			if (Math.Abs(xMove) > 0.0f || Math.Abs(zMove) > 0.0f)
			{
				float factor = _panSpeed * (Conversions.GetTileScaleInDegrees((float)_mapManager.CenterLatitudeLongitude.x, _mapManager.AbsoluteZoom));

				var latitudeLongitude = new Vector2d(_mapManager.CenterLatitudeLongitude.x + zMove * factor * 2.0f, _mapManager.CenterLatitudeLongitude.y + xMove * factor * 4.0f);

				_mapManager.UpdateMap(latitudeLongitude, _mapManager.Zoom);
			}
		}

		void PanMapUsingTouchOrMouse()
		{
			if (_useDegreeMethod)
			{
				UseDegreeConversion();
			}
			else
			{
				UseMeterConversion();
			}
		}

		void UseMeterConversion()
		{
			if (Input.GetMouseButtonUp(1))
			{
				var mousePosScreen = Input.mousePosition;
				mousePosScreen.z = _referenceCamera.transform.localPosition.y;
				var pos = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

				var latlongDelta = _mapManager.WorldToGeoPosition(pos);
			}

			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				var mousePosScreen = Input.mousePosition;
				mousePosScreen.z = _referenceCamera.transform.localPosition.y;
				_mousePosition = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

				if (_shouldDrag == false)
				{
					_shouldDrag = true;
					_origin = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
				}
			}
			else
			{
				_shouldDrag = false;
			}

			if (_shouldDrag == true)
			{
				var changeFromPreviousPosition = _mousePositionPrevious - _mousePosition;
				if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
				{
					_mousePositionPrevious = _mousePosition;
					var offset = _origin - _mousePosition;

					if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
					{
						if (null != _mapManager)
						{
							float factor = _panSpeed * Conversions.GetTileScaleInMeters((float)0, _mapManager.AbsoluteZoom) / _mapManager.UnityTileSize;
							var latlongDelta = Conversions.MetersToLatLon(new Vector2d(offset.x * factor, offset.z * factor));
							var newLatLong = _mapManager.CenterLatitudeLongitude + latlongDelta;

							_mapManager.UpdateMap(newLatLong, _mapManager.Zoom);
						}
					}
					_origin = _mousePosition;
				}
				else
				{
					if (EventSystem.current.IsPointerOverGameObject())
					{
						return;
					}
					_mousePositionPrevious = _mousePosition;
					_origin = _mousePosition;
				}
			}
		}

		void UseDegreeConversion()
		{
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				var mousePosScreen = Input.mousePosition;
				mousePosScreen.z = _referenceCamera.transform.localPosition.y * 4;
				_mousePosition = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

				if (_shouldDrag == false)
				{
					_shouldDrag = true;
					_origin = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
				}
			}
			else
			{
				_shouldDrag = false;
			}

			if (_shouldDrag == true)
			{
				var changeFromPreviousPosition = _mousePositionPrevious - _mousePosition;
				if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
				{
					_mousePositionPrevious = _mousePosition;
					var offset = _origin - _mousePosition;

					if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
					{
						if (null != _mapManager)
						{
							float factor = _panSpeed * Conversions.GetTileScaleInDegrees((float)_mapManager.CenterLatitudeLongitude.x, _mapManager.AbsoluteZoom) / _mapManager.UnityTileSize;
							var latitudeLongitude = new Vector2d(_mapManager.CenterLatitudeLongitude.x + offset.z * factor * x_factor, _mapManager.CenterLatitudeLongitude.y + offset.x * factor);
							_mapManager.UpdateMap(latitudeLongitude, _mapManager.Zoom);
						}
					}
					_origin = _mousePosition;
				}
				else
				{
					if (EventSystem.current.IsPointerOverGameObject())
					{
						return;
					}
					_mousePositionPrevious = _mousePosition;
					_origin = _mousePosition;
				}
			}
		}

		private Vector3 getGroundPlaneHitPoint(Ray ray)
		{
			float distance;
			if (!_groundPlane.Raycast(ray, out distance)) { return Vector3.zero; }
			return ray.GetPoint(distance);
		}
	}
}
