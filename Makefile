build:
	docker build -t kraki .

osx:
	dotnet publish -c release -r osx-x64
