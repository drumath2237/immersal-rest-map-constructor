using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ImmersalRestMapConstructor.CaptureData;
using TMPro;
using Unity.Collections;
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

        private int _captureIndex = 0;

        private bool isCameraConfigInit = false;


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

            _cameraManager.frameReceived += args =>
            {
                InitializeCameraConfig();
            };

            // SendImageCaptureRequest().Forget();
            // yield return null;
        }

        private void InitializeCameraConfig()
        {
            if (isCameraConfigInit)
            {
                return;
            }

            using var configs = _cameraManager.GetConfigurations(Allocator.Temp);

            var niceConfig = configs.FirstOrDefault(configuration => configuration.width == 1920);

            try
            {
                _cameraManager.currentConfiguration = niceConfig;
            }
            catch (Exception e)
            {
                Debug.Log("camera config error with: " + e);
                return;
            }

            isCameraConfigInit = true;
        }

        private async UniTask SendImageCaptureRequest()
        {
            var (jsonExist, mapInfo) = await PersistantDataFileManager.ReadCaptureMapInfoFromJson("imageData.json");

            if (!jsonExist)
            {
                return;
            }

            for (var i = 0; i < mapInfo.images.Count; i++)
            {
                var (success, result) = await ImmersalMapConstructorClient.TryRequestImage(mapInfo, i);

                Debug.Log(result.path);
            }
        }

        public void Capture()
        {
            if (Input.location.status != LocationServiceStatus.Running)
            {
                return;
            }

            InitializeCameraConfig();

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

            if (_mapInfo.focalLength == default || _mapInfo.principalOffset == default)
            {
                if (!_cameraManager.TryGetIntrinsics(out var intrinsics))
                {
                    return;
                }

                _logText.text = intrinsics.principalPoint.ToString();

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

            var dateTimeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var captureFileName = $"{dateTimeStamp}_{_captureIndex++}.png";

            var imageInfo = new CaptureImageInfo
            {
                anchor = false,
                location = Location.ConvertFromLocationInfo(Input.location.lastData),
                pose = info.cameraPose,
                run = 0,
                filename = captureFileName
            };

            _mapInfo.images.Add(imageInfo);

            PersistantDataFileManager
                .SaveCaptureImage(captureFileName, info.cameraTexture)
                .Forget();

            _texture2D = info.cameraTexture;
        }

        public void SaveJsonData()
        {
            var dateTimeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            PersistantDataFileManager
                .SaveJsonDataAsync($"{dateTimeStamp}_capture.json", JsonUtility.ToJson(_mapInfo, true))
                .Forget();
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