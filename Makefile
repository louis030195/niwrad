PROJECT_PATH ?= $(HOME)/Documents/unity/niwrad
PROTOS = $(PROJECT_PATH)/nakama/niwrad/api
NS ?= niwrad
VERSION ?= 1.0.0
EDITOR_PATH ?= $(HOME)/Unity/Hub/Editor/2019.4.0f1/Editor/Unity


.PHONY: help build-client build-headless build client server nakama proto helm
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
	docker build -t nakama nakama/niwrad
	docker build -t niwrad-unity .

client:
	./Builds/Linux/Client/$(NS).x86_64

server:
	./Builds/Linux/nakama/$(NS).x86_64
	# docker run niwrad-unity /app/niwrad.x86_64 --nakamaIp 172.17.0.1 --nakamaPort 7350

nakama:
	docker-compose -f nakama/niwrad/docker-compose.yml up --build nakama
	# docker run niwrad-unity /app/niwrad.x86_64 --nakamaIp 127.0.0.1 --nakamaPort 7350

proto:
	mkdir -p Assets/Scripts/Api/Realtime Assets/Scripts/Api/Rpc
	protoc -I $(PROTOS)/realtime \
		--csharp_out=Assets/Scripts/Api/Realtime \
		--go_out=. $(PROTOS)/realtime/*.proto
	protoc -I $(PROTOS)/rpc \
		--csharp_out=Assets/Scripts/Api/Rpc \
		--go_out=. $(PROTOS)/rpc/*.proto

helm:
	helm install $(NS) helm

default: build