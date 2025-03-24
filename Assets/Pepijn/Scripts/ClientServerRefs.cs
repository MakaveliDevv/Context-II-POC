using UnityEngine;

public class ClientServerRefs : MonoBehaviour
{
    public static ClientServerRefs instance;
    [SerializeField] public Server server;
    [SerializeField] public Client localClient;
    public bool isLion, isCrowd, isServer;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        if(server == null)
        {
            //server = GameObject.Find("Server(Clone)").GetComponent<Server>();
        }
    }
}
