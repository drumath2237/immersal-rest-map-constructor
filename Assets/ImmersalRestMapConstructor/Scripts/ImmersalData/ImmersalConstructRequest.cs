// **************************************
// referenced from:
// https://immersal.gitbook.io/sdk/cloud-service
// **************************************

using System;

namespace ImmersalRestMapConstructor.ImmersalData
{
    [Serializable]
    public struct ImmersalConstructRequest
    {
        public string token; // token
        public int bank; // id of image bank
        public string name; // name for the map
    }

    [Serializable]
    public struct ImmersalConstructResult
    {
        public int id; // id of the construction job / map
        public int size; // number of images used to create the map
        public string error;
    }
}