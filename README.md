
# niwrad

![demo](docs/images/demo.gif)

## Installation

1. [Download and import Nakama unitypackage](https://github.com/heroiclabs/nakama-unity) to Assets/Plugins for example.
2. [Download and import NuGet unitypackage](https://github.com/GlitchEnzo/NuGetForUnity) to Assets/Plugins for example.
3. [Download and import Unitask unitypackage](https://github.com/Cysharp/UniTask) to Assets/Plugins for example.
4. Install Google.Protobuf through NuGet.

## Usage

The Makefile is optimized for Linux, so if you're on different OS just read and adapt (will give instructions later).

```bash
make proto
make build
make nakama
make server
make client
```


## TODO

- [ ] Input / senses / observation system: vision, audio, raycasts (different types), taste ...
- [ ] [Nakama config file](https://heroiclabs.com/docs/install-configuration/#example-file)
- [ ] bounding volume hierarchy optimization, possibility is so to use an [octree](https://github.com/The-Tensox/octree) on nakama side, probably as a service to avoid concurrency issues, let's see ...
- [ ] Regular state persistence allowing resiliency but especially for allowing to stop and restart server with same state
- [ ] Animals can become carnivorous if there is plenty of animals and few vegetables ?
- [ ] Fix the Assets/Plugins and Assets/Packages (make something automatic download and add to gitignore ..)
- [ ] Diamond square on Nakama side
