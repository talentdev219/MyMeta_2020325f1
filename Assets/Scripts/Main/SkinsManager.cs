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

public class SkinsManager : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    public static string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();

    public static EngagementWallData engagementWallData = new EngagementWallData();
    public static ApplicationsPlayerSkinData skinData = new ApplicationsPlayerSkinData();

    public Sprite[] skin_sprites, hat_sprites;

    public Image main_menu_avatar, profile_avatar, clothes_avatar, profile_button_avatar;
    public Image main_menu_hat, profile_hat, clothes_hat, profile_button_hat;

    public Material[] materials_skin;
    public GameObject[] obj_hats;

    public GameObject player_skin, player_hatmother;

    public GameObject ClothesPage;
    GameManager gameManager;
    ProfileManager profileManager;

    public GameObject[] clothes_background;
    public Color[] b_clothes_state;

    public Button b_back_clothes, b_revert_clothes;
    public Button[] b_hats, b_skins;
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        profileManager = GetComponent<ProfileManager>();
        b_back_clothes.onClick.AddListener(() => { GoBack(); gameManager.LoadingScreen.SetActive(true); profileManager.SignIn(); });
        b_revert_clothes.onClick.AddListener(() => { RevertClothes(); });
    }
    void Update()
    {
        
    }

    public void SetupGallery()
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            clothes_avatar.sprite = skin_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color];
            clothes_hat.sprite = hat_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat];

            clothes_background[0].SetActive(false);
            clothes_background[1].SetActive(false);

            if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank == 0)
            {
                clothes_background[0].SetActive(true);
                for (int i = 0; i < GameObject.FindGameObjectsWithTag("clothes_gold").Length; i++)
                {

                    GameObject.FindGameObjectsWithTag("clothes_gold")[i].GetComponentInChildren<Text>().text = "GET";
                    GameObject.FindGameObjectsWithTag("clothes_gold")[i].GetComponent<Image>().color = b_clothes_state[0];

                    GameObject.FindGameObjectsWithTag("clothes_gold")[i].GetComponent<Button>().interactable = false;
                    GameObject.FindGameObjectsWithTag("clothes_gold")[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    GameObject.FindGameObjectsWithTag("clothes_gold")[i].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Application.OpenURL("https://www.playmymeta.com/");
                    });
                }

                for (int i = 0; i < GameObject.FindGameObjectsWithTag("skins_gold").Length; i++)
                {
                    GameObject.FindGameObjectsWithTag("skins_gold")[i].GetComponent<Button>().interactable = false;
                    GameObject.FindGameObjectsWithTag("skins_gold")[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    GameObject.FindGameObjectsWithTag("skins_gold")[i].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Application.OpenURL("https://www.playmymeta.com/");
                    });
                }
            }
            if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank > 0)
            {
                clothes_background[1].SetActive(true);
            }

            for (int i = 0; i < b_hats.Length; i++)
            {
                if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat == i)
                {
                    b_hats[i].GetComponentInChildren<Text>().text = "EQUIPPED";
                    b_hats[i].GetComponent<Image>().color = b_clothes_state[2];
                    b_hats[i].GetComponent<Button>().onClick.RemoveAllListeners();
                }
                
            }
            for (int i = 0; i < b_skins.Length; i++)
            {
                if(engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color == i)
                {
                    b_skins[i].GetComponent<Button>().onClick.RemoveAllListeners();
                }
            }

            gameManager.LoadingScreen.SetActive(false);
        }).Catch(error =>
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while accessing your account information. Check your internet connection and try again";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { SetupGallery(); });
            gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
        });
    }

    public void ChooseHat(int num) 
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            ApplicationsPlayerSkinData skinDatanew = new ApplicationsPlayerSkinData();
            skinDatanew.color = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color;
            skinDatanew.shoes = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.shoes;
            skinDatanew.torso = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.torso;
            skinDatanew.hat = num;

            RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/skin.json", skinDatanew)
            .Then(res =>
            {
                Destroy(gameManager.player_hat);
                GameObject new_player_hat = obj_hats[num];
                gameManager.player_hat = Instantiate(new_player_hat, player_hatmother.transform);
                clothes_hat.sprite = hat_sprites[num];
                profile_hat.sprite = hat_sprites[num];
                main_menu_hat.sprite = hat_sprites[num];
                for (int i = 0; i < b_hats.Length; i++)
                {
                    if (b_hats[i].GetComponentInChildren<Text>().text == "EQUIPPED")
                    {
                        b_hats[i].GetComponentInChildren<Text>().text = "EQUIP";
                        b_hats[i].GetComponent<Image>().color = b_clothes_state[1];
                    }
                    if (num == i)
                    {
                        b_hats[i].GetComponentInChildren<Text>().text = "EQUIPPED";
                        b_hats[i].GetComponent<Image>().color = b_clothes_state[2];
                    }
                }
            })
            .Catch(err =>
            {
                gameManager.LoadingScreen.SetActive(false);
                gameManager.ErrorScreen.SetActive(true);
                gameManager.t_errorscreen.text = "Error while chenging your hat. Check your internet connection and try again";
                gameManager.b_errorscreen.onClick.RemoveAllListeners();
                gameManager.b_errorscreen.onClick.AddListener(() => { ChooseHat(num); });
                gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
            });
        }).Catch(error =>
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while accessing alpha applicants list #03. Check your internet connection and try again";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { SetupGallery(); });
            gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
        });
    }
    public void ChooseSkin(int num)
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            ApplicationsPlayerSkinData skinDatanew = new ApplicationsPlayerSkinData();
            skinDatanew.color = num;
            skinDatanew.shoes = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.shoes;
            skinDatanew.torso = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.torso;
            skinDatanew.hat = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat;

            RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/skin.json", skinDatanew)
            .Then(res =>
            {
                    player_skin.GetComponent<SkinnedMeshRenderer>().material = materials_skin[num];
                    clothes_avatar.sprite = skin_sprites[num];
                    profile_avatar.sprite = skin_sprites[num];
                    main_menu_avatar.sprite = skin_sprites[num];
            })
            .Catch(err =>
            {
                gameManager.LoadingScreen.SetActive(false);
                gameManager.ErrorScreen.SetActive(true);
                gameManager.t_errorscreen.text = "Error while chenging your skin. Check your internet connection and try again";
                gameManager.b_errorscreen.onClick.RemoveAllListeners();
                gameManager.b_errorscreen.onClick.AddListener(() => { ChooseSkin(num); });
                gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
            });

        }).Catch(error =>
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while accessing alpha applicants list #04. Check your internet connection and try again";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { SetupGallery(); });
            gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
        });
    }
    public void GoBack()
    {
        ClothesPage.SetActive(false);
    }
    public void RevertClothes()
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            ApplicationsPlayerSkinData skinDatanew = new ApplicationsPlayerSkinData();
            skinDatanew.color = 0;
            skinDatanew.shoes = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.shoes;
            skinDatanew.torso = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.torso;
            skinDatanew.hat = 0;
            RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/skin.json", skinDatanew)
            .Then(res =>
            {
                Destroy(gameManager.player_hat);
                GameObject new_player_hat = obj_hats[0];
                gameManager.player_hat = Instantiate(new_player_hat, player_hatmother.transform);

                clothes_hat.sprite = hat_sprites[0];
                profile_hat.sprite = hat_sprites[0];
                main_menu_hat.sprite = hat_sprites[0];

                player_skin.GetComponent<SkinnedMeshRenderer>().material = materials_skin[0];
                clothes_avatar.sprite = skin_sprites[0];
                profile_avatar.sprite = skin_sprites[0];
                main_menu_avatar.sprite = skin_sprites[0];
                for (int i = 0; i < b_hats.Length; i++)
                {
                    if (b_hats[i].GetComponentInChildren<Text>().text == "EQUIPPED")
                    {
                        b_hats[i].GetComponentInChildren<Text>().text = "EQUIP";
                        b_hats[i].GetComponent<Image>().color = b_clothes_state[1];
                    }
                    if (i == 0)
                    {
                        b_hats[i].GetComponentInChildren<Text>().text = "EQUIPPED";
                        b_hats[i].GetComponent<Image>().color = b_clothes_state[2];
                    }
                }
            })
            .Catch(err =>
            {
                gameManager.LoadingScreen.SetActive(false);
                gameManager.ErrorScreen.SetActive(true);
                gameManager.t_errorscreen.text = "Error while chenging your hat. Check your internet connection and try again";
                gameManager.b_errorscreen.onClick.RemoveAllListeners();
                gameManager.b_errorscreen.onClick.AddListener(() => { RevertClothes(); });
                gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
            });
        }).Catch(error =>
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while reverting your clothes. Check your internet connection and try again";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { RevertClothes(); });
            gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
        });
    }
}
