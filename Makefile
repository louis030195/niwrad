PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
PROTOS = $(PROJECT_PATH)/nakama/niwrad/api
NS ?= niwrad
VERSION ?= 1.0.0
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/2019.4.0f1/Editor/Unity
GOPATH=$(HOME)/go/src
PROTOMETRY=$(GOPATH)/github.com/louis030195/protometry
CSHARP_OUT=Assets/Scripts/Api

help: ## Display this help
	@awk 'BEGIN {FS = ":.*##"; printf "Usage: make \033[36m<target>\033[0m\n\nTargets:\n"} /^[a-zA-Z0-9_-]+:.*?##/ { printf "  \033[36m%-10s\033[0m %s\n", $$1, $$2 }' $(MAKEFILE_LIST)

build: build-client build-server build-images build-proto ## build unity client, docker images and protobufs

build-client: ## build unity client
	@rm -rf Builds/Linux
	@$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinux \
		-silent-crashes -headless
	@echo "Unity client built"

build-server: ## build unity server
	@rm -rf Builds/Linux
	@$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinuxHeadless \
		-silent-crashes -headless
	@echo "Unity server built"

build-image-unity: ## build unity server docker image
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t niwrad-unity .

build-image-nakama: ## build nakama docker image
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t nakama -f ./nakama/niwrad/build/Dockerfile nakama/niwrad

build-images: build-image-unity build-image-nakama ## build docker images

build-proto: ## build protobuf stubs
	@mkdir -p Assets/Scripts/Api/Protometry Assets/Scripts/Api/Realtime Assets/Scripts/Api/Rpc
	@protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/vector3 \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		$(PROTOMETRY)/api/vector3/vector3.proto
	@protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/quaternion \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		$(PROTOMETRY)/api/quaternion/quaternion.proto
	@protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/volume \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		$(PROTOMETRY)/api/volume/volume.proto
	
	@protoc -I $(GOPATH) \
		-I $(PROTOS)/realtime \
		--csharp_out=$(CSHARP_OUT)/Realtime \
		--go_out=. $(PROTOS)/realtime/*.proto
	@protoc -I $(PROTOS)/rpc \
		--csharp_out=$(CSHARP_OUT)/Rpc \
		--go_out=. $(PROTOS)/rpc/*.proto
	@echo "Protocol buffer compiled"

deploy: ## deploy cluster
	@helm install $(NS) helm
	@echo "Cluster deployed"
#	Trick to print Nakama endpoint after deployment, a smarter approach would use something like k8s Ingress
	@pgrep -x minikube >/dev/null && (echo "Nakama endpoint:" && minikube service list | grep 7350) || true

un-deploy: ## un-deploy cluster
	@helm uninstall $(NS)
	@kubectl delete -n default deployment $(NS)-unity || (true && echo "Ignoring ...")
	@echo "Cluster un-deployed"

client: ## Run client
	@./Builds/Linux/Client/$(NS).x86_64
	@echo "Running Linux client"
