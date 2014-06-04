#!/bin/bash
#
# Weisi Dai <weisi@x-research.com>
#

export PATH=~/bin:$PATH
LOCK=/var/lock/dropbox_wlist.lock
cd $(dirname "${BASH_SOURCE[0]}")
set -e # abort on non-zero return values
mkdir $LOCK

trap "rmdir $LOCK" INT TERM EXIT
mono dropbox_wlist.exe > /dev/null 2>&1

