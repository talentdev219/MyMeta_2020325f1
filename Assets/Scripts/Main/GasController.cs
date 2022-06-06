using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.CheapRulerCs;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using System;
using Proyecto26;
using FullSerializer;
using Mapbox.Examples;

public class GasController : MonoBehaviour
{
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();
    public static EngagementWallPlayersSection engagementWallPlayersSection = new EngagementWallPlayersSection();

    public AstronautMouseController astronautMouseController;

    public GameManager gameManager;
    public AbstractMap map;
    public QuadTreeCameraMovement panMovement;
    public static float gasvalue, gasvolume = 500, price_decrease_minute = 0.1f, price_increase_minute = 1f, speed_persecond = 0.155f;
    public Image gasfiller;
    public Text t_amount;

    public GameObject noGasPage, whatistravelPage, b_travel, t_traveltime, ZoomSlider;
    public Text t_nogaspage;
    public static string string_nogaspage;
    public Sprite[] sprite_travelbutton;
    public Color[] redbutton_color;
    public Text redbutton_textlavel;

    public GameObject travelbubble;
    bool isBubbleShown = false, isBubbleDestroyed;
    public static bool isSetuppingTravel = false;
    public static bool isTravelling = false;

    GameObject player;

    public GameObject letstravelPage, noGasToTravel, travelStartedPage;
    public Button b_letstravel_start, b_letstravel_back, b_travelstarted_ok;
    public Text t_letstravel_time, t_letstravel_distance, t_letstravel_speed, t_letstravel_gas, t_letstravel_location;

    public GameObject CancelTravelPage;
    public Text t_travelpage_time, t_travelpage_location, t_travelpage_progress;
    public Image filler_travelpage;
    public Button b_travelpage_cancel, b_travelpage_close;

    public GameObject setupWaypointsPage, MainPage, TrackJourneyPage, b_redtravel;
    public Text b_redtravel_time;
    public Button b_redtravel_cancel;
    public GameObject Directions, DireactionsCustom;
    public GameObject Directions_prefab, DirectionsCustom_prefab;
    Mapbox.Unity.MeshGeneration.Factories.DirectionsFactoryModified directionsController;
    Mapbox.Unity.MeshGeneration.Factories.DirectionsFactoryCustom directionsCustomController;

    public GameObject TravelFinishedPage;
    public Text t_travelfinshed_gas, t_travelfinished_time;

    public GameObject JourneyCanceled;
    public Text t_journeycanceled_time, t_journeycanceled_gas;

    public Lean.Gui.LeanButton b_leanToggle;
    public Image leantoggle_color;
    public Color[] toggle_colors;

    double[] map_location;

    int movementdate_started;
    int isSavingGas;

    public ShadowScener shadowScener;

