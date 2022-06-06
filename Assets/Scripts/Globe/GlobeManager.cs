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

public class GlobeManager : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    public static string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();

    public static EngagementWallData engagementWallData = new EngagementWallData();
    public static ApplicationsPlayerSkinData skinData = new ApplicationsPlayerSkinData();

    public GameObject player_skin, player_hatmother;

    public Material[] materials_skin;
    public GameObject[] obj_hats;

    public Button b_back;
    public Text t_population, t_online, t_investors;
    public GameObject investors_section, online_section;

    public GameObject LoadingScreen, ErrorScreen;
    public Text t_errorscreen;
    public Button b_errorscreen;

    Mapbox.Examples.SpawnCharacterGlove characterGlove;
    GameObject player_hat;
    public GameObject player;

    public float update_delay;
    float track_min_distance = 500;
    int track_max_players = 5;
    float track_distance_fromstart = 10;
    int[] players_code = new int[10];
    GameObject[] players_spawned = new GameObject[10];

    public GameObject player_prefab;

    public Sprite[] skin_sprites, hat_sprites;

    public Vector2d startpoint;

    private void Start()
    {
        characterGlove = GetComponent<SpawnCharacterGlove>();
        b_back.onClick.AddListener(() => { SceneManager.LoadScene("Main"); });
        Auth();
    }

    public void Auth()
    {
        LoadingScreen.SetActive(true);
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();


            t_population.text = (engagementWallData.applications.players.Length + engagementWallData.applications.stats.real_value).ToString() + " users";

            int invetstors_amount = 0;
            int online_amount = 0;

            if (engagementWallData.applications.players.Length > 1)
            {
                for (int i = 0; i < engagementWallData.applications.players.Length; i++)
                {
                    if (engagementWallData.applications.players[i].main.rank > 0) invetstors_amount++;
                    if (engagementWallData.applications.players[i].main.online) online_amount++;
                }
                if (invetstors_amount > 500)
                {
                    investors_section.SetActive(true);
                    t_investors.text = invetstors_amount + " users";
                }
            }
            if (engagementWallData.applications.players.Length <= 1)
            {
                if (engagementWallData.applications.players[0].main.rank > 0) invetstors_amount++;
                if (engagementWallData.applications.players[0].main.online) online_amount++;
            }

            player.SetActive(true);
            characterGlove.Spawn();

            player_skin.GetComponent<SkinnedMeshRenderer>().material = materials_skin[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color];

            GameObject new_player_hat = obj_hats[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat];
            if (player_hat == null) player_hat = Instantiate(new_player_hat, player_hatmother.transform);

            LoadingScreen.SetActive(false);

            if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.online)
            {
                int onlineplayers = 0;

                for (int i = 0; i < engagementWallData.applications.players.Length; i++)
                {
                    if (engagementWallData.applications.players[i].main.online) onlineplayers++;
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
                        if (engagementWallData.applications.players[cycle].main.online && engagementWallData.applications.players[cycle].travel.isTravel == 0)
                        {
                            var ruler = new CheapRuler(startpoint[0], CheapRulerUnits.Miles);
                            var distance = ruler.Distance(new double[2] { (double)startpoint[0], (double)startpoint[1] }, new double[2] { (double)engagementWallData.applications.players[cycle].online.loc0, (double)engagementWallData.applications.players[cycle].online.loc1 });
                            if (distance >= track_distance_fromstart)
                            {
                                ruler = new CheapRuler((double)PlayerPrefs.GetFloat("loc0"), CheapRulerUnits.Miles);
                                distance = ruler.Distance(new double[2] { (double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1") }, new double[2] { (double)engagementWallData.applications.players[cycle].online.loc0, (double)engagementWallData.applications.players[cycle].online.loc1 });

                                if (distance >= track_min_distance)
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

                    for (int i = 0; i < amount_candidates; i++)
                    {
                        players_code[i] = temp_array[i];
                        players_spawned[i] = Instantiate(player_prefab, GameObject.Find("Map").transform);
                        players_spawned[i].transform.localScale = new Vector3(6f, 6f, 6f);
                        players_spawned[i].GetComponent<TrackGlobePrefab>().hat = engagementWallData.applications.players[players_code[i]].skin.hat;
                        players_spawned[i].GetComponent<TrackGlobePrefab>().material = engagementWallData.applications.players[players_code[i]].skin.color;
                        players_spawned[i].GetComponent<TrackGlobePrefab>().position = new Vector2d((double)engagementWallData.applications.players[players_code[i]].online.loc0, (double)engagementWallData.applications.players[players_code[i]].online.loc1);

                        players_spawned[i].GetComponent<TrackGlobePrefab>().t_username.text = engagementWallData.applications.players[players_code[i]].main.name;
                        players_spawned[i].GetComponent<TrackGlobePrefab>().t_rank.text = engagementWallData.ranks[engagementWallData.applications.players[players_code[i]].main.rank];
                        players_spawned[i].GetComponent<TrackGlobePrefab>().t_miles.text = engagementWallData.applications.players[players_code[i]].main.miles.ToString("0") + " miles";
                        players_spawned[i].GetComponent<TrackGlobePrefab>().canvas_avatar.sprite = skin_sprites[engagementWallData.applications.players[players_code[i]].skin.color];
                        players_spawned[i].GetComponent<TrackGlobePrefab>().canvas_hat.sprite = hat_sprites[engagementWallData.applications.players[players_code[i]].skin.hat];

                        players_spawned[i].GetComponent<TrackGlobePrefab>().Suit();
                    }

                    if (amount_candidates > 0) StartCoroutine(Tracking());
                }
            }

        })
        .Catch(error =>
        {
            LoadingScreen.SetActive(false);
            ErrorScreen.SetActive(true);
            t_errorscreen.text = "Error while accessing your account information. Check your internet connection and try again";
            b_errorscreen.onClick.RemoveAllListeners();
            b_errorscreen.onClick.AddListener(() => { Auth(); });
            b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
            Debug.Log(error);
        });
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
                players_spawned[i].GetComponent<TrackGlobePrefab>().position = new_loco;
            }

        }).Catch(error =>
        {
            Debug.LogError(error);
            LoadingScreen.SetActive(false);
            ErrorScreen.SetActive(true);
            t_errorscreen.text = "Error while accessing online player list. Turn off online mode in settings and turn on again.";
            b_errorscreen.onClick.RemoveAllListeners();
            b_errorscreen.onClick.AddListener(() => { Auth(); });
            GetComponentInChildren<Text>().text = "REFRESH";
            StopCoroutine(Tracking());
        });

        yield return new WaitForSeconds(update_delay);
        StartCoroutine(Tracking());
    }
}
