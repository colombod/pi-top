﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PiTop.Camera
{
    public static class PiTopModuleExtensions
    {
        public static PiTopModule UseCamera(this PiTopModule module)
        {
            module.AddDeviceFactory<int, ICamera>(deviceType =>
            {
                var ctorSignature = new[] { typeof(int) };
                var ctor = deviceType.GetConstructor(ctorSignature);

                if (ctor != null)
                {
                    return cameraIndex =>
                        (ICamera)Activator.CreateInstance(deviceType, cameraIndex)!;

                }

                throw new InvalidOperationException(
                    $"Cannot find suitable constructor for type {deviceType}, looking for signature {ctorSignature}");
            });

            module.AddDeviceFactory<FileSystemCameraSettings, FileSystemCamera>(deviceType =>
            {
                return settings => new FileSystemCamera(settings);
            });

            return module;
        }

        public static T GetOrCreateCamera<T>(this PiTopModule module, int index)
         where T : ICamera
        {
            
            IConnectedDeviceFactory<int, ICamera> factory = null!;
            try
            {
                factory = module.GetDeviceFactory<int, ICamera>();
            }
            catch (KeyNotFoundException

            )
            {

            }

            AssertFactory(factory);
            return factory.GetOrCreateDevice<T>(index);
        }

        public static T GetOrCreateCamera<T>(this PiTopModule module, DirectoryInfo directory, string imageFileSearchPattern = "*.png") where T : FileSystemCamera
        {
            return module.GetOrCreateCamera<T>(
                new FileSystemCameraSettings(directory, imageFileSearchPattern));
        }

        public static T GetOrCreateCamera<T>(this PiTopModule module, FileSystemCameraSettings settings) where T : FileSystemCamera
        {
            IConnectedDeviceFactory<FileSystemCameraSettings, FileSystemCamera> factory = null!; 
            try
            {
                
                factory = module.GetDeviceFactory<FileSystemCameraSettings, FileSystemCamera>();
            }
            catch (KeyNotFoundException

                )
            {
               
            }

            AssertFactory(factory);
            return factory.GetOrCreateDevice<T>(settings);
        }

        private static void AssertFactory<T>(T factory)
        {
            if (factory == null)
            {
                throw new InvalidOperationException($"Cannot find a factory if type {typeof(T).ToDisplayName()}, make sure to configure the module calling {nameof(UseCamera)} first.");
            }
        }

        public static void DisposeDevice<T>(this PiTopModule module, T device)
            where T : ICamera
        {
            var factory = module.GetDeviceFactory<int, ICamera>();
            AssertFactory(factory);
            factory.DisposeDevice(device);
        }

        public static void DisposeDevice(this PiTopModule module, FileSystemCamera device)
        {
            var factory = module.GetDeviceFactory<FileSystemCameraSettings, FileSystemCamera>();
            AssertFactory(factory);
            factory.DisposeDevice(device);
        }
    }
}
