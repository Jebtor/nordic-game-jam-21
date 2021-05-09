using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class SpeakerBoi : NetworkBehaviour
{
    public float WaveSpawnInterval;

    public float MaxVolume = 1;

    public NetworkVariableBool IsMuted = new NetworkVariableBool(false);

    public AudioSource audioSourceMusic;

    SoundSources m_SoundManager;
    float m_CurrentSpawnInterval;


    private void Start()
    {
        m_SoundManager = FindObjectOfType<SoundSources>();

        audioSourceMusic = GetComponent<AudioSource>();

        IsMuted.OnValueChanged += OnToggleChange;
    }

    private void Update()
    {
        if (!NetworkObject.IsOwner)
        {
            return;
        }

        if (IsMuted.Value == true)
        {
            return;
        }

        m_CurrentSpawnInterval -= Time.deltaTime;

        if (m_CurrentSpawnInterval < 0)
        {
            SpawnNewSoundAt_ServerRPC(transform.position);
            m_CurrentSpawnInterval = WaveSpawnInterval;
        }
    }

    [ServerRpc]
    public void SpawnNewSoundAt_ServerRPC(Vector3 point)
    {
        m_SoundManager.SpawnSounceAt(point);
    }

    public void OnToggleChange(bool prevValue, bool newValue)
    {
        audioSourceMusic.volume = newValue ? 0 : MaxVolume;
    }
}
