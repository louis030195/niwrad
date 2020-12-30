PROTOS = api
GOPATH=$(HOME)/go/src
CSHARP_OUT=../Assets/Scripts/Api
PROTOMETRY=$(GOPATH)/github.com/louis030195/protometry

help: ## Display this help
	@echo 'usage: make [target] ...'
	@echo
	@echo -e '\033[35mTargets:\033[0m'
	@egrep '^(.+)\:\ ##\ (.+)' ${MAKEFILE_LIST} | column -t -c 2 -s ':#'

build-proto: ## Build protobuf stubs
	mkdir -p $(CSHARP_OUT)/Realtime $(CSHARP_OUT)/Rpc

#	Protometry protoc
	protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/vector3 \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		$(PROTOMETRY)/api/vector3/vector3.proto
	protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/quaternion \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		$(PROTOMETRY)/api/quaternion/quaternion.proto
	protoc -I $(GOPATH) \
		-I $(PROTOMETRY)/api/volume \
		--csharp_out=$(CSHARP_OUT)/Protometry \
		$(PROTOMETRY)/api/volume/volume.proto

#	Niwrad protoc
	protoc -I $(GOPATH) \
		-I $(PROTOS)/realtime \
		--csharp_out=$(CSHARP_OUT)/Realtime \
		--go_out=. \
		$(PROTOS)/realtime/*.proto
	protoc -I $(GOPATH) \
		-I $(PROTOS)/rpc \
		-I $(PROTOS)/realtime \
		--csharp_out=$(CSHARP_OUT)/Rpc \
		--go_out=. \
		$(PROTOS)/rpc/*.proto
#	-I is used for reference to other proto (-I $(PROTOS)/realtime)
	@echo "\033[35mProtocol buffer compiled\033[0m"

deploy-dev: ## Deploy niwrad's Nakama dev cluster on a k8s cluster in niwrad-dev namespace using default values
	helm install niwrad deploy/helm -f deploy/helm/values.yaml -n niwrad-dev

deploy-prod: ## Deploy niwrad's Nakama prod cluster on a k8s cluster in niwrad-prod namespace using default values
	helm install niwrad deploy/helm -f deploy/helm/values.yaml -n niwrad-prod

comp: ## Run a Nakama local container
	go mod vendor
	docker-compose -f deploy/docker-compose.yml up --build
