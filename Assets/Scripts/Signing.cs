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

public class Signing : MonoBehaviour
{
    public static string localemail, localpass;
    public static string localId;
    public static string idToken;
    public static string AuthKey = "AIzaSyCmOrFzn2vgKivrS3s-vVYlaMl8lvmzsIc";
    private string DatabaseURL = "https://mymeta-78aa8-default-rtdb.firebaseio.com/";
    public static EngagementWallData engagementWallData = new EngagementWallData();
    public static string username;

    UsersData usersData = new UsersData();

    public InputField emailtext, usernametext, passwordtext, emailtext2, email3text, password3text;

    public static fsSerializer serializer = new fsSerializer();

    private bool verification_send = false;
    public GameObject Sign_up_scroll, Sign_in_scroll;

    public static bool reg_fine = false;

    public Button Continue_onSignUP, continue_onSighUp2, SignInButton, ResetButton;

    public GameObject error_massage, error_email, correct_message, confirmEmail;

    private float countdown = -1f;
    public bool isSent = false;
    public Button sendmail;
    public Text sendmailtext;

    public static int isSigned = 0;

    ApplicationsPlayerMainData new_profile_main = new ApplicationsPlayerMainData();
    ApplicationsPlayerTravelData new_profile_travel = new ApplicationsPlayerTravelData();
    ApplicationsPlayerSkinData new_profile_skin = new ApplicationsPlayerSkinData();
    ApplicationsPlayerOnlineData new_profile_online = new ApplicationsPlayerOnlineData();
    EngagementWallAppStats new_stats = new EngagementWallAppStats();

    private void Awake()
    {
        idToken = PlayerPrefs.GetString("idtoken", idToken);
        localId = PlayerPrefs.GetString("localid", localId);
        username = PlayerPrefs.GetString("username", username);

        isSigned = PlayerPrefs.GetInt("issigned", isSigned);
        if (isSigned == 1) SceneManager.LoadScene("Main");
    }

    private void Start()
    {
        Continue_onSignUP.interactable = false;
        continue_onSighUp2.interactable = false;
        SignInButton.interactable = false;
        ResetButton.interactable = false;
    }

