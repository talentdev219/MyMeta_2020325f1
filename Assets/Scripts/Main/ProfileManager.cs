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

public class ProfileManager : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    public static string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();

    public GameObject map;
    GameManager gameManager;

    public Text t_username, t_rank;
    public Text t_mmc, t_miles, t_invites, t_line, t_coupon;
    public Text[] t_clothes;
    public Text t_task_invite_amount, t_task_miles_amount, t_task_invite_prize, t_task_miles_prize;
    
    public float[] prizes_invite, prizes_miles = new float[5];
    public Image[] task_fillers;

    public float[] miles_gasvolume, miles_gasspeed;

    public Button[] b_socials;
    public Button b_coupon;

    public Image clothes_stickman, clothes_hat, clothes_torso;
    public Sprite[] sprites_hat, sprites_torso;
    public Sprite[] sprites_colors;

    public Image header_background;
    public GameObject[] header_fade;
    public Color[] header_colors;
    public GameObject ProfilePage;

    public GameObject notif_newMiles;
    public Text t_newmiles;
    string string_newmiles;

    public GameObject InvitePage;
    public Text t_invite_description, t_invite_code, t_invite_line, t_invite_invites;
    string string_invite_description;
    public Button b_invite_share;
    SkinsManager skinsManager;

    public Button b_clothesopen;

    void Start()
    {
        skinsManager = GetComponent<SkinsManager>();
        gameManager = GetComponent<GameManager>();
        gameManager.b_profile.GetComponent<Button>().onClick.AddListener(() => { ProfileButton(true); });

        string_newmiles = t_newmiles.text;
        string_invite_description = t_invite_description.text;
    }

    void Update()
    {
        
    }

    public void ProfileButton(bool open)
    {
        switch (open)
        {
            case true:
                map.SetActive(false);
                gameManager.b_profile_icon.color = gameManager.b_color_state[1];
                gameManager.b_profile_icon.sprite = gameManager.b_profile_state[1];

                gameManager.b_shop_icon.color = gameManager.b_color_state[0];
                gameManager.b_shop_icon.sprite = gameManager.b_shop_state[0];

                gameManager.b_map_icon.color = gameManager.b_color_state[0];
                gameManager.b_map_icon.sprite = gameManager.b_map_state[0];

                gameManager.ShopPage.SetActive(false);

                gameManager.b_profile.GetComponent<Button>().onClick.RemoveAllListeners();
                gameManager.b_profile.GetComponent<Button>().onClick.AddListener(() => { ProfileButton(false);});
                SignIn();
                break;
            case false:
                map.SetActive(true);
                ProfilePage.SetActive(false);
                gameManager.b_profile_icon.color = gameManager.b_color_state[0];
                gameManager.b_profile_icon.sprite = gameManager.b_profile_state[0];

                gameManager.b_shop_icon.color = gameManager.b_color_state[0];
                gameManager.b_shop_icon.sprite = gameManager.b_shop_state[0];

                gameManager.b_map_icon.color = gameManager.b_color_state[0];
                gameManager.b_map_icon.sprite = gameManager.b_map_state[0];

                gameManager.ShopPage.SetActive(false);

                gameManager.b_profile.GetComponent<Button>().onClick.RemoveAllListeners();
                gameManager.b_profile.GetComponent<Button>().onClick.AddListener(() => { ProfileButton(true);});
                break;
        }
    }

    public void SignIn()
    {
        if (PlayerPrefs.GetInt("application_code") != 0)
        {
            gameManager.LoadingScreen.SetActive(true);
            gameManager.ErrorScreen.SetActive(false);
            RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
            {
                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

                t_username.text = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.name;

                int MMC = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.MMC;
                t_mmc.text = MMC.ToString("0");
                if (MMC >= 10000) t_mmc.text = (MMC / 1000).ToString("0") + "K";
                if (MMC >= 1000000) t_mmc.text = (MMC / 1000000).ToString("0") + "M";
                int player_invites = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.invites;
                float player_miles = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles;
                t_miles.text = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles.ToString("0");
                t_invites.text = player_invites.ToString();
                t_line.text = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.line_number.ToString();
                t_rank.text = "- " + engagementWallData.ranks[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank] + " -";
                t_coupon.text = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.coupon;
                
                skinsManager.profile_avatar.sprite = skinsManager.skin_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color];
                skinsManager.profile_button_avatar.sprite = skinsManager.skin_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.color];
                
                skinsManager.profile_hat.sprite = skinsManager.hat_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat];
                skinsManager.profile_button_hat.sprite = skinsManager.hat_sprites[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].skin.hat];
                
                header_background.color = header_colors[engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank];
                if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank == 0)
                {
                    header_fade[0].SetActive(true);
                    header_fade[1].SetActive(false);
                }
                if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank != 0)
                {
                    header_fade[0].SetActive(false);
                    header_fade[1].SetActive(true);
                }

                b_coupon.onClick.RemoveAllListeners();
                b_coupon.onClick.AddListener(() => {
                    InvitePage.SetActive(true);
                    t_invite_line.text = "#" + t_line.text;
                    t_invite_invites.text = t_invites.text;
                    t_invite_code.text = t_coupon.text;
                    t_invite_description.text = string_invite_description.Replace("%players_max%", UnityEngine.Random.Range(5, 50).ToString());
                    t_invite_description.text = t_invite_description.text.Replace("%invite_percentage%", (engagementWallData.applications.stats.inviter_percentage * 100) + "%");
                    
                    string sharetext = "Hey, I just applied to the Closed Alpha of MyMeta and I want you to play with me! You should apply too and skip a part of the line with my special coupon code: " + engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.coupon;
                    if (engagementWallData.socialmedia.download_link != "") sharetext += "\n\nGo here to download it: " + engagementWallData.socialmedia.download_link;
                    b_invite_share.onClick.RemoveAllListeners();
                    b_invite_share.onClick.AddListener(() => { new NativeShare().SetSubject("Apply to MyMeta!").SetTitle("Enjoy MyMeta!").SetText(sharetext).Share(); });
                });

                b_clothesopen.onClick.RemoveAllListeners();
                b_clothesopen.onClick.AddListener(() =>
                {
                    gameManager.LoadingScreen.SetActive(false);
                    gameManager.LoadingScreen.SetActive(true);

                    skinsManager.ClothesPage.SetActive(true);
                    skinsManager.SetupGallery();
                });

                ApplicationsPlayerMainData mainData = new ApplicationsPlayerMainData();
                mainData.coupon = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.coupon;
                mainData.email = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.email;
                mainData.name = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.name;
                mainData.is_referral = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.is_referral;
                mainData.line_number = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.line_number;
                mainData.invites = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.invites;
                mainData.MMC = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.MMC;
                mainData.rank = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank;
                mainData.miles = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles;
                mainData.miles_max = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles_max;
                mainData.speed = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.speed;
                mainData.online = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.online;

                int task_invite_level = 0;
                
                for (int i = 0; i < engagementWallData.tasks.invites.Length; i++)
                {
                    if (player_invites >= engagementWallData.tasks.invites[i])
                    {
                        task_invite_level = i+1;
                    }
                }
                t_task_invite_amount.text = player_invites + " of " + engagementWallData.tasks.invites[task_invite_level];
                t_task_invite_prize.text = prizes_invite[task_invite_level] + " mil./sec";

                task_fillers[0].fillAmount = (float)player_invites / (float)engagementWallData.tasks.invites[task_invite_level];
                
                if (task_invite_level > 0)
                {
                    if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.speed < prizes_invite[task_invite_level - 1])
                    {
                        mainData.speed = prizes_invite[task_invite_level - 1];
                    }
                }

                int task_miles_level = 0;

                for (int i = 0; i < engagementWallData.tasks.miles.Length; i++)
                {
                    if (player_miles >= engagementWallData.tasks.miles[i])
                    {
                        task_miles_level = i + 1;
                    }
                }
                t_task_miles_amount.text = player_miles.ToString("0") + " of " + engagementWallData.tasks.miles[task_miles_level];
                t_task_miles_prize.text = prizes_miles[task_miles_level] + " miles";

                task_fillers[1].fillAmount = (float)player_miles / (float)engagementWallData.tasks.miles[task_miles_level];

                if (task_miles_level > 0)
                {
                    if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles_max < prizes_miles[task_miles_level - 1])
                    {
                        mainData.miles_max = prizes_miles[task_miles_level - 1];
                    }
                }
                

                if (engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles_max != miles_gasvolume[task_miles_level])
                {
                    ApplicationsPlayerMainData new_profile_main = new ApplicationsPlayerMainData();

                    new_profile_main.invites = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.invites;
                    new_profile_main.is_referral = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.is_referral;
                    new_profile_main.line_number = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.line_number;
                    new_profile_main.miles = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.miles;
                    new_profile_main.miles_max = miles_gasvolume[task_miles_level];
                    new_profile_main.MMC = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.MMC;
                    new_profile_main.name = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.name;
                    new_profile_main.rank = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.rank;
                    new_profile_main.speed = miles_gasspeed[task_miles_level];
                    new_profile_main.coupon = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.coupon;
                    new_profile_main.email = engagementWallData.applications.players[PlayerPrefs.GetInt("application_code")].main.email;


                    RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + PlayerPrefs.GetInt("application_code") + "/main.json", new_profile_main)
                    .Then(reply => {
                        notif_newMiles.SetActive(true);
                        t_newmiles.text = string_newmiles.Replace("%gasvolume%", miles_gasvolume[task_miles_level].ToString());
                        t_newmiles.text = t_newmiles.text.Replace("%speed%", miles_gasspeed[task_miles_level].ToString());
                    })
                    .Catch(err =>
                    {
                        gameManager.LoadingScreen.SetActive(false);
                        gameManager.ErrorScreen.SetActive(true);
                        gameManager.t_errorscreen.text = "Error while updating your travel limits data. Try again later";
                        gameManager.b_errorscreen.onClick.RemoveAllListeners();
                        gameManager.b_errorscreen.onClick.AddListener(() => { gameManager.ErrorScreen.SetActive(false); });
                        gameManager.GetComponentInChildren<Text>().text = "OK";
                    });
                }
                b_socials[0].onClick.RemoveAllListeners();
                b_socials[1].onClick.RemoveAllListeners();
                b_socials[2].onClick.RemoveAllListeners();
                b_socials[3].onClick.RemoveAllListeners();
                b_socials[4].onClick.RemoveAllListeners();

                b_socials[0].onClick.AddListener(() => { Application.OpenURL(engagementWallData.socialmedia.facebook); });
                b_socials[1].onClick.AddListener(() => { Application.OpenURL(engagementWallData.socialmedia.twitch); });
                b_socials[2].onClick.AddListener(() => { Application.OpenURL(engagementWallData.socialmedia.telegram); });
                b_socials[3].onClick.AddListener(() => { Application.OpenURL(engagementWallData.socialmedia.youtube); });
                b_socials[4].onClick.AddListener(() => { Application.OpenURL(engagementWallData.socialmedia.discord); });

                RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/"+PlayerPrefs.GetInt("application_code") +"/main.json", mainData);

                ProfilePage.SetActive(true);
                gameManager.LoadingScreen.SetActive(false);
            }).Catch(error =>
            {
                gameManager.LoadingScreen.SetActive(false);
                gameManager.ErrorScreen.SetActive(true);
                gameManager.t_errorscreen.text = "Error while accessing alpha applicants list #02. Check your internet connection and try again";
                gameManager.b_errorscreen.onClick.RemoveAllListeners();
                gameManager.b_errorscreen.onClick.AddListener(() => { SignIn(); });
                gameManager.b_errorscreen.GetComponentInChildren<Text>().text = "REFRESH";
            });
        }
        else
        {
            gameManager.LoadingScreen.SetActive(false);
            gameManager.ErrorScreen.SetActive(true);
            gameManager.t_errorscreen.text = "Error while trying to log you in. Please log in again";
            gameManager.b_errorscreen.onClick.RemoveAllListeners();
            gameManager.b_errorscreen.onClick.AddListener(() => { gameManager.SignOut(); });
            gameManager.GetComponentInChildren<Text>().text = "LOGOUT";
        }
    }
}
