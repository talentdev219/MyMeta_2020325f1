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

public class GameManager : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    public static string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();
    public static SettingsData settingsData = new SettingsData();
    public AbstractMap map;
    public GameObject map_object;
    public PostProcessVolume PostProc;
    public Text debugger;
    public QuadTreeCameraMovement script_panzoom;
    public AstronautMouseController script_movement;
    public GameObject player;
    public Lean.Touch.LeanTwistRotateAxis rotatorscript;
    public GameObject icon_rotation;
    public GameObject centerobj;
    public GameObject runCam, panCam;
    public Image[] icons_main_menu;
    public GameObject Search;

    [SerializeField]
    public static float moveZoom = 17f;
    public float standard_rotation, current_rotation;
    bool isbigpic = true;
    public GameObject firsttimePage;
    Vector3 playerlastpos;
    Quaternion prevcamrot;
    public GameObject slide_rotation;
    public GameObject locationprovider;
    public Lean.Gui.LeanToggle toggler;
    public ShadowScener shadowScener;

    public GameObject LoadingScreen, ErrorScreen, UpdateScreen, TechWorkScreen;
    public Text t_errorscreen; public Button b_errorscreen;
    public Button b_updatescreen;

    public GameObject b_shop, b_profile, b_map;
    public Image b_shop_icon, b_profile_icon, b_map_icon;
    public Color[] b_color_state;
    public Sprite[] b_shop_state, b_profile_state, b_map_state;

    public GameObject ProfilePage, ShopPage;
    SkinsManager skinsManager;
    public GameObject player_hat;

    public GameObject panel_space;
    public static bool isRotating = false;

    GasController gasController = new GasController();
    
    public void ResetAccount()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Auth");
    }
    private void Start()
    {
        if (PlayerPrefs.GetInt("firsttime") == 0)
        {
            PlayerPrefs.SetInt("firsttime", 1);
            firsttimePage.SetActive(true);
        }
        standard_rotation = centerobj.transform.position.y;
        current_rotation = standard_rotation;
        runCam.SetActive(true);
        panCam.SetActive(false);
        prevcamrot = runCam.transform.rotation;
        playerlastpos = player.transform.position;
        slide_rotation.GetComponent<Slider>().value = map.Zoom / 17;
        Auth();
        b_map.GetComponent<Button>().onClick.AddListener(() => { OpenMapButton(); });
        skinsManager = GetComponent<SkinsManager>();
        gasController = GetComponent<GasController>();
    }
    private void Update()
    {
        if (map.Zoom < 16.5 && isbigpic == true)
        {
            isbigpic = false;
            map.VectorData.FindFeatureSubLayerWithName("Buildings").SetActive(false);
        }
        if (map.Zoom >= 16.5 && isbigpic == false)
        {
            isbigpic = true;
            map.VectorData.GetFeatureSubLayerAtIndex(1).SetActive(true);
        }
        if (map.Zoom == 3)
        {
            panel_space.SetActive(true);
            map.UpdateMap(3.02f);
        }
    }
    public void Auth()
    {
        if (PlayerPrefs.GetInt("application_code") != 0)
        {
            ErrorScreen.SetActive(false);
            LoadingScreen.SetActive(true);
            RestClient.Get(DatabaseURL + "/engagement_wall.json")
            .Then(response =>
            {
                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

                GasController.speed_persecond = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.speed;
                GasController.gasvolume = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles_max;

                skinsManager.main_menu_avatar.sprite = skinsManager.skin_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color];
                skinsManager.main_menu_hat.sprite = skinsManager.hat_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat];

                skinsManager.player_skin.GetComponent<SkinnedMeshRenderer>().material = skinsManager.materials_skin[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color];

                GameObject new_player_hat = skinsManager.obj_hats[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat];
                if (player_hat == null) player_hat = Instantiate(new_player_hat, skinsManager.player_hatmother.transform);

                LoadingScreen.SetActive(false);

            })
            .Catch(error =>
            {
                LoadingScreen.SetActive(false);
                ErrorScreen.SetActive(true);
                t_errorscreen.text = "Error while accessing alpha applicants list #01. Check your internet connection and try again";
                b_errorscreen.onClick.RemoveAllListeners();
                b_errorscreen.onClick.AddListener(() => { Auth(); });
                GetComponentInChildren<Text>().text = "REFRESH";
            });
        }
        else
        {
            ErrorScreen.SetActive(true);
            t_errorscreen.text = "Error while trying to log you in. Please log in again";
            b_errorscreen.onClick.RemoveAllListeners();
            b_errorscreen.onClick.AddListener(() => { SignOut(); });
            GetComponentInChildren<Text>().text = "LOGOUT";
        }

        RestClient.Get(DatabaseURL + "/settings.json").Then(response =>
        {
            fsData settings = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(settings, ref settingsData).AssertSuccessWithoutWarnings();

            if (settingsData.screen_update)
            {
                UpdateScreen.SetActive(true);
                b_updatescreen.onClick.RemoveAllListeners();
                b_updatescreen.onClick.AddListener(() => {
                    if (Application.platform == RuntimePlatform.Android) Application.OpenURL(settingsData.link_android);
                    else if (Application.platform == RuntimePlatform.IPhonePlayer) Application.OpenURL(settingsData.link_ios);
                });
            }
            if (!settingsData.screen_update)
            {
                if (settingsData.screen_tech_work)
                {
                    TechWorkScreen.SetActive(true);
                }
            }

        }).Catch(error =>
        {
            LoadingScreen.SetActive(false);
            TechWorkScreen.SetActive(false);
            UpdateScreen.SetActive(false);
            ErrorScreen.SetActive(true);
            t_errorscreen.text = "Error while checking updates. Please try again later.";
            b_errorscreen.onClick.RemoveAllListeners();
            b_errorscreen.onClick.AddListener(() => { Auth(); });
            GetComponentInChildren<Text>().text = "REFRESH";
        });

    }
    public void SwitchRotation(bool isRotatingLocal)
    {
        if (isRotatingLocal)
        {
            isRotating = true;
            rotatorscript.enabled = true;
        }
        if (!isRotatingLocal)
        {
            isRotating = false;
            rotatorscript.enabled = false;
        }
    }
    public void SignOut()
    {
        PlayerPrefs.DeleteKey("loc0");
        PlayerPrefs.DeleteKey("loc1");
        PlayerPrefs.DeleteKey("idtoken");
        PlayerPrefs.DeleteKey("localid");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.SetInt("issigned", 0);
        SceneManager.LoadScene("Auth");
    }
    public void ClearDebugger()
    {
        debugger.text = "";
    }
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
    public void SwitchTaps(bool is_moving)
    {
        switch (is_moving)
        {
            case true:
                is_moving = false;
                script_panzoom.enabled = true;
                script_movement.isoff = true;
                playerlastpos = player.transform.localPosition;
                rotatorscript.enabled = true;
                prevcamrot = panCam.transform.rotation;
                panCam.SetActive(true);
                runCam.SetActive(false);
                StartCoroutine(PanCamZoomIn(0.001f, 30));
                map.Options.extentOptions.defaultExtents.cameraBoundsOptions.camera = panCam.GetComponent<Camera>();
                icon_rotation.SetActive(true);
                centerobj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                map.UpdateMap(new Vector2d((double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1")), map.Zoom);
                break;
            case false:
                is_moving = true;
                script_panzoom.enabled = false;
                script_movement.isoff = false;
                rotatorscript.enabled = false;
                centerobj.transform.rotation = Quaternion.Euler(centerobj.transform.position.x, standard_rotation, centerobj.transform.position.z);
                panCam.transform.rotation = prevcamrot;
                panCam.SetActive(false);
                runCam.SetActive(true);
                StartCoroutine(RunZoomIn(0.001f, 30));
                map.Options.extentOptions.defaultExtents.cameraBoundsOptions.camera = runCam.GetComponent<Camera>();
                centerobj.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                map.UpdateMap(new Vector2d((double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1")), moveZoom);
                if (player.activeSelf) player.transform.position = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(new Mapbox.Utils.Vector2d((double)PlayerPrefs.GetFloat("loc0"), (double)PlayerPrefs.GetFloat("loc1")));
                icon_rotation.SetActive(false);
                gasController.GetComponent<Button>().onClick.RemoveAllListeners();
                gasController.GetComponent<Button>().onClick.AddListener(() => { gasController.GetTravelData(true); });
                break;
        }
    }
    public void OpenMapButton()
    {
        map_object.SetActive(true);
        ProfilePage.SetActive(false);
        ShopPage.SetActive(false);
        b_map_icon.color = b_color_state[1];
        b_map_icon.sprite = b_map_state[1];

        b_profile_icon.color = b_color_state[0];
        b_profile_icon.sprite = b_profile_state[0];

        b_shop_icon.color = b_color_state[0];
        b_shop_icon.sprite = b_shop_state[0];
    }
    IEnumerator PanCamZoomIn(float secs, int multiplicator)
    {
        float field = panCam.GetComponent<Camera>().fieldOfView;
        panCam.GetComponent<Camera>().fieldOfView = runCam.GetComponent<Camera>().fieldOfView;
        for (int i = 0; i < multiplicator; i++)
        {
            if (panCam.GetComponent<Camera>().fieldOfView >= field) panCam.GetComponent<Camera>().fieldOfView -= ((runCam.GetComponent<Camera>().fieldOfView - field) / multiplicator);
            if (panCam.GetComponent<Camera>().fieldOfView < field) panCam.GetComponent<Camera>().fieldOfView = field;
            yield return new WaitForSeconds(secs);
        }
    }
    IEnumerator RunZoomIn(float secs, int multiplicator)
    {
        float field = runCam.GetComponent<Camera>().fieldOfView;
        runCam.GetComponent<Camera>().fieldOfView = panCam.GetComponent<Camera>().fieldOfView;
        for (int i = 0; i < multiplicator; i++)
        {
            if (runCam.GetComponent<Camera>().fieldOfView <= field) runCam.GetComponent<Camera>().fieldOfView += ((field - panCam.GetComponent<Camera>().fieldOfView) / multiplicator);
            if (panCam.GetComponent<Camera>().fieldOfView > field) runCam.GetComponent<Camera>().fieldOfView = field;
            yield return new WaitForSeconds(secs);
        }
    }
}
