module github.com/louis030195/niwrad

go 1.14

require (
	github.com/golang/protobuf v1.3.5
	github.com/heroiclabs/nakama-common v1.5.1
	github.com/louis030195/octree v0.0.0-20200502124658-d5eedbdf3820 // indirect
	github.com/louis030195/protometry v0.0.0-20200502124743-c5fd69c974e2 // indirect
	gopkg.in/yaml.v2 v2.2.8 // indirect
	k8s.io/api v0.17.0
	k8s.io/apimachinery v0.17.0
	k8s.io/client-go v0.17.0

)

replace (
    github.com/golang/protobuf => github.com/golang/protobuf v1.3.5
    github.com/louis030195/octree => /home/louis/go/src/github.com/louis030195/octree
    github.com/louis030195/protometry => /home/louis/go/src/github.com/louis030195/protometry
)
