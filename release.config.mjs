/**
 * @type {import('semantic-release').GlobalConfig}
 */
export default {
    branches: ["main", {name: 'beta', prerelease: true}, {name: 'alpha', prerelease: true}],
    repositoryUrl: 'https://github.com/RimworldCosmere/RimworldCosmere/',
    tagFormat: "${version}",
};