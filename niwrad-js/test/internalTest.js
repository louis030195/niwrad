/* eslint-env mocha */

const niwrad = require('..')
const assert = require('assert')

describe('nakama', () => {
  let bot
  let matchId
  let connected = false
  before((done) => {
    niwrad.createBot({
      host: process.env.NAKAMA_HOST,
      port: process.env.NAKAMA_PORT,
      username: process.env.USERNAME,
      password: process.env.PASSWORD,
    }).then(b => {
        bot = b
        connected = true
        bot.on('disconnect', () => connected = false)
        done()
      })
      .catch(done)
  })
  beforeEach(() => {
    assert.ok(connected)
  })
  it('should successfully connect to nakama', () => {
    assert.notEqual(bot, undefined)
    assert.notEqual(bot._client, undefined)
    assert.notEqual(bot._session, undefined)
    assert.notEqual(bot._socket, undefined)
    assert.ok(!bot._session.isexpired(Date.now() / 1000))
  })
  it('createMatch should return sucess', (done) => {
    bot.createMatch().then(response => {
      console.log(response)
      console.log(response.deserializeBinary())
      done()
    })
    .catch(done)
  })
  it('listMatches should return sucess', (done) => {
    bot.listMatches().then(response => {
      console.log(response)
      matchId = response.matchId
      done()
    })
    .catch(done)
  })
  it('joinMatch should return sucess', (done) => {
    bot.joinMatch(matchId).then(response => {
      console.log(response)
      done()
    })
    .catch(done)
  })
  it('spawnAnimal should return sucess', (done) => {
    bot.spawnAnimal(5, 5, 5).then(response => {
      console.log(response)
      done()
    })
    .catch(done)
  })
})
