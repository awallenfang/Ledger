FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Botty/Botty.csproj", "Botty/"]
COPY ["Dashboard/Dashboard.csproj", "Dashboard/"]
COPY ["Database/Database.csproj", "Database/"]
RUN dotnet restore Dashboard/Dashboard.csproj
COPY Dashboard/ Dashboard/
COPY Database/ Database/
COPY Botty/ Botty/
COPY Fluxify/ Fluxify/


WORKDIR "/src/Dashboard"
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Dashboard.dll"]

