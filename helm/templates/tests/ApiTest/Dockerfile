FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster

COPY Assets Assets
COPY Library Library
COPY helm helm

ENTRYPOINT [ "dotnet", "test", "helm/templates/tests/ApiTest/ApiTest.sln" ]
