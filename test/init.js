process.env.ENABLE_SPELLCHECK = "0"

require('ts-node').register({
    project: 'test/',
    disableWarnings: true,
    fast: true
})