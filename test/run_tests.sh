#!/bin/bash

# Variables
WEBHOOK_URL=$1
ROLE_ID="1174437656688607353"
COMMAND="bru run ./ -r --env Server"
CURRENT_DATE=$(TZ=Etc/GMT-2 date +"%Y-%m-%d %H:%M:%S")
TEMP_FILE="./bruno_output.txt"

# Reset database
curl -X POST http://172.21.40.127:12038/debug/recreate-database -o /dev/null -s -w "%{http_code}\n"

# Fetch the output from the command
$COMMAND > $TEMP_FILE

MSG="<@&$ROLE_ID>\n## Tests performed on __ $CURRENT_DATE __ $2 "
CONTENT=$(tail -n 3 $TEMP_FILE | head -n 1)

# Create the JSON payloadi
PAYLOAD=$(cat <<EOF
{
  "content": "$MSG",
  "embeds": [
    {
      "title": "Postman Collection Run Results",
      "description": "$CONTENT",
      "color": 15924992
    }
  ]
}
EOF
)

# Send the payload to the Discord webhook
#'{"content":"'$TEST'","embeds":[{"title":"Postman Collection Run Results","description":$CONTENT,"color": 15924992}]}'

# Send the payload to the Discord webhook
curl -H "Content-Type: application/json" -d "$PAYLOAD" "$WEBHOOK_URL"

curl -X POST \
        -H "Content-Type: multipart/form-data" \
        -F "file=@${TEMP_FILE}" \
        "$WEBHOOK_URL"
