/**
 * @type {import('semantic-release').GlobalConfig}
 */
export default {
    branches: ["main", {name: 'beta', prerelease: true}, {name: 'alpha', prerelease: true}],
    "plugins": [
        "@semantic-release/commit-analyzer",
        "@semantic-release/release-notes-generator",
        [
            "@semantic-release/github",
            {
                "assets": [
                    {path: './zips/**/*.zip'}
                ]
            }
        ],
        [
            "semantic-release-replace-plugin",
            {
                "replacements": [
                    {
                        "files": ["CosmereFoundation/CosmereFoundation/BuildInfo.cs"],
                        "from": "Revision = \".*\";",
                        "to": "Revision = \"${nextRelease.version}\";",
                        "results": [
                            {
                                "file": "CosmereFoundation/CosmereFoundation/BuildInfo.cs",
                                "hasChanged": true,
                                "numMatches": 1,
                                "numReplacements": 1
                            }
                        ],
                        "countMatches": true
                    },
                    {
                        "files": ["CosmereFoundation/CosmereFoundation/BuildInfo.cs"],
                        "from": "BuildTime = \".*\";",
                        "to": "BuildTime = \"${(new Date()).toISOString()}\";",
                        "results": [
                            {
                                "file": "CosmereFoundation/CosmereFoundation/BuildInfo.cs",
                                "hasChanged": true,
                                "numMatches": 1,
                                "numReplacements": 1
                            }
                        ],
                        "countMatches": true
                    }
                ]
            }
        ],
        [
            "@semantic-release/git",
            {
                "assets": ["CosmereFoundation/CosmereFoundation/BuildInfo.cs"]
            }
        ]
    ],
    tagFormat: "${version}",
};