set -o allexport; source .env; set +o allexport

~/.dotnet/tools/dotnet-ef migrations add updateVCSession2 --project Database