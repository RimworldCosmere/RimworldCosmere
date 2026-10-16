import { MOD_RELEASE_RULES, splitPublish } from '@rimworks/mod-ci';
import { deriveWorkshopMetadata } from 'semantic-release-steam/lib/about.mjs';
import { readFileSync } from 'node:fs';
import { basename } from 'node:path';

const publishedFileIds = JSON.parse(
    readFileSync(new URL('./PublishedFileIds.json', import.meta.url), 'utf8'),
);
const localizedFile = name => ({
    default: readFileSync(new URL(`./.github/README.${name}.md`, import.meta.url), 'utf8'),
    ...Object.fromEntries(
        ['spanish', 'russian', 'schinese'].map(code => [
            code,
            readFileSync(new URL(`./.github/README.${name}.${code}.md`, import.meta.url), 'utf8'),
        ]),
    ),
});

const descriptionHeader = localizedFile('header');
const descriptionFooter = localizedFile('footer');

const titles = {
    CosmereCore: 'Cosmere - Core',
    CosmereScadrial: 'Cosmere - Scadrial',
    CosmereRoshar: 'Cosmere - Roshar',
};

const mods = await Promise.all(
    Object.keys(titles).map(async name => ({
        name,
        path: name,
        title: titles[name],
        workshopIds: publishedFileIds[name],
        metadata: {
            beta: {
                title: `[Beta] ${titles[name]}`,
                tags: [...(await deriveWorkshopMetadata(name)).tags, 'Beta'],
            },
        },
    })),
);

/**
 * @type {import('semantic-release').GlobalConfig}
 */
export default {
    branches: ["main", {name: 'beta', prerelease: true}, {name: 'alpha', prerelease: true}],
    "plugins": splitPublish([
        [
            "@semantic-release/commit-analyzer",
            {
                "releaseRules": [{type: 'build', release: 'patch'}, ...MOD_RELEASE_RULES],
            }
        ],
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
                "mods": mods,
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