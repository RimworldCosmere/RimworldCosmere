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
                        "files": ["CosmereCore/CosmereCore/Core/BuildInfo.cs"],
                        "from": "Revision = \".*\";",
                        "to": "Revision = \"${nextRelease.version}\";",
                        "countMatches": true
                    },
                    {
                        "files": ["CosmereCore/CosmereCore/Core/BuildInfo.cs"],
                        "from": "BuildTime = \".*\";",
                        "to": "BuildTime = \"${(new Date()).toISOString()}\";",
                        "results": [
                            {
                                "file": "CosmereCore/CosmereCore/Core/BuildInfo.cs",
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
                "assets": ["CosmereCore/CosmereCore/Core/BuildInfo.cs"]
            }
        ],
        [
            './tools/semantic-release-steam/index.mjs',
            {
                branchTargets: {
                    main: 'stable',
                    beta: 'beta',
                },
                mods: [
                    {
                        name: 'CosmereCore',
                        path: 'CosmereCore',
                        workshopIds: {
                            stable: 'REPLACE_STABLE_ID',
                            beta: 'REPLACE_BETA_ID',
                        },
                    },
                    {
                        name: 'CosmereScadrial',
                        path: 'CosmereScadrial',
                        workshopIds: {
                            stable: 'REPLACE_STABLE_ID',
                            beta: 'REPLACE_BETA_ID',
                        },
                    },
                    {
                        name: 'CosmereRoshar',
                        path: 'CosmereRoshar',
                        workshopIds: {
                            stable: 'REPLACE_STABLE_ID',
                            beta: 'REPLACE_BETA_ID',
                        },
                    },
                ],
            },
        ]
    ],
    tagFormat: "${version}",
};