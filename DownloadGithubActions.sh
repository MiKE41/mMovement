#!/bin/sh
curl -O https://raw.githubusercontent.com/karashiiro/mMovement/master/.github/workflows/dotnet.yml
mkdir .github
mkdir .github/workflows
mv dotnet.yml .github/workflows