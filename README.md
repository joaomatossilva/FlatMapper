FlatMapper
==========

[![Build status](https://ci.appveyor.com/api/projects/status/lrh3rpq62w6ljef1?svg=true)](https://ci.appveyor.com/project/kappy/flatmapper)
[![NuGet Version](http://img.shields.io/nuget/v/Flatmapper.svg?style=flat)](https://www.nuget.org/packages/Flatmapper/) 
[![NuGet Downloads](http://img.shields.io/nuget/dt/Flatmapper.svg?style=flat)](https://www.nuget.org/packages/Flatmapper/)

FlatMapper is a library to import and export data from and to plain text files.


## Features

+ It supports character delimited and fixed lenght files
+ Non intrusive - You don't have to chnage your code
+ Simple to use


## How to use

### Fixed Lenght Layout

    var layout = new Layout<TestObject>.FixedLengthLayout()
					.HeaderLines(1)
					.WithMember(o => o.Id, set => set.WithLenght(5).WithLeftPadding('0'))
					.WithMember(o => o.Description, set => set.WithLenght(25).WithRightPadding(' '))
					.WithMember(o => o.NullableInt, set => set.WithLenght(5).AllowNull("=Null").WithLeftPadding('0'));
    

### Delimited Layout

    var layout = new Layout<TestObject>.DelimitedLayout()
		            .WithDelimiter(";")
		            .WithQuote("\"")
					.HeaderLines(1)
		            .WithMember(o => o.Id, set => set.WithLenght(5).WithLeftPadding('0'))
		            .WithMember(o => o.Description, set => set.WithLenght(25).WithRightPadding(' '))
		            .WithMember(o => o.NullableInt, set => set.WithLenght(5).AllowNull("=Null").WithLeftPadding('0'));

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

