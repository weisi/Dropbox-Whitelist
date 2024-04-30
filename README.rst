=================
Dropbox-Allowlist
=================

:author:    Weisi Dai <weisi@x-research.com>
:date:      Jun 4, 2014
:license:   MIT License

Want to sync **only** selected directories on your Linux server? Dropbox's selective sync is not enough, because it's a *disallowlist* strategy rather than a *allowlist* one.

This utility makes use the ``dropbox exclude`` command to implement the *allowlist* scheme. Actually it tries to exclude all the directories that should be excluded.

It's written in F# so we need the F# compiler ``fsharpc`` and the .Net runtime ``mono``.

Quickstart
==========

#. Clone this repo to a folder, say, ``~/dwlist``. Go into the directory.

#. Write a allowlist file and save it to ``~/dropbox.wlist`` using the following format::

    Files/Directory/A
    Files/Directory/B
    Public
    Photos/Alice
    Photos/Bob

#. Build it::

    make

#. Add the crontab entry::

    $ crontab -e

   Add the following line::

    * * * * * /path/to/dropbox_wlist_wrapper.sh

Assumptions
===========

We assume the Dropbox frontend script is ``~/bin/dropbox`` and the Dropbox folder is ``~/Dropbox``. If not, modify the corresponding values and **rebuild**.


