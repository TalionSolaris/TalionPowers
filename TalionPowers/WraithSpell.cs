using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace TalionPowers
{
    class WraithSpell : SpellCastCharge
    {
        public override void UpdateCaster()
        {
            if (spellCaster.isFiring && currentCharge == 1 &&
                PlayerControl.GetHand(spellCaster.ragdollHand.side).gripPressed)
            {
                spellCaster.isFiring = false;
                currentCharge = 0;
                Fire(false);
                Catalog.GetData<ItemData>("WraithBow").SpawnAsync(spawnedBow =>
                {
                    if (spellCaster.ragdollHand.side == Side.Left)
                    {
                        spawnedBow.transform.rotation =
                            spellCaster.ragdollHand.transform.rotation * Quaternion.Euler(0, 0, 180);
                        spellCaster.ragdollHand.Grab(spawnedBow.mainHandleLeft);
                    }
                    else
                    {
                        spawnedBow.transform.rotation = spellCaster.ragdollHand.transform.rotation;
                        spellCaster.ragdollHand.Grab(spawnedBow.mainHandleRight);
                    }

                    spawnedBow.OnGrabEvent += SpawnedBow_OnGrabEvent;
                    spawnedBow.OnUngrabEvent += SpawnedBow_OnUngrabEvent;
                }, spellCaster.ragdollHand.transform.position);
            }

            base.UpdateCaster();
        }

        private void SpawnedBow_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            GameManager.local.StartCoroutine(Timer(() =>
            {
                if (handle.item.IsHanded())
                    return;
                handle.item.Despawn();
            }, 0.1f));
            handle.item.data.GetModule<ItemModuleBow>().autoSpawnArrow = false;
        }

        private void SpawnedBow_OnGrabEvent(Handle handle, RagdollHand ragdollHand) =>
            handle.item.data.GetModule<ItemModuleBow>().autoSpawnArrow = true;

        public IEnumerator Timer(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action.Invoke();
        }
    }
}