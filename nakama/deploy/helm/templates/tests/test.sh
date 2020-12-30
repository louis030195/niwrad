#!/bin/bash

HOST=$(minikube ip)
PORT=$(kubectl get service niwrad -o jsonpath='{.spec.ports[?(@.name=="api")].nodePort}')
TOKEN=$(curl "http://$HOST:$PORT/v2/account/authenticate/email?create=true&username=mycustomusername" \
    --user 'defaultkey:' \
    --data '{"email": "a@bbbbbbbb.com", "password": "password"}' | \
    python -c 'import json,sys;obj=json.load(sys.stdin);print obj["token"]')
echo "Connected, token: $TOKEN"

curl "http://$HOST:$PORT/v2/rpc/delete_all_accounts?http_key=defaulthttpkey" \
     -H 'Content-Type: application/json' \
     -H 'Accept: application/json'
