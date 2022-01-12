# Kraki

Kraki is a lightweight command line tool for advanced configuration checks
on [KrakenD](https://github.com/devopsfaith/krakend-ce) config files. Currently,
Kraki supports:

- Linting of endpoint definitions
- Listing of endpoints by endpoint keys

## Build Kraki

The easiest way to build Kraki is using Docker:

    docker build -t kraki .

Then run Kraki with the `--help` flag to see a list of supported commands:

    docker run kraki --help

## List Endpoints

To see all endpoints that are defined in a KrakenD config file, mount the
file into the Kraki Docker container and run Kraki with the `list` command.
Assuming there is a KrakenD config file called `krakend.json` in the current
working directory, run:

    docker run -v "${PWD}:/tmp" kraki list /tmp/krakend.json

By default, endpoints are grouped by the `endpoint` key. To group
endpoints by another key, use the `--groupby` flag. For example, to group
endpoints by method:

    docker run -v "${PWD}:/tmp" kraki list --groupby method /tmp/krakend.json

Note that `--groupby` requires that the specified key is defined for all
endpoints, and that the associated value is a string.

## Linting

Kraki can lint KrakenD config files against a set of linting rules that are
specified in a `kraki.json` file. For example, the following `kraki.json`
configuration creates a linting rule that ensures that all endpoint definitions
include a key `owner` of type `string`, and that endpoints are sorted
in ascending order by `endpoint` and `method`:

```json
{
  "lint": {
    "endpoints": {
      "require": {
        "owner": "a string"
      },
      "sort": {
        "by": ["endpoint", "method"]
      }
    }
  }
}
```

Assuming that `kraki.json` and `krakend.json` are located in the current
working directory, the linting rules can be checked with:

    docker run -v "${PWD}:/tmp" kraki lint --config /tmp/kraki.json /tmp/krakend.json

## Advanced Linting Config

In the above example, `a string` is a sample value for `owner`, not a
type specification. More complex sample values can be provided if required.
For example, the following config ensures that `owner` is of type `string`, and
`tags` is an array of `string`:

```json
{
  "lint": {
    "endpoints": {
      "require": {
        "owner": "a string",
        "tags": ["a", "list", "of", "string", "tags"],
      }
    }
  }
}
```
