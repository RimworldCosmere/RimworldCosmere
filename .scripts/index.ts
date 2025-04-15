import { bootstrap } from "./bootstrap";

async function Main() {
  const {generator, genName, shouldSkip} = await bootstrap();
  
  if (shouldSkip) {
    process.stdout.write(`⏩ No changes detected for generator '${genName}'. `);
    if (process.argv.includes('--force')) {
      process.stdout.write('Forcing anyways.\n');
    } else {
      process.stdout.write('Skipping.\n');
      process.exit(0);
    }
  }
  
  generator();
}

Main();