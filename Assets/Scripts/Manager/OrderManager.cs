﻿using System.Collections.Generic;

using UnityEngine;

public class OrderManager : MonoBehaviour

{
    public static OrderManager Instance;
    [SerializeField]
    private int baseDuration;
    [SerializeField]
    [Range(1, 10)]
    private int levelToDurationMultiplier;


    // Use this for initialization

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

    }

    private void Update()
    {

    }



    public void CompletedOrder(bool success, Order order)

    {
        if (success)
        {
            AdventurerAIData data = AIManager.Instance.GetAIData(order.AIData);
            if (data != null)
                data.Inventory.AddToInventory(ItemManager.Instance.GetItemData(order.ItemID));

            GameManager.Instance.AddPlayerGold(order.GoldReward);
        }
    }

    // Filter templateList by JobTpye




    public void StartRequest(Order order, string aiName)
    {

        if (order != null)
        {
            order.AIData = aiName;
            OrderBoard.Instance.SpawnOnBoard(order);
        }
    }



    public Order GenerateOrder()
    {
        // TO DO
        Player currentPlayer = Player.Instance;

        Order newOrder = null;

        // Generate order based on job type

        int totalExperienceValue = 0;

        foreach (Job job in Player.Instance.JobList)

        {
            totalExperienceValue += job.Experience;
        }

        int randomValue = Random.Range(0, totalExperienceValue);

        totalExperienceValue = 0;

        foreach (Job job in Player.Instance.JobList)

        {

            totalExperienceValue += job.Experience;

            if (totalExperienceValue >= randomValue)

            {
                switch (job.Type)

                {
                    case JobType.ALCHEMY:

                        break;

                    case JobType.BLACKSMITH:

                        Ingot tempIngot = ItemManager.Instance.GetRandomUnlockedIngot();
                        ItemData tempIngotItemData = ItemManager.Instance.GetItemData(tempIngot.ItemID);
               
                        ItemData refData = WeaponTierManager.Instance.GetRandomWeaponInTypeClass(tempIngot.PhysicalMaterial.type);
                       
                        PhysicalMaterial currentMaterial = null;
                        CraftedItem tempCraftedItem = refData.ObjectReference.GetComponent<CraftedItem>();

                        if (tempCraftedItem)
                            currentMaterial = BlacksmithManager.Instance.GetPhysicalMaterialInfo(tempCraftedItem.GetPhysicalMaterial());

                        newOrder = new Order(((job.Level * levelToDurationMultiplier) + baseDuration)

                    ,  refData.Cost * tempIngotItemData.Cost,

                    refData.ItemID,
                    currentMaterial.type);

                        break;

                    case JobType.COMBAT:

                        break;


                }

            }
        }

        return newOrder;
    }


}