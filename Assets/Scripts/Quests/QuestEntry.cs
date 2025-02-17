﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestEntry<T> where T : Quest
{
	private bool hasStarted,isCompleted;
	private float timeToComplete;

	public QuestEntry(float _timeToComplete)
	{
		hasStarted = false;
		isCompleted = false;
        timeToComplete = _timeToComplete;
	}

    public float RemainingProgress
    {
        get
        {
            return timeToComplete;
        }
    }

	//public Session Session
	//{
	//	get
	//	{
	//		if (!isCompleted)
	//			return currentQuest.Dialog;
	//		else
	//			return currentQuest.EndDialog;
	//	}
	//}

	public bool Completed
	{
		get
		{
			return isCompleted;
		}
		set
		{
			isCompleted = value;
		}
	}

	public bool Started
	{
		get
		{
			return hasStarted;
		}
		set
		{
			hasStarted = value;
		}
	}

	

	public void StartQuest()
	{
		hasStarted = true;
	}

    public void QuestProgress()
    {
        timeToComplete--;
        if (timeToComplete <= 0)
            isCompleted = true;
    }
}

[System.Serializable]
public class QuestEntryGroup<T> where T : Quest
{
    private QuestEntry<T> currentEntry;

	private int progressionIndex;

	private JobType jobType;

	public JobType JobType
	{
		get { return this.jobType; }
	}

	public QuestEntry<T> Quest
	{
		get
		{
            return currentEntry;
		}
        set
        {
            currentEntry = value;
        }
	}

	public int ProgressionIndex
	{
		get
		{
			return progressionIndex;
		}
		set
		{
			progressionIndex = value;
		}
	}

	

	public QuestEntryGroup(JobType type)
    { 
		progressionIndex = 0;
		jobType = type;
	}
}
