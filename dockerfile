#
# Stage 1, Prebuild
#
FROM microsoft/aspnetcore-build:1.1.2 as builder

# Install runtime dependencies
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates bzip2 libfontconfig \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*
 
 # Install official PhantomJS release
RUN set -x  \
 && apt-get update \
 && apt-get install -y --no-install-recommends \
 && mkdir /srv/var \
 && mkdir /tmp/phantomjs \
   # Download Phantom JS
 && curl -L https://bitbucket.org/ariya/phantomjs/downloads/phantomjs-2.1.1-linux-x86_64.tar.bz2 | tar -xj --strip-components=1 -C /tmp/phantomjs \
   # Copy binaries only
 && mv /tmp/phantomjs/bin/phantomjs /usr/local/bin \
   # Create symbol link
   # Clean up
 && apt-get autoremove -y \
 && apt-get clean all \
 && rm -rf /tmp/* /var/lib/apt/lists/*

RUN phantomjs --version

COPY src/Squidex/package.json /tmp/package.json
RUN cd /tmp \
&& npm install \
&& npm rebuild node-sass

COPY . .

WORKDIR /

# Build Frontend
RUN cp -a /tmp/node_modules /src/Squidex/ \
 && cd /src/Squidex \
 && npm run test:coverage \
 && npm run build:copy \
 && npm run build
 
# Test Backend
RUN dotnet restore \
 && dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj \ 
 && dotnet test tests/Squidex.Core.Tests/Squidex.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Read.Tests/Squidex.Read.Tests.csproj \
 && dotnet test tests/Squidex.Write.Tests/Squidex.Write.Tests.csproj

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/ --configuration Release

#
# Stage 2, Build runtime
#
FROM microsoft/aspnetcore:1.1.2

# Default AspNet Core Workdir
WORKDIR /app

# Copy from Build Stage
COPY --from=builder /out/ .

ENTRYPOINT ["dotnet", "Squidex.dll"]