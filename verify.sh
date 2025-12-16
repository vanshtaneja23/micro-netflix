#!/bin/bash

# Colors
GREEN='\033[0;32m'
NC='\033[0m'

echo -e "${GREEN}Starting Verification...${NC}"

# 1. Create a dummy video file
echo "Creating dummy video file..."
echo "dummy content" > test_video.mp4

# 2. Upload Video
echo "Uploading video..."
RESPONSE=$(curl -s -X POST http://localhost:5000/videos/upload \
  -H "Content-Type: multipart/form-data" \
  -F "file=@test_video.mp4")

echo "Upload Response: $RESPONSE"

# Extract ID (simple grep, assuming JSON response)
ID=$(echo $RESPONSE | grep -o '"id":"[^"]*"' | cut -d'"' -f4)

if [ -z "$ID" ]; then
    echo "Failed to get Video ID"
    exit 1
fi

echo -e "${GREEN}Video ID: $ID${NC}"

# 3. Poll Status
echo "Polling status..."
for i in {1..5}; do
    STATUS_RESPONSE=$(curl -s http://localhost:5000/videos/$ID)
    echo "Status: $STATUS_RESPONSE"
    sleep 2
done

echo -e "${GREEN}Verification Complete!${NC}"
