﻿using JGM.GameStore.Libraries;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace JGM.GameStore.Panels.Helpers
{
    public class PackItem3DVisualizer : IPackItem3DVisualizer
    {
        public class Factory : PlaceholderFactory<PackItem3DVisualizer> { }

        private const float _spacingBetweenObjects = 100f;

        [Inject]
        private IAssetsLibrary _assetsLibrary;
        private Dictionary<string, GameObject> _renderObjects;

        public PackItem3DVisualizer()
        {
            _renderObjects = new Dictionary<string, GameObject>();
        }

        public void Initialize(Camera cameraPrefab)
        {
            var previewsParent = new GameObject("3DPreviews").transform;
            var previewPrefabs = _assetsLibrary.Get3DPreviews();
            var renderTexture = new RenderTexture(512, 512, 0);

            for (int i = 0; i < previewPrefabs.Length; ++i)
            {
                var spawnedPreview = GameObject.Instantiate(previewPrefabs[i]);
                spawnedPreview.transform.SetParent(previewsParent, false);
                spawnedPreview.transform.localPosition += Vector3.right * i * _spacingBetweenObjects;

                var spawnedCamera = GameObject.Instantiate(cameraPrefab);
                spawnedCamera.transform.SetParent(spawnedPreview.transform, false);
                var cameraPositioner = new PackItem3DCameraPositioner();
                spawnedCamera.transform.localPosition = cameraPositioner.GetCameraPositionFromPreviewName(previewPrefabs[i].name);
                spawnedCamera.GetComponent<Camera>().targetTexture = renderTexture;

                string spawnedPreviewName = spawnedPreview.name.Substring(0, spawnedPreview.name.Length - 7);
                spawnedPreview.name = spawnedPreviewName;
                _renderObjects.Add(spawnedPreviewName, spawnedPreview);
                spawnedPreview.SetActive(false);
            }
        }

        public RenderTexture GetRenderTexture(in string prefabName)
        {
            if (!_renderObjects.ContainsKey(prefabName))
            {
                return null;
            }

            var renderObject = _renderObjects[prefabName];
            renderObject.SetActive(true);
            return renderObject.GetComponentInChildren<Camera>().targetTexture;
        }

        public void ReturnRenderTexture(in string prefabName)
        {
            if (!_renderObjects.ContainsKey(prefabName))
            {
                return;
            }

            _renderObjects[prefabName].SetActive(false);
        }
    }
}