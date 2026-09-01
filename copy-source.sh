#!/bin/sh
set -e

if [ "$#" -ne 1 ]; then
    echo "usage: copy-source.sh <destination>" >&2
    exit 1
fi

destination="$1"

case "$destination" in
    -*)
        destination="./$destination"
        ;;
esac

root="$(cd "$(dirname "$0")" && pwd)"

if [ -f "$destination" ]; then
    echo "destination is an existing file: $destination" >&2
    exit 1
fi

mkdir -p "$destination"

for file in "$root"/src/Moquestra.CodeWriter/*.cs; do
    cp "$file" "$destination/"
done
