using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class cameraTriggerHandler : MonoBehaviour
{
    public void TriggerShake(float duration, float strength, int vibrato, float randomness)
    {
        this.transform.DOShakePosition(duration, strength, vibrato, randomness, false, true);
    }

    public void TriggerShake()
    {
        this.transform.DOShakePosition(0.5f, 0.5f, 10, 90, false, true);
    }
}
