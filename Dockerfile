FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.fsproj .
RUN dotnet restore

# set version
ARG VERSION
RUN echo "<Project><PropertyGroup><Version>${VERSION}</Version></PropertyGroup></Project>" > Directory.Build.props

# copy everything
COPY . .

# run tests
RUN dotnet test

# publish app and libraries
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "kraki.dll"]
