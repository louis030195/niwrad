local nk = require("nakama")
-- https://heroiclabs.com/docs/runtime-code-function-reference/#match


function list_matches(context, payload)
    local limit = 1000
    local authoritative = true
    local min_size = 0
    local max_size = 1200

    -- local filter = ""
    -- TODO: filter that only returns matches in "ready" state (waiting for server initialization) ?
    -- or maybe not required anyway
    local matches_list = nk.match_list(limit, authoritative, nil, min_size, max_size)
    local response = {}

    if(matches_list) then
        local no_of_matches = #matches_list
        nk.logger_info("[search_matches/list_matches] : Found matches Count : " .. no_of_matches)

        if(no_of_matches == 0) then
            nk.logger_info("no matches found")
            response["matches"] = nil
        else
            nk.logger_info("matches found")
            local match_ids = {}
            for key, value in ipairs(matches_list) 
            do
                match_ids[key] = value["match_id"]
            end
            response["matches"] = match_ids
        end
    else
        nk.logger_info("no matches found")
        response["matches"] = nil
    end

    return nk.json_encode(response)
end
nk.register_rpc(list_matches, "list_matches")


-- function create_server(context, payload)
--     if payload == nil then
--         return nk.json_encode({ response = false, message = "payload is nil" })
--     end

--     data = nk.json_decode(payload)
--     local response = {}
--     logger.Info("A server creation has been asked with config: %v", request)

-- 	args := []string{"./niwrad.x86_64", "--terrainSize", "1000", "--initialAnimals", "50", "--initialPlants", "100"}
-- 	if request.TerrainSize > -1 {
-- 		args[2] = fmt.Sprintf("%v", request.TerrainSize)
-- 	}
-- 	if request.InitialAnimals > -1 {
-- 		args[4] = fmt.Sprintf("%v", request.InitialAnimals)
-- 	}
-- 	if request.InitialPlants > -1 {
-- 		args[6] = fmt.Sprintf("%v", request.InitialPlants)
-- 	}
-- 	cmd := exec.Command(args[0], args...)
-- 	if err := cmd.Start(); err != nil {
-- 		log.Fatal(err)
-- 	}
-- 	logger.Info("New server %v", args)

-- 	// Return result to user.
-- 	response := &rpc.RunServerResponse{}
-- 	responseBytes, err := proto.Marshal(response)
-- 	if err != nil {
-- 		return "", errMarshal
-- 	}
-- 	return string(responseBytes), nil
--     return nk.json_encode(response)
-- end
-- nk.register_rpc(create_server, "create_server")
