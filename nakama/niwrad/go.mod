module github.com/louis030195/niwrad

go 1.15

require (
	github.com/golang/protobuf v1.4.3
	github.com/heroiclabs/nakama-common v1.10.0
	github.com/louis030195/octree v0.2.0
	github.com/louis030195/protometry v0.2.0
	golang.org/x/net v0.0.0-20200925080053-05aa5d4ee321 // indirect
	golang.org/x/sys v0.0.0-20200926100807-9d91bd62050c // indirect
	google.golang.org/protobuf v1.25.0
	k8s.io/api v0.19.4
	k8s.io/apimachinery v0.19.4
	k8s.io/client-go v0.19.4
)

replace (
	// Necessary hacks
	github.com/golang/protobuf => github.com/golang/protobuf v1.4.3
	golang.org/x/net => golang.org/x/net v0.0.0-20200925080053-05aa5d4ee321
	golang.org/x/sys => golang.org/x/sys v0.0.0-20200926100807-9d91bd62050c
)
