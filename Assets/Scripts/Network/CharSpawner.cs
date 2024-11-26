using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<GameObject> Player_Prefabs;
    
    [SerializeField] private GameObject[] SpawnPoints;
    
    private bool characterSpawned = false; // Trigger variable to check if character has been spawned

    public static CharSpawner Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!characterSpawned && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            SpawnCharacter();
            characterSpawned = true;
        }
    }

    public void SpawnCharacter()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            int spawnIndex = Mathf.Min(PhotonNetwork.CurrentRoom.PlayerCount - 1, SpawnPoints.Length - 1);
            
            Vector3 spawnPosition = SpawnPoints[spawnIndex].transform.position;
            
            Quaternion spawnRotation = SpawnPoints[spawnIndex].transform.rotation;

            if (DataStorage.Instance == null)
            {
                PhotonNetwork.Instantiate(Player_Prefabs[Random.Range(0, Player_Prefabs.Count)].name,
                                          spawnPosition, spawnRotation);
            }
            else
            {
                PhotonNetwork.Instantiate(Player_Prefabs[DataStorage.Instance.AvatarID].name,
                                          spawnPosition, spawnRotation);
            }
        }
    }
}
