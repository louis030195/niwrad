FROM heroiclabs/nakama-pluginbuilder:2.12.0 AS builder

ENV GO111MODULE on
ENV CGO_ENABLED 1

WORKDIR /backend

# Get the Unity server artifact
COPY ../../Builds/Linux/Server Server
COPY . .
# This is ugly as hell ?
RUN cp -r realtime /usr/local/go/src
RUN cp -r rpc /usr/local/go/src

#RUN go mod download
# RUN go mod vendor

RUN go build --trimpath --buildmode=plugin -o ./backend.so

FROM heroiclabs/nakama:2.12.0

COPY --from=builder /backend/Server .
COPY --from=builder /backend/*.so /nakama/data/modules
COPY --from=builder /backend/*.lua /nakama/data/modules