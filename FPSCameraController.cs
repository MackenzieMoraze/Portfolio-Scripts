using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// Script written by Mackenzie
/// </summary>
public class FPSCameraController : MonoBehaviour
{
    //Constants
    private const float MarkingCost = 40.0f;
    private const float MaximumLockonDistance = 35.0f;
    private const int ControllerSensitivityCorrection = 6;
    private const float CameraBobThreshold = 0.0001f;


    //Public variables 
    public float SensitivityX;
    public float SensitivityY;

    public bool InvertYAxis;
    public bool TutorialHasPressedTab;
    public bool IsInputEnabled = true;
    public bool InEnemyRadius { get; set; }

    //Private Variables
    bool HasReachedLockedPosition;
    bool TargetLock;

    float RotationY;
    float RotationX;
    float BobTime;
    float CurrentLerpTime;
    float LerpTime = 1.0f;
    float MinimumX = -60f;
    float MaximumX = 60f;

    //Assigned in Editior
    [SerializeField] Transform CameraAnchor;
    [SerializeField] GameObject PlayerBody;
    [SerializeField] Collider PlayerSlashZone;
    [SerializeField] Camera CameraReference;

    Vector3 CameraJiggleOffset;
    Enemy EnemyTarget;
    Transform LockOnLocation;
    PlayerMovementController MovementController;

    // Use this for initialization
    void Start()
    {
        RotationY = Services.GameManager.Player.transform.localEulerAngles.y;

        CameraReference.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        MovementController = Services.GameManager.Player.GetComponent<PlayerMovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Services.GameManager.CurrentGameState == GameState.Paused) return;

        if (EnemyTarget == null)
        {
            UnlockCamera();
            TargetLock = false;
            InEnemyRadius = false;
        }

        if (Services.InputManager.GetActionDown(InputAction.LockCamera))
        {
            LockToEnemy();
        }

        if (Services.InputManager.GetActionDown(InputAction.Fire))
        {
            if (IsEnemyInView())
            {
                //Debug.Log(Services.GameManager.Player.m_CurrentFocus);
                if (Services.GameManager.Player.m_CurrentFocus >= MarkingCost)
                {
                    EnemyTarget.GetComponent<Enemy>().m_Marked = true;
                    Services.GameManager.Player.m_CurrentFocus -= MarkingCost;

                }
            }
        }
        if ((TargetLock == false))
        {
            if (IsInputEnabled)
            {
                Vector2 lookInput = Services.InputManager.GetLookInput();

                if (Services.InputManager.CurrentInputType == InputType.Controller)
                {
                    RotationY += lookInput.x * SensitivityY;
                    //checks if the y axis are inverted from the options menu
                    if (Services.GameManager.InvertMouseYAxis)
                    {
                        RotationX -= lookInput.y * SensitivityX;
                    }
                    else
                    {
                        RotationX += lookInput.y * SensitivityX;
                    }
                }
                else
                {
                    RotationY += lookInput.x * SensitivityY / ControllerSensitivityCorrection;
                    if (InvertYAxis)
                    {
                        RotationX -= lookInput.y * SensitivityX / ControllerSensitivityCorrection;
                    }
                    else
                    {
                        RotationX += lookInput.y * SensitivityX / ControllerSensitivityCorrection;
                    }
                }
                //Clamps rotation so player cant break their neck up and down
                RotationX = Mathf.Clamp(RotationX, MinimumX, MaximumX);
                //Rotates playerObject with the camera
                transform.localEulerAngles = new Vector3(0, RotationY, 0);
                //line that actualy rotates the camera on x and y axis
                CameraReference.transform.localEulerAngles = new Vector3(-RotationX, RotationY, 0);
            }
        }
        else if (EnemyTarget != null && TargetLock == true)
        {
            TutorialHasPressedTab = true;

            //stores the needed target rotation in a tempary variable
            Transform targetTransform = transform;
            targetTransform.transform.LookAt(LockOnLocation);

            if (HasReachedLockedPosition == false)
            {
                float perc = CurrentLerpTime / LerpTime;

                // Smoothly rotate towards the target point.
                CameraReference.transform.rotation = Quaternion.Slerp(CameraReference.transform.rotation, targetTransform.transform.rotation, perc);
            }

            transform.localEulerAngles = new Vector3(0, CameraReference.transform.localEulerAngles.y, 0);

            RotationY = CameraReference.transform.localEulerAngles.y;

            if (CameraReference.transform.localEulerAngles.x < 50)
            {
                RotationX = -CameraReference.transform.localEulerAngles.x;
            }
            else
            {
                RotationX = 360 - CameraReference.transform.localEulerAngles.x;
            }
        }

