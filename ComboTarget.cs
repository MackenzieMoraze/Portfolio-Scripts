using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboTarget : MonoBehaviour
{
    private const int ComboCompleteDamage = 50;
    [SerializeField] Material DiagonalSlashTL;
    [SerializeField] Material DiagonalSlashTR;
    [SerializeField] Material HorizontalSlash;
    [SerializeField] Material VerticalSlash;

    [SerializeField] AttackDirection DamageDirection, SecondaryDamageDirection;

    bool TopLeftDone;
    bool TopRightDone;
    bool VerticalDone;
    bool HorizontalDone;
    bool NextDirCalculated;

    public bool ComboCompleted;
    public bool IsTutorial;
    public int NumOfStrikes;
    public ParticleSystemRenderer ParticleEffectSlash;

    Enemy EnemySelf;

    // Use this for initialization
    void Start()
    {
        EnemySelf = GetComponentInParent<Enemy>();
        GetRandomSliceDirection();
        ParticleEffectSlash.transform.position = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (NumOfStrikes == 1 && !NextDirCalculated)
        {
            GetRandomSliceDirection();
        }
        if (NumOfStrikes == 2 && !NextDirCalculated)
        {
            GetRandomSliceDirection();
        }
        if (NumOfStrikes == 3)
        {
            ComboCompleted = true;
            NumOfStrikes = 0;
            gameObject.SetActive(false);
        }
    }

    private void GetRandomSliceDirection()
    {
        //Get random number out of possible directions and assgning it as next if it hasent been used before
        int randint = Random.Range(0, 4);

        if (randint == 0 && !HorizontalDone)
        {
            ParticleEffectSlash.material = HorizontalSlash;
            DamageDirection = AttackDirection.East;
            SecondaryDamageDirection = AttackDirection.West;
            HorizontalDone = true;
            NextDirCalculated = true;
        }

        if (randint == 1 && !TopRightDone)
        {
            ParticleEffectSlash.material = DiagonalSlashTL;
            DamageDirection = AttackDirection.NorthWest;
            SecondaryDamageDirection = AttackDirection.SouthEast;
            TopRightDone = true;
            NextDirCalculated = true;
        }

        if (randint == 2 && !TopLeftDone)
        {
            ParticleEffectSlash.material = DiagonalSlashTR;
            DamageDirection = AttackDirection.NorthEast;
            SecondaryDamageDirection = AttackDirection.SouthWest;
            TopLeftDone = true;
            NextDirCalculated = true;
        }

        if (randint == 3 && !VerticalDone)
        {
            ParticleEffectSlash.material = VerticalSlash;
            DamageDirection = AttackDirection.North;
            SecondaryDamageDirection = AttackDirection.South;
            VerticalDone = true;
            NextDirCalculated = true;
        }

        // if new attack direction hasent been selected yet, run this function again
        if (!NextDirCalculated)
        {
            GetRandomSliceDirection();
        }
    }

    public void Reset()
    {
        NumOfStrikes = 0;
        ComboCompleted = false;
        TopLeftDone = false;
        TopRightDone = false;
        VerticalDone = false;
        HorizontalDone = false;
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerSlashZone"))
        {
            Player player = Services.GameManager.Player;

            if ((player.CombatManager.CurrentAttackDirection == DamageDirection || SecondaryDamageDirection == AttackDirection.Any) 
            || (player.CombatManager.CurrentAttackDirection == SecondaryDamageDirection || DamageDirection == AttackDirection.Any) 
            && !player.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                NextDirCalculated = false;
                NumOfStrikes++;

                if (NumOfStrikes == 3)
                {
                    if (EnemySelf != null)
                    {
                        if (IsTutorial == true)
                        {
                            EnemySelf.m_CurrentHealth = 0;
                        }
                        EnemySelf.TakeDamage(ComboCompleteDamage);
                        EnemySelf.m_Marked = false;
                    }
                }
            }
        }
    }
}
