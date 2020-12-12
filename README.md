
# niwrad

![Acquire activation file](https://github.com/louis030195/niwrad/workflows/Acquire%20activation%20file/badge.svg)
![Build Niwrad](https://github.com/louis030195/niwrad/workflows/Build%20Niwrad/badge.svg)

**Android version**
[![Video](docs/images/demo.gif)](https://www.youtube.com/watch?v=B0MwLHRPuP8)

* Try directly outdated WebGL version [here](http://louis030195.github.io/niwrad) ([WebGL CI is broken yet](https://github.com/game-ci/unity-builder/issues/179))
* [Or download latest version](https://github.com/louis030195/niwrad/releases). (Windows, Linux, Android, Web, iOS, MacOS).

See related writings:

* [Blog post part one](https://medium.com/swlh/a-simulation-of-evolution-part-one-62a1acfb009a)
* [Blog post part two](https://medium.com/@louis.beaumont/a-simulation-of-evolution-two-b26664d159a5)

## Features

* [x] Heuristic AI (state machine)
* [x] Artificial selection (partially)
  * [x] Spawn plants / animals by drag & drop
* [x] Parametrable experiences
  * [x] Working UX
* [x] Experience metrics (partially)
  * [x] Big ugly panel in game with number of animals ...
* [x] Optional carnivorous hosts
* [x] Android, Linux, Windows, Web, iOS, MacOS
  * [x] Mobile joysticks
  * [x] Passable UX
* [x] Multiplayer + singleplayer
  * [x] Can share experiences
  * [x] Leaderboard (yet naïve: who had the most hosts alive = biggest computer)

## Dependencies

* Online mode: [Nakama](https://github.com/heroiclabs/nakama)
* Online mode: <https://github.com/louis030195/octree>
* Online mode: deployment (Docker, Kubernetes, Helm)
* Both: Unitask, Protobuf, TextMesh Pro, new unity input system, few free assets

## Goals

* Hosts have characteristics. When hosts reproduce sexually or asexually, the offspring characteristics are its parent's plus mutations.
* Hosts behaviour code MUST be generic, so we can either implement simple heuristics like state-machines, behaviour trees, utility or try learning AI like reinforcement learning.
* Observers can trigger artificial selection, the goal is to implement actions that offer the possibility to influence evolution. Currently what came to my mind: any way to protect, harm, heal, feed ... some species

## Non-goals

* Simulating nature at the quantum level

## Development

[See](docs/DEVELOPMENT.md)
