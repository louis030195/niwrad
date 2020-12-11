FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster
# TODO: alpine/slim
# Dependencies
COPY Assets Assets
COPY Library Library
# Tests
COPY helm helm

ENTRYPOINT [ "dotnet", "test", "helm/templates/tests/ApiTest/ApiTest.sln" ]
