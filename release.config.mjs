import { splitPublish } from '@rimworks/mod-ci';
import { readFileSync } from 'node:fs';
import { basename, join } from 'node:path';

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
    "plugins": splitPublish([
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
                        "previewfile": join(import.meta.dirname, 'CosmereCore/About/Preview.png'),
                        "workshopIds": publishedFileIds.CosmereCore,
                    },
                    {
                        "name": "CosmereScadrial",
                        "path": "CosmereScadrial",
                        "previewfile": join(import.meta.dirname, 'CosmereScadrial/About/Preview.png'),
                        "workshopIds": publishedFileIds.CosmereScadrial,
                    },
                    {
                        "name": "CosmereRoshar",
                        "path": "CosmereRoshar",
                        "previewfile": join(import.meta.dirname, 'CosmereRoshar/About/Preview.png'),
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
    ]),
    tagFormat: "${version}",
};