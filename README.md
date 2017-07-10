FlatMapper
==========

[![Build status](https://ci.appveyor.com/api/projects/status/lrh3rpq62w6ljef1?svg=true)](https://ci.appveyor.com/project/kappy/flatmapper)
[![NuGet Version](http://img.shields.io/nuget/v/Flatmapper.svg?style=flat)](https://www.nuget.org/packages/Flatmapper/) 
[![NuGet Downloads](http://img.shields.io/nuget/dt/Flatmapper.svg?style=flat)](https://www.nuget.org/packages/Flatmapper/)

FlatMapper is a library to import and export data from and to plain text files in a Linq compatible way.


## Features

+ Fast - Only uses Static Reflection and Dynamic methods
+ LINQ Compatible
+ It supports character delimited and fixed length files
+ Non intrusive - You don't have to change your code. Any POCO will work
+ No external Dependencies
+ Iterative reads - Doesn't need to load the entire file into memory
+ Multi-line support (Only on character delimited and quoted)
+ Nullables support
+ Vitually any type support with FieldValueConverters
+ Fluent Interface
+ Per line/record Error handling
+ Simple to use


## How to use

### Fixed Length Layout

    var layout = new Layout<TestObject>.FixedLengthLayout()
					.HeaderLines(1)
					.WithMember(o => o.Id, set => set.WithLength(5).WithLeftPadding('0'))
					.WithMember(o => o.Description, set => set.WithLength(25).WithRightPadding(' '))
					.WithMember(o => o.NullableInt, set => set.WithLength(5).AllowNull("=Null").WithLeftPadding('0'))
                    .WithMember(o => o.NullableEnum, set => set.WithLength(10).AllowNull("======NULL").WithLeftPadding(' '))
                    .WithMember(o => o.Date, set => set.WithLength(19).WithFormat(new CultureInfo("pt-PT"))); //PT-pt default dates are always fixed 19 chars "13-12-2015 23:41:41"
    

### Delimited Layout

    var layout = new Layout<TestObject>.DelimitedLayout()
		            .WithDelimiter(";")
		            .WithQuote("\"")
					.HeaderLines(1)
		            .WithMember(o => o.Id, set => set.WithLength(5).WithLeftPadding('0'))
		            .WithMember(o => o.Description, set => set.WithLength(25).WithRightPadding(' '))
		            .WithMember(o => o.NullableInt, set => set.WithLength(5).AllowNull("=Null").WithLeftPadding('0'))
                    .WithMember(o => o.NullableEnum, set => set.WithLength(10).AllowNull("").WithLeftPadding(' '))
                    .WithMember(o => o.Date, set => set.WithFormat(new CultureInfo("pt-PT")));

### Reading and Writing

    //Reading data
    using (var fileStream = File.OpenRead("c:\temp\data.txt"))
    {
        var flatfile = new FlatFile<TestObject>(layout, fileStream);
        foreach(var objectInstance in flatfile.Read())
        {
            //Do Somethig....
        }
    }
    
    //Writing data
    using (var fileStream = File.OpenWrite("c:\temp\data.txt"))
    {
        var flatfile = new FlatFile<TestObject>(layout, fileStream)
        flatfile.Write(listOfObjects);
    }


For more detailed information please check the [wiki](https://github.com/kappy/FlatMapper/wiki)
