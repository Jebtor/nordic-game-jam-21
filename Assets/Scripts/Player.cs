using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] float SoundInterval = 1f;

    float countDown;

    SoundSources m_SoundManager;
    KinematicBody m_KinematicBody;

    void Start()
    {
        m_SoundManager = FindObjectOfType<SoundSources>();
        countDown = SoundInterval;
        m_KinematicBody = GetComponent<KinematicBody>();
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
            }
        }
    }

    [ServerRpc]
    public void SpawnNewSoundAt_ServerRPC(Vector3 point)
    {
        m_SoundManager.SpawnSounceAt(point);
    }
}
