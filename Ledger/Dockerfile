FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Ledger/Ledger.csproj", "Ledger/"]
COPY ["Dashboard/Dashboard.csproj", "Dashboard/"]
COPY ["Database/Database.csproj", "Database/"]
RUN dotnet restore Ledger/Ledger.csproj
COPY Dashboard/ Dashboard/
COPY Database/ Database/
COPY Ledger/ Ledger/


WORKDIR "/src/Ledger"
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Ledger.dll"]

