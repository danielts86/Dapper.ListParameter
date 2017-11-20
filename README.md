Dapper.ListParameter
-------------
**Project Purpose:**
Extend Dapper Dynamic Parameter to add parameter list for sql table parameter.

----------

### For Example
Suppose we need to send a list of clients to the procedure for updating the data.
The DataBase table looks like this:
```sql
CREATE TYPE [dbo].[IntList] AS TABLE(
	[IntValue] [int] NOT NULL
);

CREATE TABLE Customers(
	[FirstName] [varchar(50)] NOT NULL,
	[LastName] [varchar(50)] NOT NULL
);
```
And we have a User-Defined Table Type of: `[dbo].[tvpCustomers]` that is defined as such:
```sql

CREATE TYPE [dbo].[tvpCustomers] AS TABLE(
	[FirstName] [varchar(50)] NOT NULL,
	[LastName] [varchar(50)] NOT NULL
)
```
The C# class that will defined the same way as our type:
```csharp

class Customer
{
    public string FirstName { get; set; }

    public string LastName { get; set; }
}
```


> **Note:**

> - Class name and properties names do not need to be the same.


#### Use AddList
```csharp

var customers = new List<Customer>
{
    new Customer
    {
        FirstName = "Customer1FirstName",
        LastName = "Customer1LaseName"
    },
    new Customer
    {
        FirstName = "Customer2FirstName",
        LastName = "Customer2LaseName"
    }
};
var parameters = new DynamicParameters();
parameters.AddList("@Customers", "[dbo].[tvpCustomers]", customers, new[] { "FirstName", "LastName" });

connection.Execute("some_stored_procedure", parameters, CommandType.StoredProcedure);
```
