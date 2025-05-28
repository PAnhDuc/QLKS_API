# --------- Build Stage ---------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj và restore các packages
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ project và build
COPY . ./
RUN dotnet publish -c Release -o out

# --------- Runtime Stage ---------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy từ build stage sang
COPY --from=build /app/out ./

# App listen trên port 5000 (http)
ENV ASPNETCORE_URLS=http://+:5000

# Mặc định khi container chạy
ENTRYPOINT ["dotnet", "QLKS_API.dll"]