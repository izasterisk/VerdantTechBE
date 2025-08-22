# Multi-stage build for optimized production deployment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files in dependency order for better caching
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Controller/Controller.csproj", "Controller/"]

# Restore dependencies
RUN dotnet restore "Controller/Controller.csproj"

# Copy all source code
COPY . .

# Build and publish the application
WORKDIR "/src/Controller"
RUN dotnet publish "Controller.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Expose port (Render will use the PORT environment variable)
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "Controller.dll"]