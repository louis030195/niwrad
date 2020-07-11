package main

import (
	"context"
	"fmt"
	"github.com/golang/protobuf/jsonpb"
	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/louis030195/niwrad/rpc"
	appsv1 "k8s.io/api/apps/v1"
	apiv1 "k8s.io/api/core/v1"
	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"
	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/rest"
	"os/exec"
)

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
func getServers(ctx context.Context, nk runtime.NakamaModule, serversID []string) (*[]rpc.UnityServer, error) {
	var objectIds []*runtime.StorageRead
	for _, serverID := range serversID {
		objectIds = append(objectIds, &runtime.StorageRead{
			Collection: "server",
			Key:        serverID,
		})
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, err
	}
	var servers []rpc.UnityServer
	for _, o := range objects {
		var server rpc.UnityServer
		if err = jsonpb.UnmarshalString(o.Value, &server); err != nil {
			return nil, err
		}
		servers = append(servers, server)
	}
	return &servers, nil
}

func spawnUnityProcess(sessionID string, conf rpc.MatchConfiguration) (*string, error) {
	args := []string{
		"run",
		"--name", fmt.Sprintf("%s", sessionID), // TODO: prob not atomic: a user can have only one server ?
		"niwrad-unity",
		"/app/niwrad.x86_64",
		"--terrainSize", fmt.Sprintf("%d", conf.TerrainSize),
		"--initialAnimals", fmt.Sprintf("%d", conf.InitialAnimals),
		"--initialPlants", fmt.Sprintf("%d", conf.InitialPlants),
		"--nakamaIp", "niwrad",
		"--nakamaPort", "7350",
	}

	// TODO: imply having the artifact inside nakama docker (volume mount or copy)
	cmd := exec.Command("./niwrad.x86_64", args...) // TODO: executable name may vary (cloud build ...)
	//cmd.Stdout = os.Stdout
	if err := cmd.Start(); err != nil {
		return nil, err
	}
	res := fmt.Sprintf("%d", cmd.Process.Pid)
	return &res, nil
}

func spawnUnityPod(id string, conf rpc.MatchConfiguration) (*string, error) {
	// creates the in-cluster config
	config, err := rest.InClusterConfig()
	if err != nil {
		return nil, err
	}
	c, err := kubernetes.NewForConfig(config)
	if err != nil {
		return nil, err
	}

	// get pods in all the namespaces by omitting namespace
	// Or specify namespace to get pods in particular namespace
	//pods, err := clientset.CoreV1().Pods("").List(metav1.ListOptions{
	//	LabelSelector: metav1.
	//})
	//deps, err := clientset.ExtensionsV1beta1().Deployments("").Get("niwrad", metav1.GetOptions{})
	//if err != nil {
	//	panic(err.Error())
	//}
	//for _, p := range pods.Items {
	//	logger.Info("Pod: %v", p)
	//	logger.Info("hn: %v", p.Spec.Hostname)
	//	logger.Info("hn: %v", p.Status.PodIP)
	//	logger.Info("hn: %v", p.Spec.Containers)
	//}

	deploymentsClient := c.AppsV1().Deployments(apiv1.NamespaceDefault)

	deployment := &appsv1.Deployment{
		ObjectMeta: metav1.ObjectMeta{
			Name: fmt.Sprintf("niwrad-unity-%s", id), // Unity server pod tagged by it's session id
		},
		Spec: appsv1.DeploymentSpec{
			Selector: &metav1.LabelSelector{
				MatchLabels: map[string]string{
					"app": "niwrad-unity",
				},
			},
			Template: apiv1.PodTemplateSpec{
				ObjectMeta: metav1.ObjectMeta{
					Labels: map[string]string{
						"app": "niwrad-unity",
					},
				},
				Spec: apiv1.PodSpec{
					Containers: []apiv1.Container{
						{
							Name:  "niwrad-unity",
							Image: "niwrad-unity",
							Command: []string{ // TODO: maybe should get/create nakama account for this  host and pass
								"/app/niwrad.x86_64",
							},
							Env: []apiv1.EnvVar{
								{
									Name:      "NAKAMA_IP",
									Value:     "niwrad",
								},
								{
									Name:      "NAKAMA_PORT",
									Value:     "7350",
								},
								{
									Name:      "INITIAL_ANIMALS",
									Value:     fmt.Sprintf("%d", conf.InitialAnimals),
								},
								{
									Name:      "INITIAL_PLANTS",
									Value:     fmt.Sprintf("%d", conf.InitialPlants),
								},
								{
									Name:      "TERRAIN_SIZE",
									Value:     fmt.Sprintf("%d", conf.TerrainSize),
								},
								{
									Name:      "WORKER_ID",
									Value:     id,
								},
							},
							ImagePullPolicy: apiv1.PullNever,
						},
					},
				},
			},
		},
	}

	// Create Deployment
	result, err := deploymentsClient.Create(deployment) // TODO: delete deployment on exit ?
	if err != nil {
		return nil, err
	}
	return &result.GenerateName, nil
}

func stopUnityDeployment(deploymentName string) error {
	config, err := rest.InClusterConfig()
	if err != nil {
		return err
	}
	c, err := kubernetes.NewForConfig(config)
	if err != nil {
		return err
	}
	deploymentsClient := c.AppsV1().Deployments(apiv1.NamespaceDefault)
	err = deploymentsClient.Delete(fmt.Sprintf("niwrad-unity-%s", deploymentName), &metav1.DeleteOptions{})
	if err != nil {
		return err
	}
	return nil
}

//func rollingUpdate(c *kubernetes.Clientset, dp *v1beta1.Deployment) error {
//	dp := v1beta1.Deployment{
//		TypeMeta:   metav1.TypeMeta{},
//		ObjectMeta: metav1.ObjectMeta{},
//		Spec:       v1beta1.DeploymentSpec{},
//		Status:     v1beta1.DeploymentStatus{},
//	}
//	// Update
//	_, err := c.ExtensionsV1beta1().Deployments("").Update(dp)
//	if err != nil {
//		return err
//	}
//	return nil
//}

