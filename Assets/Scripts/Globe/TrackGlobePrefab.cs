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


public class TrackGlobePrefab : MonoBehaviour
{

	public Vector2d position, newposition;
	public int hat, material;
	AbstractMap _map;

	public GameObject hatparent, materialparent;
	GlobeManager globeManager;

	public bool moving;

	public GameObject empty;

	public GameObject panel, button_open, canvas;
	public Text t_username, t_rank, t_miles;
	public Image canvas_avatar, canvas_hat;
	void Start()
	{
		_map = GameObject.Find("Map").GetComponent<AbstractMap>();
	}

	public void Suit()
	{
		globeManager = GameObject.Find("GameManager").GetComponent<GlobeManager>();
		Instantiate(globeManager.obj_hats[hat], hatparent.transform);
		materialparent.GetComponent<SkinnedMeshRenderer>().material = globeManager.materials_skin[material];

		panel.SetActive(false); button_open.SetActive(true);
	}
	void Update()
	{
		transform.LookAt(transform.position + GameObject.FindGameObjectWithTag("MainCamera").transform.rotation * Vector3.forward, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation * Vector3.up);
		if (!moving)
		{
			var earthRadius = ((IGlobeTerrainLayer)_map.Terrain).EarthRadius;
			transform.localPosition = Conversions.GeoToWorldGlobePosition(position, earthRadius);
		}
	}
}
