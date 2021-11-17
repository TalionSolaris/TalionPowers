using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TalionPowers
{
    class RingLevel : LevelModule
    {
        private Holder ringHolderLeft;
        private Holder ringHolderRight;

        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onLevelLoad += EventManager_onLevelLoad;
            EventManager.onPossess += EventManager_onPossess;
            return base.OnLoadCoroutine();
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime != EventTime.OnEnd || !Player.currentCreature)
                return;
            ringHolderLeft = CreateHolderLeft(creature.handLeft.fingerIndex.proximal.mesh.gameObject,
                new Vector3(-0.0263f, 0.0008f, -0.001f), new Vector3(6.704f, -4.683f, -87.035f));
            ringHolderRight = CreateHolderRight(creature.handRight.fingerIndex.proximal.mesh.gameObject,
                new Vector3(-0.0303f, -0.0021f, -0.0018f), new Vector3(-0.132f, -4.668f, -91.618f));
            ringHolderLeft.allowedHandSide = Interactable.HandSide.Right;
            ringHolderRight.allowedHandSide = Interactable.HandSide.Left;
            ringHolderLeft.UnSnapped += RingHolderUnsnapped;
            ringHolderRight.UnSnapped += RingHolderUnsnapped;
            ringHolderLeft.Snapped += RingHolderLeftSnapped;
            ringHolderRight.Snapped += RingHolderRightSnapped;
            creature.handLeft.OnGrabEvent += Hand_OnGrabEvent;
            creature.handLeft.OnUnGrabEvent += HandLeft_OnUnGrabEvent;
            creature.handRight.OnGrabEvent += Hand_OnGrabEvent;
            creature.handRight.OnUnGrabEvent += HandRight_OnUnGrabEvent;
            creature.equipment.holders.Add(ringHolderLeft);
            creature.equipment.holders.Add(ringHolderRight);
            creature.equipment.EquipWeapons();
        }

        private void HandLeft_OnUnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime)
        {
            if (side == Side.Left)
            {
                ResetHolderPosition(ringHolderLeft);
            }
        }

        private void HandRight_OnUnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime)
        {
            if (side == Side.Right)
            {
                ResetHolderPosition(ringHolderRight);
            }
        }

        private void Hand_OnGrabEvent(Side side, Handle handle, float axisPosition, HandleOrientation orientation,
            EventTime eventTime)
        {
            if (handle.item.data.slot != "Ring")
            {
                if (side == Side.Left && ringHolderLeft && !ringHolderLeft.HasSlotFree())
                {
                    ringHolderLeft.transform.parent =
                        Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                    ringHolderLeft.transform.position =
                        (Player.currentCreature.handLeft.fingerIndex.proximal.mesh.position +
                         Player.currentCreature.handLeft.fingerIndex.intermediate.mesh.position) / 2;
                    ringHolderLeft.transform.rotation =
                        Player.currentCreature.handLeft.fingerIndex.proximal.mesh.rotation *
                        Quaternion.Euler(0, 0, 90);
                }

                if (side == Side.Right && ringHolderRight && !ringHolderRight.HasSlotFree())
                {
                    ringHolderRight.transform.parent =
                        Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                    ringHolderRight.transform.position =
                        (Player.currentCreature.handRight.fingerIndex.proximal.mesh.position +
                         Player.currentCreature.handRight.fingerIndex.intermediate.mesh.position) / 2;
                    ringHolderRight.transform.rotation =
                        Player.currentCreature.handRight.fingerIndex.proximal.mesh.rotation *
                        Quaternion.Euler(0, 0, 90);
                }
            }
        }

        private void RingHolderLeftSnapped(Item item)
        {
            foreach (Handle ringHandle in item.GetComponentsInChildren<Handle>())
            {
                ringHandle.allowedHandSide = Interactable.HandSide.Right;
            }
        }

        private void RingHolderRightSnapped(Item item)
        {
            foreach (Handle ringHandle in item.GetComponentsInChildren<Handle>())
            {
                ringHandle.allowedHandSide = Interactable.HandSide.Left;
            }
        }

        private void RingHolderUnsnapped(Item item)
        {
            foreach (Handle ringHandle in item.GetComponentsInChildren<Handle>())
            {
                ringHandle.allowedHandSide = Interactable.HandSide.Both;
            }
        }

        private void EventManager_onLevelLoad(LevelData levelData, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                if (!Player.local?.creature)
                    return;
                if (Player.characterData != null)
                {
                    for (int i = Player.characterData.inventory.Count - 1; i >= 0; i--)
                    {
                        if (Player.characterData.inventory[i].itemData.type != ItemData.Type.Wardrobe &&
                            Player.characterData.inventory[i].itemData.type != ItemData.Type.Spell)
                        {
                            Player.characterData.inventory.RemoveAt(i);
                        }
                    }

                    foreach (Holder holder in Player.currentCreature.equipment.holders)
                    {
                        if (holder.items.Count > 0 && holder.items[0])
                        {
                            Item item = holder.items[0];
                            Player.characterData.inventory.Add(new ContainerData.Content(item.data,
                                (item.savedValues != null) ? new List<Item.SavedValue>(item.savedValues) : null));
                        }
                    }

                    DataManager.SaveCharacter(Player.characterData);
                }
            }
        }

        private Holder CreateHolderLeft(GameObject parent, Vector3 pos, Vector3 rot)
        {
            HolderData data = Catalog.GetData<HolderData>("HolderRingLeft");
            if (data != null)
            {
                GameObject holderGameObject = new GameObject($"{data.id}-holder");
                holderGameObject.transform.parent = parent.transform;
                holderGameObject.transform.localPosition = pos;
                holderGameObject.transform.localEulerAngles = rot;
                Holder holder = holderGameObject.AddComponent<Holder>();
                holder.Load(data);
                holder.RefreshChildAndParentHolder();
                Debug.Log($"Added customHolderData:{data.id}");
                return holder;
            }

            return null;
        }

        private Holder CreateHolderRight(GameObject parent, Vector3 pos, Vector3 rot)
        {
            HolderData data = Catalog.GetData<HolderData>("HolderRingRight");
            if (data != null)
            {
                GameObject holderGameObject = new GameObject($"{data.id}-holder");
                holderGameObject.transform.parent = parent.transform;
                holderGameObject.transform.localPosition = pos;
                holderGameObject.transform.localEulerAngles = rot;
                Holder holder = holderGameObject.AddComponent<Holder>();
                holder.Load(data);
                holder.RefreshChildAndParentHolder();
                Debug.Log($"Added customHolderData:{data.id}");
                return holder;
            }

            return null;
        }

        private void ResetHolderPosition(Holder holder)
        {
            if (holder == ringHolderLeft)
            {
                holder.transform.parent = Player.currentCreature.handLeft.fingerIndex.proximal.mesh;
                holder.transform.localPosition = new Vector3(-0.0263f, 0.0008f, -0.001f);
                holder.transform.localEulerAngles = new Vector3(6.704f, -4.683f, -87.035f);
            }
            else if (holder == ringHolderRight)
            {
                holder.transform.parent = Player.currentCreature.handRight.fingerIndex.proximal.mesh;
                holder.transform.localPosition = new Vector3(-0.0303f, -0.0021f, -0.0018f);
                holder.transform.localEulerAngles = new Vector3(-0.132f, -4.668f, -91.618f);
            }
        }
    }
}