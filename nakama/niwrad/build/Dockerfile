FROM heroiclabs/nakama-pluginbuilder:2.12.0 AS builder

ENV GO111MODULE on
ENV CGO_ENABLED 1

WORKDIR /backend

COPY . .

RUN go build --trimpath --buildmode=plugin -o ./backend.so ./cmd/main

FROM heroiclabs/nakama:2.12.0
COPY --from=builder /backend/build/nakama-config.yml /nakama
COPY --from=builder /backend/*.so /nakama/data/modules
COPY --from=builder /backend/lua/*.lua /nakama/data/modules
