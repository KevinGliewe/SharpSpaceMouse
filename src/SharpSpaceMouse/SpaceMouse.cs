using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Device.Net;
using Hid.Net.Windows;

namespace SharpSpaceMouse {
    public class SpaceMouse : IDisposable {

        private enum EventType {
            Translation = 1,
            Rotation = 2,
            Buttons = 3
        }

#region Static
        private static readonly DebugLogger Logger = new DebugLogger();
        private static readonly DebugTracer Tracer = new DebugTracer();

        private const int LOGITECH_3DX_VID = 0x046d;
        private const int HID_USAGE_GENERIC_MULTIAXIS_CONTROLLER = 0x08;
        private const int HID_USAGE_PAGE_GENERIC = 0x01;

        static SpaceMouse()
        {
            WindowsHidDeviceFactory.Register(Logger, Tracer);
        }

        public static async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedMice() 
            => (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).Where(d => Is3DxDevice(d));

        private static bool Is3DxDevice(ConnectedDeviceDefinition hidInfo) {
            return hidInfo.VendorId == LOGITECH_3DX_VID &&
                   hidInfo.UsagePage == HID_USAGE_PAGE_GENERIC &&
                   hidInfo.Usage == HID_USAGE_GENERIC_MULTIAXIS_CONTROLLER;
        }
#endregion

        private bool _running = false;

        public Vector3 TranslationScale { get; set; } = new Vector3(1.0f / 350.0f);
        public Vector3 RotationScale { get; set; } = new Vector3(1.0f / 350.0f);

        public Vector3 CurrentTranslation { get; private set; } = new Vector3(0.0f);
        public Vector3 CurrentRotation { get; private set; } = new Vector3(0.0f);
        public bool[] CurrentButtons { get; private set; } = Enumerable.Repeat(false, 48).ToArray();

        public event Action<SpaceMouse, Vector3> TranslationEvent;
        public event Action<SpaceMouse, Vector3> RotationEvent;
        public event Action<SpaceMouse, bool[]> ButtonEvent; 

        private ConnectedDeviceDefinition _mouseInfo;
        private IDevice _mouseDevice;

        public SpaceMouse(ConnectedDeviceDefinition hidInfo)
        {
            _mouseInfo = hidInfo;
        }

        public void Start()
        {
            if(_running)
                throw new Exception("Device already started");

            _mouseDevice = DeviceManager.Current.GetDevice(_mouseInfo);

            _mouseDevice.InitializeAsync().Wait();

            _running = true;

            Task.Run(() => _loop());
        }

        private async Task _loop()
        {
            while (_running)
            {
                try
                {
                    var result = await _mouseDevice.ReadAsync();

                    if (result.BytesRead >= 7)
                    {
                        if (result.Data[0] == (int)EventType.Translation)
                        {
                            CurrentTranslation = toVector3(result.Data) * TranslationScale;
                            TranslationEvent?.Invoke(this, CurrentTranslation);
                        } else if (result.Data[0] == (int) EventType.Rotation)
                        {
                            CurrentRotation = toVector3(result.Data) * RotationScale;
                            RotationEvent?.Invoke(this, CurrentRotation);
                        } else if (result.Data[0] == (int)EventType.Buttons)
                        {
                            CurrentButtons = ConvertByteArrayToBoolArray(result.Data.Skip(1).ToArray());
                            ButtonEvent?.Invoke(this, CurrentButtons);
                        }

                    }

                } catch(Exception ex) { }
            }
        }

        private Vector3 toVector3(byte[] data)
        {
            Vector3 ret;
            ret.X = (float) toInt(data[1], data[2]);
            ret.Y = (float) toInt(data[3], data[4]);
            ret.Z = (float) toInt(data[5], data[6]);
            return ret;
        }

        private int toInt(byte b1, byte b2)
        {
            return (Int16)((Int16)b1 | (Int16)b2 << 8);
        }

        public void Dispose()
        {
            _running = false;
            _mouseDevice?.Dispose();
        }

        /// <summary>
        /// Convert Byte Array To Bool Array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool[] ConvertByteArrayToBoolArray(byte[] bytes) {
            System.Collections.BitArray b = new System.Collections.BitArray(bytes);
            bool[] bitValues = new bool[b.Count];
            b.CopyTo(bitValues, 0);
            Array.Reverse(bitValues);
            return bitValues;
        }
    }
}
