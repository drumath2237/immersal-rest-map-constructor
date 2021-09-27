using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        private int _captureIndex = 0;


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

            SaveCaptureImage($"{_captureIndex++}.png", info.cameraTexture).Forget();

            _texture2D = info.cameraTexture;
        }

        /// <summary>
        /// save texture for png format to persistant data path
        /// </summary>
        /// <param name="filename">not filepath. just a filename including extension</param>
        /// <param name="texture2D">texture data</param>
        private static async UniTask SaveCaptureImage(string filename, Texture2D texture2D)
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            using var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write);

            var textureBytes = texture2D.EncodeToPNG();

            await fs.WriteAsync(textureBytes, 0, textureBytes.Length);

            await fs.FlushAsync();
            
            fs.Close();
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