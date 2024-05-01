#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bot-PLayer-Tauz2.0/Bot PLayer-Tauz 2.0.csproj", "Bot-PLayer-Tauz2.0/"]
COPY ["WebHost.Extensions/WebHostExtensions.csproj", "WebHost.Extensions/"]
RUN dotnet restore "./Bot-PLayer-Tauz2.0/Bot PLayer-Tauz 2.0.csproj"
COPY . .
WORKDIR "/src/Bot-PLayer-Tauz2.0"
RUN dotnet build "./Bot PLayer-Tauz 2.0.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bot PLayer-Tauz 2.0.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot PLayer-Tauz 2.0.dll"]