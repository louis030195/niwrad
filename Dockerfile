FROM ubuntu:20.04

# TODO: actually should be a 2 stage dockerfile: builder unity artifact then runner
COPY Builds/Linux/Server /app

CMD ["/app/niwrad.x86_64", "--nakamaIp", "127.0.0.1", "--nakamaPort", "7350"]
