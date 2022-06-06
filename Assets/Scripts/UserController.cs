using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using Proyecto26;
using FullSerializer;
using Mapbox.CheapRulerCs;
using Mapbox.Unity.Location;

public class UserController : MonoBehaviour
{
    public static UsersData usersData = new UsersData();
    public static fsSerializer serializer = new fsSerializer();

    public GameObject notification_error;
    public Text error_text;
    public Button error_button;

    private void Start()
    {
        Signing.idToken = PlayerPrefs.GetString("idtoken", Signing.idToken);
        Signing.localId = PlayerPrefs.GetString("localid", Signing.localId);
        Signing.username = PlayerPrefs.GetString("player_Name", Signing.username);
    }
}
