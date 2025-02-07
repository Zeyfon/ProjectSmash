﻿using PSmash.Attributes;
using PSmash.Inventories;
using PSmash.Movement;
using PSmash.Stats;
using System;
using System.Collections;
using UnityEngine;

namespace PSmash.Combat
{
    public class PlayerFighter : MonoBehaviour
    {

        [Header("General Info")]
        [SerializeField] LayerMask whatIsDamagable;
        [SerializeField] LayerMask whatIsHooked;
        [SerializeField] LayerMask whatIsEnemy;

        [Header("Combo Attack")]
        [SerializeField] Transform attackTransform = null;

        [Header("Tool Attack")]
        [SerializeField] AudioClip noHookSound = null;
        [SerializeField] Weapon grapingHook = null;
        [SerializeField] HookRope hookRope = null;
        [SerializeField] float hookPullingPlayerSpeed = 30;

        [Header("Finishing Move")]
        [SerializeField] AudioClip finisherSound = null;
        [SerializeField] Transform steamTransform = null;
        [SerializeField]  GameObject finisherPS = null;

        [Header("Guard")]
        [SerializeField] AudioClip guardFootstepSound = null;

        public static event Action OnCameraShake;

        public delegate void FinisherCamera(bool enableFinisherCamera);
        public static event FinisherCamera OnFinisherCamera;

        BaseStats baseStats;
        Animator animator;
        AudioSource audioSource;
        Transform targetTransform;
        Vector2 damageArea;
        PlayerMovement movement;
        Transform enemyTransform;
        HookRope rope;
        Coroutine coroutine;

        void Awake()
        {
            baseStats = GetComponent<BaseStats>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            movement = GetComponent<PlayerMovement>();
        }

