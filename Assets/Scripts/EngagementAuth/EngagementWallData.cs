using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EngagementWallData
{
    public EngagementWallSocial socialmedia = new EngagementWallSocial();
    public EngagementWallTimer timer = new EngagementWallTimer();
    public EngagementWallApplications applications = new EngagementWallApplications();
    public EngagementWallItems items = new EngagementWallItems();
    public string[] ranks = new string[5];
    public EngagementWallTasks tasks = new EngagementWallTasks();
    public void engagementwall()
    {

    }
}