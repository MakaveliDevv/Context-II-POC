using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Net;

public class ServerManager : NetworkBehaviour
{
    /*
    -Make sure the client and server can communicate with one another
    */
    [SerializeField] public bool serverBuild;
    [SerializeField] private bool runLocally;
    [SerializeField] private GameObject serverPrefab, clientPrefab;
    [SerializeField] private UnityTransport unityTransport;
    Server server;
    [SerializeField] private ClientServerRefs clientServerRefs;
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

        if(!runLocally)
        {
            unityTransport.SetConnectionData(resolvedIP, 7777);
        }
        else
        {
            unityTransport.SetConnectionData("0.0.0.0", 7777);
        }

        NetworkManager.Singleton.StartServer();

        GameObject serverInstance = Instantiate(serverPrefab);
        serverInstance.GetComponent<NetworkObject>().Spawn();

        clientServerRefs.server = serverInstance.GetComponent<Server>();
        clientServerRefs.isServer = true;
    }

    private void StartAsClient()
    {
        if (unityTransport == null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        if(!runLocally) unityTransport.SetConnectionData("77.75.125.173", 7777);
        else unityTransport.SetConnectionData("127.0.0.1", 7777);

        NetworkManager.Singleton.StartClient();
        //clientServerRefs.localClient = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Client>();
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