﻿using JGM.GameStore.Events.Data;
using JGM.GameStore.Events.Services;
using JGM.GameStore.Libraries;
using JGM.GameStore.Localization;
using JGM.GameStore.Packs.Data;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace JGM.GameStore.Packs.Displayers
{
    public class OfferPackDisplayer : MonoBehaviour, IPackDisplayer
    {
        public class Factory : PlaceholderFactory<OfferPackDisplayer> { }
        public class FeaturedFactory : PlaceholderFactory<OfferPackDisplayer> { }

        [SerializeField] private LocalizedText _titleText;
        [SerializeField] private TextMeshProUGUI _remainingTimeText;
        [SerializeField] private TextMeshProUGUI _discountText;
        [SerializeField] private TextMeshProUGUI _priceBeforeDiscountText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Transform _packItemsParent;

        [Inject] private IEventTriggerService _eventTriggerService;
        [Inject] private PackItemDisplayer.Factory _packItemDisplayerFactory;
        private TimeSpan _remainingTimeSpan = TimeSpan.Zero;
        private Pack _pack;

        public void SetPackData(in Pack pack, IAssetsLibrary assetsLibrary)
        {
            _pack = pack;
            _remainingTimeSpan = pack.RemainingTime;
            RefreshRemainingTime();
            _discountText.text = $"{pack.Data.Discount * 100}%";
            _priceBeforeDiscountText.text = $"{pack.Data.PriceBeforeDiscount}$";
            _priceText.text = $"{pack.Data.Price}$";
            SetPackItems(pack, assetsLibrary);
            _titleText.RefreshText(pack.Data.TextId);
        }

        public void PurchasePack()
        {
            var eventData = new PurchasePackEventData(_pack);
            _eventTriggerService.Trigger("Purchase Pack", eventData);
        }

        private void Update()
        {
            if (_remainingTimeSpan > TimeSpan.Zero)
            {
                var deltaTimeSpan = TimeSpan.FromSeconds(Time.deltaTime);
                _remainingTimeSpan = _remainingTimeSpan.Subtract(deltaTimeSpan);
                RefreshRemainingTime();
            }
        }

        private void SetPackItems(Pack pack, IAssetsLibrary assetsLibrary)
        {
            var sortedItems = pack.Data.Items.OrderByDescending(i => i.ItemType).ToArray();
            for (int i = 0; i < sortedItems.Length; ++i)
            {
                var spawnedPackItem = _packItemDisplayerFactory.Create();
                spawnedPackItem.transform.SetParent(_packItemsParent, false);
                if (spawnedPackItem.TryGetComponent<PackItemDisplayer>(out var packItemDisplayer))
                {
                    packItemDisplayer.IconImage.sprite = assetsLibrary.GetSprite(sortedItems[i].IconName);
                    if (sortedItems[i].ItemType == PackItemData.Type.Character)
                    {
                        packItemDisplayer.AmountText.RefreshText(sortedItems[i].TextId);
                    }
                    else
                    {
                        packItemDisplayer.AmountText.RefreshText(sortedItems[i].TextId, $"{string.Format("{0:n0}", sortedItems[i].Amount)} ");
                    }
                }
            }
        }

        private void RefreshRemainingTime()
        {
            _remainingTimeText.text = $"{_remainingTimeSpan.Days}d {_remainingTimeSpan.Hours}h {_remainingTimeSpan.Minutes}m {_remainingTimeSpan.Seconds}s";
        }
    }
}