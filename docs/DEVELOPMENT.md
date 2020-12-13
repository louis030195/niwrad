# Development

```bash
git clone https://github.com/louis030195/niwrad
```

## Prerequisites

1. [Docker](https://www.docker.com)
2. [Install kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
3. [Install helm](https://helm.sh/docs/intro/install/)
4. (Optional) Install [minikube](https://kubernetes.io/docs/tasks/tools/install-minikube/) or [k3s](https://k3s.io/)
5. [Unity](https://unity.com)
6. make: `sudo apt install make`
7. [protoc, protoc-gen-go, protoc-gen-csharp](https://github.com/protocolbuffers/protobuf)
    - `sudo apt install -y protobuf-compiler`
    - `go get -u github.com/golang/protobuf/protoc-gen-go`

```make
usage: make [target] ...

Targets:
help                               Display this help
build                              Build unity client, docker images and protobufs
build-images                       Build docker images
build-executor-image               Build unity executor docker image
build-js-image                     Build js client docker image
build-integration-tests-image      Build integration tests docker image
build-nakama-image                 Build nakama docker image
build-proto                        Build protobuf stubs
deploy                             Deploy cluster
un-deploy                          Un-deploy cluster
test                               Run unit tests and integration tests
```

## Testing

```bash
# Should run Unity unit tests
# Run integration tests with Helm (imply that you have a configured k8s/k3s cluster, Helm)
make test
```

## TODO

- experience different reproductions (asexual, sexual species, predators, parasites)
- artificial selection: carnivorous, rework, limited (cooldown ?), parameterable
- record experience, better metrics
- overall UX: toasts improvement (%random sentence discord-like, deploy a GPT3 on a raspberry ? ;))
- android google play
- Steam
- Helm chart: optional [Prometheus operator](https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack) setup (ServiceMonitor CRD) or metrics pusher, [Vector](https://github.com/timberio/vector) for logs to cloud
- for more, see issues
