using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class RealWorldTimer : MonoBehaviour
{

    public Text timer_text;

    DateTime daysLeft = DateTime.Parse("1/15/2020 12:00:01 AM");
    DateTime startDate = DateTime.Now;

    void Start()
    {
        
    }

    
    void Update()
    {
        
        TimeSpan t = daysLeft - startDate;
        string countDown = string.Format("{0}дн. {1}ч. {2}мин.", t.Days, t.Hours, t.Minutes);
        timer_text.text = countDown;
    }
}
