/**
 * @type {import('semantic-release').GlobalConfig}
 */
export default {
    branches: ["main", {name: 'beta', prerelease: true}, {name: 'alpha', prerelease: true}],
    "plugins": [
        "@semantic-release/commit-analyzer",
        "@semantic-release/release-notes-generator",
        "@semantic-release/github",
        [
            "semantic-release-replace-plugin",
            {
                "replacements": [
                    {
                        "files": ["CosmereFramework/CosmereFramework/BuildInfo.cs"],
                        "from": "Revision = \".*\";",
                        "to": "Revision = \"${nextRelease.version}\"",
                        "results": [
                            {
                                "file": "CosmereFramework/CosmereFramework/BuildInfo.cs",
                                "hasChanged": true,
                                "numMatches": 1,
                                "numReplacements": 1
                            }
                        ],
                        "countMatches": true
                    },
                    {
                        "files": ["CosmereFramework/CosmereFramework/BuildInfo.cs"],
                        "from": "BuildTime = \".*\";",
                        "to": "BuildTime = \"${(new Date()).toISOString()}\"",
                        "results": [
                            {
                                "file": "CosmereFramework/CosmereFramework/BuildInfo.cs",
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
                "assets": ["CosmereFramework/CosmereFramework/BuildInfo.cs"]
            }
        ]
    ],
    tagFormat: "${version}",
};