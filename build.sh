#!/bin/bash

VERSION="1.0"
OUTPUT_DIR="./publish"

echo "Cleaning previous builds..."
rm -rf $OUTPUT_DIR

platforms=("win-x64" "linux-x64" "osx-x64" "osx-arm64")

for rid in "${platforms[@]}"
do
    echo "Building for $rid..."
    dotnet publish -c Release -f net8.0 --sc -r $rid -o "$OUTPUT_DIR/$rid" --no-self-contained false
done

echo -e "\nBuild complete! Check the $OUTPUT_DIR folder."