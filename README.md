
# niwrad

![Acquire activation file](https://github.com/louis030195/niwrad/workflows/Acquire%20activation%20file/badge.svg)
![Build project](https://github.com/louis030195/niwrad/workflows/Build%20project/badge.svg)

[![Video](docs/images/demo1.gif)](https://www.youtube.com/watch?v=B0MwLHRPuP8)

Try the offline WebGL version directly [here](http://louis030195.github.io/niwrad), if you want to try on other OS (Windows, Linux, Android, Web available) [check out latest Github Actions artifacts](https://github.com/louis030195/niwrad/actions) or releases.

**The development on online, multiplayer version is paused.**

See related writings:

* [Blog post part one](https://medium.com/swlh/a-simulation-of-evolution-part-one-62a1acfb009a)
* [Blog post part two](https://medium.com/@louis.beaumont/a-simulation-of-evolution-two-b26664d159a5)

## Features

* [x] Heuristic AI (state machine)
* [x] Artificial selection (partially)
  * [x] Spawn plants / animals by drag & drop
* [x] Reproducible experiences
  * [x] Ugly as hell UI but UX works
* [x] Experience metrics (partially)
  * [x] Big ugly panel in game with number of animals ...
* [x] Optional carnivorous hosts
* [x] Android, Linux, Windows, Web, (iOS/MacOS not tested but should work)
  * [x] Mobile joysticks

## Development

### Dependencies

* Online mode: [Nakama](https://github.com/heroiclabs/nakama)
* Online mode: <https://github.com/louis030195/octree> for "network culling" i.e. if an animal moves in (1000,0,1000) and I'm in (0,0,0) I don't want to be notified of that.
* Online mode: deployments (Docker, Kubernetes, Helm, Minikube / k3s)
* Both: Unitask, Protobuf, TextMesh Pro

### Goals

* Hosts have characteristics. When hosts reproduce sexually or asexually, the offspring characteristics are its parent's plus mutations.
* Hosts behaviour code MUST be generic, so we can either implement simple heuristics like state-machines, behaviour trees, utility or try learning AI like reinforcement learning.
* Observers can trigger artificial selection, the goal is to implement actions that offer the possibility to influence evolution. Currently what came to my mind: any way to protect, harm, heal, feed ... some species

### Non-goals

* Simulating nature at the quantum level

## Development

```bash
git clone https://github.com/louis030195/niwrad
```

### Prerequisites

#### Online

1. [Docker](https://www.docker.com)
2. [Install kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
3. [Install helm](https://helm.sh/docs/intro/install/)
4. [Install minikube](https://kubernetes.io/docs/tasks/tools/install-minikube/) (for local k8s)

#### Online & Offline

1. [Unity](https://unity.com) to build artifacts or find a client [here](https://github.com/louis030195/niwrad/actions)
2. make is recommended: `sudo apt install make`
3. [protoc, protoc-gen-go, protoc-gen-csharp](https://github.com/protocolbuffers/protobuf)
    - `sudo apt install -y protobuf-compiler`
    - `go get -u github.com/golang/protobuf/protoc-gen-go`

```make
usage: make [target] ...

targets:
help                               Display this help
build-client-artifact              Build unity client
build-server-artifact              Build unity server
build-unity-image                  Build unity server docker image
build-js-image                     Build js client docker image
build-integration-tests-image      Build integration tests docker image
build-nakama-image                 Build nakama docker image
build-proto                        Build protobuf stubs
deploy                             Deploy cluster
un-deploy                          Un-deploy cluster
client                             Run client
test                               Run unit tests and integration tests
```

## Testing

```bash
# Should run Unity unit tests (but unfortunately Unity CLI rarely works on Ubuntu 20.04 at least so it Seg Fault)
# Run integration tests with Helm (imply that you have a configured k8s/k3s cluster, Helm)
make test
```

## TODO

* experience menu -> import experiences (setup a cloud storage with some experiences for example).
* experience different reproductions (asexual, sexual species, predators, parasites)
* better metrics
* see issues
