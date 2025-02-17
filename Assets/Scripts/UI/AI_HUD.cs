﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct HUDColorSettings
{
	public Color friendly;
	public Color hostile;
	[ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
	public Color fullHealth;
	[ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
	public Color lowHealth;


}

public class AI_HUD : MonoBehaviour {

	[SerializeField] private Text playerName;
	[SerializeField] private GameObject healthbarValueObj;
	[SerializeField] private GameObject playerIndicatorObj;
    [SerializeField] private GameObject damageIndicatorObj;
    [SerializeField]
    private Text levelText;
	[Space(10f)]
	[SerializeField] private HUDColorSettings colorSettings;
	[SerializeField] [Range(0,5)] private float lerpSpeed = 1;
	
	private Material playerIndicatorMatInstance;
	private Material healthbarMatInstance;
    private Material damagebarMatInstance;


	private AI actor = null;

	// Use this for initialization
	protected void Start()
	{
		actor = transform.parent.GetComponentInParent<AI>();

		if (actor == null)
		{
			Debug.LogError("HUD must be a child of AI! Self destructing...");
			Destroy(this);
			return;
		}

		playerName.text = actor.Data.Name;

		if (healthbarValueObj && playerIndicatorObj)
		{
			playerIndicatorMatInstance = playerIndicatorObj.GetComponent<Renderer>().material;
            healthbarMatInstance = healthbarValueObj.GetComponent<Renderer>().material;
            damagebarMatInstance = damageIndicatorObj.GetComponent<Renderer>().material;


        }
		else
		{
			Debug.LogError("Healthbar object not set, HUD prefab is likely broken. Self destructing...");
			Destroy(this);
			return;
		}
	}
	
	// Update is called once per frame
	protected void Update ()
	{
        //Set level text
        levelText.text = "Lv: " + actor.Data.GetJob(JobType.COMBAT).Level;

        // Set healthbar color
        float health01 = 1 - (float)actor.StatContainer.GetStat(Stats.StatsType.HEALTH).Percentage;

		Color currentHealthbarColor = damagebarMatInstance.GetColor("_TintColor");

		Color targetColor = Color.Lerp(currentHealthbarColor,
			Color.Lerp(colorSettings.fullHealth, colorSettings.lowHealth, health01),
			Time.deltaTime * lerpSpeed);
		targetColor.a = currentHealthbarColor.a;

        damagebarMatInstance.SetColor("_TintColor", targetColor);

        // Set healthbar length
        healthbarMatInstance.SetTextureOffset("_MainTex", new Vector2(health01 * -1, 0));

        damagebarMatInstance.SetTextureOffset("_MainTex", Vector2.Lerp(
            damagebarMatInstance.GetTextureOffset("_MainTex"),
			new Vector2(health01 * -1, 0),
			Time.deltaTime * lerpSpeed));

		// Set indicator color
		// TODO: Change color based on relationship meter (wait for cx's thing)
		//playerIndicatorMatInstance.color = Color.Lerp(playerIndicatorMatInstance.color,
		//	(actor.
		//	Time.deltaTime * lerpSpeed);


		//Debug

		
        if (Input.GetKeyDown(KeyCode.L))
        {
            actor.GainExperience(JobType.COMBAT, 10000);
        }
	}
}
