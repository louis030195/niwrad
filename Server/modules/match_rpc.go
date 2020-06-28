package main

import (
	"context"
	"database/sql"
	"fmt"
	"log"
	"os/exec"

	"niwrad/rpc"

	"github.com/golang/protobuf/jsonpb"
	"github.com/golang/protobuf/proto"
	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"
	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/rest"
)

var (
	// Error responses
	errBadContext    = runtime.NewError("bad context", 3)
	errMarshal       = runtime.NewError("cannot marshal response", 13)
	errUnmarshal     = runtime.NewError("cannot unmarshal request", 13)
	errGetAccount    = runtime.NewError("cannot find account", 14)
	errUpdateAccount = runtime.NewError("cannot update account", 14)
	errGetServers    = runtime.NewError("cannot find servers", 15)
)

type sessionContext struct {
	UserID    string
	SessionID string
}

// type userStorage struct {
// 	servers []int // Foreign key
// }

// type unityServer struct {
// 	Pid int
// }

func unpackContext(ctx context.Context) (*sessionContext, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
		return nil, errBadContext
	}
	sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	if !ok {
		return nil, errBadContext
	}
	return &sessionContext{UserID: userID, SessionID: sessionID}, nil
}
func rpcCreateMatch(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error("unpack context failed: %v", err)
		return "", errBadContext
	}

	var request rpc.CreateMatchRequest
	if err = proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	// Create the party match.
	matchID, err := nk.MatchCreate(ctx, "Niwrad", map[string]interface{}{
		"creator": session.UserID,
	})

	if err != nil {
		logger.Error("match created failed: %v", err)
		response := &rpc.CreateMatchResponse{
			Result: rpc.CreateMatchCompletionResult_createMatchCompletionResultUnknownInternalFailure,
		}
		responseBytes, err := proto.Marshal(response)
		if err != nil {
			return "", errMarshal
		}
		return string(responseBytes), nil
	}

	// Return result to user.
	response := &rpc.CreateMatchResponse{
		MatchId: matchID,
		Result:  rpc.CreateMatchCompletionResult_createMatchCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}

