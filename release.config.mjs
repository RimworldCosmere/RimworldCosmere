import { readFileSync } from 'node:fs';

const publishedFileIds = JSON.parse(
    readFileSync(new URL('./PublishedFileIds.json', import.meta.url), 'utf8'),
);
const descriptionHeader = readFileSync(new URL('./.github/README.header.md', import.meta.url), 'utf8');
const descriptionFooter = readFileSync(new URL('./.github/README.footer.md', import.meta.url), 'utf8');

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
                        "files": ["CosmereCore/CosmereCore/Core/Framework/BuildInfo.cs"],
                        "from": "Revision = \".*\";",
                        "to": "Revision = \"${nextRelease.version}\";",
                        "countMatches": true
                    },
                    {
                        "files": ["CosmereCore/CosmereCore/Core/Framework/BuildInfo.cs"],
                        "from": "BuildTime = \".*\";",
                        "to": "BuildTime = \"${(new Date()).toISOString()}\";",
                        "results": [
                            {
                                "file": "CosmereCore/CosmereCore/Core/Framework/BuildInfo.cs",
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
                "assets": ["CosmereCore/CosmereCore/Core/Framework/BuildInfo.cs"]
            }
        ],
        [
            "./tools/semantic-release-steam/index.mjs",
            {
                "branchTargets": {
                    "main": "stable",
                    "beta": "beta"
                },
                "descriptionHeader": descriptionHeader,
                "descriptionFooter": descriptionFooter,
                "assetBaseUrlTemplate": "https://raw.githubusercontent.com/RimworldCosmere/RimworldCosmere/{branch}",
                "mods": [
                    {
                        "name": "CosmereCore",
                        "path": "CosmereCore",
                        "workshopIds": publishedFileIds.CosmereCore,
                    },
                    {
                        "name": "CosmereScadrial",
                        "path": "CosmereScadrial",
                        "workshopIds": publishedFileIds.CosmereScadrial,
                    },
                    {
                        "name": "CosmereRoshar",
                        "path": "CosmereRoshar",
                        "workshopIds": publishedFileIds.CosmereRoshar,
                    }
                ]
            }
        ],
        [
            "./tools/semantic-release-betahub/index.mjs",
            {
                "projectId": "pr-4628785616",
                "branches": ["beta"]
            }
        ]
    ],
    tagFormat: "${version}",
};