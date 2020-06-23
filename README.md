
# niwrad

![Acquire activation file](https://github.com/louis030195/niwrad/workflows/Acquire%20activation%20file/badge.svg)
![Build project](https://github.com/louis030195/niwrad/workflows/Build%20project/badge.svg)

![demo](docs/images/demo.gif)
![demo](docs/images/demo2.gif)

## Local usage

1. [Download and import Nakama unitypackage](https://github.com/heroiclabs/nakama-unity) to Assets/Plugins for example.
2. [Download and import NuGet unitypackage](https://github.com/GlitchEnzo/NuGetForUnity) to Assets/Plugins for example.
3. [Download and import Unitask unitypackage](https://github.com/Cysharp/UniTask) to Assets/Plugins for example.
4. Install Google.Protobuf through NuGet.
5. Install docker and docker-compose
6. Install make

### Running

```bash
# Build Unity client and server
make build

# Start Nakama server and Unity server
make nakama_and_server

# In another tab, run a client
make client
```

## OVHcloud deployment

1. Go to ovhcloud website -> login -> public cloud -> create an instance -> pick any
2. Ubuntu 18.04
3. Put your ssh key (cat ~/.ssh/id_rsa.pub for example)
4. Add a post installation script:

```bash
!#/bin/bash
sudo apt-get update
sudo apt-get install -y tmux git apt-transport-https ca-certificates curl software-properties-common make
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu bionic stable"
sudo apt update
sudo apt install -y docker-ce
su -
usermod -aG docker ${USER}
su - ubuntu
sudo curl -L https://github.com/docker/compose/releases/download/1.21.2/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose
git clone https://github.com/louis030195/niwrad
cd niwrad

# TODO: finish this, how to download cloud built artifact ?
curl https://github.com/louis030195/niwrad/suites/809531407/artifacts/8821692
# Unzip ...
tmux new -s niwrad
make nakama_and_server
```

## How it works

### Architecture

Simulate physics is hard. Unity does it decently using Physx under the hood.\
It's easier to just use Unity for that instead of the Nakama server.

![xd](docs/images/niwrad.png)

### Evolution

The principle is to simulate few similar key points among darwinian evolution, obviously the goal is to avoid to go too low level or over-engineer, we ain't got quantum computers !

- Life forms "hosts" that carry characteristics like every "survival machine" on our world that carry genes, these genes "manipulate" the hosts to try survive, here the concept of gene hasn't been introduced.
- Hosts can breed, when they do, they characteristics are "mixed" plus a slight randomness (mutation).
- Hosts behaviour must be generic, so we can either implement simple algorithm like state-machines, behaviour trees or go more complex like (deep) reinforcement learning.
- According to these implementations, the hosts will evolve by natural selection, some characteristics that help survival (speed ... ?) will increase, some that harm survival will decrease
- The fun part: players can do some actions that will trigger artificial selection, e.g. like we human selected the cows that produce the most milk, the goal is to implement actions that offer the possibility to influence evolution. Currently what came to my mind: any way to protect, harm, heal, feed ... some targeted hosts (high speed hosts ? big hosts ...)

## TODO

- [ ] [Nakama config file](https://heroiclabs.com/docs/install-configuration/#example-file) and [nakama config](https://github.com/heroiclabs/nakama/blob/master/server/config.go)
- [ ] bounding volume hierarchy optimization, possibility is so to use an [octree](https://github.com/The-Tensox/octree) on nakama side to reduce client-side unnecessary packets. 
- [ ] Regular state persistence allowing resiliency but especially for allowing to stop and restart server with same state
- [ ] Fix the Assets/Plugins and Assets/Packages (make something automatic download and add to gitignore ..)
- [ ] Implement "robot": a creature that will tweak evolution according to our will, e.g. "I want fast animals" it will kill all slow animals\
    Basically anything that can allow players to apply artificial selection
- [ ] Make the Unity host headless mode Nakama IP/port configurable
- [ ] Custom script unity-builder github workflow (building headless, client etc.)
- [ ] Keyboard hack to request/kill a server via client
- [ ] Fix ugly as hell hacks nakama side (go files mods, protos imports etc.)
- [ ] Android controller
- [ ] Find out why hosts die so fast in net ?
