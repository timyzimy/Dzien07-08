using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataBindExample
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        List<Person> persons = new List<Person>();
        private void FormMain_Load(object sender, EventArgs e)
        {
            persons.Add(new Person("Jan", "Kowalski", 55, "Zdun"));
            persons.Add(new Person("Marek", "Nowak", 35, "Social media ninja"));
            persons.Add(new Person("Zenek", "Martyniuk", 52, "Piosenkarz"));
            persons.Add(new Person("Emil", "Zatopek", 79, "Biegacz"));

            //foreach (Person item in persons)
            //{
            //    lbPersons.Items.Add(item.Fname + " " + item.Lname);
            //}
            //lbPersons.SelectedIndex = -1;

            //Data Binding - listbox
            lbPersons.DataSource = persons;
            lbPersons.DisplayMember = "FullName";

            //Data Binding - textbox
            tbFname.DataBindings.Add(new Binding("Text", persons, "Fname"));
            tbLname.DataBindings.Add(new Binding("Text", persons, "Lname"));
            tbAge.DataBindings.Add(new Binding("Text", persons, "Age"));
            tbJob.DataBindings.Add(new Binding("Text", persons, "Job"));

        }

        private void lbPersons_SelectedIndexChanged(object sender, EventArgs e)
        {
            //int index = lbPersons.SelectedIndex;
            //if (index >= 0)
            //{
            //    Person person = persons[index];
            //    tbFname.Text = person.Fname;
            //    tbLname.Text = person.Lname;
            //    tbAge.Text = person.Age.ToString();
            //    tbJob.Text = person.Job;
            //}
        }
    }
}
