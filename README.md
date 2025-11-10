# DataDrivenConstants

If you have ever worked on a project that stores a lot of configuration data, you've likely felt the pain of
redefining constants to access data in those files from your code. DataDrivenConstants fixes this by automatically
generating constant classes from your data. Simply create a (usually empty) static partial class with a marker
attribute to define the data you want and the constants will be added to that class.

DataDrivenConstants currently only supports JSON files, but additional formats may be supported in the future.

## Adding data files

Data files are accessed by the source generator as `AdditionalFiles` items in your csproj, for example:

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources/foo.json"/>
  <AdditionalFiles Include="Resources/Nested/bar.json"/>
</ItemGroup>
```

If you want to send all items of a specific type (e.g. embedded resources) to the analyzer, you can also
set the `AdditionalFileItemNames` property, like so:

```xml
<PropertyGroup>
  <AdditionalFileItemNames>EmbeddedResource</AdditionalFileItemNames>
</PropertyGroup>
```

All data types supported by DataDrivenConstants use [globs](https://en.wikipedia.org/wiki/Glob_(programming))
to select which files will contain the data. In the simplest class, the glob `**/*` will match all files, but
you can of course be more selective if you want to split your data across multiple files. It is highly
recommended that you start all your globs with `**`, `AdditionalFiles` are typically converted to absolute paths
before the source generator processes them.

## JSON

JSON files can be accessed using the `DataDrivenConstants.Marker.JsonData` attribute, for example:

```csharp
using DataDrivenConstants.Marker;

// gets the "name" property of all objects in the root list
[JsonData("$[*].name", "**/listOfObjects.json")]
public static partial class MyConstants {}
```

The query syntax used is a slight extension of [JSONPath](https://goessner.net/articles/JsonPath/) which supports
a `~` operator at the end of a query to get the keys of the selected node. This can be used to get keys of an object;
while `$.*` will give you all *values* of an object, `$.*~` will give the *keys* of the object.
[This website](https://jsonpath.com/) can be used to test your JSONPath; the `~` operator is supported in the JSONPath Plus`
parser (note that it also supports other features which are not supported here).
