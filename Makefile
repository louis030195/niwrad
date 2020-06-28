PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
NS ?= niwrad
VERSION ?= 1.0.0
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/2019.4.0f1/Editor/Unity


.PHONY: help build-client build-headless build client server nakama proto helm-dry helm
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
	helm install niwrad Server/base
	# docker-compose -f Server/docker-compose.yml up --build nakama
	# rm -rf Server/modules/Server # risky as fk

proto:
	protoc -I $(PROJECT_PATH)/realtime \
	--csharp_out=Assets/Scripts/Net/Realtime --go_out=Server/modules $(PROJECT_PATH)/realtime/*.proto
	protoc -I $(PROJECT_PATH)/rpc \
	--csharp_out=Assets/Scripts/Net/Rpc --go_out=Server/modules $(PROJECT_PATH)/rpc/*.proto

helm-dry:
	helm install --debug --dry-run $(NS) Server/modules/mychart

helm:
	helm install $(NS) Server/modules/mychart

default: build