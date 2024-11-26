using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class DataStorage : MonoBehaviourPunCallbacks
{
    public int AvatarID;
    public static DataStorage Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
