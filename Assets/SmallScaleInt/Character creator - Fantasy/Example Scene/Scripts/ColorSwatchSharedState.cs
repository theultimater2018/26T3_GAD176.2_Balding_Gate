using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SmallScaleInc.CharacterCreatorFantasy
{
    [DisallowMultipleComponent]
    public sealed class ColorSwatchSharedState : MonoBehaviour
    {
        [NonSerialized] public TextMeshProUGUI CurrentTMPColorTarget;
        [NonSerialized] public SpriteRenderer CurrentTarget;
        [NonSerialized] public List<SpriteRenderer> CurrentWeaponTargets;
        [NonSerialized] public bool IsSetup;
    }
}
