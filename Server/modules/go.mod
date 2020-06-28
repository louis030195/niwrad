module niwrad

go 1.14

require (
	golang.org/x/net v0.0.0-20200324143707-d3edc9973b7e
	// github.com/The-Tensox/octree v0.0.0-20200502124658-d5eedbdf3820
	// github.com/The-Tensox/protometry v0.0.0-20200502124743-c5fd69c974e2
	github.com/heroiclabs/nakama-common v1.5.1 // indirect
	k8s.io/client-go v0.17.0 // indirect
	github.com/golang/protobuf v1.3.5 // indirect
	gopkg.in/yaml.v2 v2.2.8

)

replace (
	github.com/golang/protobuf => github.com/golang/protobuf v1.3.5
	golang.org/x/net => golang.org/x/net v0.0.0-20200324143707-d3edc9973b7e
	gopkg.in/yaml.v2 => gopkg.in/yaml.v2 v2.2.8
)
// replace github.com/The-Tensox/octree => /home/louis/go/src/github.com/The-Tensox/octree

// replace github.com/The-Tensox/protometry => /home/louis/go/src/github.com/The-Tensox/protometry
