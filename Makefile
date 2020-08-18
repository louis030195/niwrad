PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
PROTOS = $(PROJECT_PATH)/nakama/niwrad/api
NS ?= niwrad
VERSION ?= 1.0.0
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/2019.4.0f1/Editor/Unity
GOPATH=$(HOME)/go/src
PROTOMETRY=$(GOPATH)/github.com/louis030195/protometry
CSHARP_OUT=Assets/Scripts/Api
JS_OUT=niwrad-js/lib/proto




help: ## Display this help
	@echo 'usage: make [target] ...'
	@echo
	@echo 'targets:'
	@egrep '^(.+)\:\ ##\ (.+)' ${MAKEFILE_LIST} | column -t -c 2 -s ':#'

build: build-proto build-client-artifact build-server-artifact build-images ## Build unity client, docker images and protobufs

build-client-artifact: ## Build unity client
	@rm -rf Builds/Linux
	@$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinux \
		-silent-crashes -headless
	@echo "Unity client built"

build-server-artifact: ## Build unity server
	@rm -rf Builds/Linux
	@$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinuxHeadless \
		-silent-crashes -headless
	@echo "Unity server built"

build-unity-image: ## Build unity server docker image
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

build-images: build-unity-image build-js-image build-integration-tests-image build-nakama-image ## Build docker images

build-proto: ## Build protobuf stubs
	@mkdir -p Assets/Scripts/Api/Protometry Assets/Scripts/Api/Realtime Assets/Scripts/Api/Rpc

#	Protometry protoc
	@protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/vector3 \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOMETRY)/api/vector3/vector3.proto
	@protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/quaternion \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOMETRY)/api/quaternion/quaternion.proto
	@protoc -I $(GOPATH) \
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
	@protoc -I $(GOPATH) \
		-I $(PROTOS)/realtime \
		--csharp_out=$(CSHARP_OUT)/Realtime \
		--go_out=. \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOS)/realtime/*.proto
	@protoc -I $(GOPATH) \
		-I $(PROTOS)/rpc \
		--csharp_out=$(CSHARP_OUT)/Rpc \
		--go_out=. \
		--js_out=import_style=commonjs,binary:$(JS_OUT) \
		$(PROTOS)/rpc/*.proto

	@echo "Protocol buffer compiled"

deploy: ## Deploy cluster
	@helm install $(NS) helm
	@echo "Cluster deployed"
#	Trick to print Nakama endpoint after deployment, a smarter approach would use something like k8s Ingress
	@pgrep -x minikube >/dev/null && \
	(echo "Nakama endpoint:" && minikube service list | grep -oE '.*7350.*(http:\/\/[0-9]*.[0-9]*.[0-9]*.[0-9]*:[0-9]*)') || true

un-deploy: ## Un-deploy cluster
	@helm uninstall $(NS)
	@kubectl delete -n default deployment $(NS)-unity > /dev/null || (true && echo "Ignoring ...")
	@echo "Cluster un-deployed"

client: ## Run client
	@./Builds/Linux/Client/$(NS).x86_64
	@echo "Running Linux client"

test: ## Run unit tests and integration tests
#	Who need test anyway ?
#	Welcome to Unity CLI: segmentation fault heaven
# 	$(EDITOR_PATH) -runTests -projectPath $(PROJECT_PATH) -testResults /tmp/results.xml -testPlatform editmode -headless

# 	Only run helm tests if the cluster is deployed
	@kubectl get deployment | grep -q '$(NS)' && echo "Running integration tests ..." && helm test $(NS) && kubectl logs $(NS)-test
	@echo "Everything passed"
