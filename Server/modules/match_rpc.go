package main

import (
	"context"
	"database/sql"

	"rpc"

	"github.com/heroiclabs/nakama-common/runtime"
	"google.golang.org/protobuf/proto"
)

var (
	ErrBadContext    = runtime.NewError("bad context", 3)
	ErrJsonMarshal   = runtime.NewError("cannot marshal response", 13)
	ErrJsonUnmarshal = runtime.NewError("cannot unmarshal request", 13)
)

type createPartyCompletionResult int

const (
	createPartyCompletionResultUnknownClientFailure          createPartyCompletionResult = -100
	createPartyCompletionResultAlreadyInPartyOfSpecifiedType createPartyCompletionResult = -99
	createPartyCompletionResultAlreadyCreatingParty          createPartyCompletionResult = -98
	createPartyCompletionResultAlreadyInParty                createPartyCompletionResult = -97
	createPartyCompletionResultFailedToCreateMucRoom         createPartyCompletionResult = -96
	createPartyCompletionResultNoResponse                    createPartyCompletionResult = -95
	createPartyCompletionResultLoggedOut                     createPartyCompletionResult = -94
	createPartyCompletionResultUnknownInternalFailure        createPartyCompletionResult = 0
	createPartyCompletionResultSucceeded                     createPartyCompletionResult = 1
)

type createPartyResponse struct {
	matchID                     string                      `json:"MatchID"`
	createPartyCompletionResult createPartyCompletionResult `json:"CreatePartyCompletionResult"`
}

func rpcCreateParty(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var err error
	// Create the party match.
	matchID, err := nk.MatchCreate(ctx, "Niwrad", map[string]interface{}{})

	response, err := proto.Marshal(rpc.CreateMatchResponse{
		MatchId:                     matchID,
		CreateMatchCompletionResult: rpc.CreateMatchCompletionResultSucceeded,
	})

	if err != nil {
		return "", ErrJsonMarshal
	}
	return string(response), nil
}
