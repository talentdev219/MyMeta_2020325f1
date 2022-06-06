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
using Mapbox.Unity.Map;

public class EngagementWallMain : MonoBehaviour
{
    public GameObject map;
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    private string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();
    GameManager gameManager;

    public static int application_code = 0;

    public Button[] socials;
    public Button[] b_items;
    public GameObject[] ItemUnavailable;
    public Text[] original_price; public Text[] real_price; public Text[] discount;
    int lauch_date = 0;
    public Text days_text, hours_text, minutes_text;

    public Text iteminfo_original_price, iteminfo_real_price, iteminfo_description, iteminfo_name;
    public Button[] b_itemlink_link;
    public GameObject ShopPage;
    public GameObject iteminfoPage, iteminfo_sign, iteminfo_picture;
    public Sprite[] iteminfo_sign_sprites, iteminfo_picture_sprites;

    //public GameObject gameManager.b_shop;
    //public Image gameManager.b_icon;
    //public Color[] gameManager.b_color_state;
    //public Sprite[] gameManager.b_sprite_state;

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        gameManager.b_shop.GetComponent<Button>().onClick.AddListener(() => { OpenShop(true); });
        application_code = PlayerPrefs.GetInt("application_code");

        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
        {
            fsData walltdata = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

            lauch_date = engagementWallData.timer.launch_date;

            //engagementWallData.applications.players[engagementWallData.applications.players.Length - 1].invites = 10;

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

            //string accessitems_text = "Access Items:";
            for (int i = 0; i < engagementWallData.items.access.Length; i++)
            {
                //accessitems_text += "\n" + i + " | discount: " + engagementWallData.items.access[i].discount + ", original_price: " + engagementWallData.items.access[i].original_price + ", real_price: " + engagementWallData.items.access[i].real_price;
                original_price[i].text = "$" + engagementWallData.items.access[i].original_price.ToString();
                real_price[i].text = "$" + engagementWallData.items.access[i].real_price.ToString();
                discount[i].text = engagementWallData.items.access[i].discount.ToString() + "%";
                if (!engagementWallData.items.access[i].active)
                {
                    ItemUnavailable[i].SetActive(true);
                    b_items[i].interactable = false;
                }
            }
            //Debug.Log(accessitems_text);

            //string propitems_text = "Property Items:";
            for (int i = 3; i < engagementWallData.items.property.Length + 3; i++)
            {
                //propitems_text += "\n" + i + " | discount: " + engagementWallData.items.property[i].discount + ", original_price: " + engagementWallData.items.property[i].original_price + ", real_price: " + engagementWallData.items.property[i].real_price;
                original_price[i].text = "$" + engagementWallData.items.property[i - 3].original_price.ToString();
                real_price[i].text = "$" + engagementWallData.items.property[i - 3].real_price.ToString();
                discount[i].text = engagementWallData.items.property[i - 3].discount.ToString() + "%";
                if (!engagementWallData.items.property[i - 3].active)
                {
                    ItemUnavailable[i].SetActive(true);
                    b_items[i].interactable = false;
                }
            }
            //Debug.Log(propitems_text);

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
    }

    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }

    public void OpenItemPage(string row, int num)
    {
        RestClient.Get<EngagementWallItemList>(DatabaseURL + "/engagement_wall/items/" + row + "/" + num + ".json").Then(response =>
        {
            Debug.Log("Discount: " + response.discount + ", original_price: " + response.original_price);
            iteminfoPage.SetActive(true);
            iteminfo_original_price.text = "$" + response.original_price.ToString();
            iteminfo_real_price.text = "$" + response.real_price.ToString();
            int maxnamelength = 0;
            if (response.name.Length >= 26) maxnamelength = 26;
            if (response.name.Length < 26) maxnamelength = response.name.Length;
            iteminfo_name.text = response.name.Substring(0, maxnamelength);
            if (response.name.Length >= 26) iteminfo_name.text += "...";
            iteminfo_description.text = response.description.Replace("\\n", "\n");
            iteminfo_description.text = iteminfo_description.text.Replace("\\'", "\'");
            b_itemlink_link[0].onClick.RemoveAllListeners();
            b_itemlink_link[0].onClick.AddListener(() => { OpenLink(response.link); });
            b_itemlink_link[1].onClick.RemoveAllListeners();
            b_itemlink_link[1].onClick.AddListener(() => { OpenLink(response.link); });

            int localnum = num;
            if (row == "property") localnum += 3;
            iteminfo_picture.GetComponent<Image>().sprite = iteminfo_picture_sprites[localnum];
            iteminfo_sign.SetActive(true);
            if (row == "access" && num == 0) iteminfo_sign.GetComponent<Image>().sprite = iteminfo_sign_sprites[0];
            if (row == "access" && num == 1) iteminfo_sign.GetComponent<Image>().sprite = iteminfo_sign_sprites[2];
            if (row == "access" && num == 2) iteminfo_sign.GetComponent<Image>().sprite = iteminfo_sign_sprites[1];
            if (row == "property") iteminfo_sign.SetActive(false);


        });
    }
    public void OpenShop(bool open)
    {
        switch (open)
        {
            case true:
                map.SetActive(false);
                ShopPage.SetActive(true);
                gameManager.b_shop_icon.color = gameManager.b_color_state[1];
                gameManager.b_shop_icon.sprite = gameManager.b_shop_state[1];

                gameManager.b_profile_icon.color = gameManager.b_color_state[0];
                gameManager.b_profile_icon.sprite = gameManager.b_profile_state[0];

                gameManager.b_map_icon.color = gameManager.b_color_state[0];
                gameManager.b_map_icon.sprite = gameManager.b_map_state[0];

                gameManager.ProfilePage.SetActive(false);

                gameManager.b_shop.GetComponent<Button>().onClick.RemoveAllListeners();
                gameManager.b_shop.GetComponent<Button>().onClick.AddListener(() => { OpenShop(false); });
                break;
            case false:
                map.SetActive(true);
                ShopPage.SetActive(false);
                gameManager.b_shop_icon.color = gameManager.b_color_state[0];
                gameManager.b_shop_icon.sprite = gameManager.b_shop_state[0];

                gameManager.b_profile_icon.color = gameManager.b_color_state[0];
                gameManager.b_profile_icon.sprite = gameManager.b_profile_state[0];

                gameManager.b_map_icon.color = gameManager.b_color_state[0];
                gameManager.b_map_icon.sprite = gameManager.b_map_state[0];

                gameManager.ProfilePage.SetActive(false);

                gameManager.b_shop.GetComponent<Button>().onClick.RemoveAllListeners();
                gameManager.b_shop.GetComponent<Button>().onClick.AddListener(() => { OpenShop(true); });
                break;
        }
    }
}