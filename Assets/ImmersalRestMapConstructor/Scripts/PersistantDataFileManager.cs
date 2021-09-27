using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using ImmersalRestMapConstructor.CaptureData;
using UnityEngine;

namespace ImmersalRestMapConstructor
{
    public static class PersistantDataFileManager
    {
        /// <summary>
        /// save texture for png format to persistant data path
        /// </summary>
        /// <param name="filename">not filepath. just a filename including extension</param>
        /// <param name="texture2D">texture data</param>
        public static async UniTask SaveCaptureImage(
            string filename, Texture2D texture2D
        )
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            using var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write);

            var textureBytes = texture2D.EncodeToPNG();

            await fs.WriteAsync(textureBytes, 0, textureBytes.Length);

            await fs.FlushAsync();

            fs.Close();
        }

        public static async UniTask<(bool, string)> ReadCaptureImageAsBase64(
            string filename
        )
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            if (!File.Exists(filepath))
            {
                return (false, null);
            }

            using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            var resultBytes = new byte[fs.Length];

            await fs.ReadAsync(resultBytes, 0, (int)fs.Length);
            
            fs.Close();

            var b64 = Convert.ToBase64String(resultBytes);

            return (true, b64);
        }

        public static async UniTask SaveJsonDataAsync(string filename, string jsonData)
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            using var writer = new StreamWriter(filepath, false);

            await writer.WriteAsync(jsonData);

            await writer.FlushAsync();

            writer.Close();
        }

        public static async UniTask<(bool, CaptureMapInfo info)> ReadCaptureMapInfoFromJson(string filename)
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);

            if (!File.Exists(filepath))
            {
                return (false, default);
            }

            using var reader = new StreamReader(filepath, Encoding.UTF8);
            var jsonData = await reader.ReadToEndAsync();
            
            reader.Close();

            var mapinfo = JsonUtility.FromJson<CaptureMapInfo>(jsonData);
            
            return (true, mapinfo);
        }
    }
}