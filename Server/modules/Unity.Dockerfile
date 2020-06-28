FROM ubuntu:20.04

COPY Server /app

# TODO: args env var thing, see https://docs.docker.com/develop/develop-images/dockerfile_best-practices/#entrypoint
ENTRYPOINT [ "./niwrad.x86_64" ]
CMD ["--help"]