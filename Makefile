build:
	docker build -t kraki .

lint:
	@docker run -v "${PWD}/test:/tmp" kraki lint -c /tmp/kraki.json /tmp/krakend.json

list:
	@docker run -v "${PWD}/test:/tmp" kraki list /tmp/krakend.json

test: build lint list

osx:
	dotnet publish -c release -r osx-x64
