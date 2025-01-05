#!/bin/bash

# mitmproxy -p 13000

RESULT=`curl --get --data-urlencode 'url=https://www.scribblehub.com' http://localhost:12000/ClearForWebsite`
USERAGENT=`printf "%s" "$RESULT" | jq -r .userAgent`
COOKIEFILE=`mktemp`
printf "%s" "$RESULT" | jq -r .cookiesMozillaFormat > "$COOKIEFILE"
HTTPS_PROXY=http://localhost:13000 fanficfare -o use_ssl_unverified_context=true -o use_cloudscraper=false -o "user_agent=$USERAGENT" --mozilla-cookies="${COOKIEFILE}" "$@"

