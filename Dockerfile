#Get .Net Core image
FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /app

#Copying csproj and restore
COPY *.csproj ./
RUN dotnet restore

#Copying the rest of the files and build
COPY . ./
RUN dotnet publish -c Release -o out

#Runtime
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT [ "dotnet" , "LibraryNetCoreAPI.dll" ]