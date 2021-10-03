using System;
using System.Text;
using Cysharp.Threading.Tasks;
using ImmersalRestMapConstructor.CaptureData;
using ImmersalRestMapConstructor.ImmersalData;
using UnityEngine;
using UnityEngine.Networking;

namespace ImmersalRestMapConstructor
{
    public class ImmersalMapConstructorClient
    {
        private static readonly string BASE_URL = "https://api.immersal.com";

        public static async UniTask<(bool, ImmersalImageResult)> TryRequestImage(CaptureMapInfo info, int index,
            string filename)
        {
            var (isExist, base64) = await PersistantDataFileManager.ReadCaptureImageAsBase64(filename);

            if (!isExist)
            {
                Debug.Log("file doesnt exists");
                return (false, default);
            }

            var requestData = CreateImageRequestFromCaptureMapInfo(info, index, base64);

            var request = new UnityWebRequest($"{BASE_URL}/captureb64", "POST");
            var byteRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestData));
            request.uploadHandler = new UploadHandlerRaw(byteRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                var res = await request.SendWebRequest();
                return (true, JsonUtility.FromJson<ImmersalImageResult>(res.downloadHandler.text));
            }
            catch (Exception e)
            {
                Debug.LogError("capture request failed with " + e);
                throw;
            }
        }

        public static async UniTask<(bool, ImmersalConstructResult)> TryRequestConstructMap(CaptureMapInfo info,
            string mapName = "")
        {
            var constructRequest = CreateConstructRequestFromMapInfo(info);

            if (mapName != String.Empty)
            {
                constructRequest.name = mapName;
            }

            var request = new UnityWebRequest($"{BASE_URL}/construct", "POST");
            var byteRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(constructRequest));
            request.uploadHandler = new UploadHandlerRaw(byteRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                var res = await request.SendWebRequest();
                return (true, JsonUtility.FromJson<ImmersalConstructResult>(res.downloadHandler.text));
            }
            catch (Exception e)
            {
                Debug.LogError("construct request failed with " + e);
                throw;
            }
        }

        public static async UniTask<(bool, ImmersalClearResult)> TryClearRequest(CaptureMapInfo info)
        {
            var clearRequest = new ImmersalClearRequest()
            {
                anchor = true,
                bank = 0,
                token = info.token
            };
            
            var request = new UnityWebRequest($"{BASE_URL}/clear", "POST");
            var byteRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(clearRequest));
            request.uploadHandler = new UploadHandlerRaw(byteRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                var res = await request.SendWebRequest();
                return (true, JsonUtility.FromJson<ImmersalClearResult>(res.downloadHandler.text));
            }
            catch (Exception e)
            {
                Debug.LogError("clear request failed with " + e);
                throw;
            }

        }

        private static ImmersalImageRequest CreateImageRequestFromCaptureMapInfo(CaptureMapInfo info, int index,
            string b64)
        {
            var imageData = info.images[index];

            var _p = imageData.pose.position;
            var cameraPosition = new Vector3(_p.x, _p.y, -_p.z);

            var _q = imageData.pose.rotation;
            var cameraQuaternion = new Quaternion(_q.x, _q.y, -_q.z, -_q.w);

            var cameraPoseMatrix = Matrix4x4.TRS(cameraPosition, cameraQuaternion, Vector3.one);

            var request = new ImmersalImageRequest
            {
                token = info.token,
                bank = 0,
                run = 0,
                index = index,
                anchor = imageData.anchor,

                px = cameraPoseMatrix.m03,
                py = cameraPoseMatrix.m13,
                pz = cameraPoseMatrix.m23,

                r00 = cameraPoseMatrix.m00,
                r01 = cameraPoseMatrix.m01,
                r02 = cameraPoseMatrix.m02,
                r10 = cameraPoseMatrix.m10,
                r11 = cameraPoseMatrix.m11,
                r12 = cameraPoseMatrix.m12,
                r20 = cameraPoseMatrix.m20,
                r21 = cameraPoseMatrix.m21,
                r22 = cameraPoseMatrix.m22,

                fx = info.focalLength.x,
                fy = info.focalLength.y,
                ox = info.principalOffset.x,
                oy = info.principalOffset.y,

                altitude = imageData.location.altitude,
                latitude = imageData.location.latitude,
                longitude = imageData.location.longitude,

                b64 = b64
            };

            return request;
        }

        private static ImmersalConstructRequest CreateConstructRequestFromMapInfo(CaptureMapInfo info)
        {
            return new ImmersalConstructRequest()
            {
                bank = 0,
                name = info.name,
                token = info.token
            };
        }
    }
}