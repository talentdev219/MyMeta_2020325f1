using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using UnityEngine.UI;
using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using UnityEngine.EventSystems;
using Mapbox.CheapRulerCs;
using System;

namespace Mapbox.Examples
{
	public class AstronautMouseController : MonoBehaviour
	{
		[Header("Character")]
		[SerializeField]
		GameObject character;
		[SerializeField]
		float characterSpeed;
		[SerializeField]
		Animator characterAnimator;

		[Header("References")]
		[SerializeField]
		AstronautDirections directions;
		[SerializeField]
		Transform startPoint;
		[SerializeField]
		Transform endPoint;
		[SerializeField]
		AbstractMap map;
		[SerializeField]
		GameObject rayPlane;
		[SerializeField]
		Transform _movementEndPoint;

		[SerializeField]
		LayerMask layerMask;

		Ray ray;
		RaycastHit hit;
		LayerMask raycastPlane;
		float clicktime;
		public bool moving;
		bool characterDisabled;
		//Vector3 lastposition;

		public Text debugger;
		float timer = 3;
		int touches = 0;
		public AstronautMouseController astronautMouseController;
		public AstronautDirections astronautDirections;

		public Utils.Vector2d lastposition;
		ILocationProvider _locationProvider;
		public Lean.Gui.LeanToggle leanToggle; 
		public Lean.Gui.LeanButton b_leanToggle;
		public Lean.Touch.LeanTwistRotateAxis rotatorscript;
		public Image leantoggle_color;
		public Color[] toggle_colors;
		public GameObject icon_rotation;
		public Button b_search;

		public bool isoff = false;

		float gas_decrease_timer = 0, gas_increase_timer = 0;
		bool isGasDecrease = false, isGasIncrease = true;
		public GasController gasController;

		double[] start_location = new double[2];
		double[] final_location = new double[2];

		void Start()
		{
			characterAnimator = GetComponentInChildren<Animator>();
			toggle_colors[0] = leantoggle_color.color;
			start_location[0] = (double)PlayerPrefs.GetFloat("loc0");
			start_location[1] = (double)PlayerPrefs.GetFloat("loc1");
		}

		private bool IsPointerOverUIObject()
		{
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
			eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}

		public void startscriptedMove()
		{
			startPoint.position = transform.localPosition;
			endPoint.position = new Vector3(transform.localPosition.x + 120, transform.localPosition.y, transform.localPosition.z);
			MovementEndpointControl(hit.point, true);
			directions.Query(GetPositions, startPoint, endPoint, map);
		}

