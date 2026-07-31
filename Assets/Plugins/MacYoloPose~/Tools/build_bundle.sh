#!/bin/bash
set -euo pipefail

script_dir="$(cd "$(dirname "$0")" && pwd)"
source_dir="$(cd "$script_dir/../Source" && pwd)"
project_root="$(cd "$script_dir/../../../.." && pwd)"
bundle_dir="$project_root/Assets/Plugins/macOS/MacYoloPose.bundle"
model_dir="$bundle_dir/Contents/Resources/YoloPose.mlmodelc"
binary="$bundle_dir/Contents/MacOS/MacYoloPose"
build_dir="$(mktemp -d /tmp/mac-yolo-bundle.XXXXXX)"
arm64_binary="$build_dir/MacYoloPose.arm64"
x64_binary="$build_dir/MacYoloPose.x86_64"

if [[ ! -d "$model_dir" ]]; then
    echo "Missing compiled Core ML model: $model_dir" >&2
    exit 1
fi

mkdir -p "$(dirname "$binary")"
for arch in arm64 x86_64; do
    output="$build_dir/MacYoloPose.$arch"
    xcrun clang++ -arch "$arch" -std=c++17 -fobjc-arc -dynamiclib \
        -framework CoreML -framework Foundation \
        -Wl,-install_name,@rpath/MacYoloPose \
        -mmacosx-version-min=12.0 \
        "$source_dir/MacYoloPose.mm" \
        -o "$output"
done

lipo -create "$arm64_binary" "$x64_binary" -output "$binary"

lipo -archs "$binary"
otool -L "$binary"
