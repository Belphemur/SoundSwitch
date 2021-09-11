'use strict'
const Config = require('conventional-changelog-conventionalcommits')
const conventionalChangelogCore = require('conventional-changelog-core');


  
const config = Config({
    "types": [{
            "type": "boost",
            "section": "Enhancements"
        }, {
            "type": "lang",
            "section": "Languages"
        }, {
            "type": "fix",
            "section": "Bug Fixes"
        }, {
            "type": "feat",
            "section": "Features"
        }, {
            "type": "tests",
            "section": "Tests"
        }
    ]

})

conventionalChangelogCore(config)
  .pipe(process.stdout);