FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/StrEnum.Dapper/StrEnum.Dapper.csproj ./src/StrEnum.Dapper/StrEnum.Dapper.csproj
COPY test/StrEnum.Dapper.UnitTests/StrEnum.Dapper.UnitTests.csproj ./test/StrEnum.Dapper.UnitTests/StrEnum.Dapper.UnitTests.csproj
RUN dotnet restore

# copy everything else and build app
COPY ./ ./
WORKDIR /source
RUN dotnet build -c release -o /out/package --no-restore /p:maxcpucount=1

FROM build as test
RUN dotnet test /p:maxcpucount=1

FROM build as pack-and-push
WORKDIR /source

ARG PackageVersion
ARG NuGetApiKey

RUN dotnet pack ./src/StrEnum.Dapper/StrEnum.Dapper.csproj -o /out/package -c Release
RUN dotnet nuget push /out/package/StrEnum.Dapper.$PackageVersion.nupkg -k $NuGetApiKey -s https://api.nuget.org/v3/index.json