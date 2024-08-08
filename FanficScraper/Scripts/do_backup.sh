#!/bin/bash
set -euxo pipefail

KEY=`age-keygen -y "$KEYFILE_PATH"`
curl -sSL "$FANFICSCRAPER_ADDRESS/Api/Backup?key=$KEY&includeEpubs=$INCLUDE_EPUBS" | age --decrypt -i "$KEYFILE_PATH" > "$OUTPUT_FILE"
