using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImmersalRestMapConstructor.CaptureData
{
    [Serializable]
    public struct Location
    {
        public float longitude;
        public float latitude;
        public float altitude;

        public static Location ConvertFromLocationInfo(LocationInfo info)
        {
            return new Location
            {
                altitude = info.altitude,
                latitude = info.latitude,
                longitude = info.longitude
            };
        }
    }

    [Serializable]
    public struct CaptureImageInfo
    {
        public Pose pose;
        public int run; // incremented when tracking failed
        public bool anchor;
        public Location location;
        public string filename;
    }

    [Serializable]
    public struct CaptureMapInfo
    {
        public string token;
        public string name;
        public Vector2 focalLength;
        public Vector2 principalOffset;
        public List<CaptureImageInfo> images;
    }
}