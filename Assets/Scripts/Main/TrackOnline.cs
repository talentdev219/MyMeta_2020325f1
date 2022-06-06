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

public class TrackOnline : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    public static string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();
    GameManager gameManager; SkinsManager skinsManager; AbstractMap map;

    public float update_delay;
    float track_max_distance = 10;
    int track_max_players = 10;
    float track_distance_fromstart = 1;
    public InitializeMapWithLocationProvider locationProvider;
    int[] players_code = new int[10];
    GameObject[] players_spawned = new GameObject[10];

    public GameObject player_prefab, prefab_spawn_parent;
    public string hatparent_name = "mixamorig:HeadTop_End_end";
    public string materialparent_name = "Simple.Character";

    bool isVisible = true;
    bool spawned = false;

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        skinsManager = GetComponent<SkinsManager>();
        map = GameObject.Find("Map").GetComponent<AbstractMap>();
        Auth();
    }

    void Update()
    {
        if (spawned)
        {
            if (map.Zoom < 14 && isVisible)
            {
                isVisible = false;
                for (int i = 0; i < players_spawned.Length; i++)
                {
                    players_spawned[i].SetActive(false);
                }
            }
            if (map.Zoom >= 14 && !isVisible)
            {
                isVisible = true;
                for (int i = 0; i < players_spawned.Length; i++)
                {
                    players_spawned[i].SetActive(true);
                }
            }
        }
        
    }

    public void Auth()
    {
        if (PlayerPrefs.GetInt("application_code") != 0)
        {
            RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
            {
                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

                if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.online)
                {
                    int onlineplayers = 0;

                    for (int i = 0; i < engagementWallData.applications.players.Length; i++)
                    {
                        if (engagementWallData.applications.players[i].main.online && i != PlayerPrefs.GetInt("application_code")) onlineplayers++;
                    }

                    int maxplayers = engagementWallData.applications.players.Length;
                    if (maxplayers > track_max_players) maxplayers = track_max_players;
                    if (maxplayers > onlineplayers) maxplayers = onlineplayers;

                    int[] temp_array = new int[maxplayers];
                    int amount_candidates = 0;

                    for (int i = 0; i < maxplayers; i++)
                    {
                        int closest_player = 9999999;
                        double closest_distance = 9999999;

                        for (int cycle = 0; cycle < engagementWallData.applications.players.Length; cycle++)
                        {
                            if (engagementWallData.applications.players[cycle].main.online && engagementWallData.applications.players[cycle].travel.isTravel == 0 && cycle != PlayerPrefs.GetInt("application_code"))
                            {
                                var ruler = new CheapRuler(locationProvider.startpoint[0], CheapRulerUnits.Miles);
                                var distance = ruler.Distance(new double[2] { (double)locationProvider.startpoint[0], (double)locationProvider.startpoint[1] }, new double[2] { (double)engagementWallData.applications.players[cycle].online.loc0, (double)engagementWallData.applications.players[cycle].online.loc1 });
                                if (distance >= track_distance_fromstart)
                                {
                                    ruler = new CheapRuler((double)PlayerPrefs.GetFloat("loc0"), CheapRulerUnits.Miles);
                                    distance = ruler.Distance(new double[2] { (double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1") }, new double[2] { (double)engagementWallData.applications.players[cycle].online.loc0, (double)engagementWallData.applications.players[cycle].online.loc1 });
                                    if (distance <= track_max_distance)
                                    {
                                        if (distance <= closest_distance)
                                        {
                                            if (temp_array.Length > 0)
                                            {
                                                bool exists = false;
                                                for (int arraylist = 0; arraylist < temp_array.Length; arraylist++)
                                                {
                                                    if (!exists && temp_array[arraylist] == cycle)
                                                    {
                                                        exists = true;
                                                    }
                                                }

                                                if (!exists)
                                                {
                                                    closest_distance = distance;
                                                    closest_player = cycle;
                                                }
                                            }
                                            else
                                            {
                                                closest_distance = distance;
                                                closest_player = cycle;
                                            }

                                        }
                                    }
                                }
                            }

                            if (cycle == engagementWallData.applications.players.Length - 1)
                            {
                                if (closest_player != 9999999)
                                {
                                    amount_candidates++;
                                    temp_array[i] = closest_player;
                                }
                                
                            }
                        }
                    }

                    if (players_code.Length > 0)
                    {

                        players_code = new int[amount_candidates];
                        players_spawned = new GameObject[amount_candidates];

                        spawned = true;

                        for (int i = 0; i < amount_candidates; i++)
                        {
                            players_code[i] = temp_array[i];
                            players_spawned[i] = Instantiate(player_prefab);
                            players_spawned[i].GetComponent<TrackPrefab>().hat = engagementWallData.applications.players[players_code[i]].skin.hat;
                            players_spawned[i].GetComponent<TrackPrefab>().material = engagementWallData.applications.players[players_code[i]].skin.color;
                            players_spawned[i].GetComponent<TrackPrefab>().position = new Vector2d((double)engagementWallData.applications.players[players_code[i]].online.loc0, (double)engagementWallData.applications.players[players_code[i]].online.loc1);
                            
                            players_spawned[i].GetComponent<TrackPrefab>().t_username.text = engagementWallData.applications.players[players_code[i]].main.name;
                            players_spawned[i].GetComponent<TrackPrefab>().t_rank.text = engagementWallData.ranks[engagementWallData.applications.players[players_code[i]].main.rank];
                            players_spawned[i].GetComponent<TrackPrefab>().t_miles.text = engagementWallData.applications.players[players_code[i]].main.miles.ToString("0") + " miles";
                            players_spawned[i].GetComponent<TrackPrefab>().canvas_avatar.sprite = skinsManager.skin_sprites[engagementWallData.applications.players[players_code[i]].skin.color];
                            players_spawned[i].GetComponent<TrackPrefab>().canvas_hat.sprite = skinsManager.hat_sprites[engagementWallData.applications.players[players_code[i]].skin.hat];

                            players_spawned[i].GetComponent<TrackPrefab>().Suit();
                        }

                        if (amount_candidates > 0) StartCoroutine(Tracking());
                    }
                }

            }).Catch(error =>
            {
                gameManager.LoadingScreen.SetActive(false);
                gameManager.ErrorScreen.SetActive(true);
                gameManager.t_errorscreen.text = "Error while accessing alpha applicants list #05. Check your internet connection and try again";
                gameManager.b_errorscreen.onClick.RemoveAllListeners();
                gameManager.b_errorscreen.onClick.AddListener(() => { Auth(); });
                gameManager.GetComponentInChildren<Text>().text = "REFRESH";
            });
        }
        else
        {
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while trying to log you in. Please log in again";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { gameManager.SignOut(); });
            GetComponentInChildren<Text>().text = "LOGOUT";
        }
    }

    IEnumerator Tracking()
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            for (int i = 0; i < players_code.Length; i++)
            {
                Vector2d new_loco = new Vector2d((double)engagementWallData.applications.players[players_code[i]].online.loc0, (double)engagementWallData.applications.players[players_code[i]].online.loc1);
                players_spawned[i].GetComponent<TrackPrefab>().position = new_loco;
            }

        }).Catch(error =>
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while accessing online player list. Turn off online mode in settings and turn on again.";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { Auth(); });
            gameManager.GetComponentInChildren<Text>().text = "REFRESH";
            StopCoroutine(Tracking());
            
        });

        yield return new WaitForSeconds(update_delay);
        StartCoroutine(Tracking());
    }
}
