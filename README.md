
# niwrad

## Installation

1. [Download and import Nakama unitypackage](https://github.com/heroiclabs/nakama-unity) to Assets/Plugins for example.
2. [Download and import NuGet unitypackage](https://github.com/GlitchEnzo/NuGetForUnity) to Assets/Plugins for example.
3. Install Google.Protobuf through NuGet.

## Usage

The Makefile is optimized for Linux, so if you're on different OS just read and adapt (will give instructions later).

```bash
make proto
make build
make run
```

```bash
# Monitor logs
tail -f ~/.config/unity3d/com.niwrad/Niwrad/Player.log
```


## TODO

- [ ] Input / senses / observation system: vision, audio, raycasts (different types), taste ...
