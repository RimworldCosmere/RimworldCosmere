import {program} from "commander";
import {bootstrap} from "./bootstrap";
import fs from "fs/promises";
import path from "path";

interface Options {
    force: boolean;
    dryRun: boolean;
    delete: boolean;
    verbose: boolean;
}

export let options: Options;


async function Main() {
    program
        .description("Cosmere Generator Scripts")
        .option('-f, --force', 'Forcing generation of templates. Will ignore a lack of changes', false)
        .option('--dryRun', 'Dry runs for generation of templates.', false)
        .option('-d, --delete', 'Delete all generated files before starting', false)
        .option('-v, --verbose', 'Extra logs', false)

    options = program.parse().opts<Options>();

    if (options.delete) {
        await deleteGeneratedFiles(path.resolve(__dirname, '..', 'CosmereCore'))
        await deleteGeneratedFiles(path.resolve(__dirname, '..', 'CosmereFramework'))
        await deleteGeneratedFiles(path.resolve(__dirname, '..', 'CosmereResources'))
        await deleteGeneratedFiles(path.resolve(__dirname, '..', 'CosmereScadrial'))
        await deleteGeneratedFiles(path.resolve(__dirname, '..', 'CosmereRoshar'))
    }

    console.dir(options);

    for (const genName of ['GenesAndTraits', 'MetallicArtsMetals', 'Resources', 'FeruchemicalHediffs']) {
        const {generator, shouldSkip} = await bootstrap(genName);

        if (shouldSkip) {
            process.stdout.write(`⏩ No changes detected for generator '${genName}'. `);
            if (options.force) {
                process.stdout.write('Forcing anyways.\n');
            } else {
                process.stdout.write('Skipping.\n');
                process.exit(0);
            }
        }

        generator();
    }


}

async function deleteGeneratedFiles(dir: string): Promise<void> {
    const entries = await fs.readdir(dir, {withFileTypes: true});

    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);

        if (entry.isDirectory()) {
            await deleteGeneratedFiles(fullPath); // Recurse into subdirectories
        } else if (/\.(generated\.cs|generated\.xml)$/i.test(entry.name)) {
            await fs.unlink(fullPath);
            if (options.verbose) console.log(`Deleted: ${fullPath}`);
        }
    }
}

Main();