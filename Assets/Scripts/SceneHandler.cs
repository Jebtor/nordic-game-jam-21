using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.Transports.UNET;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] Text m_TextIP;

    string m_Ip;

    bool m_IsHost;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Host()
    {
        m_IsHost = true;
        SceneManager.LoadScene(1);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Join()
    {
        m_Ip = m_TextIP.text;
        if (string.IsNullOrEmpty(m_Ip))
            m_Ip = "127.0.0.1";
        m_IsHost = false;

        SceneManager.LoadScene(1);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (m_IsHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            var connectionSettings = NetworkManager.Singleton.GetComponent<UNetTransport>();
            Debug.Log(m_Ip);
            connectionSettings.ConnectAddress = m_Ip;
            connectionSettings.ConnectPort = 7777;

            NetworkManager.Singleton.StartClient();
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
