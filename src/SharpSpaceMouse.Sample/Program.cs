using System;
using System.Linq;

namespace SharpSpaceMouse.Sample {
    class Program {


        static void Main(string[] args)
        {
            var mouseInfo = SpaceMouse.GetConnectedMice().Result.First();

            SpaceMouse mouse;
            if (mouseInfo != null)
            {
                using (mouse = new SpaceMouse(mouseInfo))
                {
                    mouse.TranslationEvent += Mouse_TranslationEvent;
                    mouse.RotationEvent += Mouse_RotationEvent;
                    mouse.ButtonEvent += Mouse_ButtonEvent;
                    mouse.ReaderLoopExceptionEvent += Mouse_ReaderLoopExceptionEvent;

                    mouse.Start();

                    Console.WriteLine("Started with device " + mouseInfo.ProductName);

                    Console.ReadLine();

                    Console.WriteLine("Abort");
                    mouse.Abort();

                    while (mouse.IsActive())
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    Console.WriteLine("Ended");
                }
            }

            Console.ReadLine();
        }

        private static void Mouse_ReaderLoopExceptionEvent(object sender, SpaceMouse.SpaceMouseExceptionEventArgs e)
        {
            Console.WriteLine("Exception: " + e.Exception.ToString());
        }

        private static void Mouse_ButtonEvent(SpaceMouse arg1, bool[] arg2) {
            Console.WriteLine($"B {String.Join("", arg2.Select(b => b ? "1": "0"))}");
        }

        private static void Mouse_RotationEvent(SpaceMouse arg1, System.Numerics.Vector3 arg2) {
            Console.WriteLine($"R {arg2.X} {arg2.Y} {arg2.Z}");
        }

        private static void Mouse_TranslationEvent(SpaceMouse arg1, System.Numerics.Vector3 arg2) {
            Console.WriteLine($"T {arg2.X} {arg2.Y} {arg2.Z}");
        }
    }
}
