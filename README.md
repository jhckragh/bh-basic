# bh-basic

A primitive BASIC-interpreter implemented in C# (for fun). The supported BASIC dialect is the one described by Brian Harvey in one of his Logo books (without the colons that enable compound commands):

https://people.eecs.berkeley.edu/%7Ebh/pdf/v2ch06.pdf

## Usage

```dotnet run --project Interpreter -- SOURCEFILE```

Some example programs can be found in the `Samples` directory, e.g.

```
$ dotnet run --project Interpreter -- Samples\fib.txt
0
1
1
2
3
5
8
13
21
...
```

## License

Public domain: http://creativecommons.org/publicdomain/zero/1.0/
