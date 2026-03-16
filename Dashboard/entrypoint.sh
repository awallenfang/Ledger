#!/bin/sh
cd /src/Dashboard
npm install
npm run watch:css &
dotnet watch run --project .