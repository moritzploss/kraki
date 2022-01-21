VERSION := $(shell git describe --abbrev=0 | tr -d 'v')

build:
	@docker build -t kraki --build-arg VERSION=$(VERSION) .

lint:
	@docker run -v "${PWD}/examples:/tmp" kraki lint -c /tmp/kraki.json /tmp/krakend.json

list:
	@docker run -v "${PWD}/examples:/tmp" kraki list /tmp/krakend.json

test:
	dotnet test

osx:
	dotnet publish -c release -r osx-x64
