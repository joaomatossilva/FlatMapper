using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FlatMapper;

namespace ImportFootprint
{
    public class Importer
    {
        private readonly Layout<DataItem> layout;
        private const string file = "testData.txt";

        public Importer()
        {
            layout = new Layout<DataItem>.DelimitedLayout()
                .WithQuote("\"")
                .WithDelimiter(";")
                .WithMultiLine(false)
                .WithMember(x => x.Id)
                .WithMember(x => x.Name)
                .WithMember(x => x.Date)
                .WithMember(x => x.DayOfWeek)
                .WithMember(x => x.Description);
        }

        public void Import()
        {
            using (var fileStream = File.OpenRead(file))
            {
                var flatFile = new FlatFile<DataItem>(layout, fileStream);
                var items = flatFile.Read();
                using (var enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                    }
                }

            }
        }

        public void GenerateFile()
        {
            if (File.Exists(file))
            {
                return;
            }

            using (var fileStream = File.OpenWrite(file))
            {
                var flatFile = new FlatFile<DataItem>(layout, fileStream);
                var items = GenerateItems(5_000_000);
                flatFile.Write(items);
            }

            IEnumerable<DataItem> GenerateItems(int count)
            {
                var baseDate = DateTime.MinValue;
                var borderLine = DateTime.MaxValue.AddDays(-2);
                for (int i = 0; i < count; i++)
                {
                    yield return new DataItem
                    {
                        Id = i,
                        Name = "Item" + i,
                        Date = baseDate,
                        DayOfWeek = baseDate.DayOfWeek,
                        Description = $"Item {i} {baseDate.DayOfWeek}"
                    };
                    if (baseDate.Date > borderLine)
                    {
                        baseDate = DateTime.MinValue;
                    }
                    baseDate = baseDate.AddDays(1);
                }
            }
        }
    }
}
