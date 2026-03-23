FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get update && apt-get install -y nodejs
WORKDIR /src
COPY ["Ledger/Ledger.csproj", "Ledger/"]
COPY ["Dashboard/Dashboard.csproj", "Dashboard/"]
COPY ["Database/Database.csproj", "Database/"]
RUN dotnet restore Dashboard/Dashboard.csproj
COPY Dashboard/package*.json Dashboard/
RUN cd Dashboard && npm install
COPY Dashboard/ Dashboard/
COPY Database/ Database/
COPY Ledger/ Ledger/
RUN cd Dashboard && npm run build:css

WORKDIR "/src/Dashboard"
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Dashboard.dll"]

