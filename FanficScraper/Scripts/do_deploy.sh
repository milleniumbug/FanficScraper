#!/bin/bash
set -euxo pipefail

cd ..
rm -r "$LOCALTARGET" || true
dotnet publish -c Release -r "$ARCH" --self-contained true -o "${LOCALTARGET}"
dotnet ef migrations bundle --self-contained -r "$ARCH" -f -o "${LOCALTARGET}/efbundle"
rm "${LOCALTARGET}/appsettings"* || true
rsync -ru --delete -e "$REMOTESSH" "${LOCALTARGET}"/ "$TARGETSSH":"$REMOTEDROP"
$REMOTESSH "$TARGETSSH" "REMOTEDB=\"$REMOTEDB\" REMOTETARGET=\"$REMOTETARGET\" REMOTEDROP=\"$REMOTEDROP\" CONNECTIONSTRING=\"$CONNECTIONSTRING\"" 'bash' <<'ENDSSH'
set -euxo pipefail
sudo service fanficscraper stop
sqlite3 "$REMOTEDB" "$(printf '.backup %s/app.db.bak' ~ )"
sudo cp -r "$REMOTEDROP"/* "$REMOTETARGET"
cd "$REMOTETARGET"
sudo ./efbundle --connection "$CONNECTIONSTRING"
sudo service fanficscraper start
ENDSSH