    public Text debugger;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        string_nogaspage = t_nogaspage.text;
        gasvalue = PlayerPrefs.GetFloat("gas");
        if (gasvalue > 100)
        {
            gasvalue = 100;
            PlayerPrefs.SetFloat("gas", gasvalue);
        }
        b_travel.SetActive(false);
        if (gasvalue == 0)
        {
            gasvalue = 100;
            PlayerPrefs.SetFloat("gas", gasvalue);
        }
        GetTravelData(false);
    }

    void Update()
    {
        gasfiller.fillAmount = gasvalue / 100;
        t_amount.text = gasvalue.ToString("0") + "%";

        double[] this_location = new double[2];
        this_location[0] = (double)PlayerPrefs.GetFloat("loc0"); this_location[1] = (double)PlayerPrefs.GetFloat("loc1");
        var ruler = new CheapRuler((double)PlayerPrefs.GetFloat("loc0"), CheapRulerUnits.Miles);

        map_location = new double[2];
        map_location[0] = map.CenterLatitudeLongitude.x;
        map_location[1] = map.CenterLatitudeLongitude.y;
        
        var distance = ruler.Distance(this_location, map_location);

        debugger.text = distance + "\n\n" + map_location[0] + "\n" + map_location[1];

        if (distance >= 10 && distance < 500)
        {
            if (!isBubbleShown)
            {
                b_travel.GetComponent<Image>().color = new Color(b_travel.GetComponent<Image>().color.r, b_travel.GetComponent<Image>().color.g, b_travel.GetComponent<Image>().color.b, 1f);
                b_travel.GetComponent<Button>().onClick.RemoveAllListeners();

                if (!isTravelling) b_travel.GetComponent<Button>().onClick.AddListener(() => { SetupTravelSpot(); });
                if (isTravelling) b_travel.GetComponent<Button>().onClick.AddListener(() => { GetTravelData(true); });

                isBubbleShown = true;
                if (!isBubbleDestroyed)
                {
                    travelbubble.SetActive(true);
                }
            }
        }

        if (distance <= 9)
        {
            if (isBubbleShown)
            {
                b_travel.GetComponent<Image>().color = new Color(b_travel.GetComponent<Image>().color.r, b_travel.GetComponent<Image>().color.g, b_travel.GetComponent<Image>().color.b, 0.5f);
                b_travel.GetComponent<Button>().onClick.RemoveAllListeners();

                if (!isTravelling) b_travel.GetComponent<Button>().onClick.AddListener(() => { whatistravelPage.SetActive(true); });
                if (isTravelling) b_travel.GetComponent<Button>().onClick.AddListener(() => { GetTravelData(true); });

                isBubbleShown = false;
                if (!isBubbleDestroyed)
                {
                    isBubbleDestroyed = true;
                    travelbubble.SetActive(false);
                    Destroy(travelbubble);
                }
            }
        }

        if (PlayerPrefs.GetInt("isSavingGas") == 0 && (!astronautMouseController.moving || !astronautMouseController.enabled) && !isTravelling)
        {
            PlayerPrefs.SetInt("isSavingGas", 1);
            movementdate_started = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            PlayerPrefs.SetInt("movementdate_started", movementdate_started);
        }
        if (PlayerPrefs.GetInt("isSavingGas") == 1 && astronautMouseController.moving && astronautMouseController.enabled)
        {
            PlayerPrefs.SetInt("isSavingGas", 0);
            float add_gas = (((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - PlayerPrefs.GetInt("movementdate_started")) / 60) * price_increase_minute;
            PlayerPrefs.DeleteKey("movementdate_started");
            gasvalue += (add_gas * 100) / gasvolume;
            if (gasvalue > 100) gasvalue = 100;
            PlayerPrefs.SetFloat("gas", gasvalue);
        }
        if (PlayerPrefs.GetInt("isSavingGas") == 1 && isTravelling)
        {
            PlayerPrefs.SetInt("isSavingGas", 0);
            float add_gas = (((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - PlayerPrefs.GetInt("movementdate_started")) / 60) * price_increase_minute;
            PlayerPrefs.DeleteKey("movementdate_started");
            gasvalue += (add_gas * 100) / gasvolume;
            if (gasvalue > 100) gasvalue = 100; 
            PlayerPrefs.SetFloat("gas", gasvalue);
        }
    }
    public void SetupTravelSpot()
    {
        Directions = Instantiate(Directions_prefab);
        directionsController = Directions.GetComponent<Mapbox.Unity.MeshGeneration.Factories.DirectionsFactoryModified>();
        directionsController.gasController = GetComponent<GasController>();
        if (map.GetComponent<Mapbox.Examples.QuadTreeCameraMovement>().enabled) map.GetComponent<Mapbox.Examples.QuadTreeCameraMovement>().enabled = false;
        isSetuppingTravel = true;
        b_redtravel.SetActive(true);
        setupWaypointsPage.SetActive(true);
        MainPage.SetActive(false);
        Directions.SetActive(true);
        directionsController._waypoints[0].transform.position = player.transform.position;
        directionsController._waypoints[1].transform.position = map.GeoToWorldPosition(map.CenterLatitudeLongitude);
        b_redtravel_cancel.onClick.RemoveAllListeners();
        b_redtravel_cancel.onClick.AddListener(() => {
            Destroy(Directions);
            MainPage.SetActive(true);
            setupWaypointsPage.SetActive(false);
            isSetuppingTravel = false;
            if (!map.GetComponent<Mapbox.Examples.QuadTreeCameraMovement>().enabled) map.GetComponent<Mapbox.Examples.QuadTreeCameraMovement>().enabled = true;
            directionsController._waypoints[0].transform.position = new Vector3(0f, directionsController._waypoints[0].transform.position.y, 0f);
            directionsController._waypoints[1].transform.position = new Vector3(0f, directionsController._waypoints[0].transform.position.y, 0f);
            Destroy(GameObject.Find("direction waypoint entity"));
            if (!isBubbleDestroyed) Destroy(travelbubble);
        });
    }
    public void GetTravelData(bool OpenUI)
    {
        if (PlayerPrefs.GetInt("application_code") == 99999)
        {
            isTravelling = false;
            b_travel.SetActive(true);
            b_travel.GetComponent<Button>().onClick.RemoveAllListeners();
            b_travel.GetComponent<Button>().onClick.AddListener(() => { GetTravelData(true); });
        }
        if (PlayerPrefs.GetInt("application_code") != 0)
        {
            RestClient.Get(GameManager.DatabaseURL + "/engagement_wall/applications/players/"+ PlayerPrefs.GetInt("application_code") + ".json").Then(response =>
            {
                b_travel.SetActive(true);
                b_travel.GetComponent<Button>().onClick.RemoveAllListeners();
                b_travel.GetComponent<Button>().onClick.AddListener(() => { GetTravelData(true); });

                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallPlayersSection).AssertSuccessWithoutWarnings();

                if (engagementWallPlayersSection.travel.isTravel == 0) isTravelling = false;
                if (engagementWallPlayersSection.travel.isTravel == 1) isTravelling = true;

                if (isTravelling)
                {
                    int timeleft = (int)engagementWallPlayersSection.travel.dateEnd - (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    
                    if (timeleft > 0)
                    {
                        float temp_gasamount = engagementWallPlayersSection.travel.gasStart - (((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - (int)engagementWallPlayersSection.travel.dateStart) * engagementWallPlayersSection.travel.speed);
                        gasvalue = (temp_gasamount * 100) / gasvolume;
                        PlayerPrefs.SetFloat("gas", gasvalue);
                        
                        b_travel.GetComponent<Image>().color = new Color(b_travel.GetComponent<Image>().color.r, b_travel.GetComponent<Image>().color.g, b_travel.GetComponent<Image>().color.b, 1f);
                        t_traveltime.SetActive(true);
                        t_traveltime.GetComponent<Text>().text = (timeleft / 60) + " min.";

                        if (OpenUI)
                        {
                            panMovement.enabled = false;
                            MainPage.SetActive(false);
                            TrackJourneyPage.SetActive(true);
                            b_travelpage_cancel.onClick.RemoveAllListeners();
                            b_travelpage_cancel.onClick.AddListener(() => { CancelTravelPage.SetActive(true); });
                            string latlon = engagementWallPlayersSection.travel.loc1.Split(',')[0] + "." + engagementWallPlayersSection.travel.loc1.Split(',')[1].Substring(0, 3) + ", " + engagementWallPlayersSection.travel.loc1.Split(',')[2] + "." + engagementWallPlayersSection.travel.loc1.Split(',')[3].Substring(0, 3);
                            t_travelpage_location.text = "MOVING TO\n<color=#30C5FF>" + latlon + "</color>";
                            string traveltime_temp = (((int)engagementWallPlayersSection.travel.dateEnd - (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds) / 60).ToString("0");
                            if (traveltime_temp != "0") t_travelpage_time.text = traveltime_temp + " mins";
                            if (traveltime_temp == "0") t_travelpage_time.text = "< 1 min";

                            int timespend = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - (int)engagementWallPlayersSection.travel.dateStart;
                            float percentage_reached = ((float)(timespend * engagementWallPlayersSection.travel.speed) * 100) / engagementWallPlayersSection.travel.distance;
                            if (percentage_reached > 100) percentage_reached = 100;
                            t_travelpage_progress.text = "REACHED " + percentage_reached.ToString("0") + "%";
                            filler_travelpage.fillAmount = percentage_reached / 100;

                            DireactionsCustom = Instantiate(DirectionsCustom_prefab);
                            directionsCustomController = DireactionsCustom.GetComponent<Mapbox.Unity.MeshGeneration.Factories.DirectionsFactoryCustom>();

                            double[] start_positions = new double[2];
                            start_positions[0] = double.Parse(engagementWallPlayersSection.travel.loc0.Split(',')[0] + "." + engagementWallPlayersSection.travel.loc0.Split(',')[1], System.Globalization.CultureInfo.InvariantCulture);
                            start_positions[1] = double.Parse(engagementWallPlayersSection.travel.loc0.Split(',')[2] + "." + engagementWallPlayersSection.travel.loc0.Split(',')[3], System.Globalization.CultureInfo.InvariantCulture);

                            double[] end_posititons = new double[2];
                            end_posititons[0] = double.Parse(engagementWallPlayersSection.travel.loc1.Split(',')[0] + "." + engagementWallPlayersSection.travel.loc1.Split(',')[1], System.Globalization.CultureInfo.InvariantCulture);
                            end_posititons[1] = double.Parse(engagementWallPlayersSection.travel.loc1.Split(',')[2] + "." + engagementWallPlayersSection.travel.loc1.Split(',')[3], System.Globalization.CultureInfo.InvariantCulture);

                            Mapbox.Utils.Vector2d startpos = new Mapbox.Utils.Vector2d(start_positions[0], start_positions[1]);
                            Mapbox.Utils.Vector2d endpos = new Mapbox.Utils.Vector2d(end_posititons[0], end_posititons[1]);
                            directionsCustomController.Query(startpos, endpos);

                            b_travelpage_close.onClick.RemoveAllListeners();
                            b_travelpage_close.onClick.AddListener(() => {
                                panMovement.enabled = true;
                                Destroy(DireactionsCustom);
                                MainPage.SetActive(true);
                                TrackJourneyPage.SetActive(false);
                                Destroy(GameObject.Find("direction waypoint entity"));
                            });
                        }
                        if (!OpenUI)
                        {
                            StartCoroutine(LateTravelDataUpdate());
                            b_travel.GetComponent<Button>().onClick.RemoveAllListeners();
                            b_travel.GetComponent<Button>().onClick.AddListener(() => { GetTravelData(true); });
                        }
                    }

                    if (timeleft <= 0)
                    {
                        isTravelling = false;
                        gasvalue = 0; PlayerPrefs.SetFloat("gas", gasvalue);
                        FinishJourney(false);
                        if (!b_leanToggle.interactable) leantoggle_color.color = toggle_colors[0];
                        b_leanToggle.interactable = true;
                    }
                }

            });
        }
        
    }
    public IEnumerator LateTravelDataUpdate()
    {
        gameManager.LoadingScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        gameManager.toggler.TurnOn();

        player.SetActive(false);
        yield return new WaitForSeconds(1);
        if (b_leanToggle.interactable)
        {
            toggle_colors[0] = leantoggle_color.color;
            leantoggle_color.color = toggle_colors[1];
        }
        b_leanToggle.interactable = false;
        gameManager.LoadingScreen.SetActive(false);
        StopCoroutine(LateTravelDataUpdate());
    }
    public void FinishJourney(bool isCancel)
    {
        gameManager.LoadingScreen.SetActive(true);
        RestClient.Get(GameManager.DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + ".json").Then(response =>
        {
            if (!isCancel)
            {
                t_traveltime.SetActive(false);
                t_traveltime.GetComponent<Text>().text = "0 min.";

                b_travel.SetActive(true);

                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallPlayersSection).AssertSuccessWithoutWarnings();

                isTravelling = false;

                int time = (int)engagementWallPlayersSection.travel.dateEnd - (int)engagementWallPlayersSection.travel.dateStart;
                t_travelfinished_time.text = (time / 60).ToString("0") + " mins.";

                float gas = time * engagementWallPlayersSection.travel.speed;
                t_travelfinshed_gas.text = ((gas * 100) / gasvolume).ToString("0") + " %";

                gasvalue = 100 - ((gas * 100) / gasvolume);
                PlayerPrefs.SetFloat("gas", gasvalue);

                double[] end_posititons = new double[2];

                end_posititons[0] = double.Parse(engagementWallPlayersSection.travel.loc1.Split(',')[0] + "." + engagementWallPlayersSection.travel.loc1.Split(',')[1], System.Globalization.CultureInfo.InvariantCulture);
                end_posititons[1] = double.Parse(engagementWallPlayersSection.travel.loc1.Split(',')[2] + "." + engagementWallPlayersSection.travel.loc1.Split(',')[3], System.Globalization.CultureInfo.InvariantCulture);
                Mapbox.Utils.Vector2d endpos = new Mapbox.Utils.Vector2d(end_posititons[0], end_posititons[1]);

                ApplicationsPlayerTravelData new_profile_travel = new ApplicationsPlayerTravelData();
                new_profile_travel.isTravel = 0;
                new_profile_travel.dateEnd = 0;
                new_profile_travel.dateStart = 0;
                new_profile_travel.distance = 0;
                new_profile_travel.gasStart = 0;
                new_profile_travel.speed = 0;
                new_profile_travel.loc0 = "";
                new_profile_travel.loc1 = "";

                ApplicationsPlayerMainData new_profile_main = new ApplicationsPlayerMainData();
                new_profile_main.invites = engagementWallPlayersSection.main.invites;
                new_profile_main.is_referral = engagementWallPlayersSection.main.is_referral;
                new_profile_main.line_number = engagementWallPlayersSection.main.line_number;
                new_profile_main.miles = engagementWallPlayersSection.main.miles + engagementWallPlayersSection.travel.distance;
                new_profile_main.miles_max = engagementWallPlayersSection.main.miles_max;
                new_profile_main.MMC = engagementWallPlayersSection.main.MMC;
                new_profile_main.name = engagementWallPlayersSection.main.name;
                new_profile_main.rank = engagementWallPlayersSection.main.rank;
                new_profile_main.speed = engagementWallPlayersSection.main.speed;
                new_profile_main.coupon = engagementWallPlayersSection.main.coupon;
                new_profile_main.email = engagementWallPlayersSection.main.email;

                RestClient.Put(GameManager.DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/main.json", new_profile_main).Catch(error =>
                {
                    gameManager.LoadingScreen.SetActive(false);
                    gameManager.ErrorScreen.SetActive(true);
                    gameManager.t_errorscreen.text = "Error while writing journey information #4: " + error.Message;
                    gameManager.b_errorscreen.onClick.RemoveAllListeners();
                    gameManager.b_errorscreen.onClick.AddListener(() => { FinishJourney(isCancel); });
                    gameManager.GetComponentInChildren<Text>().text = "try again";
                });

                RestClient.Put(GameManager.DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/travel.json", new_profile_travel).Then(res1 =>
                {
                    TravelFinishedPage.SetActive(true);
                    player.SetActive(true);
                    player.transform.position = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(endpos);
                    PlayerPrefs.SetFloat("loc0", (float)end_posititons[0]);
                    PlayerPrefs.SetFloat("loc1", (float)end_posititons[1]);

                    gameManager.LoadingScreen.SetActive(false);

                }).Catch(error =>
                {
                    gameManager.LoadingScreen.SetActive(false);
                    gameManager.ErrorScreen.SetActive(true);
                    gameManager.t_errorscreen.text = "Error while writing journey information #3: " + error.Message;
                    gameManager.b_errorscreen.onClick.RemoveAllListeners();
                    gameManager.b_errorscreen.onClick.AddListener(() => { FinishJourney(isCancel); });
                    gameManager.GetComponentInChildren<Text>().text = "try again";
                });
            }

            if (isCancel)
            {
                t_traveltime.SetActive(false);
                t_traveltime.GetComponent<Text>().text = "0 min.";

                b_travel.SetActive(true);

                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallPlayersSection).AssertSuccessWithoutWarnings();

                isTravelling = false;

                int time = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - (int)engagementWallPlayersSection.travel.dateStart;
                t_journeycanceled_time.text = (time / 60).ToString("0") + " mins.";

                float gas = time * engagementWallPlayersSection.travel.speed;
                t_journeycanceled_gas.text = ((gas * 100) / gasvolume).ToString("0") + " %";

                gasvalue = 100 - ((gas * 100) / gasvolume);
                PlayerPrefs.SetFloat("gas", gasvalue);

                double[] end_posititons = new double[2];
                end_posititons[0] = double.Parse(engagementWallPlayersSection.travel.loc0.Split(',')[0] + "." + engagementWallPlayersSection.travel.loc0.Split(',')[1], System.Globalization.CultureInfo.InvariantCulture);
                end_posititons[1] = double.Parse(engagementWallPlayersSection.travel.loc0.Split(',')[2] + "." + engagementWallPlayersSection.travel.loc0.Split(',')[3], System.Globalization.CultureInfo.InvariantCulture);
                Mapbox.Utils.Vector2d endpos = new Mapbox.Utils.Vector2d(end_posititons[0], end_posititons[1]);

                ApplicationsPlayerTravelData new_profile_travel = new ApplicationsPlayerTravelData();
                new_profile_travel.isTravel = 0;
                new_profile_travel.dateEnd = 0;
                new_profile_travel.dateStart = 0;
                new_profile_travel.distance = 0;
                new_profile_travel.gasStart = 0;
                new_profile_travel.speed = 0;
                new_profile_travel.loc0 = "";
                new_profile_travel.loc1 = "";

                RestClient.Put(GameManager.DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/travel.json", new_profile_travel).Then(res1 =>
                {
                    CancelTravelPage.SetActive(false);
                    JourneyCanceled.SetActive(true);
                    setupWaypointsPage.SetActive(false);
                    TrackJourneyPage.SetActive(false);
                    MainPage.SetActive(true);
                    player.SetActive(true);
                    player.transform.position = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(endpos);
                    PlayerPrefs.SetFloat("loc0", (float)end_posititons[0]);
                    PlayerPrefs.SetFloat("loc1", (float)end_posititons[1]);

                    if (!b_leanToggle.interactable)
                    {
                        leantoggle_color.color = toggle_colors[0];
                        b_leanToggle.interactable = true;
                    }
                    map.GetComponent<QuadTreeCameraMovement>().enabled = true;
                    gameManager.LoadingScreen.SetActive(false);
                    b_travel.GetComponent<Button>().onClick.RemoveAllListeners();
                    b_travel.GetComponent<Button>().onClick.AddListener(() => { SetupTravelSpot(); });
                    Destroy(GameObject.Find("direction waypoint entity"));
                    Destroy(GameObject.Find("DirectionsTracking(Clone)"));

                }).Catch(err =>
                {
                    gameManager.LoadingScreen.SetActive(false);
                    gameManager.ErrorScreen.SetActive(true);
                    gameManager.t_errorscreen.text = "Error while writing journey information #2: " + err.Message;
                    gameManager.b_errorscreen.onClick.RemoveAllListeners();
                    gameManager.b_errorscreen.onClick.AddListener(() => { FinishJourney(isCancel); });
                    gameManager.GetComponentInChildren<Text>().text = "try again";
                });
            }

        }).Catch(error =>
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while writing journey information #1: " + error.Message;
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { FinishJourney(isCancel); });
            gameManager.GetComponentInChildren<Text>().text = "try again";
        });
    }
}
