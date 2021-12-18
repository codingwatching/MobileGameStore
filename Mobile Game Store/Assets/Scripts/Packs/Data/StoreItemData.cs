﻿using JGM.GameStore.Transaction;
using JGM.GameStore.Utils;
using System;
using static JGM.GameStore.Transaction.UserWallet;

namespace JGM.GameStore.Packs.Data
{
    [Serializable]
    public class StoreItemData
    {
        public enum Type
        {
            Coins,
            Gems,
            Character
        }

        public Type ItemType => _itemType;
        public bool IsCharacter => ItemType == Type.Character;
        public int Amount { get; private set; }
        public string ItemId { get; private set; }
        public string TextId { get; private set; }
        public string IconName { get; private set; }
        public string PrefabName { get; private set; }

        private IUserWallet _userWallet;
        private Type _itemType = Type.Coins;

        public void ApplyTransaction()
        {
            switch (ItemType)
            {
                case Type.Coins:
                    {
                        Transaction.Transaction trans = _userWallet.CreateTransaction(Currency.Coins, Amount);
                        trans.StartTransaction();
                    }
                    break;

                case Type.Gems:
                    {
                        Transaction.Transaction trans = _userWallet.CreateTransaction(Currency.Gems, Amount);
                        trans.StartTransaction();
                    }
                    break;

                case Type.Character:
                    {
                        // Nothing to do actually
                    }
                    break;
            }
        }

        public static StoreItemData CreateFromJson(JSONNode data)
        {
            var newStoreItemData = new StoreItemData();

            if (data.HasKey("type"))
            {
                DataParser.EnumTryParse(data["type"], true, out newStoreItemData._itemType);
            }

            if (newStoreItemData.IsCharacter)
            {
                if (data.HasKey("itemId"))
                {
                    newStoreItemData.ItemId = data["itemId"];
                }
                newStoreItemData.Amount = 1;
            }
            else
            {
                if (data.HasKey("amount"))
                {
                    newStoreItemData.Amount = data["amount"].AsInt;
                }
                newStoreItemData.ItemId = string.Empty;
            }

            if (data.HasKey("tidName"))
            {
                newStoreItemData.TextId = data["tidName"];
            }

            if (data.HasKey("icon"))
            {
                newStoreItemData.IconName = data["icon"];
            }

            if (data.HasKey("prefab"))
            {
                newStoreItemData.PrefabName = data["prefab"];
            }

            return newStoreItemData;
        }

        public override string ToString() => $"{{ {ItemType} | {Amount} | {ItemId} }}";

        private StoreItemData() { }
    }
}