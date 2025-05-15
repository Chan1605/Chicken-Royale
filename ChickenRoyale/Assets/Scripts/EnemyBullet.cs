using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int Damage { get; private set; }

    public void SetDamage(int damage)
    {
        Damage = damage;
    }
}
