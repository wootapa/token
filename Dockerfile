FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
WORKDIR /src
COPY . .
RUN dotnet publish -p:PublishSingleFile=true -r linux-musl-x64 --self-contained true -p:PublishTrimmed=True -p:TrimMode=Link -c Release -o /src/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine
COPY --from=build-env /src/publish/ /app
WORKDIR /app
ENTRYPOINT ["./token"]