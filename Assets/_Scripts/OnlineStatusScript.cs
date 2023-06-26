using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Authentication;

public class OnlineStatusScript : NetworkBehaviour
{
    [SerializeField] Text text;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetConnectedStatus()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            text.text = "CONNECTED";
        }
        else
        {
            text.text = "NOT CONNECTED";
        }
    }

    public void SetText(string input)
    {
        text.text = input;
    }
}
