﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetRevenue : MonoBehaviour
{

    [SerializeField]
    private Text revenueText;
	
	// Update is called once per frame
	void Update ()
    {
        if(revenueText)
            revenueText.text = "Upcoming tax:" + Player.Instance.Tax + "\n" + "Gold after deduction:" + (Player.Instance.Gold - Player.Instance.Tax);
	}
}
