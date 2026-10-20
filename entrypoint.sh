#!/bin/bash
set -e
echo "Iniciando VetSync API..."
exec dotnet VetSync.Api.dll
