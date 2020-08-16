
# niwrad-js

Design inspired by [mineflayer](https://github.com/PrismarineJS/mineflayer):

- Custom lib
- Plugins
- High level

## Installation

```bash
npm i
```

[nakama-js doesn't handle nodejs](https://github.com/heroiclabs/nakama-js/issues/23), so there is a hack:

```bash
sed -i "2i require('es6-promise').polyfill()\nrequire('isomorphic-fetch')\nvar btoa = require('btoa')\nvar atob = require('atob')\nconst URLSearchParams = require('url-search-params')\nvar WebSocket = require('ws')\nvar self = {}" node_modules/@heroiclabs/nakama-js/dist/nakama-js.cjs.js
```

## Usage

```js
var niwrad = require('niwrad-js')

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
```

See [example](examples/basic.js)

## Roadmap

- Working helm tests
- Browser version, sort of web "management" interface ?
