﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Change the color of the player when the last 3 platforms are the same state. 
/// </summary>
public class PlayerColorChange : MonoBehaviourPunCallbacks
{
    PlayerIdentity pIdentity; 

    //Photon Custom Properties 
    public PlatformState PlatState
    {
        get
        {
            if(pIdentity.isBot)
                return (PlatformState)Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["plat_state"]);
            else
                return (PlatformState)Convert.ToInt32(photonView.Owner.CustomProperties["plat_state"] ); 
        } 
        set
        {
            Hashtable h = new Hashtable { { "plat_state", Convert.ToByte((int)value) } };

            if (pIdentity.isBot)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(h); 
            } else
            {
                photonView.Owner.SetCustomProperties(h);
            }
        }
    }

    public PlatformStateEvent OnPlatStateChange = new PlatformStateEvent();


    public int ColorStreak
    {
        get
        {
            return colorStreak; 
        }
        set
        {
            colorStreak = value;
            OnColorStreakChange.Invoke(value); 
        }
    }

    public IntEvent OnColorStreakChange = new IntEvent(); 
    
    /// <summary>
    /// The number of times the player has bounced on the same platform color (lastPlatState) in a row. 
    /// </summary>
    public int colorStreak;
    
    

    /// <summary>
    /// The platfrom state of the last bounce
    /// </summary>
    PlatformState lastPlatState = PlatformState.FIRE;

    #region Unity Callbacks

    private void Awake()
    {
        pIdentity = GetComponent<PlayerIdentity>();
    }
    private void Start()
    {
        //initalize color of other players that have loaded
        if (!photonView.IsMine)
        {
            OnPlatStateChange.Invoke((PlatformState)((int)PlatState));
        } else //init yourself 
        {
            GetComponent<PlayerMovement>().OnBounce.AddListener(RespondToBounce);
            GetComponent<PlayerDeath>().OnDeath.AddListener(ClearStreaks);



            //init vars
            colorStreak = 0;
        }
    }

    #endregion
    
    

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("plat_state"))
            OnPlatStateChange.Invoke(PlatState); 
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (pIdentity.isBot && propertiesThatChanged.ContainsKey("plat_state"))
            OnPlatStateChange.Invoke(PlatState);
    }




    #region Custom Methods 

    /// <summary>
    /// processes bouncing for color change LOCALLY 
    /// </summary>
    void RespondToBounce()
    {
        PlatformState state = GetPlatformBelowState();
        ProcessNewBounce(state); 
    }

    /// <summary>
    /// returns platformstate.NULL if there is no platform below 
    /// </summary>
    /// <returns></returns>
    public PlatformState GetPlatformBelowState()
    {
        GameObject plat = GetComponent<PlayerMovement>().PlatformBelow;
        if (plat == null)
            return PlatformState.NULL; 
        else 
            return plat.GetComponent<PlatformAppearance>().CurrentState;
    } 

    /// <summary>
    /// Updates colorStreak to reflect new bounce state.
    /// LOCAL 
    /// </summary>
    /// <param name="state"></param>
    void ProcessNewBounce(PlatformState state)
    {
        if (state == lastPlatState)
            ColorStreak++;
        else
            ColorStreak = 1; 

        if(ColorStreak == 3)
        {
            PlatState = state; //photon custom property 
        }

        lastPlatState = state;
    }

    void ClearStreaks()
    {
        ColorStreak = 0; 
    } 

    #endregion
}
