# 1. Doing a multi-stage build to create a smaller image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de projeto (.csproj) para restaurar as dependências
COPY ["src/DevJobAlerter.Worker/DevJobAlerter.Worker.csproj", "src/DevJobAlerter.Worker/"]
COPY ["src/DevJobAlerter.Application/DevJobAlerter.Application.csproj", "src/DevJobAlerter.Application/"]
COPY ["src/DevJobAlerter.Domain/DevJobAlerter.Domain.csproj", "src/DevJobAlerter.Domain/"]
COPY ["src/DevJobAlerter.Infrastructure/DevJobAlerter.Infrastructure.csproj", "src/DevJobAlerter.Infrastructure/"]

RUN dotnet restore "src/DevJobAlerter.Worker/DevJobAlerter.Worker.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/DevJobAlerter.Worker"
RUN dotnet publish "DevJobAlerter.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Final stage/image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# Copy the output from the build stage
COPY --from=build /app/publish .

# Define the entry point
ENTRYPOINT ["dotnet", "DevJobAlerter.Worker.dll"]   