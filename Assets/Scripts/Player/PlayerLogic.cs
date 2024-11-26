using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerLogic : MonoBehaviourPunCallbacks
{
    [Header("Movement")]

        private CharacterController _characterController;
        
        private GameObject JoyStick_GO;
        private FixedJoystick Joystick;
        
        private float HorizontalInput;
        private float VerticalInput;
        
        private float OriginalMoveSpeed = 3f;
        private float MoveSpeed;
        private Vector3 MovementDirection;
        
        private GameObject VirtualCamera_GO;
        private CinemachineVirtualCamera VirtualCamera;
        private CinemachineTransposer Transposer;
        
        public bool AbleToMove;
        private bool ReverseControllerActivate = false;
        [SerializeField] private float VirtualCamOffset_Y = 7.5f;
        [SerializeField] private float VirtualCamOffset_Z = 7.5f;

    [Header("Gravity")]

        [SerializeField] private float Gravity = -9.81f * 2;
        private bool IsGrounded;
        private Vector3 JumpDirection;
        [SerializeField] private GameObject IsGrounded_CheckPoint;
        [SerializeField] private float IsGrounded_Radius;
        [SerializeField] private LayerMask WhatIsGround;

    [Header("Audio")]

        [SerializeField] private AudioClip ShootingSound;
        [SerializeField] private List<AudioClip> WalkingSoundList;
        private Animator _animator;
        private AudioSource _audioSource;

    [Header("Shoot")]

        [SerializeField] private GameObject Bullet;
        [SerializeField] private GameObject Skill;
        [SerializeField] private GameObject ShootingPoint;
        private float ShootingCoolDown = 1f;
        private float ReverseCoolDown = 0.125f;
        
        private GameObject ShootButton_GO;
        private Button ShootButton;
        
        private GameObject ReverseButton_GO;
        private Button ReverseButton;
        
        private GameObject SkillButton_GO;
        private Button SkillButton;
        
        private GameObject CooldownEffect_SkillGO;
        private GameObject CooldownEffect_ShootGO;
        private Image CooldownEffect_Skill;
        private Image CooldownEffect_Shoot;

        private GameObject KillMark_GO; 
        [SerializeField] private AudioClip KillSound;

        private float CurrentShootCoolDown = 0;
        private bool AbleToShoot;
        private bool Fire;
        private float CurrentReverseCoolDown = 0;
        private bool ShootASkill;

    [Header("Other Stuffs")]

        [SerializeField] public bool IsAlive;
        private float RespawnCoolDown = 5f;
        private float CurrentRespawnCoolDown = 0;
        [SerializeField] private GameObject[] RespawnPoints;
        private int RecentRespawnPoint;
        public int DeathCount { get; private set; } = 0;
        public int Score { get; private set; } = 0;
        private int Temp_Score;

    [Header("Network Setup Guarantee")]

       private bool SetupCompleted;

    private void Awake()
    {
        SetupCompleted = false;
        AbleToShoot = true;
    }

    private void Start()
    {
        if (this.gameObject.GetComponent<PhotonView>().IsMine == true && SetupCompleted == false)
        {
            // Add Joystick Controller to Local Player

            JoyStick_GO = GameObject.FindGameObjectWithTag("Joystick");

            if (JoyStick_GO != null)
            {
                Joystick = JoyStick_GO.GetComponent<FixedJoystick>();
            }

            // Add Shoot Button to Local Player

            ShootButton_GO = GameObject.FindGameObjectWithTag("Shoot Button");

            if (ShootButton_GO != null)
            {
                ShootButton = ShootButton_GO.GetComponent<Button>();
            }

            // Add Skill Button to Local Player

            SkillButton_GO = GameObject.FindGameObjectWithTag("Skill Button");

            if (SkillButton_GO != null)
            {
                SkillButton = SkillButton_GO.GetComponent<Button>();
            }

            // Add Reverse Button to Local Player

            ReverseButton_GO = GameObject.FindGameObjectWithTag("Reverse Button");

            if (ReverseButton_GO != null)
            {
                ReverseButton = ReverseButton_GO.GetComponent<Button>();
            }

            // Add Virtual to Local Player

            VirtualCamera_GO = GameObject.FindGameObjectWithTag("Virtual Camera");

            if (VirtualCamera_GO != null)
            {
                VirtualCamera = VirtualCamera_GO.GetComponent<CinemachineVirtualCamera>();

                if (VirtualCamera)
                {
                    VirtualCamera.Follow = this.gameObject.transform;
                    VirtualCamera.LookAt = this.gameObject.transform;
                    Transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
                }
            }

            // Add Cooldown Effect (Shoot) to Local Player

            CooldownEffect_ShootGO = GameObject.FindGameObjectWithTag("Shoot Cooldown");

            if (CooldownEffect_ShootGO != null)
            {
                CooldownEffect_Shoot = CooldownEffect_ShootGO.GetComponent<Image>();
            }

            // Add Cooldown Effect (Shoot) to Local Player

            CooldownEffect_SkillGO = GameObject.FindGameObjectWithTag("Skill Cooldown");

            if (CooldownEffect_SkillGO != null)
            {
                CooldownEffect_Skill = CooldownEffect_SkillGO.GetComponent<Image>();
            }

            // Add Kill Mark Local Player

            KillMark_GO = GameObject.FindGameObjectWithTag("Kill Mark");

            if (KillMark_GO != null)
            {
                KillMark_GO.transform.localScale = Vector3.zero;
            }

            // 2 reverse sides fight each other like league of legends so need to reverse controller and camera

            if (this.gameObject.transform.position.z > 10)
            {
                ReverseControllerActivate = true;
            }

            if (this.gameObject.transform.position.z < -10)
            {
                ReverseControllerActivate = false;
            }

            // Initialize components and values

            MoveSpeed = OriginalMoveSpeed;
            
            CurrentShootCoolDown = ShootingCoolDown;
            
            _animator = this.gameObject.GetComponent<Animator>();
            
            _characterController = this.gameObject.GetComponent<CharacterController>();
            
            _audioSource = this.gameObject.GetComponent<AudioSource>();

            IsAlive = true;
            
            RespawnPoints = GameObject.FindGameObjectsWithTag("Respawn Point");

            // Setup completed check point

            SetupCompleted = true;
        }

    }
    private void Update()
    {
        if (SetupCompleted == false)
        {
            return;
        }

        // Score Calculate && Update

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable
            {
               { "DeathCount", this.DeathCount }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            Player otherPlayer = PhotonNetwork.PlayerListOthers.FirstOrDefault();

            if (otherPlayer != null && otherPlayer.CustomProperties.TryGetValue("DeathCount", out object deathCount))
            {
                Score = (int)deathCount;
            }

            ScoreboardManager.Instance.YourScore = Score;
            ScoreboardManager.Instance.EnemyScore = DeathCount;

            StartCoroutine(ScoreUpdate());

            if (Score > Temp_Score) 
            {
                KillEffect();
            }
        }

        // Movement Input 

        if (Joystick != null && Transposer != null)
        {
            if (ReverseControllerActivate == false && IsAlive == true)
            {
                HorizontalInput = Joystick.Horizontal;
                VerticalInput = Joystick.Vertical;

                Transposer.m_FollowOffset = new Vector3(0, VirtualCamOffset_Y, -1 * VirtualCamOffset_Z);
            }

            if (ReverseControllerActivate == true && IsAlive == true) 
            {
                HorizontalInput = Joystick.Horizontal * -1;
                VerticalInput = Joystick.Vertical * -1;

                Transposer.m_FollowOffset = new Vector3(0, VirtualCamOffset_Y, VirtualCamOffset_Z);
            }
        }

        // Fix normalize problem

        if (Mathf.Abs(HorizontalInput) > 0.1 && Mathf.Abs(VerticalInput) > 0.1)
        {
            MoveSpeed = OriginalMoveSpeed * 0.75f;
        }
        else
        {
            MoveSpeed = OriginalMoveSpeed;
        }


        // Shoot Animation Play - Spawn the bullet

        if (ShootButton != null)
        {
            ShootButton.onClick.AddListener(OnShootButtonDown);
        }
        if (CurrentShootCoolDown > 0)
        {
            CurrentShootCoolDown -= Time.deltaTime;
            CooldownEffect_Skill.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            CooldownEffect_Shoot.transform.localScale = new Vector3(1.725f, 1.725f, 1.725f);
            CooldownEffect_Skill.fillAmount = CurrentShootCoolDown / ShootingCoolDown;
            CooldownEffect_Shoot.fillAmount = CurrentShootCoolDown / ShootingCoolDown;
            AbleToShoot = false;
        }
        else 
        {
            CooldownEffect_Skill.transform.localScale = Vector3.zero;
            CooldownEffect_Shoot.transform.localScale = Vector3.zero;
        }

        if (Fire == true && CurrentShootCoolDown <= 0 && IsGrounded == true) 
        {
            AbleToShoot = true;
            CurrentShootCoolDown = ShootingCoolDown;
        }

        CooldownEffect_Skill.transform.position = SkillButton.transform.position;
        CooldownEffect_Shoot.transform.position = ShootButton.transform.position;

        // Reverse Controller

        if (ReverseButton != null)
        {
            ReverseButton.onClick.AddListener(OnReverseButtonDown);
        }

        if (CurrentReverseCoolDown > 0)
        {
            CurrentReverseCoolDown -= Time.deltaTime;
            AbleToMove = false;
            HorizontalInput = 0;
            VerticalInput = 0;
        }
        else 
        {
            AbleToMove = true;
        }

        // Skill Controller

        if (SkillButton != null)
        {
            SkillButton.onClick.AddListener(OnSkillButtonDown);
        }

        // Animations play

        IsGrounded = Physics.CheckSphere(IsGrounded_CheckPoint.transform.position, IsGrounded_Radius, WhatIsGround);

        _animator.SetFloat("Moving", Mathf.Abs(HorizontalInput) + Mathf.Abs(VerticalInput));
        _animator.SetFloat("Velocity", JumpDirection.y);
        _animator.SetBool("IsGrounded", IsGrounded);
        _animator.SetBool("Shoot", AbleToShoot);
    }

    IEnumerator ScoreUpdate() 
    { 
        yield return new WaitForSeconds(0.5f);
        Temp_Score = Score;
    }

    private void FixedUpdate()
    {
        if (SetupCompleted == false)
        {
            return;
        }

        // Movement Action

        MovementDirection = new Vector3(HorizontalInput, 0, VerticalInput) * MoveSpeed * Time.deltaTime;

        if (Mathf.Abs(HorizontalInput) > 0f || Mathf.Abs(VerticalInput) > 0f)
        {
            this.gameObject.transform.forward = new Vector3(HorizontalInput, 0, VerticalInput);
        }

        if (AbleToMove == true)
        {
            _characterController.Move(MovementDirection);

        }

        // Gravity System 

        if (IsGrounded == false)
        {
            JumpDirection.y += Gravity * Time.deltaTime;
        }
        else 
        {
            JumpDirection.y = 0f;
        }

        if (JumpDirection.y < 0) 
        {
            JumpDirection.y = Mathf.Max(JumpDirection.y, Gravity * Time.deltaTime * 15);
        }

        _characterController.Move(JumpDirection * Time.deltaTime);

        // Respawn

        if (CurrentRespawnCoolDown > 0)
        {
            CurrentRespawnCoolDown -= Time.deltaTime;
        }

        if (CurrentRespawnCoolDown <= 0 && IsAlive == false)
        {
            Respawn();
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(IsGrounded_CheckPoint.transform.position, IsGrounded_Radius);
    }

    public void ShootSystem()
    {

        if (this.gameObject.GetComponent<PhotonView>().IsMine == true && PhotonNetwork.CurrentRoom.PlayerCount >= 2 && IsAlive == true)
        {
            photonView.RPC("Spawn", RpcTarget.Others, ShootingPoint.transform.position, ShootASkill);
            if (ShootASkill == false) 
            {
                _audioSource.PlayOneShot(ShootingSound);
            }
        }

        if (this.gameObject.GetComponent<PhotonView>().IsMine == true && PhotonNetwork.CurrentRoom.PlayerCount == 1 && IsAlive == true)
        {
            photonView.RPC("Spawn", RpcTarget.MasterClient, ShootingPoint.transform.position, ShootASkill);
            if (ShootASkill == false)
            {
                _audioSource.PlayOneShot(ShootingSound);
            }
        }

        Fire = false;
    }

    [PunRPC]
    private void Spawn(Vector3 position, bool isSkill)
    {
        if (isSkill)
        {
            GameObject skill = PhotonNetwork.Instantiate(Skill.name, position, Quaternion.Euler(Skill.transform.eulerAngles.x, ShootingPoint.transform.eulerAngles.y, Skill.transform.eulerAngles.z));
        }
        else
        {
            GameObject bullet = PhotonNetwork.Instantiate(Bullet.name, position, ShootingPoint.transform.rotation);
        }
    }

    // No need to sync the sound because they are execute beside the sync animation
    public void WalkingSoundSystem()
    {
        if (SetupCompleted == true)
        {
            _audioSource.PlayOneShot(WalkingSoundList[Random.Range(0, WalkingSoundList.Count - 1)]);
        }
    }

    public void Die()
    {
        if (IsAlive && SetupCompleted)
        {
            IsAlive = false;
            _characterController.enabled = false;
            transform.localScale = Vector3.zero;
            HorizontalInput = 0;
            VerticalInput = 0;
            CurrentRespawnCoolDown = RespawnCoolDown;
            DeathCount++;

        }
    }
    private void Respawn()
    {
        if (!IsAlive && SetupCompleted)
        {
            IsAlive = true;

            if (RecentRespawnPoint == 0)
            {
                this.gameObject.transform.position = RespawnPoints[1].transform.position;
                RecentRespawnPoint = 1;
            }
            else 
            {
                this.gameObject.transform.position = RespawnPoints[0].transform.position;
                RecentRespawnPoint = 0;
            }

            ReverseControllerActivate = this.gameObject.transform.position.z > 10;
            this.gameObject.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            _characterController.enabled = true;
        }
    }

    private void OnReverseButtonDown()
        {
        if (ReverseControllerActivate == false && CurrentReverseCoolDown <= 0)
        {
            ReverseControllerActivate = true;
            CurrentReverseCoolDown = ReverseCoolDown;
        }

        if (ReverseControllerActivate == true && CurrentReverseCoolDown <= 0)
        {
            ReverseControllerActivate = false;
            CurrentReverseCoolDown = ReverseCoolDown;
        }
    }

    public void OnShootButtonDown()
    {
        if (CurrentShootCoolDown <= 0)
        {

            ShootASkill = false;
            Fire = true;
            
        }
    }
    public void OnSkillButtonDown()
    {
        if (CurrentShootCoolDown <= 0)
        {
            ShootASkill = true;
            Fire = true;
        }
    }

    private void KillEffect()
    {
        if (Score < ScoreboardManager.Instance.MaxScore)
        {
            if (KillMark_GO.transform.localScale == Vector3.zero)
            {
                KillMark_GO.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                _audioSource.PlayOneShot(KillSound);
                StartCoroutine(KillMarkEffect_Wait());
            }
        }
    }

    IEnumerator KillMarkEffect_Wait()
    {
        yield return new WaitForSeconds(3.1f);
        KillMark_GO.transform.localScale = Vector3.zero;
    }
}
