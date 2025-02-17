﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpellManager : MonoBehaviour {


    public static SpellManager Instance;

    [System.Serializable]
    class SpellEntry
    {
        [SerializeField] private string gestureName;
        [SerializeField] private Spell spell;
        [SerializeField] private bool isUnlocked;

        public SpellEntry()
        {
            gestureName = "";
            spell = null;
            isUnlocked = false;
        }

        public Spell _Spell
        {
            get { return this.spell; }
            set { this.spell = value; }
        }

        public string GestureName
        {
            get { return this.gestureName; }
        }

        public bool IsUnlocked
        {
            get { return this.isUnlocked; }
            set { this.isUnlocked = value; }
        }

    }


    [SerializeField] private List<SpellEntry> spellBook;

    [InspectorButton("AddNewEntry",ButtonWidth = 200)]
    public bool addNewEntry;

    private void AddNewEntry()
    {
        spellBook.Add(new SpellEntry());
    }



    private void Awake()
    {
        Instance = this;
    }


    public Spell GetSpell(string gestureName)
    {
        foreach (SpellEntry entry in spellBook)
        {
            if (entry.GestureName == gestureName && entry.IsUnlocked)
            {
                return entry._Spell;
            }
        }

        return null;


    }


}
