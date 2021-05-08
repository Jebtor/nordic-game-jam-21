using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.InputSystem;

public class SoundSources : MonoBehaviour
{
    const int k_Max = 1024;

    struct SoundSource
    {
        public float3 origin;
        public float time;
        public float duration;
    }

    static readonly int s_TimeProperty = Shader.PropertyToID("_GameTime");
    static readonly int s_SoundCountProperty = Shader.PropertyToID("_SoundCount");
    static readonly int s_SoundSourcesBufferProperty = Shader.PropertyToID("_SoundSourcesBuffer");

    ComputeBuffer m_SoundSourcesGPU;
    NativeArray<SoundSource> m_SoundSourcesCPU;

    int m_BufferEnd;
    int m_BufferStart;

    unsafe void OnEnable()
    {
        m_SoundSourcesGPU = new ComputeBuffer(k_Max, sizeof(SoundSource), ComputeBufferType.Structured);
        m_SoundSourcesCPU = new NativeArray<SoundSource>(k_Max, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        m_BufferStart = m_BufferEnd = 0;
    }

    unsafe void OnDisable()
    {
        m_SoundSourcesGPU?.Dispose();
        m_SoundSourcesCPU.Dispose();

        m_BufferStart = m_BufferEnd = 0;
    }

    void Update()
    {
        while (m_BufferStart < m_BufferEnd && Time.time > m_SoundSourcesCPU[m_BufferStart].time + m_SoundSourcesCPU[m_BufferStart].duration)
            m_BufferStart++;

        HandleInput();
        UploadToGPU();
    }

    void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            var camera = Camera.main;
            var plane = new Plane(Vector3.up, 0f);
            var ray = camera.ScreenPointToRay(new Vector3(mouse.position.x.ReadValue(), mouse.position.y.ReadValue()));

            plane.Raycast(ray, out var distance);
            var hit = ray.GetPoint(distance);
            SpawnNewSoundAt(hit);
        }  
    }

    public void SpawnNewSoundAt(float3 point)
    {
        Debug.Assert(m_BufferEnd < k_Max);

        m_SoundSourcesCPU[m_BufferEnd] = new SoundSource
        {
            origin = point,
            time = Time.time,
            duration = 5f,
        };

        m_BufferEnd++;

        Debug.Log($"sound {m_BufferEnd} added at {point}.");
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
