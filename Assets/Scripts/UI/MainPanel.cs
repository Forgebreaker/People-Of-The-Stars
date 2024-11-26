using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Threading.Tasks;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MainPanel : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button RoomCreateButton;
    private bool isCreatingRoom;

    [SerializeField] private Button MatchFindButton;
    [SerializeField] private Button ExitGameButton;

    [SerializeField] private TMP_Text Connecting_MSG;
    [SerializeField] private List<GameObject> SkinList;
    
    [SerializeField] private Button ChooseAvatar_Left;
    [SerializeField] private Button ChooseAvatar_Right;
    
    public bool SetupCompleted;
    public int CurrentAvatarID;

    public static MainPanel Instance;

    private void Awake()
    {
        SetupCompleted = false;
        CurrentAvatarID = 0;

        PhotonNetwork.ConnectUsingSettings();
     
        if (PhotonNetwork.InRoom == true)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    void Start()
    {
        if (SetupCompleted == false && PhotonNetwork.IsConnected)
        {
            RoomCreateButton.onClick.AddListener(OnClick_CreateRoom);
            MatchFindButton.onClick.AddListener(OnClick_MatchFind);
            ExitGameButton.onClick.AddListener(OnClick_ExitGame);

            ChooseAvatar_Left.onClick.AddListener(DecreaseAvatarID);
            ChooseAvatar_Right.onClick.AddListener(IncreaseAvatarID);
            
            SetupCompleted = true;
        }
    }

    private void Update()
    {
        if (SetupCompleted)
        {
            for (int counter = 0; counter < SkinList.Count; counter++)
            {
                SkinList[counter].SetActive(counter == CurrentAvatarID);
            }
            DataStorage.Instance.AvatarID = CurrentAvatarID;
        }
    }

    private void IncreaseAvatarID()
    {
        CurrentAvatarID += 1;
        if (CurrentAvatarID > SkinList.Count - 1)
        {
            CurrentAvatarID = 0;          
        }
    }

    private void DecreaseAvatarID()
    {
        CurrentAvatarID -= 1;
        if (CurrentAvatarID < 0)
        {
            CurrentAvatarID = SkinList.Count - 1;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------//

    private async void OnClick_CreateRoom()
    {
        Connecting_MSG.text = "Creating room...";
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Connecting_MSG.text = "Connecting to Master Server...";
            PhotonNetwork.ConnectUsingSettings();
            while (!PhotonNetwork.IsConnectedAndReady)
            {
                await Task.Delay(100);
            }
        }

        isCreatingRoom = true;
        PhotonNetwork.CreateRoom(null, roomOptions);

        float timer = 0f;
        while (isCreatingRoom && timer < 5f)
        {
            await Task.Delay(1000);
            timer += 1f;
        }

        if (isCreatingRoom)
        {
            Connecting_MSG.text = "Failed to create room...";
            isCreatingRoom = false;
        }
    }

    public override void OnCreatedRoom()
    {
        Connecting_MSG.text = "Room created successfully!";
        isCreatingRoom = false;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Connecting_MSG.text = "Failed to create room: " + message;
        isCreatingRoom = false;
    }

    //-------------------------------------------------------------------------------------------------------------------------//

    private async void OnClick_MatchFind()
    {
        Connecting_MSG.text = "Joining room...";

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Connecting_MSG.text = "Connecting to Master Server...";
            PhotonNetwork.ConnectUsingSettings();
            while (!PhotonNetwork.IsConnectedAndReady)
            {
                await Task.Delay(100);
            }
        }

        PhotonNetwork.JoinRandomRoom();
    }

    //-------------------------------------------------------------------------------------------------------------------------//

    private void OnClick_ExitGame()
    {
        Connecting_MSG.text = "Exiting game...";
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    public override void OnConnectedToMaster()
    {
        Connecting_MSG.text = "Connected to Master Server!";
    }

    public override void OnJoinedRoom()
    {
        Connecting_MSG.text = "Joined room successfully!";
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsMasterClient)
        {
            Connecting_MSG.text = "Loading level...";
            PhotonNetwork.LoadLevel("Battleground");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Connecting_MSG.text = "Failed! No room available";
    }
}
