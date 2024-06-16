#!/bin/bash
while read story; do
	printf '{ "Url": "%s", "Passphrase": "%s" }' "$story" "$PASSWORD" \
	 | curl -X POST --header "Content-Type: application/json" -d @- "$INSTANCE/Api/StoryAsync"
done
