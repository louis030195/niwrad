PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
PROTOS = $(PROJECT_PATH)/nakama/niwrad/api
NS ?= niwrad
VERSION ?= 1.0.0
UNITY_VERSION=2020.1.4f1
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/$(UNITY_VERSION)/Editor/Unity
LICENSE_PATH ?= $(HOME)/.local/share/unity3d/Unity/
GOPATH=$(HOME)/go/src
PROTOMETRY=$(GOPATH)/github.com/louis030195/protometry
CSHARP_OUT=Assets/Scripts/Api
JS_OUT=niwrad-js/lib/proto

UNITY_USERNAME?=foo
UNITY_PASSWORD?=bar
UNITY_LICENSE_CONTENT?=$(shell cat "${LICENSE_PATH}/Unity_lic.ulf")

# TODO: make k8s namespace for niwrad

# $(pgrep -x minikube && echo "Detected minikube" && eval ${minikube -p minikube docker-env})

help: ## Display this help
	@echo 'usage: make [target] ...'
	@echo
	@echo -e '\033[35mTargets:\033[0m'
	@egrep '^(.+)\:\ ##\ (.+)' ${MAKEFILE_LIST} | column -t -c 2 -s ':#'

build: ## Build unity client, docker images and protobufs
build: build-proto build-client-artifact build-server-artifact build-images

build-client-artifact: ## Build unity client
	# rm -rf Builds/Linux
	# $(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
	# 	-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinux \
	# 	-silent-crashes -headless
	# @echo "\033[35mUnity client built\033[0m"
	echo "Doesn't work !!! THANKS UNITY FOR NOT SUPPORTING LINUX "
	# docker run -it --rm -e "UNITY_LICENSE_CONTENT=$(UNITY_LICENSE_CONTENT)" \
	# 	-e "TEST_PLATFORM=linux" -e "WORKDIR=/root/project" -v "$(pwd):/root/project" \
	# 	gableroux/unity3d:$(UNITY_VERSION) \
	# 	bash

	# 	/opt/Unity/Editor/Unity \
	# 	-logFile /dev/stdout \
	# 	-batchmode -buildLinux64Player


build-server-artifact: ## Build unity server
	echo "Doesn't work !!! THANKS UNITY FOR NOT SUPPORTING LINUX "

	# rm -rf Builds/Linux
	# $(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
	# 	-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinuxHeadless \
	# 	-silent-crashes -headless -nographics
	# @echo "\033[35mUnity server built\033[0m"


build-images: ## Build docker images
build-images: build-executor-image build-js-image build-integration-tests-image build-nakama-image

build-executor-image: ## Build unity executor docker image
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t niwrad-unity .

build-js-image: ## Build js client docker image
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t niwrad-js -f ./niwrad-js/Dockerfile niwrad-js

build-integration-tests-image: ## Build integration tests docker image
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t niwrad-integration-tests -f ./helm/templates/tests/ApiTest/Dockerfile .

build-nakama-image: ## Build nakama docker image
	# Don't forget to run `eval $(minikube -p minikube docker-env)` if using minikube :)
	docker build -t nakama -f ./nakama/niwrad/build/Dockerfile nakama/niwrad

build-proto: ## Build protobuf stubs
	mkdir -p Assets/Scripts/Api/Protometry Assets/Scripts/Api/Realtime Assets/Scripts/Api/Rpc

#	Protometry protoc
	protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/vector3 \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOMETRY)/api/vector3/vector3.proto
	protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/quaternion \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOMETRY)/api/quaternion/quaternion.proto
	protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/volume \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOMETRY)/api/volume/volume.proto
#	Ugly hack until a better solution is found ?
#	mv $(JS_OUT)/github.com/louis030195/protometry/api/quaternion/* $(JS_OUT)
#	mv $(JS_OUT)/github.com/louis030195/protometry/api/vector3/* $(JS_OUT)
#	mv $(JS_OUT)/github.com/louis030195/protometry/api/volume/* $(JS_OUT)
#	rm -rf $(JS_OUT)/github.com

#	Niwrad protoc
	protoc -I $(GOPATH) \
		-I $(PROTOS)/realtime \
		--csharp_out=$(CSHARP_OUT)/Realtime \
		--go_out=. \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOS)/realtime/*.proto
	protoc -I $(GOPATH) \
		-I $(PROTOS)/rpc \
		--csharp_out=$(CSHARP_OUT)/Rpc \
		--go_out=. \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOS)/rpc/*.proto

	@echo "\033[35mProtocol buffer compiled\033[0m"

deploy: ## Deploy cluster
	helm install $(NS) helm
#	Trick to print Nakama endpoint after deployment, a smarter approach would use something like k8s Ingress
#	TODO: fix this shit
# 	@INGRESS_HOST=$(minikube ip)
# 	@json='{.spec.ports[?(@.name=="api")].nodePort}'
# 	@INGRESS_PORT=$(kubectl get service ${NS} -o jsonpath=${json})
# 	@echo "Connect to Nakama at $(INGRESS_HOST):$(INGRESS_PORT)"

un-deploy: ## Un-deploy cluster
	helm uninstall $(NS)
#	TODO: make that if works (only delete unity executors if they exists)
#	ifeq ($(kubectl get deployment -l tier=executor)
	kubectl delete deployment -l tier=executor
	kubectl delete pod -l tier=executor
	kubectl delete pod niwrad-test
#	endif
	@echo "\033[35mCluster un-deployed\033[0m"

client: ## Run client
	./Builds/Linux/Client/$(NS).x86_64
	@echo "\033[35mRunning Linux client\033[0m"

test: ## Run unit tests and integration tests
#	Who need test anyway ?
#	Welcome to Unity CLI: segmentation fault heaven
# 	$(EDITOR_PATH) -runTests -projectPath $(PROJECT_PATH) -testResults /tmp/results.xml -testPlatform editmode -headless

# 	Only run helm tests if the cluster is deployed
	kubectl get deployment | grep -q '$(NS)' && echo "Running integration tests ..." && helm test $(NS) && kubectl logs $(NS)-test
	@echo "\033[35mEverything passed\033[0m"

