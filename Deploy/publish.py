#!/usr/bin/env python3
"""
Publish Notepad.Avalonia to NuGet.org.

Usage:
    python publish.py            # bump version, build, pack, push
    python publish.py --dry-run  # bump version, build, pack — skip push
    python publish.py --no-bump  # skip version bump (use current version)

Requires:
    - .NET SDK on PATH (dotnet)
    - Deploy/.env with NUGET_API_KEY=<key>  (never committed — see .gitignore)
"""

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path

# ── Paths ─────────────────────────────────────────────────────────────────────

SCRIPT_DIR = Path(__file__).parent
REPO_ROOT   = SCRIPT_DIR.parent
CSPROJ      = REPO_ROOT / "Notepad.Avalonia" / "Notepad.Avalonia.csproj"
ARTIFACTS   = REPO_ROOT / "artifacts"
ENV_FILE    = SCRIPT_DIR / ".env"

# ── Helpers ───────────────────────────────────────────────────────────────────

def load_env(path: Path) -> dict:
    env = {}
    if not path.exists():
        return env
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, value = line.partition("=")
        env[key.strip()] = value.strip()
    return env


def run(cmd: list, **kwargs):
    print(f"\n>>> {' '.join(str(c) for c in cmd)}")
    result = subprocess.run(cmd, **kwargs)
    if result.returncode != 0:
        sys.exit(result.returncode)


def bump_version(csproj: Path) -> tuple:
    content = csproj.read_text(encoding="utf-8")
    m = re.search(r"<Version>(\d+)\.(\d+)\.(\d+)</Version>", content)
    if not m:
        print("ERROR: <Version> tag not found in", csproj)
        sys.exit(1)
    major, minor, patch = int(m[1]), int(m[2]), int(m[3])
    old_ver = f"{major}.{minor}.{patch}"
    new_ver = f"{major}.{minor}.{patch + 1}"
    updated = content.replace(f"<Version>{old_ver}</Version>",
                               f"<Version>{new_ver}</Version>")
    csproj.write_text(updated, encoding="utf-8")
    print(f"Version bumped: {old_ver} -> {new_ver}")
    return old_ver, new_ver


def current_version(csproj: Path) -> str:
    content = csproj.read_text(encoding="utf-8")
    m = re.search(r"<Version>(\d+\.\d+\.\d+)</Version>", content)
    if not m:
        print("ERROR: <Version> tag not found in", csproj)
        sys.exit(1)
    return m[1]

# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="Publish Notepad.Avalonia to NuGet.org")
    parser.add_argument("--dry-run",  action="store_true", help="Skip the push step")
    parser.add_argument("--no-bump",  action="store_true", help="Skip version bump")
    args = parser.parse_args()

    env_vars = load_env(ENV_FILE)
    api_key  = env_vars.get("NUGET_API_KEY") or os.environ.get("NUGET_API_KEY")
    if not api_key and not args.dry_run:
        print(f"ERROR: NUGET_API_KEY not found in {ENV_FILE} or environment.")
        print("       Create Deploy/.env with:  NUGET_API_KEY=<your-key>")
        sys.exit(1)

    # 1. Bump version
    if args.no_bump:
        version = current_version(CSPROJ)
        print(f"Skipping bump — current version: {version}")
    else:
        _, version = bump_version(CSPROJ)

    # 2. Build
    run(["dotnet", "build", str(CSPROJ), "-c", "Release"], cwd=REPO_ROOT)

    # 3. Pack
    ARTIFACTS.mkdir(exist_ok=True)
    run(["dotnet", "pack", str(CSPROJ), "-c", "Release",
         "--no-build", "-o", str(ARTIFACTS)], cwd=REPO_ROOT)

    nupkg = ARTIFACTS / f"Notepad.Avalonia.{version}.nupkg"
    if not nupkg.exists():
        matches = list(ARTIFACTS.glob(f"Notepad.Avalonia.{version}*.nupkg"))
        if not matches:
            print(f"ERROR: .nupkg not found in {ARTIFACTS}")
            sys.exit(1)
        nupkg = matches[0]

    print(f"\nPackage: {nupkg}")

    # 4. Push
    if args.dry_run:
        print("\n[dry-run] Skipping push to NuGet.org.")
    else:
        run([
            "dotnet", "nuget", "push", str(nupkg),
            "--api-key",  api_key,
            "--source",   "https://api.nuget.org/v3/index.json",
            "--skip-duplicate",
        ], cwd=REPO_ROOT)
        print(f"\nPublished Notepad.Avalonia {version} to NuGet.org")


if __name__ == "__main__":
    main()
