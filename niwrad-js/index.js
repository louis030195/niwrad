const EventEmitter = require('events').EventEmitter
const nakamajs = require('@heroiclabs/nakama-js')

// Protos
const rpc = require('./lib/proto/rpc_pb')
const realtime = require('./lib/proto/realtime_pb')
const quaternion = require('./lib/proto/github.com/louis030195/protometry/api/quaternion/quaternion_pb')
const vector3 = require('./lib/proto/github.com/louis030195/protometry/api/vector3/vector3_pb')
const volume = require('./lib/proto/github.com/louis030195/protometry/api/volume/volume_pb')

module.exports = {
  createBot,
  rpc,
  realtime,
  quaternion,
  vector3,
  volume
}

// Plugins
const match = require('./lib/plugins/match')


async function createBot(options = {}) {
  options.plugins = options.plugins || {}
  options.logErrors = options.logErrors === undefined ? true : options.logErrors
  options.loadInternalPlugins = options.loadInternalPlugins !== false
  const bot = new Bot()
  if (options.logErrors) {
    bot.on('error', err => {
      console.log(err)
    })
  }
  match(bot, options)
  
  await bot.connect(options)
  return bot
}

class Bot extends EventEmitter {
  constructor() {
    super()
    this._client = null
  }

  async connect(options) {
    try {
      this._client = new nakamajs.Client("defaultkey", options.host, options.port)
      this._client.ssl = options.ssl ? options.ssl : false
      this.username = options.username
      this._session = await this._client.authenticateEmail({ email: options.username, password: options.password })
      this._socket = this._client.createSocket(this._client.ssl, false)
      this._socket.ondisconnect = (data) => {
        this.emit('disconnect', data)
      }
      this._socket.onnotification = (data) => {
        this.emit('notification', data)
      }
      this._socket.onchannelpresence = (data) => {
        this.emit('channelPresence', data)
      }
      this._socket.onchannelmessage = (data) => {
        this.emit('channelMessage', data)
      }
      this._socket.onmatchdata = (data) => {
        this.emit('matchData', data)
      }
      this._socket.onmatchpresence = (data) => {
        this.emit('matchPresence', data)
      }
      this._socket.onmatchmakermatched = (data) => {
        this.emit('matchmakerMatched', data)
      }
      this._socket.onstatuspresence = (data) => {
        this.emit('statusPresence', data)
      }
      this._socket.onstreampresence = (data) => {
        this.emit('streamPresence', data)
      }
      this._socket.onstreamdata = (data) => {
        this.emit('streamData', data)
      }
      await this._socket.connect(this._session)
    } catch (err) {
      this.emit('error', err)
    }
  }

  end() {
    this._client.end()
  }
}
