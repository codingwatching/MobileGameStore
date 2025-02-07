﻿using JGM.GameStore.Libraries;
using JGM.GameStore.Packs.Data;
using JGM.GameStore.Utils;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace JGM.GameStore.Packs
{
    public class PacksController : MonoBehaviour, IPacksController
    {
        public IPacksController.ShopPackEvent OnPackActivated { get; set; } = new IPacksController.ShopPackEvent();
        public IPacksController.ShopPackEvent OnPackRemoved { get; set; } = new IPacksController.ShopPackEvent();
        public List<Pack> ActivePacks { get; private set; }

        private const string _storeDataFileName = "store_data";
        private const int _numberOfActiveOfferPacks = 3;
        private const int _offersHistoryMaxSize = _numberOfActiveOfferPacks + 1;

        [Inject] private IAssetsLibrary _assetsLibrary;
        [Inject] private Pack.Factory _packFactory;

        private List<Pack> _activeOfferPacks;
        private List<PackData> _offerPacksDatabase;
        private Queue<string> _offerPacksHistory;

        public void Initialize()
        {
            ActivePacks = new List<Pack>();
            _activeOfferPacks = new List<Pack>();
            _offerPacksDatabase = new List<PackData>();
            _offerPacksHistory = new Queue<string>();

            var storeText = _assetsLibrary.GetText(_storeDataFileName);
            var storeJson = JSONNode.Parse(storeText.text);

            _offerPacksDatabase.Clear();
            _activeOfferPacks.Clear();
            ActivePacks.Clear();

            if (storeJson.HasKey("packs"))
            {
                var packsData = storeJson["packs"].AsArray;
                for (int i = 0; i < packsData.Count; ++i)
                {
                    var storePackData = new PackData();
                    storePackData.PopulateDataFromJson(packsData[i]);
                    if (storePackData.PackType != PackData.Type.Offer)
                    {
                        CreateAndActivatePack(storePackData);
                    }
                    else
                    {
                        _offerPacksDatabase.Add(storePackData);
                    }
                }
            }
        }

        public void Refresh()
        {
            var packsToRemove = new List<Pack>();

            for (int i = 0; i < ActivePacks.Count; ++i)
            {
                ActivePacks[i].CheckExpiration();
                bool needsToBeRemoved = (ActivePacks[i].PackState == Pack.State.Expired);
                if (needsToBeRemoved)
                {
                    packsToRemove.Add(ActivePacks[i]);
                }
            }

            for (int i = 0; i < packsToRemove.Count; ++i)
            {
                RemovePack(packsToRemove[i]);
            }

            int loopCount = 50;
            while (_activeOfferPacks.Count < _numberOfActiveOfferPacks && loopCount > 0)
            {
                loopCount--;

                var poolOfSelectablePacks = new List<PackData>();
                for (int i = 0; i < _offerPacksDatabase.Count; ++i)
                {
                    if (_offerPacksHistory.Contains(_offerPacksDatabase[i].Id))
                    {
                        continue;
                    }

                    poolOfSelectablePacks.Add(_offerPacksDatabase[i]);
                }

                bool anyValidCandidates = (poolOfSelectablePacks.Count > 0);
                if (anyValidCandidates)
                {
                    int randomPackIndex = Random.Range(0, poolOfSelectablePacks.Count);
                    var newPackData = poolOfSelectablePacks[randomPackIndex];
                    CreateAndActivatePack(newPackData);
                }
                else
                {
                    _offerPacksHistory.Dequeue();
                }
            }
        }

        private void CreateAndActivatePack(PackData storePackData)
        {
            var storePack = _packFactory.Create();
            storePack.SetData(storePackData);
            ActivePacks.Add(storePack);

            if (storePack.Data.PackType == PackData.Type.Offer)
            {
                _activeOfferPacks.Add(storePack);
                _offerPacksHistory.Enqueue(storePackData.Id);

                while (_offerPacksHistory.Count > _offersHistoryMaxSize)
                {
                    _offerPacksHistory.Dequeue();
                }
            }

            storePack.Activate();
            OnPackActivated?.Invoke(storePack);
        }

        private void RemovePack(Pack storePack)
        {
            ActivePacks.Remove(storePack);
            _activeOfferPacks.Remove(storePack);
            OnPackRemoved?.Invoke(storePack);
        }
    }
}