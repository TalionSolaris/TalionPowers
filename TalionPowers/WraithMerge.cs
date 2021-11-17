using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace TalionPowers
{
    class WraithMerge : SpellMergeData
    {
        public override void Merge(bool active)
        {
            base.Merge(active);
            if (currentCharge < 0.8)
                return;
            Catalog.GetData<ItemData>("WraithGlaive").SpawnAsync(glaive =>
            {
                glaive.transform.position = Vector3.Lerp(Player.currentCreature.handLeft.transform.position,
                    Player.currentCreature.handRight.transform.position, 0.5f);
                glaive.rb.useGravity = false;
                glaive.OnGrabEvent += Glaive_OnGrabEvent;
                glaive.OnUngrabEvent += Glaive_OnUngrabEvent;
                GameManager.local.StartCoroutine(Timer(() =>
                {
                    if (glaive.IsHanded())
                        return;
                    glaive.Despawn();
                }, 3));
            });
        }

        private void Glaive_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            GameManager.local.StartCoroutine(Timer(() =>
            {
                if (handle.item.IsHanded())
                    return;
                handle.item.Despawn();
            }, 3));
        }

        private void Glaive_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            handle.item.rb.useGravity = true;
        }

        public IEnumerator Timer(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action.Invoke();
        }
    }
}
