using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class Player : NetworkBehaviour
{
    [SerializeField] float SoundInterval = 1f;
    [SerializeField] bool m_CaptureMouse = true;

    public bool IsAlive => Health.Value > 0;

    float countDown;

    SoundSources m_SoundManager;
    KinematicBody m_KinematicBody;

    bool m_IsGrounded;

    NetworkVariableInt Health;

    void Start()
    {
        m_SoundManager = FindObjectOfType<SoundSources>();
        countDown = SoundInterval;
        m_KinematicBody = GetComponent<KinematicBody>();
        m_IsGrounded = m_KinematicBody.isGrounded;

        if (m_CaptureMouse)
            Cursor.lockState = CursorLockMode.Locked;

        var settings = new NetworkVariableSettings 
        { 
            ReadPermission = NetworkVariablePermission.Everyone, 
            WritePermission = NetworkVariablePermission.ServerOnly, 
            SendTickrate = 120f 
        };

        Health = new NetworkVariableInt(settings, 10);
    }

    void Update()
    {
        if (NetworkObject.IsOwner)
            HandleInput();

        countDown -= Time.deltaTime;
        if(countDown <= 0f)
        {
            var ray = new Ray(transform.position, Vector3.down);
            if(m_KinematicBody.isGrounded && Physics.Raycast(ray, out var hit, 1.3f))
            {
                SpawnNewSoundAt_ServerRPC(hit.point);
            }

            countDown = SoundInterval;
        }

        if(!m_IsGrounded && m_KinematicBody.isGrounded)
        {
            Land();
        }

        m_IsGrounded = m_KinematicBody.isGrounded;
    }

    void Land()
    {
        var ray = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(ray, out var hit, 1.3f))
            SpawnNewSoundAt_ServerRPC(hit.point);
    }

    void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            var camera = Camera.main;
            var ray = new Ray(camera.transform.position, camera.transform.forward);//camera.ScreenPointToRay(new Vector3(mouse.position.x.ReadValue(), mouse.position.y.ReadValue()));

            if (Physics.Raycast(ray, out var hit))
            {
                SpawnNewSoundAt_ServerRPC(hit.point);
                Shoot_ServerRPC(ray.origin, ray.direction);
            }
        }
    }

    [ServerRpc]
    public void Shoot_ServerRPC(Vector3 origin, Vector3 direction)
    {
        var ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var player = hit.transform.GetComponent<Player>();
                Debug.Log($"Hit player {player.name}");
                player.GetHit_ClientRPC();
                player.Health.Value--;

                if (player.Health.Value <= 0)
                    player.Die_clientRPC();
            }
        }
    }

    [ServerRpc]
    public void SpawnNewSoundAt_ServerRPC(Vector3 point)
    {
        m_SoundManager.SpawnSounceAt(point);
    }

    [ClientRpc]
    public void GetHit_ClientRPC()
    {
        if(NetworkObject.IsLocalPlayer)
            Debug.Log("I got hit");
    }

    [ClientRpc]
    public void Die_clientRPC()
    {
        if (NetworkObject.IsLocalPlayer)
            Debug.Log("I died");
    }
}
