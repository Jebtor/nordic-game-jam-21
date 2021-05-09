using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationControlledSounds : MonoBehaviour
{
    public AudioSource leftFoot;
    public AudioSource rightFoot;
    public AudioClip[] stepAudioSources;

    public void leftStepSound()
    {
        leftFoot.clip = stepAudioSources[Random.Range(0, stepAudioSources.Length)];
        leftFoot.Play();
    }

    public void rightStepSound()
    {
        rightFoot.clip = stepAudioSources[Random.Range(0, stepAudioSources.Length)];
        rightFoot.Play();
    }
}
