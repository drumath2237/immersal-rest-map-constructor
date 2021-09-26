//**************************************************************************************
//
// referenced from:
// https://immersal.gitbook.io/sdk/cloud-service/rest-api#save-captured-image-to-the-map
//
//**************************************************************************************


using System;

namespace ImmersalRestMapConstructor.ImmersalData
{
    [Serializable]
    public struct ImmersalImageRequest
    {
        public string token;
        public int bank;
        public int run;
        public int index;
        public bool anchor;
        public double px; // camera x position
        public double py; // camera y position
        public double pz; // camera z position
        public double r00; // rotation matrix row 0, col 0
        public double r01; // rotation matrix row 0, col 1
        public double r02; // rotation matrix row 0, col 2
        public double r10; // rotation matrix row 1, col 0
        public double r11; // rotation matrix row 1, col 1
        public double r12; // rotation matrix row 1, col 2
        public double r20; // rotation matrix row 2, col 0
        public double r21; // rotation matrix row 2, col 1
        public double r22; // rotation matrix row 2, col 2
        public double fx; // camera intrinsics focal length x
        public double fy; // camera intrinsics focal length y
        public double ox; // camera intrinsics principal point x
        public double oy; // camera intrinsics principal point y
        public double latitude; // WGS84 latitude
        public double longitude; // WGS84 longitude
        public double altitude; // GPS elevation
        public string b64; // Base64-encoded PNG image, 8-bit grayscale or 24-bit RGB
    }

    [Serializable]
    public struct ImmersalImageResult
    {
        public string path;
    }
}