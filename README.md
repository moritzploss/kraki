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

## Lint Config Files

Kraki can lint KrakenD config files against a set of linting rules that are
specified in a `kraki.json` file. In particular, endpoint definitions can be
validated against a [JSON Schema](https://json-schema.org/) specification.
For example, the following `kraki.json` configuration creates a linting rule
that ensures that all endpoint definitions include a key `owner` of type
`string`, and that endpoints are sorted in ascending order by `endpoint` and
`method`:

```json
{
  "lint": {
    "endpoints": {
      "schema": {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "title": "Endpoint",
        "type": "object",
        "properties": {
           "owner": { "type": "string" },
        },
        "required": ["owner"]
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
endpoints.
