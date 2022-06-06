using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using UnityEngine.SceneManagement;
using Mapbox.Examples;
using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using Proyecto26;
using FullSerializer;
using Mapbox.CheapRulerCs;


public class TrackPrefab : MonoBehaviour
{

    public Vector2d position, newposition;
    public int hat, material;
    AbstractMap _map;

    public GameObject hatparent, materialparent;
    SkinsManager skinsManager;

	[Header("Character")]
	GameObject character;
	[SerializeField]
	public float characterSpeed;
	Animator characterAnimator;

	AstronautDirections directions;
	Transform startPoint;
	Transform endPoint;
	Transform _movementEndPoint;

	public bool moving;

	public GameObject empty;

	public GameObject panel, button_open, canvas;
	public Text t_username, t_rank, t_miles;
	public Image canvas_avatar, canvas_hat;

	void Start()
    {
        _map = GameObject.Find("Map").GetComponent<AbstractMap>();
        characterAnimator = GetComponentInChildren<Animator>();
        character = gameObject;
        _movementEndPoint = GameObject.Find("Target").GetComponent<Transform>();
        directions = GetComponent<AstronautDirections>();
        startPoint = Instantiate(empty, gameObject.transform).transform;
        endPoint = Instantiate(empty, gameObject.transform).transform;
    }

    public void Suit()
    {
        skinsManager = GameObject.Find("GameManager").GetComponent<SkinsManager>();
        Instantiate(skinsManager.obj_hats[hat], hatparent.transform);
        materialparent.GetComponent<SkinnedMeshRenderer>().material = skinsManager.materials_skin[material];

		panel.SetActive(false); button_open.SetActive(true);
	}

	public void GoMove()
    {
		if (moving)
        {
			moving = false;
			characterAnimator.SetBool("IsWalking", false);
		}
		startPoint.localPosition = transform.localPosition;
		endPoint.localPosition = _map.GeoToWorldPosition(newposition);
		directions.Query(GetPositions, startPoint, endPoint, _map);
		moving = true;
	}

    void Update()
    {
		canvas.transform.LookAt(canvas.transform.position + GameObject.FindGameObjectWithTag("MainCamera").transform.rotation * Vector3.forward, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation * Vector3.up);
		
		if (!moving) transform.localPosition = _map.GeoToWorldPosition(position);
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

	#region Utility

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