// Request to create a Unity server, should check if user is allowed to do that
// If yes, spawn the server with the config and store the server (with process PID and other infos) in the user account
func rpcRunUnityServer(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error("unpack context failed: %v", err)
		return "", errBadContext
	}

	user, userStorage, err := getUserInfo(ctx, nk, session.UserID)
	if err != nil {
		logger.Error("failed to get user info")
		return "", err
	}
	servers, err := getServers(ctx, nk, userStorage.Servers)
	if err != nil {
		logger.Error("failed to get user info")
		return "", err
	}

	var request rpc.RunServerRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	logger.Info("A server creation has been asked by {\"username\": %s, \"storage\":%v, \"servers\":%v} with config: %v",
		user.Username,
		userStorage,
		servers,
		request)

	args := []string{"--terrainSize", "", "--initialAnimals", "", "--initialPlants", ""}
	if request.Configuration.TerrainSize < 1 {
		request.Configuration.TerrainSize = 1000
	}
	if request.Configuration.InitialAnimals < 1 {
		request.Configuration.InitialAnimals = 50
	}
	if request.Configuration.InitialPlants < 1 {
		request.Configuration.InitialPlants = 100
	}
	args[1] = fmt.Sprintf("%v", request.Configuration.TerrainSize)
	args[3] = fmt.Sprintf("%v", request.Configuration.InitialAnimals)
	args[5] = fmt.Sprintf("%v", request.Configuration.InitialPlants)

	cmd := exec.Command("./niwrad.x86_64", args...) // TODO: executable name may vary (cloud build ...)
	// cmd.Stdout = os.Stdout
	if err := cmd.Start(); err != nil {
		log.Fatal(err)
	}
	pid := cmd.Process.Pid
	server := rpc.UnityServer{Pid: int32(pid), Configuration: request.Configuration}
	logger.Info("New server %v", server)

	// creates the in-cluster config
	config, err := rest.InClusterConfig()
	if err != nil {
		panic(err.Error())
	}
	// creates the clientset
	clientset, err := kubernetes.NewForConfig(config)
	if err != nil {
		panic(err.Error())
	}
	// get pods in all the namespaces by omitting namespace
	// Or specify namespace to get pods in particular namespace
	pods, err := clientset.CoreV1().Pods("").List(metav1.ListOptions{})
	if err != nil {
		panic(err.Error())
	}
	logger.Info("Pods:\n", pods.Items)

	// Examples for error handling:
	// - Use helper functions e.g. errors.IsNotFound()
	// - And/or cast to StatusError and use its properties like e.g. ErrStatus.Message
	// _, err = clientset.CoreV1().Pods("default").Get(context.TODO(), "example-xxxxx", metav1.GetOptions{})
	// if errors.IsNotFound(err) {
	// 	fmt.Printf("Pod example-xxxxx not found in default namespace\n")
	// } else if statusError, isStatus := err.(*errors.StatusError); isStatus {
	// 	fmt.Printf("Error getting pod %v\n", statusError.ErrStatus.Message)
	// } else if err != nil {
	// 	panic(err.Error())
	// } else {
	// 	fmt.Printf("Found example-xxxxx pod in default namespace\n")
	// }

	// jsonServer, err := json.Marshal(server)
	// if err != nil {
	// 	logger.Error("Failed to marshal server")
	// 	return "", errMarshal
	// }
	// objects := []*runtime.StorageWrite{
	// 	{
	// 		Collection:      "server",
	// 		Key:             fmt.Sprintf("%d", server.Pid),
	// 		UserID:          session.UserID,
	// 		Value:           string(jsonServer),
	// 		PermissionRead:  2,
	// 		PermissionWrite: 1,
	// 	},
	// }

	// if _, err := nk.StorageWrite(ctx, objects); err != nil {
	// 	logger.Error("Failed to write server to storage: %s", err.Error())
	// 	return "", errUpdateAccount
	// }
	// if userStorage != nil { // This user is already in storage
	// 	userStorage.Servers = append(userStorage.Servers, int32(pid))
	// } else {
	// 	userStorage = &rpc.User{Servers: []int32{int32(pid)}}
	// }
	// jsonUser, err := json.Marshal(userStorage)
	// if err != nil {
	// 	logger.Error("Failed to marshal user")
	// 	return "", errMarshal
	// }
	// objects = []*runtime.StorageWrite{
	// 	{
	// 		Collection:      "user",
	// 		Key:             session.UserID,
	// 		UserID:          session.UserID,
	// 		Value:           string(jsonUser),
	// 		PermissionRead:  2,
	// 		PermissionWrite: 1,
	// 	},
	// }

	// if _, err := nk.StorageWrite(ctx, objects); err != nil {
	// 	logger.Error("Storage update error: %s", err.Error())
	// 	return "", errUpdateAccount
	// }

	// Return result to user.
	response := &rpc.RunServerResponse{
		Result: rpc.RunServerCompletionResult_runServerCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}

// TODO: may need batch query
func getUserInfo(ctx context.Context, nk runtime.NakamaModule, userID string) (*api.User, *rpc.User, error) {
	var users []*api.User
	var err error
	if users, err = nk.UsersGetId(ctx, []string{
		userID,
	}); err != nil || len(users) == 0 { // I don't know if it's possible to have err == nil and len == 0
		return nil, nil, errGetAccount
	}
	u := users[0]

	objectIds := []*runtime.StorageRead{
		{
			Collection: "user",
			Key:        userID,
		},
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, nil, errGetAccount
	}
	var userStorage rpc.User
	for _, o := range objects {
		if o.UserId == userID {
			if err = jsonpb.UnmarshalString(o.Value, &userStorage); err != nil {
				return nil, nil, errGetAccount
			}
		}
	}
	return u, &userStorage, nil
}

// Simple join
func getServers(ctx context.Context, nk runtime.NakamaModule, serversID []int32) (*[]rpc.UnityServer, error) {
	objectIds := []*runtime.StorageRead{}
	for _, serverID := range serversID {
		objectIds = append(objectIds, &runtime.StorageRead{
			Collection: "server",
			Key:        fmt.Sprintf("%d", serverID),
		})
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, errGetServers
	}
	var servers []rpc.UnityServer
	for _, o := range objects {
		var server rpc.UnityServer
		if err = jsonpb.UnmarshalString(o.Value, &server); err != nil {
			return nil, errGetServers
		}
		servers = append(servers, server)
	}
	return &servers, nil
}
