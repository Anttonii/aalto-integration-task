## Aalto Product Aggregation

This repository is my implementation of a product aggregation task. The console application targets .NET 6.0 and uses C# language version 9.0.

### Installation & Usage

To run the app, clone this repository and run the following commands in the root directory:

```bash
$ dotnet build
$ dotnet run
```

By default the program will send a request to the hardcoded address `https://fakestoreapi.com/products`.

Exemplary program output is saved to the file `output.json`, running the program will generate an identical .json file named `grouped_products.json`.

### License

This repository is under the MIT license.