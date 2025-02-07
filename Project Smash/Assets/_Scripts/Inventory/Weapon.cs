﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSmash.Inventories
{
    [CreateAssetMenu(menuName = "Items/Weapon")]
    public class Weapon : Item
    {
        [Header("ANIMATION INFO")]
        [SerializeField] int animatorIntValue = 0;
        [SerializeField] AudioClip weaponAttackAudioClip = null;

        [Header("VALUES")]
        [SerializeField] Vector2 weaponDamageArea;
        [SerializeField] float damage;
        [SerializeField] float weaponAttackImpulse = 12f;
        [Range(0,2)]
        [SerializeField] float knockbackForceToApplyToEnemyAttacked;
        [Range(0,1f)]
        [SerializeField] float attackForceTime;

        public float GetDamage()
        {
            return damage;
        }

        public float GetWeaponAttackImpulse()
        {
            return weaponAttackImpulse;
        }

        public float GetKnockbackForceToApplyToEnemyAttacked()
        {
            return knockbackForceToApplyToEnemyAttacked;
        }

        public float GetAttackForceTime()
        {
            return attackForceTime;
        }

        public int GetAnimatorInt()
        {
            return animatorIntValue;
        }

        public Vector2 GetWeaponDamageArea()
        {
            return weaponDamageArea;
        }
        public AudioClip GetWeaponAttackAudioClip()
        {
            return weaponAttackAudioClip;
        }

    }
}
