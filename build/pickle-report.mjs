#!/usr/bin/env node
// Turns a Pickle report directory into a GitHub job summary, and gates the build on it.
// Counts come from summary.json; the per-failure detail comes from the JSON the dashboard
// template carries inside report.html, which is the only report file holding step text.

import { appendFileSync, existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

const EXIT_FAILURES = 1;
const EXIT_NO_REPORT = 2;
const EXIT_EMPTY = 3;

function die(code, message) {
  process.stderr.write(`pickle-report: ${message}\n`);
  process.exit(code);
}

// HtmlReportWriter escapes the payload's own "</" as "<\/", so the first closing tag is ours.
function readPayload(html) {
  const tag = html.indexOf('id="pickle-report"');
  if (tag < 0) return null;
  const start = html.indexOf('>', tag) + 1;
  const end = html.indexOf('</script>', start);
  if (start === 0 || end < 0) return null;
  return JSON.parse(html.slice(start, end));
}

function cell(text) {
  return String(text ?? '')
    .replace(/\|/g, '\\|')
    .replace(/</g, '&lt;')
    .replace(/\r?\n/g, '<br>');
}

function failingStep(steps) {
  const step = (steps ?? []).find((s) => s.status === 'Failed');
  return step ? `${step.keyword} ${step.text}` : '(no step failed - a hook or the run itself did)';
}

const dir = process.argv[2];
if (!dir) die(EXIT_NO_REPORT, 'usage: pickle-report.mjs <report-dir>');

const summaryPath = join(dir, 'summary.json');
const htmlPath = join(dir, 'report.html');

if (!existsSync(summaryPath)) {
  die(
    EXIT_NO_REPORT,
    `no summary.json under ${dir} - this run never reported. the game died before it finished: ` +
      'check Player.log for "XIO: fatal IO error" or "Desktop is 0 x 0".',
  );
}
if (!existsSync(htmlPath)) {
  // report.html is the last file AutorunBootstrap.WriteReports writes, so losing it alone
  // means the report write threw partway.
  die(EXIT_NO_REPORT, `${dir} has summary.json but no report.html - the report write failed partway.`);
}

let summary;
try {
  summary = JSON.parse(readFileSync(summaryPath, 'utf8'));
} catch (error) {
  die(EXIT_NO_REPORT, `summary.json under ${dir} is not valid JSON: ${error.message}`);
}

let payload;
try {
  payload = readPayload(readFileSync(htmlPath, 'utf8'));
} catch (error) {
  die(EXIT_NO_REPORT, `report.html under ${dir} carries no readable payload: ${error.message}`);
}
if (!payload) die(EXIT_NO_REPORT, `report.html under ${dir} has no pickle-report payload.`);

const total = summary.total ?? 0;
const passed = summary.passed ?? 0;
const failed = summary.failed ?? 0;
const skipped = summary.skipped ?? 0;
const flaky = summary.flaky ?? 0;
const exitReason = summary.exitReason ?? 'unknown';

const failures = [];
for (const feature of payload.features ?? []) {
  for (const scenario of feature.scenarios ?? []) {
    if (scenario.outcome !== 'Failed') continue;
    failures.push({
      // path is the feature name today; ScenarioResult drops SourcePath, so no writer emits a
      // real path. Keeping it first means we pick one up for free if Pickle ever adds it.
      feature: feature.path || feature.name || '(unnamed feature)',
      scenario: scenario.name,
      step: failingStep(scenario.steps),
      message: scenario.failureMessage || '(no message)',
    });
  }
}

let code = 0;
let verdict;
if (total === 0) {
  code = EXIT_EMPTY;
  verdict =
    `**Zero scenarios ran** (exitReason \`${exitReason}\`). Pickle exits 0 for an empty run, ` +
    'so this is a failure here. A filter term needs the full feature file name; a stem matches nothing.';
} else if (failed > 0) {
  code = EXIT_FAILURES;
  verdict = `**${failed} of ${total} scenarios failed.**`;
} else if (exitReason !== 'passed') {
  code = EXIT_FAILURES;
  verdict = `**The run did not finish** (exitReason \`${exitReason}\`). The counts below are partial.`;
} else {
  verdict = `**All ${total} scenarios passed.**`;
}

const lines = ['## Pickle suite', '', verdict, ''];
lines.push(`${passed} passed, ${failed} failed, ${skipped} skipped, exitReason \`${exitReason}\`.`);
if (flaky > 0) lines.push('', `${flaky} scenario(s) failed at least once before passing.`);

if (failures.length > 0) {
  lines.push('', '| Feature | Scenario | Failing step | Message |', '|---|---|---|---|');
  for (const f of failures) {
    lines.push(`| ${cell(f.feature)} | ${cell(f.scenario)} | ${cell(f.step)} | ${cell(f.message)} |`);
  }
}
lines.push('');

const markdown = `${lines.join('\n')}\n`;
process.stdout.write(markdown);
if (process.env.GITHUB_STEP_SUMMARY) appendFileSync(process.env.GITHUB_STEP_SUMMARY, markdown);

if (code !== 0) process.stderr.write(`pickle-report: gate failed - report: ${dir}\n`);
process.exit(code);
