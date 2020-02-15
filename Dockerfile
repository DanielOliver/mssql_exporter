FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY src/core/*.csproj ./core/
COPY src/server/*.csproj ./server/
WORKDIR /app/core
RUN dotnet restore
WORKDIR /app/server
RUN dotnet restore

# copy and build app and libraries
WORKDIR /app
COPY src/core/. ./core/
COPY src/server/. ./server/
COPY metrics.json ./
WORKDIR /app/server
RUN dotnet publish -c Release -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS runtime
EXPOSE 80
WORKDIR /app
RUN apk add --no-cache icu-libs
COPY --from=build /app/server/out ./
ENTRYPOINT ["./mssql_exporter", "serve"]