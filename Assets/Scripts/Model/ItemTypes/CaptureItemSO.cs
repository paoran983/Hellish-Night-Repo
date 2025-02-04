using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Items/CaptureItemSO")]
    public class CaptureItemSO : ItemSO, IDestroyableItem, CaptureItemAction
    {
        [SerializeField] private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Use";
        [field: SerializeField] public AudioClip actionSFX { get; private set; }

        [field: SerializeField] public int captureItemRate { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState)
        {
            throw new NotImplementedException();
        }

        public void PerformCaptureAction(PlayerController player)
        {
            player.StartCapture();
        }

        public int CaptureItemRate { get { return captureItemRate; } set { captureItemRate = value; } }
    }
}