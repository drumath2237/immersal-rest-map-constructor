using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ImmersalRestMapConstructor
{
    public class CameraCaptureComponent : MonoBehaviour
    {
        [SerializeField] private ARCameraManager _cameraManager;

        [SerializeField] private TextMeshProUGUI _logText;

        [SerializeField] private MeshRenderer _renderer;


        private Texture2D _texture2D;


        public void Capture()
        {
            CaptureAsync().Forget();
        }

        private async UniTask CaptureAsync()
        {
            var (result, info) =
                await ARCameraCapture.GetCameraCaptureInfoAsync(_cameraManager, this.GetCancellationTokenOnDestroy());

            _texture2D = info.cameraTexture;
        }

        private void Update()
        {
            _logText.text = ARSession.state.ToString();
            if (_renderer != null && _texture2D != null)
            {
                _renderer.material.mainTexture = _texture2D;
            }
        }
    }
}