using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using System;
using UnityEngine.UI;
using UnityEngine.Serialization;
using FullSerializer;
using Unity.IO;
using UnityEngine.SceneManagement;
using System.Text;

public class EngagementWall : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    private string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();

    public static int application_code = 0;

    public Button[] socials;
    public Button[] b_items;
    public GameObject[] ItemUnavailable;
    public Text[] original_price; public Text[] real_price; public Text[] discount;
    public Text applications_amount;
    int lauch_date = 0;
    public Text days_text, hours_text, minutes_text;

    public Text iteminfo_original_price, iteminfo_real_price, iteminfo_description, iteminfo_name;
    public Button[] b_itemlink_link;
    public GameObject iteminfoPage, iteminfo_sign, iteminfo_picture;
    public Sprite[] iteminfo_sign_sprites, iteminfo_picture_sprites;

    public Button b_apply_send, b_apply_website;
    public Text apply_participants, apply_original_price, apply_real_price;

    public Text apply_notif_users, apply_notif_text;
    public Text apply_coupon_users, apply_coupon_text;
    public Button b_apply_notif, b_apply_apply;
    public Button b_apply_coupon;
    public GameObject apply_notif, applyPage;
    public GameObject applycoupon;
    public InputField apply_username, apply_email, apply_coupon;

    public GameObject ErrorApplyDataPage; public Text t_erroraplydata; public string[] erroraply_messages;

    public GameObject InvitePage;
    public GameObject b_main_apply, b_main_invite;
    public Text invite_place, invite_invites, invite_coupon, invite_description;
    public Button[] b_invite_share;
    public GameObject itempagediscount;

    string ALPHABET = "AG8FOLE2WVTCPY5ZH3NIUDBXSMQK7946";

    public string[] badwords;

    void Start()
    {
        if (PlayerPrefs.GetInt("application_code") != 0)
        {
            application_code = PlayerPrefs.GetInt("application_code");
            b_main_invite.SetActive(true);
            b_main_invite.GetComponent<Button>().onClick.AddListener(() => { OpenInvite(); });
            b_main_apply.SetActive(false);
        }
        if (PlayerPrefs.GetInt("application_code") == 0)
        {
            b_main_invite.SetActive(false);
            b_main_apply.SetActive(true);
        }

        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            lauch_date = engagementWallData.timer.launch_date;

            applications_amount.text = (engagementWallData.applications.stats.start_value + engagementWallData.applications.players.Length).ToString() + " Citizens Applied";

            socials[0].onClick.AddListener(() => { OpenLink(engagementWallData.socialmedia.facebook); });
            socials[1].onClick.AddListener(() => { OpenLink(engagementWallData.socialmedia.twitch); });
            socials[2].onClick.AddListener(() => { OpenLink(engagementWallData.socialmedia.telegram); });
            socials[3].onClick.AddListener(() => { OpenLink(engagementWallData.socialmedia.youtube); });
            socials[4].onClick.AddListener(() => { OpenLink(engagementWallData.socialmedia.discord); });

            b_items[0].onClick.AddListener(() => { OpenItemPage("access", 0); });
            b_items[1].onClick.AddListener(() => { OpenItemPage("access", 1); });
            b_items[2].onClick.AddListener(() => { OpenItemPage("access", 2); });
            b_items[3].onClick.AddListener(() => { OpenItemPage("property", 0); });
            b_items[4].onClick.AddListener(() => { OpenItemPage("property", 1); });
            b_items[5].onClick.AddListener(() => { OpenItemPage("property", 2); });
            b_items[6].onClick.AddListener(() => { OpenItemPage("property", 3); });

            b_main_apply.GetComponent<Button>().onClick.AddListener(() => { OpenApply(); });

            for (int i = 0; i < engagementWallData.items.access.Length; i++)
            {
                original_price[i].text = "$" + engagementWallData.items.access[i].original_price.ToString();
                real_price[i].text = "$" + engagementWallData.items.access[i].real_price.ToString();
                discount[i].text = engagementWallData.items.access[i].discount.ToString() + "%";
                if (!engagementWallData.items.access[i].active)
                {
                    ItemUnavailable[i].SetActive(true);
                    b_items[i].interactable = false;
                }
            }

            for (int i = 3; i < engagementWallData.items.property.Length + 3; i++)
            {
                original_price[i].text = "$" + engagementWallData.items.property[i - 3].original_price.ToString();
                real_price[i].text = "$" + engagementWallData.items.property[i - 3].real_price.ToString();
                discount[i].text = engagementWallData.items.property[i - 3].discount.ToString() + "%";
                if (!engagementWallData.items.property[i - 3].active)
                {
                    ItemUnavailable[i].SetActive(true);
                    b_items[i].interactable = false;
                }
            }

        }).Catch(error =>
        {
            Debug.Log(error);
        });
    }

    void Update()
    {
        if (lauch_date > 0)
        {
            int current_date = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            if (lauch_date > current_date)
            {
                int time_left = lauch_date - current_date;
                int days, hours, minutes;

                days = (int)((time_left / (60 * 60 * 24)));
                hours = (int)((time_left / (60 * 60)));
                minutes = (int)((time_left / 60) - (hours * 60));
                hours = hours - (days * 24);

                days_text.text = days.ToString();
                if (days < 10) days_text.text = "0" + days.ToString();
                if (days <= 0) days_text.text = "00";
                hours_text.text = hours.ToString();
                if (hours < 10) hours_text.text = "0" + hours.ToString();
                if (hours <= 0) hours_text.text = "00";
                minutes_text.text = minutes.ToString();
                if (minutes < 10) minutes_text.text = "0" + minutes.ToString();
                if (minutes <= 0) minutes_text.text = "00";
            }
        }

        if (apply_email.text.Contains("@") && apply_username.text.Length > 3)
        {
            b_apply_apply.interactable = true;
        }
        if (applyPage.activeSelf && (!apply_email.text.Contains("@") || apply_username.text.Length <= 3))
        {
            b_apply_apply.interactable = false;
        }
    }

    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }

    public void OpenItemPage(string row, int num)
    {
        RestClient.Get<EngagementWallItemList>(DatabaseURL + "/engagement_wall/items/"+row+"/"+num+".json").Then(response =>
        {
            itempagediscount.SetActive(true);
            this.GetComponent<AuthUIAnimationController>().OpenPage("main_to_items");
            iteminfo_original_price.text = "$" + response.original_price.ToString();
            if (response.original_price == 0) itempagediscount.SetActive(false);
            iteminfo_real_price.text = "$" + response.real_price.ToString();
            int maxnamelength = 0;
            if (response.name.Length >= 26) maxnamelength = 26;
            if (response.name.Length < 26) maxnamelength = response.name.Length;
            iteminfo_name.text = response.name.Substring(0, maxnamelength);
            if (response.name.Length >= 26) iteminfo_name.text += "...";
            iteminfo_description.text = response.description.Replace("\\n", "\n");
            iteminfo_description.text = iteminfo_description.text.Replace("\\'", "\'");
            b_itemlink_link[0].onClick.RemoveAllListeners();
            b_itemlink_link[0].onClick.AddListener(() => { OpenLink(response.link);});
            b_itemlink_link[1].onClick.RemoveAllListeners();
            b_itemlink_link[1].onClick.AddListener(() => { OpenLink(response.link);});

            int localnum = num;
            if (row == "property") localnum += 3;
            iteminfo_picture.GetComponent<Image>().sprite = iteminfo_picture_sprites[localnum];
            iteminfo_sign.SetActive(true);
            if (row == "access" && num == 0) iteminfo_sign.GetComponent<Image>().sprite = iteminfo_sign_sprites[0];
            if (row == "access" && num == 1) iteminfo_sign.GetComponent<Image>().sprite = iteminfo_sign_sprites[2];
            if (row == "access" && num == 2) iteminfo_sign.GetComponent<Image>().sprite = iteminfo_sign_sprites[1];
            if (row == "property") iteminfo_sign.SetActive(false);


        }).Catch(error =>
        {
            Debug.Log(error);
        });
    }

    public void OpenApply()
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();
            this.GetComponent<AuthUIAnimationController>().OpenPage("main_to_apply");
            b_apply_website.onClick.RemoveAllListeners();
            b_apply_website.onClick.AddListener(() => { OpenLink(engagementWallData.items.access[0].link); Debug.Log(engagementWallData.items.access[0].link); });
            apply_original_price.text = "$" + engagementWallData.items.access[0].original_price.ToString();
            apply_real_price.text = "$" + engagementWallData.items.access[0].real_price.ToString();

            apply_participants.text = (engagementWallData.applications.stats.real_value + engagementWallData.applications.players.Length + 1).ToString();
            b_apply_apply.onClick.RemoveAllListeners();
            b_apply_apply.onClick.AddListener(() =>
            {
                ApplyDatabase();
            });

        }).Catch(error =>
        {
            Debug.Log(error);
        });

    }

    public void ApplyDatabase()
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            bool isFake = false; bool isBadword = false;
            for (int i = 0; i < engagementWallData.applications.players.Length; i++)
            {
                if (apply_username.text.ToLower() != engagementWallData.applications.players[i].main.name.ToLower())
                {
                    if (apply_email.text.ToLower() == engagementWallData.applications.players[i].main.email.ToLower())
                    {
                        isFake = true;
                        ErrorApplyDataPage.SetActive(true);
                        t_erroraplydata.text = erroraply_messages[1];
                    }
                }

                else
                {
                    isFake = true;
                    ErrorApplyDataPage.SetActive(true);
                    t_erroraplydata.text = erroraply_messages[0];
                }
            }

            if (!isFake)
            {
                for (int i = 0; i < badwords.Length; i++)
                {
                    if (apply_username.text.ToLower().Contains(badwords[i].ToLower()))
                    {
                        isBadword = true;
                        ErrorApplyDataPage.SetActive(true);
                        t_erroraplydata.text = erroraply_messages[2];
                    }
                }

                if (!isBadword)
                {

                    int line_number = (engagementWallData.applications.stats.start_value + engagementWallData.applications.players.Length + 1);
                    int players_max = engagementWallData.applications.stats.players_limit;

                    int referral_place = (line_number) - (int)((line_number) * engagementWallData.applications.stats.referral_percentage);

                    if (((engagementWallData.applications.stats.start_value + engagementWallData.applications.players.Length) * engagementWallData.applications.stats.players_percentage) < players_max)
                    {
                        players_max = (int)(engagementWallData.applications.players.Length * engagementWallData.applications.stats.players_percentage);
                    }

                    ApplicationsPlayerMainData new_profile_main = new ApplicationsPlayerMainData();
                    ApplicationsPlayerTravelData new_profile_travel = new ApplicationsPlayerTravelData();
                    ApplicationsPlayerSkinData new_profile_skin = new ApplicationsPlayerSkinData();
                    ApplicationsPlayerOnlineData new_profile_online = new ApplicationsPlayerOnlineData();
                    EngagementWallAppStats new_stats = new EngagementWallAppStats();

                    new_stats.players_limit = engagementWallData.applications.stats.players_limit;
                    new_stats.players_percentage = engagementWallData.applications.stats.players_percentage;
                    new_stats.real_value = engagementWallData.applications.stats.real_value + 1;
                    new_stats.start_value = engagementWallData.applications.stats.start_value;
                    new_stats.referral_percentage = engagementWallData.applications.stats.referral_percentage;
                    new_stats.inviter_percentage = engagementWallData.applications.stats.inviter_percentage;

                    new_profile_main.email = apply_email.text;
                    new_profile_main.name = apply_username.text;
                    new_profile_main.is_referral = false;
                    new_profile_main.line_number = line_number;
                    new_profile_main.invites = 0;
                    new_profile_main.MMC = 0;
                    new_profile_main.rank = 0;
                    new_profile_main.miles = 0;
                    new_profile_main.miles_max = 500;
                    new_profile_main.speed = 0.155f;
                    new_profile_main.online = true;

                    new_profile_travel.isTravel = 0;
                    new_profile_travel.dateEnd = 0;
                    new_profile_travel.dateStart = 0;
                    new_profile_travel.distance = 0;
                    new_profile_travel.gasStart = 0;
                    new_profile_travel.speed = 0;
                    new_profile_travel.loc0 = "";
                    new_profile_travel.loc1 = "";

                    new_profile_skin.color = 0;
                    new_profile_skin.hat = 0;
                    new_profile_skin.torso = 0;
                    new_profile_skin.shoes = 0;

                    new_profile_online.loc0 = 0;
                    new_profile_online.loc1 = 0;
                    new_profile_online.state = "static";

                    var stringChars = new char[6];
                    var random = new System.Random();
                    for (int i = 0; i < stringChars.Length; i++)
                    {
                        stringChars[i] = ALPHABET[random.Next(ALPHABET.Length)];
                    }
                    var finalString = new String(stringChars);

                    for (int i = 0; i < engagementWallData.applications.players.Length; i++)
                    {
                        if (engagementWallData.applications.players[i].main.coupon == apply_coupon.text)
                        {
                            int inviter_place = engagementWallData.applications.players[i].main.line_number - (int)(engagementWallData.applications.players.Length * engagementWallData.applications.stats.inviter_percentage);
                            Debug.Log("invitor's place was: " + engagementWallData.applications.players[i].main.line_number + ", now it will be: " + inviter_place);
                            Debug.Log("Referaller's place was: " + line_number + ", now it will be: " + referral_place);
                            engagementWallData.applications.players[i].main.invites++;

                            new_profile_main.is_referral = true;
                            new_profile_main.line_number = referral_place;

                            for (int b = 0; b < engagementWallData.applications.players.Length; b++)
                            {
                                if (engagementWallData.applications.players[b].main.line_number > inviter_place && engagementWallData.applications.players[b].main.line_number < engagementWallData.applications.players[i].main.line_number)
                                {
                                    engagementWallData.applications.players[b].main.line_number++;
                                    RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + b + ".json", engagementWallData.applications.players[b]).Catch(err =>
                                    {
                                        Debug.Log(err);
                                    });
                                }
                            }
                            for (int b = 0; b < engagementWallData.applications.players.Length; b++)
                            {
                                if (engagementWallData.applications.players[b].main.line_number > referral_place)
                                {
                                    engagementWallData.applications.players[b].main.line_number++;
                                    RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + b + ".json", engagementWallData.applications.players[b]).Catch(err =>
                                    {
                                        Debug.Log(err);
                                    });
                                }
                            }
                            engagementWallData.applications.players[i].main.line_number = inviter_place;
                            RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + i + ".json", engagementWallData.applications.players[i]).Catch(err =>
                            {
                                Debug.Log(err);
                            });
                        }
                        if (engagementWallData.applications.players[i].main.coupon == finalString)
                        {
                            stringChars = new char[6];
                            random = new System.Random();
                            for (int b = 0; b < stringChars.Length; b++)
                            {
                                stringChars[b] = ALPHABET[random.Next(ALPHABET.Length)];
                            }
                            finalString = new String(stringChars);
                            i = 0;
                        }
                    }

                    new_profile_main.coupon = finalString;

                    RestClient.Put(DatabaseURL + "/engagement_wall/applications/stats.json", new_stats).Catch(error =>
                    {
                        Debug.Log(error);
                    });
                    RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/travel.json", new_profile_travel).Then(res1 => {
                        RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/skin.json", new_profile_skin).Then(res2 =>
                        {
                            RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/main.json", new_profile_main).Then(reply =>
                            {
                                RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/online.json", new_profile_online).Then(reply1 =>
                                {
                                    application_code = engagementWallData.applications.players.Length;
                                    PlayerPrefs.SetInt("application_code", application_code);

                                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                    dateTime = dateTime.AddSeconds((double)(lauch_date * 1000));
                                    string date = "July 1, 2022";

                                    if (!new_profile_main.is_referral)
                                    {
                                        apply_notif.SetActive(true);
                                        apply_notif_users.text = line_number.ToString();

                                        apply_notif_text.text = apply_notif_text.text.Replace("%line_number%", line_number.ToString());
                                        apply_notif_text.text = apply_notif_text.text.Replace("%date%", date);
                                        apply_notif_text.text = apply_notif_text.text.Replace("%users_amount%", UnityEngine.Random.Range(5, 50).ToString());
                                        Debug.Log(apply_notif_text.text);
                                        b_apply_notif.onClick.RemoveAllListeners();
                                        b_apply_notif.onClick.AddListener(() => { OpenInvite(); });
                                    }

                                    if (new_profile_main.is_referral)
                                    {
                                        applycoupon.SetActive(true);
                                        apply_coupon_users.text = line_number.ToString();

                                        apply_coupon_text.text = apply_coupon_text.text.Replace("%line_number%", line_number.ToString());
                                        apply_coupon_text.text = apply_coupon_text.text.Replace("%date%", date);
                                        apply_coupon_text.text = apply_coupon_text.text.Replace("%users_amount%", players_max.ToString());
                                        Debug.Log(apply_coupon_text.text);
                                        b_apply_coupon.onClick.RemoveAllListeners();
                                        b_apply_coupon.onClick.AddListener(() => { OpenInvite(); });
                                    }

                                });
                            });
                        });
                    });

                }
            }

        }).Catch(error =>
        {
            Debug.Log(error);
        });

    }

    public void OpenInvite()
    {
        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            InvitePage.SetActive(true);
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();
            Debug.Log(engagementWallData.applications.stats.inviter_percentage);

            int players_max = engagementWallData.applications.stats.players_limit;
            if (((engagementWallData.applications.stats.start_value + engagementWallData.applications.players.Length) * engagementWallData.applications.stats.players_percentage) < players_max)
            {
                players_max = (int)(engagementWallData.applications.players.Length * engagementWallData.applications.stats.players_percentage);
            }

            invite_place.text = "#" + engagementWallData.applications.players[application_code].main.line_number.ToString();
            invite_invites.text = engagementWallData.applications.players[application_code].main.invites.ToString();
            invite_coupon.text = engagementWallData.applications.players[application_code].main.coupon;
            invite_description.text = invite_description.text.Replace("%invite_percentage%", (engagementWallData.applications.stats.inviter_percentage * 100) + "%");
            invite_description.text = invite_description.text.Replace("%players_max%", UnityEngine.Random.Range(5, 50).ToString());
            Debug.Log(response.Text);
            Debug.Log(engagementWallData.applications.stats.inviter_percentage);
            Debug.Log(engagementWallData.applications.stats.players_limit);
            Debug.Log(engagementWallData.applications.stats.real_value);
            Debug.Log(engagementWallData.applications.stats.start_value);
            Debug.Log(engagementWallData.applications.stats.referral_percentage);

            b_invite_share[0].onClick.RemoveAllListeners();
            b_invite_share[1].onClick.RemoveAllListeners();
            b_invite_share[1].interactable = true;
            b_invite_share[0].interactable = true;

            string sharetext = "Hey, I just applied to the Closed Alpha of MyMeta and I want you to play with me! You should apply too and skip a part of the line with my special coupon code: " + engagementWallData.applications.players[application_code].main.coupon;
            if (engagementWallData.socialmedia.download_link != "") sharetext += "\n\nGo here to download it: " + engagementWallData.socialmedia.download_link;
            b_invite_share[0].onClick.AddListener(() => { new NativeShare().SetSubject("Apply to MyMeta!").SetTitle("Enjoy MyMeta!").SetText(sharetext).Share(); });
            b_invite_share[1].onClick.AddListener(() => { new NativeShare().SetSubject("Apply to MyMeta!").SetTitle("Enjoy MyMeta!").SetText(sharetext).Share(); });
        }).Catch(error =>
        {
            Debug.Log(error);
        });
    }

    public void TestTheMap()
    {
        PlayerPrefs.SetInt("issigned", 1);
        SceneManager.LoadScene("Main");
    }
}