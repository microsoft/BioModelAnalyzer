// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

process.env.ENABLE_SPELLCHECK = "0"

require('ts-node').register({
    project: 'test/',
    disableWarnings: true,
    fast: true
})
