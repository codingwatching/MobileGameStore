﻿using JGM.GameStore.Events.Data;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace JGM.GameStore.Panels
{
    public class ErrorPanel : MonoBehaviour
    {
        [SerializeField] private Transform _transformWindow;

        public async void ShowErrorMessage(IGameEventData gameEventData)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _transformWindow.gameObject.SetActive(false);
        }
    }
}