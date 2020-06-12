local nk = require("nakama")
-- https://heroiclabs.com/docs/runtime-code-function-reference/#match


function list_matches(context, payload)
    local limit = 1000
    local authoritative = false
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

-- TODO: hooks not registered ??
local function on_match_create(context, payload)
    nk.logger_info("[Match] match created" .. payload)
    return nk.json_encode({["lol"] = "lol"})
end
nk.register_req_after(on_match_create, "MatchCreate")

local function on_match_join(context, payload)
    nk.logger_info("[Match] player joined, " .. payload)
    return nk.json_encode({["yo"] = "yo"})
end
nk.register_req_after(on_match_join, "MatchJoin")

-- TODO: on_match_leave if empty delete match