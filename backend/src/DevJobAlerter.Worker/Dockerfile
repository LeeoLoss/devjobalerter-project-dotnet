# 1. Multi-stage build to restore, build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files (.csproj) to restore dependencies
COPY ["backend/src/DevJobAlerter.Worker/DevJobAlerter.Worker.csproj", "backend/src/DevJobAlerter.Worker/"]
COPY ["backend/src/DevJobAlerter.Application/DevJobAlerter.Application.csproj", "backend/src/DevJobAlerter.Application/"]
COPY ["backend/src/DevJobAlerter.Domain/DevJobAlerter.Domain.csproj", "backend/src/DevJobAlerter.Domain/"]
COPY ["backend/src/DevJobAlerter.Infrastructure/DevJobAlerter.Infrastructure.csproj", "backend/src/DevJobAlerter.Infrastructure/"]

# Restore dependencies for the Worker project
RUN dotnet restore "backend/src/DevJobAlerter.Worker/DevJobAlerter.Worker.csproj"

# Copy the rest of the solution source code and publish
COPY . .
WORKDIR "/src/backend/src/DevJobAlerter.Worker"
RUN dotnet publish "DevJobAlerter.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Final runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# Copy published binaries from the build stage
COPY --from=build /app/publish .

# Set entry point execution
ENTRYPOINT ["dotnet", "DevJobAlerter.Worker.dll"]