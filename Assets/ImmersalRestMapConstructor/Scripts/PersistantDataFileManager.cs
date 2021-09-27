using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ImmersalRestMapConstructor
{
    public class PersistantDataFileManager
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

        public static async UniTask SaveJsonDataAsync(string filename, string jsonData)
        {
            var filepath = Path.Combine(Application.persistentDataPath, filename);
            using var writer = new StreamWriter(filepath, false);

            await writer.WriteAsync(jsonData);

            await writer.FlushAsync();

            writer.Close();
        }
    }
}