using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShieldLogic : MonoBehaviourPunCallbacks
{
    private AudioSource _audioSource;
    [SerializeField] private AudioClip ShieldDeploy;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (_audioSource != null) 
        { 
            _audioSource.PlayOneShot(ShieldDeploy);
        }
    }
    void Update()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(DestroyAfterDelayRoutine());
        }
    }

    IEnumerator DestroyAfterDelayRoutine()
    {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.Destroy(gameObject);
    }
}
