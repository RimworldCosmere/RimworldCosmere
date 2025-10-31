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
                        "files": ["CosmereCore/CosmereCore/BuildInfo.cs"],
                        "from": "Revision = \".*\";",
                        "to": "Revision = \"${nextRelease.version}\";",
                        "results": [
                            {
                                "file": "CosmereCore/CosmereCore/BuildInfo.cs",
                                "hasChanged": true,
                                "numMatches": 1,
                                "numReplacements": 1
                            }
                        ],
                        "countMatches": true
                    },
                    {
                        "files": ["CosmereCore/CosmereCore/BuildInfo.cs"],
                        "from": "BuildTime = \".*\";",
                        "to": "BuildTime = \"${(new Date()).toISOString()}\";",
                        "results": [
                            {
                                "file": "CosmereCore/CosmereCore/BuildInfo.cs",
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
                "assets": ["CosmereCore/CosmereCore/BuildInfo.cs"]
            }
        ]
    ],
    tagFormat: "${version}",
};