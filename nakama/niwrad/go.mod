module github.com/louis030195/niwrad

go 1.14

require (
	github.com/golang/protobuf v1.3.5
	github.com/heroiclabs/nakama-common v1.5.1
	github.com/louis030195/octree v0.0.0-20200727155128-619771c4a0c8
	github.com/louis030195/protometry v0.0.0-20200729072249-534cf8dbbc1f
	golang.org/x/net v0.0.0-20200324143707-d3edc9973b7e // indirect
	gopkg.in/yaml.v2 v2.2.8 // indirect
	k8s.io/api v0.17.0
	k8s.io/apimachinery v0.17.0
	k8s.io/client-go v0.17.0
)

replace (
	//	github.com/golang/protobuf => github.com/golang/protobuf v1.3.5
	github.com/louis030195/octree => /home/louis/go/src/github.com/louis030195/octree
	github.com/louis030195/protometry => /home/louis/go/src/github.com/louis030195/protometry
)
