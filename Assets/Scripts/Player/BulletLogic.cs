using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletLogic : MonoBehaviourPunCallbacks
{
    [SerializeField] private float MoveSpeed = 15f;
    [SerializeField] private GameObject BulletEffect;

    void Update()
    {
        if (photonView.IsMine)
        {
            transform.position += transform.up * MoveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine == true)
        {
            photonView.RPC("SpawnExplosion", RpcTarget.All, transform.position);
            PhotonNetwork.Destroy(gameObject);
        }

        if (other.CompareTag("Player"))
        {
            PlayerLogic player = other.GetComponent<PlayerLogic>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    [PunRPC]
    private void SpawnExplosion(Vector3 position)
    {
        Instantiate(BulletEffect, position, Quaternion.identity);
    }
}
