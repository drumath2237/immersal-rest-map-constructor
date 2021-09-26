using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using ImmersalRestMapConstructor.CaptureData;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ImmersalRestMapConstructor
{
    public class CameraCaptureComponent : MonoBehaviour
    {
        [SerializeField] private ARCameraManager _cameraManager;

        [SerializeField] private TextMeshProUGUI _logText;

        [SerializeField] private MeshRenderer _renderer;


        private Texture2D _texture2D;

        private CaptureMapInfo _mapInfo;


        private IEnumerator Start()
        {
            if (!Input.location.isEnabledByUser)
            {
                yield break;
            }

            Input.location.Start();

            _mapInfo = new CaptureMapInfo
            {
                name = "Test Map",
                token = "Test Token",
                images = new List<CaptureImageInfo>()
            };
        }

        public void Capture()
        {
            if (Input.location.status != LocationServiceStatus.Running)
            {
                return;
            }

            if (_mapInfo.focalLength == default || _mapInfo.principalOffset == default)
            {
                if (!_cameraManager.TryGetIntrinsics(out var intrinsics))
                {
                    return;
                }

                _mapInfo.focalLength = new Vector2
                {
                    x = intrinsics.focalLength.x,
                    y = intrinsics.focalLength.y
                };

                _mapInfo.principalOffset = new Vector2
                {
                    x = intrinsics.principalPoint.x,
                    y = intrinsics.principalPoint.y
                };
            }


            CaptureAsync().Forget();
        }

        private async UniTask CaptureAsync()
        {
            var (result, info) =
                await ARCameraCapture.GetCameraCaptureInfoAsync(_cameraManager, this.GetCancellationTokenOnDestroy());

            if (!result)
            {
                return;
            }

            var imageInfo = new CaptureImageInfo
            {
                anchor = false,
                location = Location.ConvertFromLocationInfo(Input.location.lastData),
                pose = info.cameraPose,
                run = 0
            };

            _mapInfo.images.Add(imageInfo);

            _logText.text = JsonUtility.ToJson(imageInfo, true);

            _texture2D = info.cameraTexture;
        }

        public void SaveJsonData()
        {
            SaveJsonDataAsync().Forget();
        }

        private async UniTask SaveJsonDataAsync()
        {
            var filename = Path.Combine(Application.persistentDataPath, "imageData.json");
            using var writer = new StreamWriter(filename, false);

            await writer.WriteAsync(JsonUtility.ToJson(_mapInfo, true));

            await writer.FlushAsync();
            
            writer.Close();

        }

        private void Update()
        {
            // _logText.text = ARSession.state.ToString();
            if (_renderer != null && _texture2D != null)
            {
                _renderer.material.mainTexture = _texture2D;
            }
        }

        private void OnDestroy()
        {
            Input.location.Stop();
        }
    }
}