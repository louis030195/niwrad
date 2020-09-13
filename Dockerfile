FROM ubuntu:20.04

# TODO: actually should be a 2 stage dockerfile: builder unity artifact then runner
COPY executor /app

ENTRYPOINT ["/app/StandaloneLinux64/StandaloneLinux64"]
