FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-arm32v7 AS build-env
WORKDIR /app
#Restore dependencies separately so you don't have to re-grab them each time.
COPY *.csproj ./
RUN dotnet restore
#Grab the rest of the project files.
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim-arm32v7
WORKDIR /app
COPY --from=build-env /app/out .
CMD ["dotnet", "SpeedDatingBot.dll"]