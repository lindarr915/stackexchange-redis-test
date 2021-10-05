FROM mcr.microsoft.com/dotnet/runtime:5.0-focal AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /src
COPY ["RedisDotnetSample.csproj", "./"]
RUN dotnet restore "RedisDotnetSample.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "RedisDotnetSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedisDotnetSample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedisDotnetSample.dll"]
