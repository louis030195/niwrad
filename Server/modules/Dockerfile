FROM heroiclabs/nakama-pluginbuilder:2.12.0 AS builder

ENV GO111MODULE on
ENV CGO_ENABLED 1

WORKDIR /backend
COPY . .

# RUN go mod download
# RUN go mod vendor

RUN go build --trimpath --buildmode=plugin -o ./backend.so

FROM heroiclabs/nakama:2.12.0

COPY --from=builder /backend/backend.so /nakama/data/modules