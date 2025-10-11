FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Back.csproj", "."]
RUN dotnet restore "Back.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "Back.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Back.dll"]