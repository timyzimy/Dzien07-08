using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBindExample
{
    class Person
    {
        private string fname;
        private string lname;
        private int age;
        private string job;

        public Person(string fname, string lname, int age, string job)
        {
            this.fname = fname; this.lname = lname; this.age = age; this.job = job;
        }

        public string Fname { get { return fname; } }
        public string Lname { get { return lname; } }
        public int Age { get { return age; } }
        public string Job { get { return job; } }

        public String FullName { get { return fname + " " + lname;  } }

    }
}
