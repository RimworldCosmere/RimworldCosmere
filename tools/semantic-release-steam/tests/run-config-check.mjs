import plugin from '../index.mjs';

await plugin.verifyConditions(
  {
    branchTargets: { main: 'stable', beta: 'beta' },
    mods: [
      { name: 'CosmereCore', path: 'CosmereCore', workshopIds: { stable: process.env.COSMERE_CORE_STABLE, beta: process.env.COSMERE_CORE_BETA } },
      { name: 'CosmereScadrial', path: 'CosmereScadrial', workshopIds: { stable: process.env.COSMERE_SCADRIAL_STABLE, beta: process.env.COSMERE_SCADRIAL_BETA } },
      { name: 'CosmereRoshar', path: 'CosmereRoshar', workshopIds: { stable: process.env.COSMERE_ROSHAR_STABLE, beta: process.env.COSMERE_ROSHAR_BETA } },
    ],
  },
  {
    branch: { name: process.env.BRANCH_NAME ?? 'beta' },
    env: process.env,
    logger: console,
  },
);

console.log('steam release config ok');
