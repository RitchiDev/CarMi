using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBackgroundMusicOnStart : MonoBehaviour
{
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    // This is so that saved volume has the chance to update before the music starts playing
    private void Start()
    {
        m_AudioSource.Play();
    }
}
