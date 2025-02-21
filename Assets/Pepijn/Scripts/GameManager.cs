using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Net;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private bool serverBuild;
    [SerializeField] private GameObject serverPrefab, clientPrefab;
    [SerializeField] private UnityTransport unityTransport;
    private string serverAddress = "context2server.atlas-technologies.co.uk";

    private void Start()
    {
        if (serverBuild)
        {
            StartAsServer();
        }
        else
        {
            StartAsClient();
        }
    }

    private void StartAsServer()
    {
        if (unityTransport == null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        string resolvedIP = ResolveDNS(serverAddress);
        if (string.IsNullOrEmpty(resolvedIP))
        {
            Debug.LogError("Failed to resolve DNS for " + serverAddress);
            return;
        }

        unityTransport.SetConnectionData(resolvedIP, 7777);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.StartServer();

        GameObject serverInstance = Instantiate(serverPrefab);
        serverInstance.GetComponent<NetworkObject>().Spawn();
    }

    private void StartAsClient()
    {
        if (unityTransport == null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        unityTransport.SetConnectionData("77.75.125.173", 7777);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to the server.");
    }

    private string ResolveDNS(string hostname)
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            foreach (var addr in addresses)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return addr.ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("DNS Resolution failed: " + e.Message);
        }
        return null;
    }
}