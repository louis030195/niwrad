const { rpc, realtime, quaternion, vector3, volume } = require('../..')

function inject (bot) {
  bot.listMatches = () => {
    return bot._client.rpc(bot._session, "list_matches", {})
  }
  bot.joinMatch = (id) => {
    bot.matchId = id
    return bot._socket.send({ match_join: { match_id: id } })
  }
  bot.createMatch = () => {
    var message = new rpc.CreateMatchRequest()
    return bot._client.rpc(bot._session, "create_match", message.serializeBinary())
  }
  bot.stopMatch = (id) => {
    var message = new rpc.StopMatchRequest({matchId : id})
    return bot._client.rpc(bot._session, "stop_match", message.serializeBinary())
  }
  bot.spawnAnimal = (x, y, z) => {
    var message = new realtime.Packet()
    message.requestSpawn = new realtime.Spawn()
    message.requestSpawn.animal = new realtime.Animal()
    message.requestSpawn.animal.transform = new realtime.Transform()
    message.requestSpawn.animal.transform.position = new vector3.Vector3(x, y, z)
    bot._socket.send({ match_data_send: { match_id: bot.matchId, op_code: 0, data: message.serializeBinary() } })
  }
}

module.exports = inject
