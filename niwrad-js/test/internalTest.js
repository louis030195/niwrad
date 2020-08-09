/* eslint-env mocha */

const niwrad = require('..')
const assert = require('assert')
const host = '172.17.0.2'
const port = 30681 // TODO: something less hard-coded :)
const username = 'email@example.com'
const password = '12345678'

describe('connect', () => {
  let bot
  before(async () => {
    bot = await niwrad.createBot({
      host: host,
      port: port,
      username: username,
      password: password,
    })
  })

  it('should return -1 when the value is not present', () => {
    bot.createMatch()
    assert.equal([1, 2, 3].indexOf(4), -1)
  })
})
