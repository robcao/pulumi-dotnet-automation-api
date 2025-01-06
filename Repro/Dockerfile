FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-noble AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build

ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1

WORKDIR /src
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ENV NUGET_CERT_REVOCATION_MODE=offline

COPY . /src

RUN dotnet restore "Repro.csproj"
RUN dotnet build "Repro.csproj" --configuration ${BUILD_CONFIGURATION} --output /app/build -p:Version=${VERSION}

FROM build AS publish

ARG BUILD_CONFIGURATION=Release
ARG PULUMI_VERSION=3.144.1
ARG PULUMI_LANGUAGE_DOTNET_VERSION=3.71.1
ARG VERSION=0.0.1
ARG RANDOM_PLUGIN=4.16.8

RUN dotnet publish "Repro.csproj" \
  --configuration ${BUILD_CONFIGURATION} \
  --output /app/publish \
  /p:UseAppHost=true \
  --self-contained \
  --os linux \
  --arch x64 \
  -p:Version=${VERSION}
RUN curl -fsSL https://get.pulumi.com/ | bash -s -- --version ${PULUMI_VERSION}

# By default, downloading pulumi comes with the core set of language plugins.
# These plugins are not the latest versions though, and lag behind for months.
# We want to be able to control the precise version of the .NET language plugin, so we specifically download the version we need, and swap out the version that comes with pulumi by default.
# The pulumi-language-dotnet plugin will be put into the /root/.pulumi/dotnet directory.
RUN curl -L -o pulumi-language-dotnet.tar.gz https://github.com/pulumi/pulumi-dotnet/releases/download/v${PULUMI_LANGUAGE_DOTNET_VERSION}/pulumi-language-dotnet-v${PULUMI_LANGUAGE_DOTNET_VERSION}-linux-amd64.tar.gz
RUN mkdir -p /root/.pulumi/dotnet
RUN tar -xzf pulumi-language-dotnet.tar.gz -C /root/.pulumi/dotnet
RUN rm pulumi-language-dotnet.tar.gz

RUN /root/.pulumi/bin/pulumi plugin install resource random ${RANDOM_PLUGIN} --exact

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /root/.pulumi/bin/pulumi /pulumi/bin/pulumi
COPY --from=publish /root/.pulumi/dotnet/*-dotnet* /pulumi/bin/
COPY --from=publish /root/.pulumi/plugins /root/.pulumi/plugins
ENV PATH="/pulumi/bin:${PATH}"

ENTRYPOINT ["./Repro"]
