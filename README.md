# nsysmon

The .NET based Network Monitoring System

## Architecture

The NSysmon.Core library is the home of the `PollEngine` and designed around
the concept of a `PollNode` and a `DataCache`.

A PollNode is an abstract class which should represent
a thing like servers, database instances, services, or applications you wish
to monitor.

Each instance of a PollNode will have a collection of DataCaches.
A DataCache encapsulate two concepts:

1. A cached data object. This object represents a point-in-time snapshot of
   the database polled by this DataCache for a particular PollNode instance.
2. A cache update function. This function is called when the PollEngine whenever
   the cached data object needs to be updated or refreshed.

The data is meant to be used as an active point-in-time representation of your
systems to give your Ops team a consolidated overview all servers, databases,
and applications.

## Design Roadmap

The design is still in the very early prototype phase. Some of our ideas include:

* Expose an HTTP endpoint which can display simple debug information;
  such as the status the PollEngine, each PollNode and each DataCache.
* Persist DataCache objects to a database to record time-series data and
  produce trend graphs.
* API to expose the data to front-end dashboard and admin control panels.

## Credits

Many aspects of the architecture, and several code files are derived
from the wonderful [Opserver](https://github.com/opserver/Opserver) project developed by Stack Exchange Inc.

* Most code is derived from this commit: [a170ea8bcda9](https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b)
* Their [MIT License](https://github.com/opserver/Opserver/blob/a170ea8bcda9f9e52d4aaff7339f3d198309369b/license.txt), Copyright (c) 2013 Stack Exchange Inc.