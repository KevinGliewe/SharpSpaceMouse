# SharpSpaceMouse

This is an .NET driver for the 3dconnexion SpaceMouse using `Usb.Net` and `Hid.Net` from [Device.Net](https://github.com/MelbourneDeveloper/Device.Net). Compatible with `netstandard2.0` of higher.

## Usage

A few examples of useful commands and/or tasks.

```C#
// Get the first connected SpaceMouse
var mouseInfo = SpaceMouse.GetConnectedMice().Result.First();

// Create a SpaceMouse instance
var mouse = new SpaceMouse(mouseInfo);

// Subscribe to the events
mouse.TranslationEvent += Mouse_TranslationEvent;
mouse.RotationEvent += Mouse_RotationEvent;
mouse.ButtonEvent += Mouse_ButtonEvent;

void Mouse_ButtonEvent(SpaceMouse arg1, bool[] arg2) {
    Console.WriteLine($"B {String.Join("", arg2.Select(b => b ? "1": "0"))}");
}

void Mouse_RotationEvent(SpaceMouse arg1, System.Numerics.Vector3 arg2) {
    Console.WriteLine($"R {arg2.X} {arg2.Y} {arg2.Z}");
}

void Mouse_TranslationEvent(SpaceMouse arg1, System.Numerics.Vector3 arg2) {
    Console.WriteLine($"T {arg2.X} {arg2.Y} {arg2.Z}");
}

// Start listening for events
mouse.Start();
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.