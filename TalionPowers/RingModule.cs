using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace TalionPowers
{
    class RingModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<RingModuleMB>();
            base.OnItemLoaded(item);
        }
    }

    class RingModuleMB : MonoBehaviour
    {
        private Item ring;
        private RagdollHand ringHand;
        private Creature corruptCreature;
        private EffectInstance corruptEffect;
        private bool corrupting;
        private bool wearing;

        public void Awake()
        {
            ring = GetComponentInParent<Item>();
            ring.OnSnapEvent += Ring_OnSnapEvent;
            ring.OnUnSnapEvent += Ring_OnUnSnapEvent;
        }

        public void Update()
        {
            if (ring && ring.holder != null)
            {
                wearing = true;
            }
            else if (ring && ring.holder == null)
            {
                wearing = false;
            }

            if (wearing && ringHand && ringHand.grabbedHandle && ringHand.grabbedHandle.item)
            {
                if (ring.data.id == "CelebrimborRing" &&
                    (ringHand.grabbedHandle.item.data.id == "BrightLordHammer" ||
                     ringHand.grabbedHandle.item.data.id == "NazgulHammer") &&
                    PlayerControl.GetHand(ringHand.side).alternateUsePressed)
                {
                    ringHand.grabbedHandle.item.Despawn();
                    Catalog.GetData<ItemData>("WraithHammer").SpawnAsync(spawnedHammer =>
                    {
                        if (ringHand.side == Side.Left)
                        {
                            ringHand.Grab(spawnedHammer.mainHandleLeft);
                        }
                        else if (ringHand.side == Side.Right)
                        {
                            ringHand.Grab(spawnedHammer.mainHandleRight);
                        }
                    }, ringHand.transform.position);
                }
                else if (ring.data.id == "IsildurRing" &&
                         (ringHand.grabbedHandle.item.data.id == "BrightLordHammer" ||
                          ringHand.grabbedHandle.item.data.id == "WraithHammer") &&
                         PlayerControl.GetHand(ringHand.side).alternateUsePressed)
                {
                    ringHand.grabbedHandle.item.Despawn();
                    Catalog.GetData<ItemData>("NazgulHammer").SpawnAsync(spawnedHammer =>
                    {
                        if (ringHand.side == Side.Left)
                        {
                            ringHand.Grab(spawnedHammer.mainHandleLeft);
                        }
                        else if (ringHand.side == Side.Right)
                        {
                            ringHand.Grab(spawnedHammer.mainHandleRight);
                        }
                    }, ringHand.transform.position);
                }
            }

            if (wearing)
            {
                if (ringHand.caster.telekinesis.catchedHandle &&
                    ringHand.caster.telekinesis.catchedHandle.name == "HandleNeck" && !corrupting)
                {
                    Dominion(ringHand.caster.telekinesis.catchedHandle);
                }
            }
        }

        private void Ring_OnUnSnapEvent(Holder holder)
        {
            ringHand.OnGrabEvent -= RingHand_OnGrabEvent;
            ringHand.OnUnGrabEvent -= RingHand_OnUnGrabEvent;
            ringHand = null;
        }

        private void Ring_OnSnapEvent(Holder holder)
        {
            ringHand = holder.GetComponentInParent<RagdollHand>();
            ringHand.OnGrabEvent += RingHand_OnGrabEvent;
            ringHand.OnUnGrabEvent += RingHand_OnUnGrabEvent;
        }

        private void RingHand_OnUnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime)
        {
            if (corrupting)
            {
                corrupting = false;
            }
        }

        private void RingHand_OnGrabEvent(Side side, Handle handle, float axisPosition, HandleOrientation orientation,
            EventTime eventTime) => Dominion(handle);

        private void Dominion(Handle handle)
        {
            if (handle.gameObject.GetComponentInParent<Creature>() &&
                (handle.name == "HandleNeck" || handle.name == "HandleSkull") && wearing &&
                handle.gameObject.GetComponentInParent<Creature>() != Player.currentCreature &&
                !handle.gameObject.GetComponentInParent<Creature>().ragdoll.headPart.isSliced)
            {
                corruptEffect = Catalog.GetData<EffectData>("Domination")
                    .Spawn(handle.GetComponentInParent<Creature>().ragdoll.headPart.transform);
                corruptEffect.SetVfxFloat("Size", 0.3f);
                corruptEffect.onEffectFinished += CorruptEffect_onEffectFinished;
                if (ring.data.id == "IsildurRing")
                {
                    corruptEffect.SetVfxVector3("Color", new Vector3(0, 1, 0));
                }
                else if (ring.data.id == "CelebrimborRing")
                {
                    corruptEffect.SetVfxVector3("Color", new Vector3(0, 1, 1));
                }

                corruptEffect.Play();
                corrupting = true;
                StartCoroutine(Timer(() =>
                {
                    if (!corrupting)
                        return;
                    corruptCreature = handle.gameObject.GetComponentInParent<Creature>();
                    if (ring.data.id == "IsildurRing")
                    {
                        foreach (CreatureData.EyeColor color in corruptCreature.data.eyesColors)
                        {
                            color.iris = Color.green;
                            color.sclera = Color.black;
                        }

                        corruptCreature.SetColor(Color.green, Creature.ColorModifier.EyesIris, true);
                        corruptCreature.SetColor(Color.black, Creature.ColorModifier.EyesSclera, true);
                        if (corruptCreature.isKilled)
                        {
                            corruptCreature.Resurrect(corruptCreature.maxHealth, corruptCreature);
                            corruptCreature.brain.Load(corruptCreature.brain.instance.id);
                        }

                        corruptCreature.SetFaction(2);
                        corruptCreature.brain.Load(corruptCreature.brain.instance.id);
                        corrupting = false;
                    }
                    else if (ring.data.id == "CelebrimborRing" && !corruptCreature.isKilled)
                    {
                        foreach (CreatureData.EyeColor color in corruptCreature.data.eyesColors)
                        {
                            color.iris = Color.white;
                            color.sclera = new Color(0, 1, 1);
                        }

                        corruptCreature.SetColor(Color.white, Creature.ColorModifier.EyesIris, true);
                        corruptCreature.SetColor(new Color(0, 1, 1), Creature.ColorModifier.EyesSclera, true);
                        corruptCreature.SetFaction(2);
                        corruptCreature.brain.Load(corruptCreature.brain.instance.id);
                        corrupting = false;
                    }
                }, 3));
            }
        }

        private void CorruptEffect_onEffectFinished(EffectInstance effectInstance) => effectInstance.Despawn();

        public IEnumerator Timer(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action.Invoke();
        }
    }

    public static class Manager
    {
        public static void ForEachEffect(this EffectInstance effect, Action<Effect> action)
        {
            effect.effects.ForEach(action);
        }

        public static void SetVfxVector3(this EffectInstance effect, string property, Vector3 value)
        {
            effect.ForEachEffect(effectChild =>
            {
                if (effectChild is EffectVfx vfx && vfx.vfx.HasVector3(property))
                    vfx.vfx.SetVector3(property, value);
            });
        }

        public static void SetVfxFloat(this EffectInstance effect, string property, float value)
        {
            effect.ForEachEffect(effectChild =>
            {
                if (effectChild is EffectVfx vfx && vfx.vfx.HasFloat(property))
                    vfx.vfx.SetFloat(property, value);
            });
        }
    }
}