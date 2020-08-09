FROM ubuntu:20.04

# TODO: actually should be a 2 stage dockerfile: builder unity artifact then runner
COPY Builds/Linux/Server /app

ENTRYPOINT ["/app/niwrad.x86_64"]
