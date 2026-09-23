import { readFileSync } from 'node:fs';
import { basename } from 'node:path';

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
            "semantic-release-steam",
            {
                "appId": "294100",
                "outputReadme": true,
                "assetDirNameTransform": modPath => [
                    basename(modPath).replace(/^Cosmere/, '').toLowerCase(),
                    'fallback',
                ],
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