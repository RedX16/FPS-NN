using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button ServerButton;
    [SerializeField] Button HostButton;
    [SerializeField] Button ClientButton;

    private void Awake()
    {
        ServerButton.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); });
        HostButton.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); });
        ClientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
    }
}
