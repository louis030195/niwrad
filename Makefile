PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
NS ?= niwrad
VERSION ?= 1.0.0
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/2019.4.0f1/Editor/Unity
IMAGE_NAME ?= niwrad
CONTAINER_NAME ?= niwrad
CONTAINER_INSTANCE ?= default


# TODO: clean this makefile :)
.PHONY: help build-client build-headless build client server nakama nakama_and_server proto docker-build docker-run
help:
		@echo ''
		@echo 'Usage: make [TARGET]'
		@echo 'Targets:'
		@echo '  install    	blabla'
		@echo ''


build-client:
	rm -rf Builds/Linux
	$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinux \
		-silent-crashes -headless 

build-headless:
	rm -rf Builds/Linux
	$(EDITOR_PATH) -batchmode -quit -logFile /tmp/$(NS)_unity_build.log -projectPath $(PROJECT_PATH) \
		-buildLinux64Player $(PROJECT_PATH)/Builds/Linux -executeMethod Editor.Builds.BuildLinuxHeadless \
		-silent-crashes -headless

build: build-client build-headless

client:
	./Builds/Linux/Client/$(NS).x86_64

server:
	./Builds/Linux/Server/$(NS).x86_64

nakama:
	cp -r ./Builds/Linux/Server Server/modules
	docker-compose -f Server/docker-compose-auto.yml up --build nakama
	# rm -rf Server/modules/Server # risky as fk
	#docker-compose -f Server/docker-compose.yml up

nakama_and_server: 
	docker-compose -f Server/docker-compose-auto.yml up --build nakama &
	# TODO: wait for docker boot :)
	sleep 20
	./Builds/Linux/Server/$(NS).x86_64

proto:
	protoc -I $(PROJECT_PATH)/realtime \
	--csharp_out=Assets/Scripts/Net/Realtime --go_out=Server/modules $(PROJECT_PATH)/realtime/*.proto
	protoc -I $(PROJECT_PATH)/rpc \
	--csharp_out=Assets/Scripts/Net/Rpc --go_out=Server/modules $(PROJECT_PATH)/rpc/*.proto


docker-build:
	# FIXME: make build always say "failed to build" (but succeed) it break the make pipeline
	# also u need to press enter -.-
	# make proto
	# make build
	docker build -t $(NS)/$(IMAGE_NAME):$(VERSION) -f Dockerfile .

docker-run:
	docker run --rm --name $(CONTAINER_NAME)-$(CONTAINER_INSTANCE) $(PORTS) $(VOLUMES) $(ENV) $(NS)/$(IMAGE_NAME):$(VERSION)

default: build