        if (EnemyTarget == null)
        {
            if (TargetLock)
            {
                UnlockCamera();
            }
        }
        CurrentLerpTime += Time.deltaTime;

        if (CurrentLerpTime >= LerpTime)
        {
            CurrentLerpTime = LerpTime;
        }

        if (Mathf.Abs(MovementController.m_Velocity.magnitude) <= CameraBobThreshold)
        {
            BobTime = 0.0f;
        }
        else
        {
            BobTime += Time.deltaTime;
        }
        // JiggleCamera(bobTime);
    }

    void LateUpdate()
    {
        PlayerBody.transform.localEulerAngles = new Vector3(CameraReference.transform.localEulerAngles.x, 0, 0);
        PlayerSlashZone.transform.localEulerAngles = new Vector3(CameraReference.transform.localEulerAngles.x - 90.0f, 0, 0);
        CameraReference.transform.position = CameraAnchor.position;
    }

    public void Respawn()
    {
        RotationX = 0.0f;
        RotationY = 0.0f;
        EnemyTarget = null;
    }

    void UnlockCamera()
    {
        if (TargetLock)
        {
            SetInputEnabled(true);
            HasReachedLockedPosition = false;
        }
    }

    void LockCamera()
    {
        SetInputEnabled(false);
        HasReachedLockedPosition = false;
    }

    public Enemy FindNearestEnemy()
    {
        Enemy[] Enemys;
        Enemy ClosestEnemy = null;

        Enemys = FindObjectsOfType<Enemy>();

        float tempDistance = float.PositiveInfinity;

        foreach (Enemy enemy in Enemys)
        {
            float distance = Vector3.Distance(gameObject.transform.position, enemy.transform.position);

            if (distance < tempDistance)
            {
                tempDistance = distance;
                ClosestEnemy = enemy;
            }
        }
        return ClosestEnemy;
    }

    public void SetInputEnabled(bool inputEnabled)
    {
        IsInputEnabled = inputEnabled;
    }

    public void LockToEnemy()
    {
        EnemyTarget = FindNearestEnemy();

        if (EnemyTarget != null)
        {
            if (IsEnemyInView())
            {
                if (EnemyTarget.LockOnLocation == null)
                {
                    LockOnLocation = EnemyTarget.transform;
                }
                else
                {
                    LockOnLocation = EnemyTarget.LockOnLocation;
                }
                if (Vector3.Distance(gameObject.transform.position, LockOnLocation.position) > MaximumLockonDistance)
                {
                    EnemyTarget = null;
                }
                else
                {
                    if (TargetLock == true)
                    {
                        UnlockCamera();
                    }
                    else
                    {
                        LockCamera();
                        CurrentLerpTime = 0.0f;
                    }
                    TargetLock = !TargetLock;
                }
            }
        }
    }

    bool IsEnemyInView()
    {
        //check if we already have an enemy to save resources
        if (EnemyTarget == null)
        {
            EnemyTarget = FindNearestEnemy();
        }

        Camera cam = CameraReference;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        //if we have an enemy check to see if they are in the camera view
        if (EnemyTarget != null)
        {
            if (GeometryUtility.TestPlanesAABB(planes, EnemyTarget.GetComponent<Collider>().bounds))
            {
                // Enemy is in camera Frestrum
                return true;
            }
            else
            {
                //no enemy detected
                return false;
            }
        }
        //No enemy in range
        return false;
    }

    //this function is not 100% complete because we abandoned the idea near it being finished
    void JiggleCamera(float time)
    {
        Vector3 vel = MovementController.m_Velocity;
        vel.y = 0;
        float velocity = vel.magnitude;
        float lerp = 0.0f;

        if (Mathf.Abs(velocity) <= 0.0001f)
        {
            lerp += Time.deltaTime;
            //TODO: lerp from current offset to zero;
            CameraJiggleOffset.y = Mathf.Lerp(CameraJiggleOffset.y, Vector3.zero.y, lerp / 0.2f);
        }
        else
        {
            lerp = 0.0f;
            CameraJiggleOffset = Vector3.zero;
        }
        //TODO: make starting walking look better
        float bobOscillate = Mathf.Sin((time * velocity * (2 * Mathf.PI)) * 0.05f);//the magic number gets us the speed of which the cycle completes a loop (Smaller is slower)

        //TODO: MAKE THIS A COMFORTABLE RANGE WITH THE NEW PLAYER MODEL
        float ocilateRange = 0.1f * bobOscillate;//How far up and down the camera will shift when walking

        CameraJiggleOffset.y += ocilateRange; //Oscillate the position in y (up) axis

    }
}
