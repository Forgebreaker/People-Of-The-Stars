using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeEffect : MonoBehaviourPunCallbacks
{
    [SerializeField] private AudioClip ExplodeSound;
    private AudioSource _audioSource;
    private bool Played = false;
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        if (!Played)
        {
            _audioSource.PlayOneShot(ExplodeSound);
            Played = true;
        }

        Destroy(gameObject, 1f);
    }
}