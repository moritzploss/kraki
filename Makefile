build:
	docker build -t kraki .

test: build
	@docker run -v "${PWD}/test:/tmp" kraki lint -c /tmp/kraki.json /tmp/krakend.json;
	@docker run -v "${PWD}/test:/tmp" kraki list /tmp/krakend.json;

osx:
	dotnet publish -c release -r osx-x64
