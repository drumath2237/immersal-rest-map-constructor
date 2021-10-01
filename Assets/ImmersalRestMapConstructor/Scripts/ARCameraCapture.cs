using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ImmersalRestMapConstructor
{
    public struct CaptureInfo
    {
        public Texture2D cameraTexture;
        public Pose cameraPose;
    }

    public static class ARCameraCapture
    {
        public static async UniTask<(bool, CaptureInfo)> GetCameraCaptureInfoAsync(ARCameraManager manager,
            CancellationToken token)
        {
            if (!manager.subsystem.TryAcquireLatestCpuImage(out var image))
            {
                return (false, default);
            }

            var resultTextureTask = ConvertARCameraImageToTextureAsync(image, token);
            image.Dispose();

            var cameraTransform = manager.transform;
            var cameraPose = new Pose
            {
                position = cameraTransform.position,
                rotation = cameraTransform.rotation
            };

            return (true, new CaptureInfo { cameraPose = cameraPose, cameraTexture = await resultTextureTask });
        }

        /// <summary>
        /// Convert CPUImage to Texture2D async.
        /// referenced from:
        /// https://github.com/drumath2237/Immersal-Server-Localizer/blob/main/Packages/ImmersalServerLocalizer/Runtime/Scripts/ARImageProcessingUtil.cs
        /// </summary>
        /// <param name="image"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async UniTask<Texture2D> ConvertARCameraImageToTextureAsync(
            XRCpuImage image,
            CancellationToken cancellationToken
        )
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                transformation = XRCpuImage.Transformation.MirrorX,
                outputFormat = TextureFormat.RGB24,
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
            };

            using var conversionTask = image.ConvertAsync(conversionParams);
            await UniTask.WaitWhile(() => !conversionTask.status.IsDone(),
                PlayerLoopTiming.Update, cancellationToken);

            if (conversionTask.status != XRCpuImage.AsyncConversionStatus.Ready)
            {
                Debug.LogError("conversion task failed");
                return null;
            }

            var texture2d = new Texture2D(
                conversionTask.conversionParams.outputDimensions.x,
                conversionTask.conversionParams.outputDimensions.y,
                conversionTask.conversionParams.outputFormat,
                false);

            var bytes = conversionTask.GetData<byte>();

            await UniTask.SwitchToMainThread(cancellationToken);

            texture2d.LoadRawTextureData(bytes);
            texture2d.Apply();
            bytes.Dispose();

            return texture2d;
        }
    }
}