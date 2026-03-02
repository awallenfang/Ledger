FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Botty/Botty.csproj", "Botty/"]
COPY ["Database/Database.csproj", "Database/"]
COPY ["Fluxer.Net/Fluxer.Net/Fluxer.Net.csproj", "Fluxer.Net/"]
RUN dotnet restore Botty/Botty.csproj
COPY Botty/ Botty/
COPY Database/ Database/
COPY Fluxer.Net/Fluxer.Net/ Fluxer.Net


WORKDIR "/src/Botty"
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Botty.dll"]

