var niwrad = require('..')

async function basic() {
  let bot = await niwrad.createBot({
    host: '172.17.0.2',
    port: 31563,
    username: 'email@example.com',
    password: '12345678',
  })
  bot.on('error', err => console.log(err))
  let matches = await bot.listMatches()
  console.log(`matches: ${JSON.stringify(matches)}`)
  if ('matches' in matches['payload']) {
    await bot.joinMatch(matches[0])
  }
}

basic()