        #region Finisher
        public bool IsEnemyStunned()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, 1), transform.right, 2, whatIsEnemy);
            if (!hit) 
                return false;
            targetTransform = hit.transform;
            return hit.transform.GetComponent<EnemyHealth>().IsStunned();
        }


        public void FinisherMove()
        {
            StartCoroutine(DoFinisherMove());
        }

        IEnumerator DoFinisherMove()
        {
            targetTransform.GetComponent<EnemyHealth>().SetStateToFinisher();
            gameObject.layer = LayerMask.NameToLayer("PlayerGhost");
            PositionPlayerInFronOfEnemy();
            OnFinisherCamera(true);
            StartCoroutine(StartPlayerAndTargetFinisherAnimations(targetTransform));
            yield return null;
        }

        void PositionPlayerInFronOfEnemy()
        {
            if (transform.position.x - targetTransform.transform.position.x > 0)
            {
                //Player is at right side of enemy
                GetComponent<Rigidbody2D>().MovePosition(targetTransform.position + new Vector3(1, 0, 0));
            }
            else
            {
                //Player is at left side of enemy
                GetComponent<Rigidbody2D>().MovePosition(targetTransform.position + new Vector3(-1, 0, 0));
            }
        }
        IEnumerator StartPlayerAndTargetFinisherAnimations(Transform targetTransform)
        {
            animator.SetInteger("Attack", 80);
            while (animator.GetInteger("Attack") != 81)
            {
                yield return null;
            }
            targetTransform.GetComponent<EnemyHealth>().StartFinisherAnimation();
            yield return null;
        }

        public void EndingFinisherMove()
        {
            OnFinisherCamera(false);
        }

        public Transform CanDoUtility()
        {
            Transform hookTarget = GetComponentInChildren<GrapingHookTargetDetector>().GetHookTarget();
            return hookTarget;
        }

        public void SubweaponUtility(Transform hookTarget)
        {
            animator.SetInteger("Attack", 70);
            coroutine = StartCoroutine(ThrowHook(hookTarget));
        }

        public void GrapingHook()
        {
            if(coroutine == null)
                StartCoroutine(ThrowHook(null));
        }

        IEnumerator ThrowHook(Transform targetForMobility)
        {
            rope = Instantiate(hookRope, attackTransform.position, Quaternion.identity, attackTransform);

            if (targetForMobility == null)
            {
                yield return rope.DoHookShot(null);
                enemyTransform = rope.EnemyHitByHook();
                if (enemyTransform != null)
                {
                    if (CanEnemyBePulled(enemyTransform))
                    {
                        enemyTransform.GetComponent<IGrapingHook>().Hooked();
                        animator.SetInteger("Attack", 72);
                        yield return new WaitForSeconds(0.25f);
                        yield return PullingEnemyWithGrapingHook();
                        animator.SetInteger("Attack", 75);
                        coroutine = null;
                        yield break;
                    }
                    else
                    {         
                        audioSource.PlayOneShot(noHookSound);
                    }
                }
                Destroy(rope.gameObject, 0.01f);
                animator.SetInteger("Attack", 78);
            }
            else
            {
                yield return rope.DoHookShot(targetForMobility);
                animator.SetInteger("Attack", 73);
                yield return null;
                yield return PulledTowardsTarget(targetForMobility);
                Destroy(rope.gameObject, 0.01f);
                animator.SetInteger("Attack", 75);
            }
            coroutine = null;
        }

        bool CanEnemyBePulled(Transform enemyTransform)
        {
            return GetComponent<Equipment>().GetEquippedSubweapon().GetMyLevel() >= enemyTransform.GetComponent<EnemyHealth>().GetMyLevel();
        }

        IEnumerator PullingEnemyWithGrapingHook()
        {
            rope.SetRopeToFollowEnemyDisplacement(enemyTransform);
            float distance = Mathf.Infinity;
            enemyTransform.GetComponent<IGrapingHook>().Pulled();
            while (distance>1.25)
            {
                print("Checking Distance");
                distance = Vector3.Distance(enemyTransform.position, transform.position);
                print(distance);
                yield return null;
            }
        }

        IEnumerator PulledTowardsTarget(Transform target)
        {
            rope.PlayerPulled();
            Vector2 direction = ((target.position + new Vector3(0, 2)) - transform.position).normalized;
            movement.GrapingHookMovement(direction, hookPullingPlayerSpeed);
            float distance = Mathf.Infinity;
            while (distance > 3)
            {
                distance = Vector3.Distance(target.position, transform.position);
                movement.GrapingHookMovement(direction, hookPullingPlayerSpeed);
                yield return new WaitForFixedUpdate();
            }
        }


        //Anim Event
        void StartSteam()
        {
            steamTransform.GetComponent<ParticleSystem>().Play();
        }

        //Anim Event
        void StopSteam()
        {
            steamTransform.GetComponent<ParticleSystem>().Stop();
        }

        //Anim Event
        void FinisherAttack()
        {
            float finisherDamage = ((int)baseStats.GetStat(StatsList.Attack) + GetComponent<Equipment>().GetMainWeapon().GetDamage()) * 2;
            print(targetTransform.name);
            targetTransform.GetComponent<EnemyHealth>().TakeFinisherAttackDamage(transform.position, finisherDamage);
            Instantiate(finisherPS, attackTransform.position, Quaternion.identity);
            OnCameraShake();
        }

        //Anim Event
        void FinisherSound()
        {
            audioSource.pitch = 0.3f;
            audioSource.PlayOneShot(finisherSound);
        }

        #endregion

        #region Guard/Parry
        //AnimEvent
        void GuardFootstepSound()
        {
            audioSource.PlayOneShot(guardFootstepSound);
        }

        #endregion

        #region ComboAttack
        //Anim Event
        void AttackDamage(int index)
        {
            //print("NormalAttack");
            Weapon weapon = GetComponent<Equipment>().GetMainWeapon();
            float extraWeaponDamage = GetComponent<Equipment>().GetExtraWeaponDamage(weapon);
            damageArea = weapon.GetWeaponDamageArea();
            //damageArea = new Vector2(1.9f, 1.6f);
            Attack(attackTransform, weapon, extraWeaponDamage);
        }

        /// <summary>
        /// AnimEvent for Attacks
        /// </summary>
        /// <returns></returns>
        void AttackImpulse(int index)
        {
            float attackImpulse;
            if (index == 1)
            {
                attackImpulse = GetComponent<Equipment>().GetMainWeaponAttackImpulse();
            }
            else
            {
                attackImpulse = GetComponent<Equipment>().GetSubWeaponAttackImpulse();
            }
            GetComponent<PlayerMovement>().MovementImpulse(attackImpulse);
        }

        void AttackSound(int index)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.75f, 1.1f);
            audioSource.PlayOneShot(GetComponent<Equipment>().GetMainWeapon().GetWeaponAttackAudioClip());
        }

        #endregion

        #region SubWeaponAttack

        //Anim Event
        void SubWeaponAttackDamage()
        {
            Weapon weapon = GetComponent<Equipment>().GetEquippedSubweapon();
            print(weapon.displayName);
            float extraWeaponDamage = GetComponent<Equipment>().GetExtraWeaponDamage(weapon);
            damageArea = weapon.GetWeaponDamageArea();
            Attack(attackTransform, weapon, extraWeaponDamage);
        }

        //AnimEvent
        void SubWeaponAttackSound(int index)
        {
            //if(index == 1)
            //{
                audioSource.pitch = UnityEngine.Random.Range(0.75f, 1.5f);
                audioSource.PlayOneShot(GetComponent<Equipment>().GetEquippedSubweapon().GetWeaponAttackAudioClip());
            //}

        }

        #endregion

        void Attack(Transform attackOriginPosition, Weapon weapon, float extraWeaponDamage)
        {
            //print("Looking to Damage Enemy");
            Collider2D[] colls = Physics2D.OverlapBoxAll(attackOriginPosition.position, damageArea, 0, whatIsDamagable);
            if (colls.Length == 0)
            {
                //print("Nothing was damaged");
                return;
            }
            foreach (Collider2D coll in colls)
            {
                IDamagable target = coll.GetComponent<IDamagable>();
                if (target == null || coll.GetComponent<Projectile>())
                    continue;
                //print(currentWeapon.name);
                float damage = extraWeaponDamage + baseStats.GetStat(StatsList.Attack);
                target.TakeDamage(transform, weapon, AttackType.NotUnblockable, damage , weapon.GetKnockbackForceToApplyToEnemyAttacked());
                //print("Sendingdamage from player to  " + target);

            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(attackTransform.position, damageArea);
        }
    }
}

