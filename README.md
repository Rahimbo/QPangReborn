# QPangReborn (QPang / MangaFighter emulator)

This is our attempt at an QPang emulator. We have decided to open-source our prototype emulators because we are not actively working on the project anymore. Hopefully someone else can continue with our work or assist us.

You can find QPang and MangaFighter installations in our GDrive folder. There is also a patch for QPang 20120502.

https://drive.google.com/drive/folders/0B94To1DD6pC7YnVPYkYtZzlBUTg

## How do I run QPang?

1. Download and install `QPangSetup_NL 20120502.exe` from the GDrive folder.
2. Download all files from the `20120502 Patch` folder and place them in the QPang installation folder.
3. Rename or remove the "HShield" directory (I recommend to rename it so you have a backup).
4. Execute `Run QPang.bat`.

We have to bypass the launcher (`QPang.exe`) because the `Server.Updater` is not finished and it is quicker. It is also useful when you want to use OllyDbg or something else like it.

## How do I run the emulators?

1. Clone the GitHub project.
2. Open the `.sln` in VS2017. (Make sure you have [.NETCore 2.0](https://www.microsoft.com/net/core) installed as well)
3. Right-click on the solution in the `Solution Explorer` view and press `Set StartUp Projects...`.
4. Select `Multiple startup projects`.
5. Set the action for `Server.Auth` and `Server.Lobby` to `Start`.
6. Press `Apply` and `OK`.

## Links

- https://www.facebook.com/qpangreborn (Dutch)
- https://qpangreborn.tumblr.com (Dutch)
- https://www.qpang.nl
- https://soundcloud.com/qpangreborn