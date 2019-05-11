using System;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{


    //Constants
    const float PlayerMass = 3.0f;
    const float DodgeTimerLength = 0.3f;
    const float JumpTimerLength = 0.25f;
    const float MinimunPushForce = 0.2f;
    private const float GravityScale = 1.3f;
    private const int OutsideForceAmplfier = 5;
    private const float FirePushPlayerForce = 15.0f;

    /////////////////////////

    [SerializeField] float JumpAmmount = 7.0f;
    [SerializeField] float DodgeSpeed = 14.0f;
    [SerializeField] float MaxSpeed = 7.0f;
    float MoveSpeed;
    float JumpLerp;
    float FallLerp;

    bool CanJump;
    bool IsJumping;
    bool DodgeInput;
    bool JumpInput;
    public bool TutorialDash;
    public bool IsPlayerMovementDissabled { get; set; }

    public Vector3 PlayerVelocity = Vector3.zero;
    public Vector3 OutsideForce = Vector3.zero;
    Vector3 MoveDirection = Vector3.zero;

    //Timers
    Timer DodgeTimer;
    Timer JumpTimer;

    // Walk SFX variables
    float FootstepSFXDelay = 0.5f;
    float FootstepDelayCountdown;
    int FootstepCount;
    bool InWater;
    bool isGrounded;

    Player PlayerReference;
    CharacterController CharacterController;


    void Awake()
    {
        PlayerReference = GetComponent<Player>();
        CharacterController = GetComponent<CharacterController>();

        DodgeTimer = Services.TimerManager.CreateTimer("DodgeTimer", DodgeTimerLength, false);
        JumpTimer = Services.TimerManager.CreateTimer("JumpTimer", JumpTimerLength, false);

        MoveSpeed = MaxSpeed;
    }

    void Update()
    {
        float horizontalMovement = Services.InputManager.GetAxisRawValue(InputAxis.MoveHorizontal);
        float frontwardMovement = Services.InputManager.GetAxisRawValue(InputAxis.MoveFrontward);

        //Get any special movement 
        JumpInput = Services.InputManager.GetAction(InputAction.Jump);
        DodgeInput = Services.InputManager.GetActionDown(InputAction.Dash);

        MoveDirection = (horizontalMovement * transform.right + frontwardMovement * transform.forward);

        if (DodgeInput && DodgeTimer.IsRunning() == false && MoveDirection != Vector3.zero)
        {
            if (PlayerReference.m_CurrentStamina >= PlayerReference.SizeOfStaminaChunks)
            {
                PlayerReference.m_CurrentStamina -= PlayerReference.SizeOfStaminaChunks;
                MoveSpeed = DodgeSpeed;
                TutorialDash = true;
                DodgeTimer.Restart();
            }
        }

        if (DodgeTimer.IsFinished())
        {
            MoveSpeed = MaxSpeed;
        }

        // Walk SFX logic
        UpdateWalkingSFX();
    }

    void FixedUpdate()
    {
        if (IsPlayerMovementDissabled) return;

        PlayerVelocity = (MoveDirection * MoveSpeed);

        isGrounded = CharacterController.isGrounded;

        if (!isGrounded || IsJumping)
        {
            if (JumpTimer.IsRunning())
            {
                JumpLerp += Time.deltaTime;
                PlayerVelocity.y += Mathf.Lerp(JumpAmmount, 0.0f, JumpLerp / 0.25f);
            }
            else
            {
                //only apply gravity when in the air when not jumping
                IsJumping = false;
                FallLerp += Time.deltaTime;
                PlayerVelocity.y -= FallLerp * JumpAmmount * GravityScale;
            }
        }

        isGrounded = CharacterController.isGrounded;

        //If we are grounded update accordinly
        if (isGrounded)
        {
            CanJump = true;
            PlayerVelocity.y -= CharacterController.stepOffset / Time.deltaTime;

            if (JumpInput && JumpTimer.IsRunning() == false && CanJump)
            {
                JumpTimer.Restart();
                PlayerVelocity.y = 0.0f;
                PlayerVelocity.y += JumpAmmount;
                IsJumping = true;
                CanJump = false;
            }
            JumpLerp = 0;
            FallLerp = 0;
        }

        if (OutsideForce.magnitude > MinimunPushForce)
        {
            CharacterController.Move(OutsideForce * Time.deltaTime);
        }
        else
        {
            OutsideForce = Vector3.zero;
        }

        // consumes the impact energy each cycle:
        OutsideForce = Vector3.Lerp(OutsideForce, Vector3.zero, OutsideForceAmplfier * Time.deltaTime);

        //After all calculations move player
        CharacterController.Move(PlayerVelocity * Time.deltaTime);

    }

    void OnEnable()
    {
        GameManager.OnPause += OnPause;
        GameManager.OnResume += OnResume;
    }
    void OnDisable()
    {
        GameManager.OnPause -= OnPause;
        GameManager.OnResume -= OnResume;
    }

    void UpdateWalkingSFX()
    {
        if (CharacterController.velocity != Vector3.zero && CharacterController.isGrounded)
        {
            FootstepDelayCountdown -= Time.fixedDeltaTime;

            PlayerReference.Animator.SetBool("Walking", true);

            if (FootstepCount > 10)
                FootstepCount = 0;

            if (FootstepDelayCountdown <= 0)
            {
                if (InWater == false)
                {
                    switch (FootstepCount)
                    {
                        case 0:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_1);
                            break;
                        case 1:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_2);
                            break;
                        case 2:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_3);
                            break;
                        case 3:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_4);
                            break;
                        case 4:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_5);
                            break;
                        case 5:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_6);
                            break;
                        case 6:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_7);
                            break;
                        case 7:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_8);
                            break;
                        case 8:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_9);
                            break;
                        case 9:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Groud_10);
                            break;
                    }

                }
                else if (InWater == true)
                {

                    switch (FootstepCount)
                    {
                        case 0:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_1);
                            break;
                        case 1:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_2);
                            break;
                        case 2:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_3);
                            break;
                        case 3:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_4);
                            break;
                        case 4:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_5);
                            break;
                        case 5:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_6);
                            break;
                        case 6:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_7);
                            break;
                        case 7:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_8);
                            break;
                        case 8:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_9);
                            break;
                        case 9:
                            Services.AudioManager.PlayPlayerSFX(PlayerSFX.Player_Footsteps_Water_10);
                            break;
                    }

                }
                FootstepCount++;
                FootstepDelayCountdown = FootstepSFXDelay;
            }
        }
        else
        {
            PlayerReference.Animator.SetBool("Walking", false);
        }
    }

    //this function handles outside forces and applies them to the player
    public void PushPlayer(Vector3 dir, float force)
    {
        dir = Services.GameManager.Player.transform.position - dir;
        dir.Normalize();
        if (dir.y < 0)
        {
            dir.y = -dir.y;
        }
        OutsideForce += dir.normalized * force / PlayerMass;
    }

    void OnPause()
    {
        GetComponent<FPSCameraController>().enabled = false;
        GetComponent<PlayerCombatManager>().enabled = false;
    }

    void OnResume()
    {
        GetComponent<FPSCameraController>().enabled = true;
        GetComponent<PlayerCombatManager>().enabled = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Water"))
        {
            InWater = true;
        }
        else
        {
            InWater = false;
        }

        if (hit.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (hit.gameObject.CompareTag("Fire"))
        {
            PushPlayer((hit.transform.position), FirePushPlayerForce);
            Services.GameManager.Player.TakeDamage(Constants.FireDamage);
        }
    }
}
