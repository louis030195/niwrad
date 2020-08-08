PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
PROTOS = $(PROJECT_PATH)/nakama/niwrad/api
NS ?= niwrad
VERSION ?= 1.0.0
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/2019.4.0f1/Editor/Unity
GOPATH=$(HOME)/go/src
PROTOMETRY=$(GOPATH)/github.com/louis030195/protometry
CSHARP_OUT=Assets/Scripts/Api


.PHONY: help build-client build-server build-images build-proto deploy un-deploy client
help:
		@echo ''
		@echo 'Usage: make [TARGET]'
		@echo 'Targets:'
		@echo '  build    		build unity client, docker images and protobufs'
		@echo '  build-client    	build unity client'
		@echo '  build-server    	build unity server'
		@echo '  build-images    	build docker images'
		@echo '  build-proto    	build protobuf stubs'
		@echo '  deploy    		deploy cluster'
		@echo '  un-deploy    		un-deploy cluster'
		@echo '  client    		launch Linux client'
		@echo ''

build: build-client build-server build-images build-proto

build-client:
	@rm -rf Builds/Linux
	@$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinux \
		-silent-crashes -headless
	@echo "Unity client built"

build-server:
	@rm -rf Builds/Linux
	@$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinuxHeadless \
		-silent-crashes -headless
	@echo "Unity server built"

build-images:
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t nakama -f ./nakama/niwrad/build/Dockerfile nakama/niwrad
	docker build -t niwrad-unity .

build-proto:
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

deploy:
	@helm install $(NS) helm
	@echo "Cluster deployed"
#	Trick to print Nakama endpoint after deployment, a smarter approach would use something like k8s Ingress
	@pgrep -x minikube >/dev/null && (echo "Nakama endpoint:" && minikube service list | grep 7350) || true

un-deploy:
	@helm uninstall $(NS)
	@kubectl delete -n default deployment $(NS)-unity || true && echo "Ignoring ..."
	@echo "Cluster un-deployed"

client:
	@./Builds/Linux/Client/$(NS).x86_64
	@echo "Running Linux client"
