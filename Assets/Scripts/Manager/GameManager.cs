﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, ICanSerialize
{

    public static GameManager Instance;
    public static bool nightApproaching = false;
    public static bool firstGame = false;

    #region ClockRegion
    [SerializeField]
    [Range(1, 10000)]
    public float secondsPerDay;

    private GameClock gameClock;

    public GameClock GameClock
    {
        get
        {
            return gameClock;
        }
    }



    [SerializeField]
    [Range(0.25f, 1)]
    private float nightTime = .25f, dayTime = .75f;
    [SerializeField]
    [Range(0, 1)]
    private float startTime;

    #endregion



    public enum GameState
    {
        DAYMODE,
        NIGHTMODE
    }

    private GameState currentState;

    public GameState State
    {
        get
        {
            return currentState;
        }
    }



    protected void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            GameClock checkClock = (GameClock)SerializeManager.Load(SerializedFileName);
            if (checkClock != null)
            {
                gameClock = checkClock;
            }
            else
            {
                gameClock = new GameClock(secondsPerDay, startTime);
                firstGame = true;
            }

        }
        else
        {
            Debug.Log("There should only be one instance of gamemanager running");
            Destroy(this);
        }
    }


    public string SerializedFileName
    {
        get
        {
            return "GameClock";
        }
    }

    protected void Update()
    {
        GameHandle();


    }

    private void GameHandle()
    {
        if (currentState == GameState.DAYMODE)
        {
            if (gameClock.TimeOfDay > nightTime || gameClock.TimeOfDay < dayTime)
            {
                AddPlayerGold(-Player.Instance.Tax);
                Player.Instance.Tax = 0;
                PrepareForNight();
            }
        }
        if (currentState == GameState.NIGHTMODE)
        {
            if (gameClock.TimeOfDay < nightTime && gameClock.TimeOfDay > dayTime)
            {
                AIManager.Instance.InstantiateMerchant();
                currentState = GameState.DAYMODE;
                WaveManager.Instance.StopSpawn();
                if (Player.Instance.Tax != 0)
                    MessageManager.Instance.SendMail("INVOICE:" + gameClock.TimeOfDay + ":D", "The town has suffered a total of " + Player.Instance.Tax + " in damages. Please acquire the amount by the start of the following night\n\nFrom:\nSecretary of State Van Allen", null);
            }
        }

    }

    private void PrepareForNight()
    {
        currentState = GameState.NIGHTMODE;
        WaveManager.Instance.SpawnWave(gameClock.Day);
        nightApproaching = true;

    }

    public void AddTax(int value)
    {
        Player.Instance.Tax += value;
    }
    //   private void RequestBoardUpdate()
    //   {
    //  if (gameClock.SecondSinceStart > nextRequest && TownManager.Instance.CurrentTown != null)//update
    //  {
    //      nextRequest = (nextRequest + (requestConstant / TownManager.Instance.CurrentTown.Population));
    //      if (!OrderBoard.Instance.IsMaxedOut)
    //          OrderManager.Instance.NewRequest();
    //  }
    //}
    public void AddPlayerGold(int value)
    {
        Player.Instance.AddGold(value);
        if (Player.Instance.Gold < 0)
        {
            Player.Instance.Die();
        }
    }

    public void Save()
    {
        SerializeManager.Save(SerializedFileName, gameClock);
    }

    public void GetMagicRole()
    {
        Player.Instance.Data.AddJob(JobType.MAGIC);
        Player.Instance.Gold -= 5000;
    }

}