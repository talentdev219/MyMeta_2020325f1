using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class Share_Android : MonoBehaviour
{

    public Texture2D ShareImage;
    public GameObject coupon_got_panel, ilove_panel;
    public string subjectText;

    public string Appstore_link, PlayMarket_link;


    private void Start()
    {
        subjectText = "А ты в курсе, что рядом с тобой спрятан РЕАЛЬНЫЙ подарок? Скачай приложение, введи код своего друга (указан ниже) и получи 300 алмазов в Axis Q!" + "\n" + "AppStore: " + Appstore_link + "/n" + "PlayMarket" + PlayMarket_link
        + "/n" + "КОД (введи его в игре):" + Signing.localId;
    }

    public void ClickShare()
    {
        StartCoroutine(TakeSSAndShare());
        coupon_got_panel.SetActive(false);
        ilove_panel.SetActive(true);
    }

    public void ClickShare_MYID()
    {
        StartCoroutine(TakeSSAndShare());
    }

    private IEnumerator TakeSSAndShare()
    {
        yield return new WaitForEndOfFrame();

        string filePath = Path.Combine(Application.temporaryCachePath, "AxisShare.png");
        File.WriteAllBytes(filePath, ShareImage.EncodeToPNG());

        new NativeShare().AddFile(filePath).SetSubject("Смотри, это Axis Q!").SetText(subjectText).Share();

    }

    public void Close_Love()
    {
        coupon_got_panel.SetActive(false);
        ilove_panel.SetActive(true);
    }
}
