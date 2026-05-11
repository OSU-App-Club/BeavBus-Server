FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /CorvallisBus.Web

COPY . ./
RUN dotnet restore
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /CorvallisBus.Web
COPY --from=build /CorvallisBus.Web/out .
ENTRYPOINT ["dotnet", "CorvallisBus.Web.dll"]