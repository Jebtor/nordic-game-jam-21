using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.InputSystem;

public class SoundSources : MonoBehaviour
{
    const int k_Max = 100;

    static readonly int TimeProperty = Shader.PropertyToID("_GameTime");
    static readonly int SoundsCountProperty = Shader.PropertyToID("_SoundsCount");
    static readonly int SoundsOriginsBufferProperty = Shader.PropertyToID("_SoundOriginsBuffer");
    static readonly int SoundsTimesBufferProperty = Shader.PropertyToID("_SoundTimesBuffer");

    ComputeBuffer m_SoundOriginsGPU;
    NativeArray<float3> m_SoundOriginsCPU;

    ComputeBuffer m_SoundTimesGPU;
    NativeArray<float> m_SoundTimesCPU;

    int count;

    unsafe void Start()
    {
        m_SoundOriginsGPU = new ComputeBuffer(k_Max, sizeof(float3), ComputeBufferType.Structured);
        m_SoundOriginsCPU = new NativeArray<float3>(k_Max, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        m_SoundTimesGPU = new ComputeBuffer(k_Max, sizeof(float), ComputeBufferType.Structured);
        m_SoundTimesCPU = new NativeArray<float>(k_Max, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        count = 0;
    }

    void OnDisable()
    {
        m_SoundOriginsGPU?.Dispose();
        m_SoundOriginsCPU.Dispose();

        m_SoundTimesGPU?.Dispose();
        m_SoundTimesCPU.Dispose();
    }

    void Update()
    {
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

            //var ray = new Ray { direction = camera.transform.forward, origin = camera.transform.position };
            plane.Raycast(ray, out var distance);
            var hit = ray.GetPoint(distance);
            SpawnNewSoundAt(hit);
        }  
    }

    public void SpawnNewSoundAt(float3 point)
    {
        Debug.Assert(count < k_Max);

        m_SoundOriginsCPU[count] = point;
        m_SoundTimesCPU[count] = Time.time;
        count++;

        Debug.Log($"sound {count} added at {point}.");
    }

    void UploadToGPU()
    {
        m_SoundOriginsGPU.SetData(m_SoundOriginsCPU);

        Shader.SetGlobalFloat(TimeProperty, Time.time);
        Shader.SetGlobalFloat(SoundsCountProperty, count);
        Shader.SetGlobalBuffer(SoundsOriginsBufferProperty, m_SoundOriginsGPU);
        Shader.SetGlobalBuffer(SoundsTimesBufferProperty, m_SoundTimesGPU);

    }
}
