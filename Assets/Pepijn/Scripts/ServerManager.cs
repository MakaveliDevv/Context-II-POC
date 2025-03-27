using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Net;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System;

public class ServerManager : NetworkBehaviour
{
    /*
    -Make sure the client and server can communicate with one another
    */
    [SerializeField] public bool serverBuild, webBuild;
    [SerializeField] private bool runLocally;
    [SerializeField] private GameObject serverPrefab, clientPrefab;
    [SerializeField] private UnityTransport unityTransport;
    Server server;
    [SerializeField] private ClientServerRefs clientServerRefs;
    [SerializeField] ClientManager clientManager;
    GameObject serverObj;
    private string serverAddress = "context2server.atlas-technologies.co.uk";
    private ushort port = 7777;

    private void Start()
    {
        if(SceneManager.GetActiveScene().name != "Lobby") return;

        DontDestroyOnLoad(gameObject);
        if (serverBuild)
        {
            if(serverObj == null) StartAsServer();
        }
        else
        {
            //StartAsClient();
        }
    }

    public void JoinServer()
    {
        if(clientManager.nameInputField.text == "Enter your name..." || clientManager.nameInputField.text == "") return;

        if (!serverBuild)
        {
            StartAsClient();
            clientManager.startButton.SetActive(false);
            clientManager.readyButton.SetActive(true);
            clientManager.clientConnectedTexts.SetActive(true);
        }
    }

    private void StartAsServer()
    {
        //if (unityTransport == null)
        //{
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //}

        if(!runLocally)
        {
            unityTransport.SetConnectionData("0.0.0.0", 7777);
        }
        else
        {
            unityTransport.SetConnectionData("0.0.0.0", 7777);
        }

        NetworkManager.Singleton.StartServer();

        GameObject serverInstance = Instantiate(serverPrefab);
        serverObj = serverInstance;
        serverInstance.GetComponent<NetworkObject>().Spawn();

        clientServerRefs.server = serverInstance.GetComponent<Server>();
        clientServerRefs.isServer = true;

        Debug.Log("Server started");
    }

    private void StartAsClient()
    {
        
        if (unityTransport == null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        if(webBuild)
        {
            serverAddress = "context2server.atlas-technologies.co.uk";
            port = 443;
        }
        else
        {
            serverAddress = "context2server.atlas-technologies.co.uk";
            port = 7777;
        }

        string resolvedIP = ResolveDNS(serverAddress);

        if(!runLocally) unityTransport.SetConnectionData(resolvedIP, port);
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