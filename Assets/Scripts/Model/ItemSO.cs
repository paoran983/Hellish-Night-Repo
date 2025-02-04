using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model {
    public abstract class ItemSO : ScriptableObject {

        [field: SerializeField] public bool isStackable { get; set; }
        private int ID => GetInstanceID();
        [field: SerializeField] private int maxStackSize { get; set; } = 1;
        [field: SerializeField] private string itemName { get; set; }
        [field: SerializeField][field: TextArea] public string description { get; set; }
        [field: SerializeField] private Sprite image { get; set; }
        [field: SerializeField] private float weight { get; set; }
        [field: SerializeField] private int value { get; set; }
        [field:SerializeField]  private List<ItemParameter> parameterList { get; set; }

        [field: SerializeField] private List<Move> moveList;
        [SerializeField] public Type type;


        [SerializeField]
        public enum Type {
            Default,
            Helmet,
            Armour,
            Accessory,
            Ability,
            Consumable,
            Money,
            Capture,
            Sword,
            CurvedSword,
            Pistol,
            Rifle,
            Shotgun,
            Sniper,
            Shield,
            BluntWeapon,
            Axe
        }
        public int GetID() {
            return ID;
        }
        public int GetMaxStackSize() {
            return maxStackSize;
        }
        public string GetName() {
            return itemName;
        }
        public Sprite GetImage() {
            return image;
        }
        public List<ItemParameter> GetList() {
            return parameterList;
        }       
        public Sprite Image {
            get { return image; }
            set { image = value; }
        }

        public List<Move> MoveList {
            get {
                return moveList;
            }
            set { 
                moveList = value; 
            }
        }
        public float Weight {
            get { return weight; }
            set { weight = value; }
        }
        public int Value {
            get { return value; }
            set { this.value = value; }
        }
    }

    [Serializable]
    public struct ItemParameter: IEquatable<ItemParameter> {
        public ItemParameterSO itemParameter;
        public float val;
        public ItemParameter(ItemParameterSO itemParameter, float val) {
            this.itemParameter=itemParameter;
            this.val=val;
        }
        public bool Equals(ItemParameter other) {
            return other.itemParameter == itemParameter;
        }
    }
}
