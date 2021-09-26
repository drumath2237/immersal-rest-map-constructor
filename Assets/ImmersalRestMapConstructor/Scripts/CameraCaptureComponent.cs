using System;
using System.Collections;
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


        private IEnumerator Start()
        {
            if (!Input.location.isEnabledByUser)
            {
                yield break;
            }
            
            Input.location.Start();
        }

        public void Capture()
        {
            if (Input.location.status != LocationServiceStatus.Running)
            {
                return;
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

            _logText.text = JsonUtility.ToJson(imageInfo, true);

            _texture2D = info.cameraTexture;
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