    private void Update()
    {
        if (isSent == true)
        {
            countdown -= Time.deltaTime;
            sendmail.interactable = false;
            sendmailtext.text = "Send again in " + countdown.ToString("0"); 

            if (countdown <= 0)
            {
                sendmailtext.text = "Send again";
                sendmail.interactable = true;
                countdown = 90f;
                isSent = false;
            }
        }

        if (emailtext.text != "" || usernametext.text != "")
        {
            Continue_onSignUP.interactable = true;
        }

        if (passwordtext.text != "")
        {
            continue_onSighUp2.interactable = true;
        }

        if (email3text.text != "" || password3text.text != "")
        {
            SignInButton.interactable = true;
        }

        if (emailtext2.text != "")
        {
            ResetButton.interactable = true;
        }

        if (verification_send == true)
        {
            string emailVerification = "{\"idToken\":\"" + idToken + "\"}";
            RestClient.Post(
                "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key=" + AuthKey,
                emailVerification).Then(
                emailResponse =>
                {
                    fsData emailVerificationData = fsJsonParser.Parse(emailResponse.Text);
                    EmailConfirmationInfo emailConfirmationInfo = new EmailConfirmationInfo();
                    serializer.TryDeserialize(emailVerificationData, ref emailConfirmationInfo).AssertSuccessWithoutWarnings();

                    if (emailConfirmationInfo.users[0].emailVerified)
                    {
                        RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
                        {
                            fsData walltdata = fsJsonParser.Parse(response.Text);
                            serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();
                            RestClient.Put(DatabaseURL + "/engagement_wall/applications/stats.json", new_stats);
                            RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/travel.json", new_profile_travel).Then(res1 => {
                                RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/skin.json", new_profile_skin).Then(res2 =>
                                {
                                    RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/main.json", new_profile_main).Then(reply =>
                                    {
                                        RestClient.Put(DatabaseURL + "/engagement_wall/applications/players/" + engagementWallData.applications.players.Length + "/online.json", new_profile_online).Then(reply1 =>
                                        {
                                            PlayerPrefs.SetInt("application_code", engagementWallData.applications.players.Length);
                                            verification_send = false;
                                            isSigned = 1;
                                            PlayerPrefs.SetInt("issigned", isSigned);
                                            SceneManager.LoadScene("Main");

                                        });

                                    });
                                });
                            });
                        });
                    }
                });
        }
    }

    public void PostToDatabase(bool emptyScore = false, string idTokenTemp = "")
    {
        if (idTokenTemp == "")
        {
            idTokenTemp = idToken;
        }

        if (emptyScore)
        {

            UsersProfile usersProfile = new UsersProfile();
            UsersVariables usersVariables = new UsersVariables();

            RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
            {
                fsData walltdata = fsJsonParser.Parse(response.Text);
                serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

                usersProfile.username = username;

                usersVariables.idToken = idTokenTemp;
                usersVariables.localId = localId;
                usersVariables.exp = 0;
                usersVariables.coins = 0;
                usersVariables.engagement_wall = engagementWallData.applications.players.Length;

                RestClient.Put(DatabaseURL + "/users/" + localId + "/profile.json?auth=" + idTokenTemp, usersProfile);
                RestClient.Put(DatabaseURL + "/users/" + localId + "/variables.json?auth=" + idTokenTemp, usersVariables);

                PlayerPrefs.SetString("localid", localId);
                PlayerPrefs.SetString("idtoken", idTokenTemp);

                PlayerPrefs.SetString("username", username);

                localemail = emailtext.text;
                PlayerPrefs.SetString("pass", localpass);
                localpass = passwordtext.text;
                PlayerPrefs.SetString("email", localemail);

                verification_send = true;
                error_massage.SetActive(false);

            }).Catch(err => { Debug.Log(err); });
        }
    }

    private void SignUp_User(string email, string usernametype, string password)
    {
        string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        RestClient.Post<SignResponnse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=" + AuthKey, userData).Then(
        response =>
        {

            string emailVerification = "{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"" + response.idToken + "\"}";
            RestClient.Post("https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key=" + AuthKey, emailVerification);

            idToken = response.idToken;
            localId = response.localId;
            username = usernametype;

            PostToDatabase(true, response.idToken);

            reg_fine = true;

            isSent = true;

            RestClient.Get(DatabaseURL + "/engagement_wall.json").Then(response =>
            {
                int line_number = (engagementWallData.applications.stats.start_value + engagementWallData.applications.players.Length + 1);

                new_stats.players_limit = engagementWallData.applications.stats.players_limit;
                new_stats.players_percentage = engagementWallData.applications.stats.players_percentage;
                new_stats.real_value = engagementWallData.applications.stats.real_value + 1;
                new_stats.start_value = engagementWallData.applications.stats.start_value;
                new_stats.referral_percentage = engagementWallData.applications.stats.referral_percentage;
                new_stats.inviter_percentage = engagementWallData.applications.stats.inviter_percentage;

                new_profile_main.email = email;
                new_profile_main.name = usernametype;
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
                string ALPHABET = "AG8FOLE2WVTCPY5ZH3NIUDBXSMQK7946";
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = ALPHABET[random.Next(ALPHABET.Length)];
                }
                var finalString = new String(stringChars);
                new_profile_main.coupon = finalString;

            });

        }).Catch(error =>
        {
            error_massage.SetActive(true);
            correct_message.SetActive(false);
            error_email.SetActive(false);
        });
    }

    private void SignIn_User(string email, string password)
    {
        if (email == "signup" && password == "signup")
        {
            Sign_in_scroll.SetActive(false);
            Sign_up_scroll.SetActive(true);
        }
        else
        {
            string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
            RestClient.Post<SignResponnse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + AuthKey, userData).Then(
                response =>
                {

                    string emailVerification = "{\"idToken\":\"" + response.idToken + "\"}";
                    RestClient.Post(
                        "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key=" + AuthKey,
                        emailVerification).Then(
                        emailResponse =>
                        {

                            fsData emailVerificationData = fsJsonParser.Parse(emailResponse.Text);
                            EmailConfirmationInfo emailConfirmationInfo = new EmailConfirmationInfo();
                            serializer.TryDeserialize(emailVerificationData, ref emailConfirmationInfo).AssertSuccessWithoutWarnings();

                            if (emailConfirmationInfo.users[0].emailVerified)
                            {
                                idToken = response.idToken;
                                localId = response.localId;

                                PlayerPrefs.SetString("username", username);

                                RestClient.Get<UsersData>(DatabaseURL + "/users/" + localId + ".json").Then(responses =>
                                {
                                    usersData = responses;
                                    PlayerPrefs.SetString("localid", usersData.variables.localId);
                                    PlayerPrefs.SetString("idtoken", usersData.variables.idToken);

                                    localId = usersData.variables.localId;
                                    localemail = email3text.text;
                                    PlayerPrefs.SetString("email", localemail);

                                    localpass = password3text.text;
                                    PlayerPrefs.SetString("pass", localpass);
                                    error_massage.SetActive(false);

                                    RestClient.Get(DatabaseURL + "/engagement_wall.json")
                                    .Then(reply =>
                                    {
                                        fsData walltdata = fsJsonParser.Parse(reply.Text);
                                        serializer.TryDeserialize(walltdata, ref engagementWallData).AssertSuccessWithoutWarnings();

                                        PlayerPrefs.SetInt("application_code", responses.variables.engagement_wall);

                                        isSigned = 1;
                                        PlayerPrefs.SetInt("issigned", isSigned);
                                        SceneManager.LoadScene("Main");

                                    });
                                });
                            }
                            else
                            {
                                confirmEmail.SetActive(true);
                            }
                        });

                }).Catch(error =>
                {
                    error_massage.SetActive(true);
                    correct_message.SetActive(false);
                    error_email.SetActive(false);
                });
        }
        
    }

    private void ResetPassword(string email)
    {
        string userData = "{\"requestType\":\"PASSWORD_RESET\",\"email\":\"" + email + "\"}";
        RestClient.Post<ResetPass>("https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + AuthKey, userData).Then(
            response =>
            {
                email = response.email;
                error_email.SetActive(false);
                correct_message.SetActive(true);

            }).Catch(error =>
            {
                correct_message.SetActive(false);
                error_email.SetActive(true);
                error_massage.SetActive(false);
            });
    }

    public void SendEmail()
    {
        string emailVerification = "{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"" + idToken + "\"}";
        RestClient.Post("https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key=" + AuthKey, emailVerification);
        isSent = true;
    }

    public void Reset_button()
    {
        ResetPassword(emailtext2.text);
    }

    public void SignUp_button()
    {
        SignUp_User(emailtext.text, usernametext.text, passwordtext.text);
    }

    public void SignIn_buton()
    {
        SignIn_User(email3text.text, password3text.text);
    }

    public void FromUP_to_IN()
    {
        Sign_up_scroll.SetActive(false);
        Sign_in_scroll.SetActive(true);
    }

    public void FromIN_to_UP()
    {
        Sign_up_scroll.SetActive(true);
        Sign_in_scroll.SetActive(false);
    }

    public void BackFromReset()
    {
        error_email.SetActive(false);
        correct_message.SetActive(false);
    }
}
