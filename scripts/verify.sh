#!/usr/bin/env bash
# Harness feedback loop: build gates, (optionally) boot the API, run gates.
# Exit code 0 = all gates green. Non-zero = at least one gate red.
set -uo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
gates_sln="$root/gates/Gates.sln"
api_project="$root/src/Api/Api.csproj"

echo "== Gate: dotnet build (gates) =="
dotnet build "$gates_sln"
if [ $? -ne 0 ]; then
  echo "Gate build failed."
  exit 1
fi

api_pid=""
if [ -f "$api_project" ]; then
  echo "== Booting API for functional gates =="
  dotnet run --project "$api_project" --urls http://localhost:5087 >/tmp/gate-api.log 2>&1 &
  api_pid=$!
  sleep 5
else
  echo "src/Api not found — functional gates (AC-xx) will fail as expected (nothing implemented yet)."
fi

cleanup() {
  if [ -n "$api_pid" ]; then
    kill "$api_pid" >/dev/null 2>&1
  fi
}
trap cleanup EXIT

echo "== Gate: dotnet test (gates) =="
export GATE_API_BASE_URL="http://localhost:5087"
dotnet test "$gates_sln" --logger "console;verbosity=normal"
test_exit=$?

if [ $test_exit -eq 0 ]; then
  echo "All gates green."
else
  echo "At least one gate is red."
fi

exit $test_exit
