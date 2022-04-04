FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
WORKDIR /src
COPY *.csproj .
RUN dotnet restore -r linux-musl-x64 /p:PublishReadyToRun=true
COPY . .
RUN dotnet publish \
    -c Release \
    -o /src/publish \
    -r linux-musl-x64 \
    --self-contained true \
    --no-restore \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=True \
    /p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine
COPY --from=build-env /src/publish /app
WORKDIR /app
ENTRYPOINT ["./token"]