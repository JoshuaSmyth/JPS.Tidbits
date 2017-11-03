using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace RayCasterGDI
{
    public class CommonColors
    {
        public const uint Red = 0xFFFF0000;
        public const uint Green = 0xFF00FF00;
        public const uint Blue = 0xFF0000FF;
        public const uint Yellow = 0xFFFFFF00;
        public const uint Magenta = 0xFFFF00FF;
        public const uint Cyan = 0xFF00FFFF;
        public const uint Black = 0xFF000000;
        public const uint DarkGrey = 0xFF222222;
    }

    struct Material
    {
        public uint Color;
    }

    struct Plane
    {
        public Vector3 Normal;
        public float Distance;
        public uint MaterialIndex;
    }

    struct Sphere
    {
        public Vector3 Position;
        public float Radius;
        public uint MaterialIndex;
    }

    struct Camera
    {
         public Vector3 Position;
         public Vector3 CameraX;
         public Vector3 CameraY;
         public Vector3 CameraZ;
         public float Distance;
         public Camera(Vector3 position, float distance) {
             Position = position;
             Distance = distance;

             var up = new Vector3(0, 0, 1);

             CameraZ = position;
             CameraX = Vector3.CrossProduct(CameraZ, up);
             CameraY = Vector3.CrossProduct(CameraZ, CameraX);
             
             CameraX.Normalize();
             CameraY.Normalize();
             CameraZ.Normalize();
         }
    }

    struct World
    {
         public Material[] Materials;
         public Plane[] Planes;
         public Sphere[] Spheres;
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 2 Hrs in
            // https://www.youtube.com/watch?v=pq7dV4sR7lg&t=6433s

            // PARGB Format
            // Image is width then height
            // Origin is top left
            var dbm = new DirectBitmap(1280, 720);

            // INIT WORLD
            var world = new World();
            {
                world.Materials = new Material[3];
                world.Materials[0].Color = CommonColors.DarkGrey;
                world.Materials[1].Color = CommonColors.Yellow;
                world.Materials[2].Color = CommonColors.Cyan;

                world.Planes = new Plane[1];
                world.Planes[0] = new Plane {Distance = 0, Normal = new Vector3(0, 0, 1)};
                world.Planes[0].MaterialIndex = 1;

                world.Spheres = new Sphere[1];
                world.Spheres[0].MaterialIndex = 2;
            }


            // TODO(Joshua) Look up the camera math
            var camera = new Camera(new Vector3(0, 50, 1), 1);
            var screenW = 1.0f;
            var screenH = 1.0f;
            var halfScreenW = screenW * 0.5f;
            var halfScreenH = screenH * 0.5f;
            var screenOrigin = camera.Position - camera.Distance*camera.CameraZ;

            var i=0;
            for(int r=0;r<dbm.Height;r++)
            {
                for(int c=0;c<dbm.Width;c++)
                {
                    float screenY = -1 + 2.0f* (r / (float) dbm.Height);
                    float screenX = -1 + 2.0f* (c / (float) dbm.Width);

                    var screenP = screenOrigin + screenX*halfScreenW*camera.CameraX + screenY*halfScreenH*camera.CameraY;

                    var rayOrigin = camera.Position;
                    var rayDirection = screenP - camera.Position;
                    rayDirection.Normalize();

                    var color = RayCast(ref world, rayOrigin, rayDirection);
                    dbm.Bits[i] = color;
                    i++;
                }
            }

            dbm.Bitmap.Save("test.bmp", ImageFormat.Bmp);
        }

        public static uint RayCast(ref World world, Vector3 rayOrigin, Vector3 rayDirection) {
            uint matIndex = 0;
            float hitDistance = float.MaxValue;
            for(int i=0;i<world.Planes.Length;i++)
            {
                float distance = RayIntersectionsPlane(rayDirection, rayOrigin, world.Planes[i]);
                if (distance > 0 && distance < hitDistance)
                {
                    matIndex = world.Planes[i].MaterialIndex;
                    hitDistance = distance;
                }
            }

            return world.Materials[matIndex].Color;
        }

        public static float RayIntersectionsPlane(Vector3 rayDirection, Vector3 rayOrigin, Plane plane) {

            float epsilon = 0.0001f;
            
            float demoniator = Vector3.DotProduct(plane.Normal, rayDirection);
            if (demoniator < -epsilon || demoniator > epsilon) {
                var dp = Vector3.DotProduct(plane.Normal, rayOrigin);
                float d = (plane.Distance- dp) / demoniator;
                return d;
            }

            return float.MaxValue;
        }

    }
}
