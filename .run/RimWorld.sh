#!/usr/bin/env bash

cleanup() {
    echo "Script is being terminated. Cleaning up..."
    kill $second_pid 2>/dev/null
    exit 1
}

# Trap common termination signals
trap cleanup SIGINT SIGTERM SIGHUP SIGQUIT

arch -x86_64 "/Applications/RimWorldMac.app/Contents/MacOS/RimWorld by Ludeon Studios" "$@" &
second_pid=$!

(
    while kill -0 "$second_pid" 2>/dev/null; do
        sleep 1
    done
    echo "Second process exited normally. Exiting script..."
    # Disable the trap so cleanup doesn't run
    trap - SIGINT SIGTERM SIGHUP SIGQUIT
    exit 0
) &

watcher_pid=$!

# Wait for either:
# - trap to fire and cleanup to run (from signal)
# - or background watcher to exit (normal second_process termination)
wait $watcher_pid