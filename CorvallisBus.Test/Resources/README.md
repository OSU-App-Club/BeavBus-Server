# Protobuf Tests

Tests in the `GtfsRealtime` directory rely on Protobuf files in this directory to exist.

## Editing Existing Files

Use the [`protoc`](https://protobuf.dev/installation/) compiler to extract/decode the existing test input. The header file is located in
`CorvallisBus.Core/Generated/GtfsRealtime.proto`.

For example, to decode the `ServiceAlert.pb` test file:
```bash
$ pwd # CorvallisBus.Test/Resources
$ protoc --decode transit_realtime.FeedMessage --proto_path=$(pwd)/../.. $(pwd)/../../CorvallisBus.Core/Generated/GtfsRealtime.proto < ServiceAlert.pb > ServiceAlert_Decomp.txt
```

This will produce a file named `ServiceAlert_Decomp.txt` containing the full contents, in Protobuf form, of that message.

### Compiling Changes

Again using `protoc`, simply encode by swapping inputs/outputs and using `--encode` instead of `--decode`

For example, to encode the `ServiceAlert.pb` test file:
```bash
$ pwd # CorvallisBus.Test/Resources
$ protoc --encode transit_realtime.FeedMessage --proto_path=$(pwd)/../.. $(pwd)/../../CorvallisBus.Core/Generated/GtfsRealtime.proto < ServiceAlert_Decomp.txt > ServiceAlert.pb 
```

## Test Expectations

These files should try and resemble actual [`Corvallis Transit System`](https://corvallistransit.com) Gtfs realtime messages as much as possible. As such, tests are designed to check deserialization of a CTS message, not a full GtfsRealtime message with all values. If CTS updates their Gtfs Realtime messaging, these tests should fail, as it could be indicitive of a future issue within the server itself.

As such, it should be expected that these files are routinely compared and updated to new data from CTS, and that all GtfsRealtime deserialization and testing unit tests pass as expected.

## Security

Protobuf files are binary files, and are not easily understood for changes. As a way to assert security, the decompiled protobuf must be present in the repository. This has the added benefit of providing better understanding of test cases.