using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace windbg_test
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var collection = new ObservableCollection<Person>();
            for (int i = 0; i < 8; i++)
                collection.Add(new Person { Name = $"name_{i}", Age = 7 + i });

            Console.Read();
        }
    }


    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
