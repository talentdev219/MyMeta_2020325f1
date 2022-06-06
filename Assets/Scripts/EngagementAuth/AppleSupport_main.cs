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

public class AppleSupport_main : MonoBehaviour
{
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    private string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static fsSerializer serializer = new fsSerializer();
    public static EngagementWallData engagementWallData = new EngagementWallData();
    public static SettingsData settingsData = new SettingsData();
    public GameObject menu_shop;
    void Start()
    {
        Checking();
    }

    public void Checking()
    {
        RestClient.Get(DatabaseURL + "/settings.json").Then(response =>
        {
            fsData settings = fsJsonParser.Parse(response.Text);
            serializer.TryDeserialize(settings, ref settingsData).AssertSuccessWithoutWarnings();

            if (!settingsData.apple_support)
            {
                menu_shop.SetActive(true);
            }
        });
    }
}