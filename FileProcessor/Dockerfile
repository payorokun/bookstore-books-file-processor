# Use the official .NET image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image for building the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FileProcessor.csproj", "FileProcessor/"]
RUN dotnet restore "FileProcessor.csproj"
COPY . .
WORKDIR "/src/FileProcessor"
RUN dotnet build "FileProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileProcessor.csproj" -c Release -o /app/publish

# Use the runtime image for the final container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileProcessor.dll"]
