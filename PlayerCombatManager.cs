using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerCombatManager : MonoBehaviour
{
    [SerializeField] private AttackDirection m_TargetAttackDirection = AttackDirection.Null;
    public AttackDirection TargetAttackDirection
    {
        get
        {
            return m_TargetAttackDirection;
        }
        private set
        {
            m_TargetAttackDirection = value;
        }
    }

    [SerializeField] private AttackDirection m_CurrentAttackDirection = AttackDirection.Null;
    public AttackDirection CurrentAttackDirection
    {
        get
        {
            return m_CurrentAttackDirection;
        }
        private set
        {
            m_CurrentAttackDirection = value;
        }
    }

    //Crosshair variables
    [SerializeField] GameObject CrosshairCanvas;
    [SerializeField] GameObject SlashIndicator;
    [SerializeField] Image[] CrosshairImages = new Image[(int)AttackDirection._NumberOfDirections];
    [SerializeField] Sprite SwordUISprite;

    //Constants for Crosshair Color and Scale
    const Color DefaultColor = new Color(1, 1, 1, 0.02f);
    const Color SelectedColor = new Color(1, 0, 0, 1);
    const Vector3 DefaultScale = new Vector3(0.5f, 0.25f);
    const Vector3 SelectedScale = new Vector3(1.5f, 1.5f, 1.0f);

    //Private Variables
    bool InCombatMode;
    float MouseAngle;
    int SwordSwingCount;

    Player PlayerReference;
    GameObject PlayerSlashZone;
    

    //Used for in editor Debug menu
    [SerializeField] public GameObject m_TeleportPoint1;
    [SerializeField] public GameObject m_TeleportPoint2;
    [SerializeField] public GameObject m_TeleportPoint3;
    [SerializeField] public GameObject m_TeleportPoint4;

    void Start()
    {
        PlayerReference = Services.GameManager.Player;
        CrosshairCanvas = GameObject.Find("/Canvas/Canvas_Crosshair/Crosshair");

        for (int i = 0; i < (int)AttackDirection._NumberOfDirections; i++)
        {
            CrosshairImages[i] = CrosshairCanvas.transform.GetChild(i).GetComponent<Image>();
        }
        PlayerSlashZone = GameObject.Find("/Player/PlayerSlashZone");
    }

    void Update()
    {
        // Disable the sword collider if an attack animation is not running.
        if (PlayerReference.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") 
        || PlayerReference.Animator.GetCurrentAnimatorStateInfo(0).IsName("Heal") 
        || PlayerReference.Animator.GetCurrentAnimatorStateInfo(0).IsName("Hold") 
        || PlayerReference.Animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
        {
            DisableSwordCollider();
        }
        else // Otherwise enable it
        {
            EnableSwordCollider();
        }

        if (!InCombatMode)
        {
            CrosshairCanvas.SetActive(false);
            SlashIndicator.SetActive(false);
        }
        else
        {
            CrosshairCanvas.SetActive(true);
            SlashIndicator.SetActive(true);
        }

        // If the player is pressing the attack button
        if (Services.InputManager.GetAction(InputAction.Attack))
        {
            if (PlayerReference.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")
            || PlayerReference.Animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                // Disable camera input
                PlayerReference.CameraController.SetInputEnabled(false);

                // Set the current attack to null
                CurrentAttackDirection = AttackDirection.Null;

                if (!InCombatMode)
                {
                    PlayerReference.Animator.SetBool("ShouldHold", true);
                }
                // We are now in combat mode
                InCombatMode = true;
            }

            Vector2 lookInput = Services.InputManager.GetLookInput();
            if (Services.InputManager.CurrentInputType == InputType.Keyboard)
            {
                if (lookInput.sqrMagnitude < 0.5f)
                {
                    lookInput = Vector2.zero;
                }
                else
                {
                    lookInput.Normalize();
                }
            }
            HandleLookInput(lookInput);
        }
        else // The player released the attack button
        {
            // If the player is in combat mode and has a target direction
            if (InCombatMode && TargetAttackDirection != AttackDirection.Null)
            {
                // Set the animator to play the animation
                PlayerReference.Animator.SetTrigger(TargetAttackDirection.ToString());

                // Set the current attack to the target attack
                CurrentAttackDirection = TargetAttackDirection;

                // Multiple sword swing sfx functionality
                SwordSwingCount++;
                switch (SwordSwingCount)
                {
                    case 1:
                        Services.AudioManager.PlayPlayerSFX(PlayerSFX.Sword_Slash_1);
                        break;
                    case 2:
                        Services.AudioManager.PlayPlayerSFX(PlayerSFX.Sword_Slash_2);
                        break;
                    case 3:
                        Services.AudioManager.PlayPlayerSFX(PlayerSFX.Sword_Slash_3);
                        SwordSwingCount = 0;
                        break;
                }
            }

            // Enable camera input
            PlayerReference.CameraController.SetInputEnabled(true);
            //Get out of the sword hold animation
            PlayerReference.Animator.SetBool("ShouldHold", false);

            // No longer in combat mode
            InCombatMode = false;

            // Reset taret direction to null
            TargetAttackDirection = AttackDirection.Null;
        }
        // Reset all the crosshair images' color and scale
        ResetCrosshairImages();

        // Set the currently selected direction's crosshair to show as selected
        if (TargetAttackDirection != AttackDirection.Null)
        {
            CrosshairImages[(int)TargetAttackDirection].sprite = SwordUISprite;
            CrosshairImages[(int)TargetAttackDirection].color = SelectedColor;
            CrosshairImages[(int)TargetAttackDirection].transform.localScale = SelectedScale;
        }
    }

    private void ResetCrosshairImages()
    {
        foreach (Image image in CrosshairImages)
        {
            image.sprite = null;
            image.color = DefaultColor;
            image.transform.localScale = DefaultScale;
        }
    }

    private void HandleLookInput(Vector2 lookInput)
    {
        if (!InCombatMode) return;

        if (lookInput.magnitude >= 0.9999f && Services.InputManager.CurrentInputType == InputType.Keyboard)
        {
            MouseAngle = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;

            if (MouseAngle < 0)
            {
                MouseAngle = 360 + MouseAngle;
            }
            MouseAngle = 360 - MouseAngle;
        }
        else if (lookInput.magnitude >= 0.5f && Services.InputManager.CurrentInputType == InputType.Controller)
        {
            MouseAngle = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;

            if (MouseAngle < 0)
            {
                MouseAngle = 360 + MouseAngle;
            }
            MouseAngle = 360 - MouseAngle;
        }

        if (MouseAngle >= 20.0f && MouseAngle <= 70.0f)
        {
            TargetAttackDirection = AttackDirection.SouthEast;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -45);
        }
        else if (MouseAngle > 70.0f && MouseAngle < 110.0f)
        {
            TargetAttackDirection = AttackDirection.South;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -90);
        }
        else if (MouseAngle >= 110.0f && MouseAngle <= 160.0f)
        {
            TargetAttackDirection = AttackDirection.SouthWest;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -135);
        }
        else if (MouseAngle > 160.0f && MouseAngle < 200.0f)
        {
            TargetAttackDirection = AttackDirection.West;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -180);
        }
        else if (MouseAngle >= 200.0f && MouseAngle <= 250.0f)
        {
            TargetAttackDirection = AttackDirection.NorthWest;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -225);
        }
        else if (MouseAngle > 250.0f && MouseAngle < 290.0f)
        {
            TargetAttackDirection = AttackDirection.North;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -270);
        }
        else if (MouseAngle >= 290.0f && MouseAngle <= 340.0f)
        {
            TargetAttackDirection = AttackDirection.NorthEast;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, -315);
        }
        else if (MouseAngle > 340.0f || MouseAngle < 20.0f && MouseAngle != 0)
        {
            TargetAttackDirection = AttackDirection.East;
            SlashIndicator.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

    }

    public void EnableSwordCollider()
    {
        PlayerSlashZone.GetComponent<CapsuleCollider>().enabled = true;
    }

    public void DisableSwordCollider()
    {
        PlayerSlashZone.GetComponent<CapsuleCollider>().enabled = false;
    }


#if UNITY_EDITOR
    // Code used by our ingame debug menu
    public void TeleportToPosition1()
    {
        gameObject.transform.position = m_TeleportPoint1.gameObject.transform.position;
    }
    public void TeleportToPosition2()
    {
        gameObject.transform.position = m_TeleportPoint2.gameObject.transform.position;
    }
    public void TeleportToPosition3()
    {
        gameObject.transform.position = m_TeleportPoint3.gameObject.transform.position;
    }
    public void TeleportToPosition4()
    {
        gameObject.transform.position = m_TeleportPoint4.gameObject.transform.position;
    }
#endif
}
