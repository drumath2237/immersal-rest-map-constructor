// **************************************
// referenced from:
// https://immersal.gitbook.io/sdk/cloud-service
// **************************************

using System;

namespace ImmersalRestMapConstructor.ImmersalData
{
    [Serializable]
    public struct ImmersalClearRequest
    {
        public int bank; // id of image bank to clear
        public bool anchor;
        public string token;
    }

    [Serializable]
    public struct ImmersalClearResult
    {
        public string error;
    }
}