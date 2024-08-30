#!/bin/bash
scp -r dist hs1b:/home/steam/fivem/server-data/resources/jackz-builder-js/ && \
rcon-cli hs1b:30120 "goLZ6gsFpbgD84463XL@" -c "refresh; restart jackz-builder-js"