		void Update()
		{
			if (isoff)
			{
				transform.localPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(new Utils.Vector2d((double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1")));
			}

			if (!isoff)
			{
				if (characterDisabled)
					return;

				CamControl();

				bool click = false;

				if (moving)
				{
					if (b_leanToggle.interactable)
					{
						toggle_colors[0] = leantoggle_color.color;
						leantoggle_color.color = toggle_colors[1];
					}
					b_search.interactable = false;
					b_leanToggle.interactable = false;

					if (GasController.gasvalue - (GasController.price_decrease_minute / 60 * gas_decrease_timer) <= 0)
					{
						//stop walking
						moving = false;
						StopAllCoroutines();
						gasController.noGasPage.SetActive(true);
						int minutes = (int)(100 / GasController.price_increase_minute);
						gasController.t_nogaspage.text = GasController.string_nogaspage.Replace("%time%", minutes.ToString() + " minutes");

						isGasDecrease = false;
						gas_decrease_timer = 0;
						if (gas_increase_timer <= 0)
						{
							isGasIncrease = true;
						}
					}

					if (gas_increase_timer > 0)
					{
						isGasIncrease = false;
						gas_increase_timer = 0;
						if (gas_decrease_timer <= 0)
						{
							isGasDecrease = true;
						}
					}

					if (isGasDecrease) gas_decrease_timer += Time.deltaTime;
					if (gas_decrease_timer >= 1)
					{
						GasController.gasvalue -= GasController.price_decrease_minute / 60 * gas_decrease_timer;
						PlayerPrefs.SetFloat("gas", GasController.gasvalue);
						gas_decrease_timer = 0;
					}
				}

				if (!moving)
				{
					b_search.interactable = true;
					b_leanToggle.interactable = true;
					leantoggle_color.color = toggle_colors[0];

					if (gas_decrease_timer > 0)
					{
						GasController.gasvalue -= GasController.price_decrease_minute / 60 * gas_decrease_timer;
						PlayerPrefs.SetFloat("gas", GasController.gasvalue);

						final_location[0] = (double)PlayerPrefs.GetFloat("loc0");
						final_location[1] = (double)PlayerPrefs.GetFloat("loc1");
						var ruler = new CheapRuler(start_location[0], CheapRulerUnits.Miles);

						isGasDecrease = false;
						gas_decrease_timer = 0;
						if (gas_increase_timer <= 0)
						{
							isGasIncrease = true;
						}
					}
				}

				if (isGasIncrease) gas_increase_timer += Time.deltaTime;

				if (Input.GetMouseButtonDown(0))
				{
					clicktime = Time.time;
				}
				if (Input.GetMouseButtonUp(0) && !IsPointerOverUIObject())
				{
					if (Time.time - clicktime < 0.15f)
					{
						click = true;
					}
				}

				if (Input.touchSupported && Input.touchCount > 0 && !IsPointerOverUIObject())
				{
					touches++;
					click = true;
					debugger.text += "\ntouch count: " + Input.touchCount;
				}

				if (click && GasController.gasvalue - (GasController.price_decrease_minute / 60 * gas_decrease_timer) > 1)
				{
					if (Input.touchCount > 0) ray = cam.ScreenPointToRay(Input.GetTouch(0).position);
					if (Input.touchCount <= 0) ray = cam.ScreenPointToRay(Input.mousePosition);

					if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
					{
						debugger.text += "\ntouch count: " + Input.touchCount;

						startPoint.position = transform.localPosition;
						endPoint.position = hit.point;
						MovementEndpointControl(hit.point, true);
						directions.Query(GetPositions, startPoint, endPoint, map);
					}
				}

				if (click && GasController.gasvalue - (GasController.price_decrease_minute / 60 * gas_decrease_timer) <= 1)
				{
					gasController.noGasPage.SetActive(true);
					int minutes = (int)(100 / GasController.price_increase_minute);
					gasController.t_nogaspage.text = GasController.string_nogaspage.Replace("%time%", minutes.ToString() + " minutes");
				}
			}
			
		}

		#region Character : Movement
		List<Vector3> futurePositions;
		bool interruption;
		void GetPositions(List<Vector3> vecs)
		{
			futurePositions = vecs;

			if (futurePositions != null && moving)
			{
				interruption = true;
			}
			if (!moving)
			{
				interruption = false;
				MoveToNextPlace();
			}
		}

		Vector3 nextPos;
		void MoveToNextPlace()
		{
			
			if (futurePositions.Count > 0)
			{
				
				nextPos = futurePositions[0];
				futurePositions.Remove(nextPos);

				moving = true;
				characterAnimator.SetBool("IsWalking", true);
				StartCoroutine(MoveTo());
			}
			else if (futurePositions.Count <= 0)
			{
				
				moving = false;
				//lastposition = Mapbox.Unity.Utilities.Conversions.;
				characterAnimator.SetBool("IsWalking", false);
			}
		}

		Vector3 prevPos;
		IEnumerator MoveTo()
		{
			prevPos = transform.localPosition;

			float time = CalculateTime();
			float t = 0;

			StartCoroutine(LookAtNextPos());

			while (t < 1 && !interruption)
			{
				t += Time.deltaTime / time;

				transform.localPosition = Vector3.Lerp(prevPos, nextPos, t);

				yield return null;
			}

			interruption = false;
			MoveToNextPlace();
		}

		float CalculateTime()
		{
			float timeToMove = 0;

			timeToMove = Vector3.Distance(prevPos, nextPos) / characterSpeed;

			return timeToMove;
		}
		#endregion

		#region Character : Rotation
		IEnumerator LookAtNextPos()
		{
			Quaternion neededRotation = Quaternion.LookRotation(nextPos - character.transform.position);
			Quaternion thisRotation = character.transform.localRotation;

			float t = 0;
			while (t < 1.0f)
			{
				t += Time.deltaTime / 0.25f;
				var rotationValue = Quaternion.Slerp(thisRotation, neededRotation, t);
				character.transform.rotation = Quaternion.Euler(0, rotationValue.eulerAngles.y, 0);
				yield return null;
			}
		}
		#endregion

		#region CameraControl
		[Header("CameraSettings")]
		[SerializeField]
		Camera cam;
		Vector3 previousPos = Vector3.zero;
		Vector3 deltaPos = Vector3.zero;

		void CamControl()
		{
			deltaPos = transform.position - previousPos;
			deltaPos.y = 0;
			cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + deltaPos, Time.time);
			previousPos = transform.position;
		}
		#endregion

		#region Utility
		public void DisableCharacter()
		{
			characterDisabled = true;
			moving = false;
			StopAllCoroutines();
			character.SetActive(false);
		}

		public void EnableCharacter()
		{
			characterDisabled = false;
			character.SetActive(true);
		}

		public void LayerChangeOn()
		{
			Debug.Log("OPEN");
		}

		public void LayerChangeOff()
		{
			Debug.Log("CLOSE");
		}

		void MovementEndpointControl(Vector3 pos, bool active)
		{
			_movementEndPoint.position = new Vector3(pos.x, 0.2f, pos.z);
			_movementEndPoint.gameObject.SetActive(active);
		}
		#endregion
	}
}
