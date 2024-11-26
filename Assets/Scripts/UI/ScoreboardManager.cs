using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScoreboardManager : MonoBehaviourPunCallbacks
{
    public static ScoreboardManager Instance { get; private set; }

    [SerializeField] private TMP_Text Player1_Name;
    [SerializeField] private TMP_Text Player1_Score;
    
    [SerializeField] private TMP_Text Player2_Name;
    [SerializeField] private TMP_Text Player2_Score;
    
    [SerializeField] private GameObject ExitButton;
    [SerializeField] private GameObject Victory_Board;
    [SerializeField] private GameObject Defeated_Board;

    private AudioSource _audioSource;
    [SerializeField] private AudioClip Victory;
    [SerializeField] private AudioClip Defeated;

    public int YourScore { get; set; } = 0;
    public int EnemyScore { get; set; } = 0;

    public int MaxScore = 5;
    private bool GameEnded = false;

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

        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        SetPlayerLabels();
        CheckEndGame();
    }

    private void SetPlayerLabels()
    {
        Player[] players = PhotonNetwork.PlayerList;

        if (players.Length == 1)
        {
            YourScore = 0;
            EnemyScore = 0;
            Player1_Name.text = "[ You ]";
            Player1_Score.text = $"{YourScore} / {MaxScore}";
            Player2_Name.text = "";
            Player2_Score.text = "";
        }

        if (players.Length >= 2)
        {
            Player1_Name.text = "[ You ]";
            Player2_Name.text = "[ Enemy ]";

            Player1_Score.text = $"{YourScore} / {MaxScore}";
            Player2_Score.text = $"{EnemyScore} / {MaxScore}";
        }
    }

    private void CheckEndGame()
    {
        if (GameEnded) return;

        if (YourScore >= MaxScore)
        {
            DisplayVictory();
            YourScore = 0;
            EnemyScore = 0;
            _audioSource.PlayOneShot(Victory);
        }
        else if (EnemyScore >= MaxScore)
        {
            DisplayDefeat();
            YourScore = 0;
            EnemyScore = 0;
            _audioSource.PlayOneShot(Defeated);
        }
    }

    private void DisplayVictory()
    {
        this.gameObject.transform.localScale = Vector3.zero;
        Victory_Board.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        GameEnded = true;
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            StartCoroutine(ExitRoomAndLoadLobbyWithDelay());
        }
    }

    private void DisplayDefeat()
    {
        this.gameObject.transform.localScale = Vector3.zero;
        Defeated_Board.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        GameEnded = true;
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            StartCoroutine(ExitRoomAndLoadLobbyWithDelay());
        }
    }

    private IEnumerator ExitRoomAndLoadLobbyWithDelay()
    {
        yield return new WaitForSeconds(3f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        yield return new WaitUntil(() => !PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady);

        SceneManager.LoadScene("Lobby");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!GameEnded)
        {
            DisplayVictory();
            YourScore = 0;
            EnemyScore = 0;
            _audioSource.PlayOneShot(Victory);
        }
    }

    public void ExitMatch()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            StartCoroutine(ExitRoomAndLoadLobby());
        }
    }

    private IEnumerator ExitRoomAndLoadLobby()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        yield return new WaitUntil(() => !PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady);

        SceneManager.LoadScene("Lobby");
    }
}
