set -o allexport; source .env; set +o allexport

~/.dotnet/tools/dotnet-ef migrations add updateGuildUserSettings2 --project Database