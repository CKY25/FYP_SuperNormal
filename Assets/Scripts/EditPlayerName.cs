using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class EditPlayerName : MonoBehaviour
{
    public static EditPlayerName Instance { get; private set; }

    [SerializeField] private TMP_InputField playerNameInput;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        playerNameInput.text = "Player" + UnityEngine.Random.Range(1, 100);
        editName();

        if(PlayerPrefs.HasKey("volume"))
            AudioListener.volume = PlayerPrefs.GetFloat("volume");
        else
            AudioListener.volume = 0.5f;
    }

    public void editName()
    {
        this.playerName = playerNameInput.text;
    }

    public string GetPlayerName()
    {
        return this.playerName;
    }
}
