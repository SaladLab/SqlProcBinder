[![Build status](https://ci.appveyor.com/api/projects/status/8k2s8e9h86q4q6vt?svg=true)](https://ci.appveyor.com/project/veblush/sqlprocbinder)
 [![NuGet Status](http://img.shields.io/nuget/v/SqlProcBinder.svg?style=flat)](https://www.nuget.org/packages/SqlProcBinder/)

# SqlProcBinder

Calling stored procedure from C# is quite verbose with ADO.NET.
This library lets you call stored procedure like calling a general C# method.

Here is a stored procedure `Sum` that program need to call.
```sql
CREATE PROCEDURE [dbo].[Sum] (@a int, @b int, @ans int OUTPUT) AS BEGIN
    SET @ans = @a + @b
END
```

SqlProcBinder.CodeGenerator reads stored procedures and generates stub classes like:
```csharp
public class Sum {
 public static async Task<Result> ExecuteAsync(IDbContext dc, int a, int b) {
   var ctx = dc.CreateCommand();
   ...
 }
}
```

With this generated class, it becomes easy to call stored procedures from C#.

```csharp
var r = await Sum.ExecuteAsync(dc, 1, 2);
Console.WriteLine(r.ans); // 3
```

## Where can I get it?

```
PM> Install-Package SqlProcBinder
PM> Install-Package SqlProcBinder.CodeGenerator
```

## How to use

#### Generate stub class

The first thing to do is generating stub code. Stored procedure files are required in this process.
For example, Sum.sql is provided, following command generates Sum.cs.
(CodeGenerator.exe might be found at Packages/SqlProcBinder.CodeGenerator.0.3.1/tools directory.)

```
CodeGenerator.exe -s Sum.sql -t Sum.cs
```

When there're plenty of SQL files or elaborate control is required, external option
files can be used like:

```
CodeGenerator.exe -i SqlProc.json -t Sum.cs
```

The example of external option is [here](https://github.com/SaladLab/SqlProcBinder/blob/master/core/CodeGenerator.Tests/Sql/%40SqlProc.json).
Also you can see generated class [here](https://github.com/SaladLab/SqlProcBinder/blob/master/core/CodeGenerator.Tests/Properties/SqlProc.CodeGen.cs) by this option.

#### Using stub class

Let generated Sum.Sql file to be included in your project.
And make sure that SqlProcBinder package is also installed.

```csharp
var r = await Sum.ExecuteAsync(dc, 1, 2);
Console.WriteLine(r.ans); // 3
```

`dc` is an instance of `IDbContext` type. It is simply ok with creating `SimpleDbContext` and passing it.
But if you want to do something before and after calling stored procedure, it is better to
write own class implementing `IDbContext`.

## Features

#### Output parameter

Output parameter can be used. Following procedure specifies `@ans` parameter as
an output one.

```sql
CREATE PROCEDURE [dbo].[Sum] (@a int, @b int, @ans int OUTPUT) AS BEGIN
    SET @ans = @a + @b
END
```

Matched C# feature is `out` but C# async function doesn't allow out parameter so
return variable is used to fetch the output values.

```csharp
public class Sum {
    public struct Result {
        public int AffectedRowCount;
        public int ans;
    }
    public static async Task<Result> ExecuteAsync(IDbContext dc, int a, int b)  {
       ...

var r = await Sum.ExecuteAsync(dc, 1, 2);
Console.WriteLine(r.ans); // 3
```

#### Return value

Return value of stored procedure can be understood as a special output value.
So you can use it like output parameter.

```sql
CREATE PROCEDURE [dbo].[Sum] (@a int, @b int) AS BEGIN
    RETURN @ans = @a + @b
END
```

```csharp
public class Sum {
    public struct Result {
        public int AffectedRowCount;
        public int Return;
    }
    public static async Task<Result> ExecuteAsync(IDbContext dc, int a, int b)
        ...

var r = await SumAndReturn.ExecuteAsync(dc, 1, 2);
Console.WriteLine(ret.Return); // 3
```

#### Nullable

By default, all paramters are non-nullable because null is not easy to tame.
When you don't pass output value in stored procedures,
zero value of T would be fetched for that variable.

But when `Nullable` property is set in Procs, null value can be used.
```json
{ "Path": "Sum.sql", "Nullable": true }
```

Generated class would be like following and null will be around the corner.
```csharp
public class Sum {
    public struct Result {
        public int AffectedRowCount;
        public int? answer;
    }
    public static async Task<Result> ExecuteAsync(IDbContext dc, int? a, int? b)
        ...

var r = await SumAndReturn.ExecuteAsync(dc, 1, null);
Console.WriteLine(ret.Return); // null
```

#### RaiseError

SQL has `RAISERROR` statement for raising error like `throw` of C#.
When error is raised in stored procedure, this error can be propagated into calling
C# method.

```sql
CREATE PROCEDURE [dbo].[Error] @msg as nvarchar(100) AS BEGIN
    RAISERROR (@message, 16, 1)
END
```

Raised error is propagated as a SqlException.
```csharp
try {
    await Error.ExecuteAsync(dc, "Test");
}
catch (SqlException ex) {
    Console.WriteLine(ex.Message); // "Test"
}
```

#### Return Rowset

Following stored procedure returns rowset [1..@count] values.
```sql
CREATE PROCEDURE [dbo].[GenerateInt] (@count as int) AS BEGIN
    SELECT TOP (@count) n = ROW_NUMBER() OVER (ORDER BY number)
    FROM [master]..spt_values ORDER BY n
END
```

To receive rowset, `Rowset` property should be set in Procs.
```json
{ "Path": "GenerateInt.sql", "Rowset": "DbDataReader" },
```

With generated class, you can fetch rowset via Result.Rowset variable.
And don't forget that Rowset instance should be disposed.
```csharp
var ret = await GenerateInt.ExecuteAsync(dc, 10);
using (ret.Rowset)
{
    while (await ret.Rowset.ReadAsync())
        Console.WriteLine(ret.Rowset.GetInt32(0)); // 1 2 3 ... 10
}
```

#### Typed Rowset

DbDataReader doesn't provide compile time type safety.
To handle this limitation typed rowset is provided.

For receiving a typed rowset, `DrInt` is configured in Procs and Rowsets.
`Fields` property defines fields of Rowset and format mimicks C# struct member declaration.

```json
{
  "Procs": [ { "Path": "GenerateInt.sql", "Rowset": "DrInt" } ],
  "Rowsets": [ { "Name": "DrInt", "Fields": ["int Value" ] } ]
}
```

With typed rowset, you can use it as `List<Row>`.
```
var ret = await GenerateInt.ExecuteAsync(dc, 10);
foreach (var row in await ret.Rowset.FetchAllRowsAndDisposeAsync())
    Console.WriteLine(row.Value); // 1 2 3 ... 10
```

#### Return Rowset as List\<Row\>

If you want to use typed rowset more simply, `RowsetFetch` can be an option.
```json
{ "Path": "GenerateInt.sql", "Rowset": "DbDataReader", "RowsetFetch": true }
```

Returned `Rows` is List<Row>.
```csharp
var ret = await GenerateInt.ExecuteAsync(dc, 10);
foreach (var row in ret.Rows) // Rows is type of List<Row>
    Console.WriteLine(row.Value); // 1 2 3 ... 10
}
```

#### User Table Type

MSSQL provides a way passing table data to stored procedures.
```sql
CREATE TYPE [dbo].[Vector3List] AS TABLE(
    [X] [float] NOT NULL,
    [Y] [float] NOT NULL,
    [Z] [float] NOT NULL
)

CREATE PROCEDURE [dbo].[Vector3ListSum]
    @values Vector3List READONLY,
    @ans float OUTPUT
AS
BEGIN
    SELECT @ans = SUM(X) + SUM(Y) + SUM(Z) FROM @values
END
```

Instance of `DataTable` is used for passing data. For type-safety, table-type
class can be generated.
```json
"TableTypes": [ { "Path": "Vector3List.sql" } ]
```

Previous option generates `Vector3List` class.
```csharp
public class Vector3List {
    public DataTable Table { get; set; }
    public void Add(double X, double Y, double Z) { ... }
}
```

With generated `Vector3List`, table data can be passed to stored procedures safely.
```csharp
var list = new Sql.Vector3List();
list.Add(1, 2, 3);
list.Add(4, 5, 6);

var ret = await Vector3ListSum.ExecuteAsync(_db.DbContext, list.Table);
Console.WriteLine(ret.ans); // 21
```
