using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using MLAPI.NetworkVariable.Collections;
using MLAPI.NetworkVariable;
using MLAPI;
using System;

public class SoundSources : NetworkBehaviour
{
    const int k_Max = 1024;

    [Serializable]
    struct SoundSource
    {
        public Vector3 origin;
        public float time;
        public float duration;
    }

    static readonly int s_TimeProperty = Shader.PropertyToID("_GameTime");
    static readonly int s_SoundCountProperty = Shader.PropertyToID("_SoundCount");
    static readonly int s_SoundSourcesBufferProperty = Shader.PropertyToID("_SoundSourcesBuffer");

    ComputeBuffer m_SoundSourcesGPU;
    NativeArray<SoundSource> m_SoundSourcesCPU;


    NetworkList<Vector3> m_SoundPositions;
    //NetworkList<float> m_SoundTime;
    //NetworkList<float> m_SoundDurations;

    int m_BufferEnd;
    int m_BufferStart;

    unsafe void OnEnable()
    {
        m_SoundSourcesGPU = new ComputeBuffer(k_Max, sizeof(SoundSource), ComputeBufferType.Structured);
        m_SoundSourcesCPU = new NativeArray<SoundSource>(k_Max, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        var settings = new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly,
            SendTickrate = 120f,
        };

        m_SoundPositions = new NetworkList<Vector3>(settings);
        //m_SoundTime = new NetworkList<float>(settings);
        //m_SoundDurations = new NetworkList<float>(settings);

        m_SoundPositions.OnListChanged += OnPositionsChanged;
        //m_SoundTime.OnListChanged += OnTimesChanged;
        //m_SoundDurations.OnListChanged += OnDurationsChanged;

        m_BufferStart = m_BufferEnd = 0;
    }

    unsafe void OnDisable()
    {
        m_SoundSourcesGPU?.Dispose();
        m_SoundSourcesCPU.Dispose();

        m_SoundPositions.OnListChanged -= OnPositionsChanged;
        //m_SoundTime.OnListChanged -= OnTimesChanged;
        //m_SoundDurations.OnListChanged -= OnDurationsChanged;

        m_BufferStart = m_BufferEnd = 0;
    }

    void OnPositionsChanged(NetworkListEvent<Vector3> changeEvent)
    {
        if (changeEvent.Type != NetworkListEvent<Vector3>.EventType.Add)
            return;

        var i = changeEvent.Index;

        var sound = m_SoundSourcesCPU[i];
        sound.origin = m_SoundPositions[i];
        sound.time = Time.time;
        sound.duration = 5f;
        m_SoundSourcesCPU[i] = sound;

        m_BufferEnd = math.max(m_BufferEnd, i + 1);

        //Debug.Log("Data changed");
    }

    void Update()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsConnectedClient)
            return;

        //while (m_BufferStart < m_BufferEnd && Time.time > m_SoundSourcesCPU[m_BufferStart].time + m_SoundSourcesCPU[m_BufferStart].duration)
        //    m_BufferStart++;

        UploadToGPU();
    }

    public void SpawnSounceAt(float3 point)
    {
        Debug.Assert(m_BufferEnd < k_Max);
        Debug.Assert(NetworkManager.Singleton.IsHost);

        var soundSource = new SoundSource
        {
            origin = point,
            time = Time.time,
            duration = 5f,
        };

        m_SoundPositions.Add(soundSource.origin);
        //m_SoundDurations.Add(soundSource.duration);
        //m_SoundTime.Add(soundSource.time);

        m_SoundSourcesCPU[m_BufferEnd] = soundSource;

        m_BufferEnd++;
    }

    void UploadToGPU()
    {
        var count = m_BufferEnd - m_BufferStart;
        m_SoundSourcesGPU.SetData(m_SoundSourcesCPU, m_BufferStart, 0, count);

        Shader.SetGlobalFloat(s_TimeProperty, Time.time);
        Shader.SetGlobalFloat(s_SoundCountProperty, count);
        Shader.SetGlobalBuffer(s_SoundSourcesBufferProperty, m_SoundSourcesGPU);
    }
}
