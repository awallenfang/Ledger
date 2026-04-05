set -o allexport; source .env; set +o allexport

~/.dotnet/tools/dotnet-ef migrations add updateRankForGlobalSettings2 --project Database