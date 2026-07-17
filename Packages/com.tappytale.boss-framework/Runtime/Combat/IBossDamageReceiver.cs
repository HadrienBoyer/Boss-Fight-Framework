using UnityEngine;

namespace TappyTale.BossFight.Combat
{
    public interface IBossDamageReceiver
    {
        bool ReceiveDamage(float amount, GameObject source);
    }
}