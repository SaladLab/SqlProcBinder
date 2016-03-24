[![Build status](https://ci.appveyor.com/api/projects/status/8k2s8e9h86q4q6vt?svg=true)](https://ci.appveyor.com/project/veblush/sqlprocbinder)

# SqlProcBinder

Calling stored procedure from C# is quite verbose with ADO.NET.
This library let you to call stored procedure like calling general C# method.

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
Console.WriteLine(r.ans); // = 3
```

## Where can I get it?

```
PM> Install-Package SqlProcBinder
PM> Install-Package SqlProcBinder.CodeGenerator
```

## How to use

#### Generate stub class

First thing to do is generating stub code. Stored procedure files are required in this process.
For example, Sum.sql is provided, following command generates Sum.cs.
(CodeGenerator.exe might be found at Packages/SqlProcBinder.CodeGenerator.0.3.1/tools directory.)

```
CodeGenerator.exe -s Sum.sql -t Sum.cs
```

When there're plenty of sql file or elaborate control is required, external option
file can be used like:

```
CodeGenerator.exe -i SqlProc.json -t Sum.cs
```

The example of external option is [here](https://github.com/SaladLab/SqlProcBinder/blob/master/core/CodeGenerator.Tests/Sql/%40SqlProc.json).
Also you can see generated class [here](https://github.com/SaladLab/SqlProcBinder/blob/master/core/CodeGenerator.Tests/Properties/SqlProc.CodeGen.cs) by this option.

#### Using stub class

Let generated Sum.Sql to be included in your project.
And make sure that SqlProcBinder package is also installed.

```csharp
var r = await Sum.ExecuteAsync(dc, 1, 2);
Console.WriteLine(r.ans); // = 3
```

`dc` is an instance of `IDbContext` type. It is simply ok with creating `SimpleDbContext` and passing it.
But if you want to do something before and after calling stored procedure, it is better to
write own class implementing `IDbContext`.

## Features

#### Parameter

Null value is not supported. For removing complexity to deal with null value,
every value is initialized with zero value. Output parameter is supported.

#### Return Value

```sql
CREATE PROCEDURE [dbo].[Sum] (@a int, @b int) AS BEGIN
    RETURN @ans = @a + @b
END
```

```csharp
var r = await SumAndReturn.ExecuteAsync(dc, 1, 2);
Console.WriteLine(ret.Return); // 3
```

#### Return Rowset

```sql
CREATE PROCEDURE [dbo].[GenerateInt] (@count as int) AS BEGIN
    SELECT TOP (@count) n = ROW_NUMBER() OVER (ORDER BY number)
    FROM [master]..spt_values ORDER BY n
END
```

```csharp
var ret = await GenerateInt.ExecuteAsync(dc, 10);
var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
Console.WriteLine(values); // 1 2 3 ... 9 10
```

#### RaiseError

```sql
RAISERROR ("ErrorMessage", 16, 1)
```

Raised exception can be transfered to .NET as `SqlException`.
