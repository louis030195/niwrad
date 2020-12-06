PROJECT_PATH ?= $(HOME)/Documents/niwrad
PROTOS = $(PROJECT_PATH)/nakama/niwrad/api
NS ?= niwrad
VERSION ?= 1.0.0
GOPATH=$(HOME)/go/src
PROTOMETRY=$(GOPATH)/github.com/louis030195/protometry
CSHARP_OUT=Assets/Scripts/Api
JS_OUT=niwrad-js/lib/proto

# TODO: make k8s namespace for niwrad

help: ## Display this help
	@echo 'usage: make [target] ...'
	@echo
	@echo -e '\033[35mTargets:\033[0m'
	@egrep '^(.+)\:\ ##\ (.+)' ${MAKEFILE_LIST} | column -t -c 2 -s ':#'

build: ## Build unity client, docker images and protobufs
build: build-proto build-images

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

un-deploy: ## Un-deploy cluster
	helm uninstall $(NS)
	kubectl delete deployment -l tier=executor
	kubectl delete pod -l tier=executor
	kubectl delete pod niwrad-test
	@echo "\033[35mCluster un-deployed\033[0m"

test: ## Run unit tests and integration tests
# 	Only run helm tests if the cluster is deployed
	kubectl get deployment | grep -q '$(NS)' && echo "Running integration tests ..." && helm test $(NS) && kubectl logs $(NS)-test
	@echo "\033[35mEverything passed\033[0m"

