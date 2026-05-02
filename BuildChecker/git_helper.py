#!/usr/bin/env python3
"""
Updates submodules and checks git setup for RSIEdit.
"""

import os
import shutil
import subprocess
import sys
import time
from pathlib import Path
from typing import List

QUIET = len(sys.argv) == 2 and sys.argv[1] == "--quiet"


def run_command(command: List[str], capture: bool = False) -> subprocess.CompletedProcess:
    text = ' '.join(command)
    if not QUIET:
        print("$ {}".format(text))

    sys.stdout.flush()

    if capture:
        completed = subprocess.run(command, stdout=subprocess.PIPE, text=True)
    else:
        completed = subprocess.run(command)

    if completed.returncode != 0:
        print("Error: command exited with code {}!".format(completed.returncode))

    return completed


def update_submodules():
    if 'GITHUB_ACTIONS' in os.environ:
        return

    if os.path.isfile("DISABLE_SUBMODULE_AUTOUPDATE"):
        return

    if shutil.which("git") is None:
        raise FileNotFoundError("git not found in PATH")

    run_command(["git", "submodule", "update", "--init", "--recursive"])


def check_for_zip_download():
    if run_command(["git", "rev-parse"]).returncode != 0:
        print(
            "It appears that you downloaded this repository directly from GitHub. (Using the .zip download option)"
            "When downloading straight from GitHub, important git metadata is missing."
            "Please clone the repository properly using git:"
            "git clone https://github.com/Inconnu1337/RSIEdit-FORK"
            "Closing automatically in 30 seconds."
        )
        time.sleep(30)
        exit(1)


if __name__ == '__main__':
    check_for_zip_download()
    update_submodules()
