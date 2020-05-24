using MySql.Data.MySqlClient;
using RentACar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentACar
{
    public partial class FormCarList : Form
    {
        public FormCarList()
        {
            InitializeComponent();
        }

        private void FormCarList_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        BindingSource bSource = new BindingSource();

        private void LoadData()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            String sql = @"SELECT c.id, b.name AS brand, m.name AS model, t.name AS car_type,  
                          c.registration_plate, 
                          c.engine, c.manufacturer_year, c.avail, c.fuel
                          FROM cars AS c , car_types AS t, car_models AS m, car_brands AS b
                        WHERE
                          c.type_id = t.id AND c.model_id = m.id AND m.brand_id = b.id";
            adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // przypisanie danych uzyskanych z MarianDB do grida
            bSource.DataSource = dt;
            grid.DataSource = bSource;

            // customizacja kolumn
            grid.Columns["id"].HeaderText = "ID";
            grid.Columns["brand"].HeaderText = "Marka";
            grid.Columns["model"].HeaderText = "Model";
            grid.Columns["car_type"].HeaderText = "Właśność";
            grid.Columns["registration_plate"].HeaderText = "Nr rejestracyjny";
            
            grid.Columns["engine"].HeaderText = "Silnik [cm3]";
            grid.Columns["engine"].DefaultCellStyle.Alignment = 
                DataGridViewContentAlignment.MiddleRight;

            grid.Columns["manufacturer_year"].HeaderText = "Rok produkcji";
            grid.Columns["manufacturer_year"].DefaultCellStyle.Alignment = 
                DataGridViewContentAlignment.MiddleRight;

            grid.Columns["avail"].HeaderText = "Dostępny";
            grid.Columns["avail"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            grid.Columns["fuel"].HeaderText = "Paliwo";

        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == grid.Columns["avail"].Index)
            {
                e.Value = Convert.ToInt32(e.Value) == 1 ? "TAK" : "NIE";
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count==0)
            {
                return;
            }
            DialogResult res=  MessageBox.Show("Czy na pewno usunąć rekord?", "Pytanie",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res!=DialogResult.Yes)
            {
                return;
            }

            String sql = "DELETE FROM cars WHERE id = @rowId";

            using (MySqlCommand deleteCommand = new MySqlCommand(sql, GlobalData.connection))
            {
                //pobranie wartości ID rekordu z grid'a
                int selectedIndex = grid.SelectedRows[0].Index;
                int rowId = Convert.ToInt32( grid["id", selectedIndex].Value );

                // wykonanie zapytania na bazy
                deleteCommand.Parameters.Add("@rowId", MySqlDbType.Int32).Value = rowId;
                deleteCommand.ExecuteNonQuery();

                //update danych w datagridview
                grid.Rows.RemoveAt(selectedIndex);


            }


        }

        private void tsbInsert_Click(object sender, EventArgs e)
        {
            AddNewCar();
        }

        private void AddNewCar()
        {
            FormAddCar form = new FormAddCar();
            if (form.ShowDialog()==DialogResult.OK)
            {
                RefreshData();
            }
        }

        // odświeżanie danych
        private void RefreshData()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            String sql = @"SELECT c.id, b.name AS brand, m.name AS model, t.name AS car_type,  
                          c.registration_plate, 
                          c.engine, c.manufacturer_year, c.avail, c.fuel
                          FROM cars AS c , car_types AS t, car_models AS m, car_brands AS b
                        WHERE
                          c.type_id = t.id AND c.model_id = m.id AND m.brand_id = b.id";
            adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // przypisanie danych uzyskanych z MarianDB do grida
            bSource.DataSource = dt;
            grid.DataSource = bSource;
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void mnuRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void mnuEditCar_Click(object sender, EventArgs e)
        {
            //edycja pojazdu
            if (grid.SelectedRows.Count==0)
            {
                return;
            }

            int selectedIndex = grid.SelectedRows[0].Index;
            int rowId = Convert.ToInt32(grid["id", selectedIndex].Value);

            FormAddCar form = new FormAddCar();
            form.RowId = rowId;
            if (form.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (grid.SelectedRows.Count>0)
            {
                int selectedIndex = grid.SelectedRows[0].Index;
                int avail = Convert.ToInt16(grid["avail", selectedIndex].Value);
                mnuCarOper.Text = (avail == 1) ? "Wydaj" : "Zdaj";
            }
        }

        private void mnuCarOper_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0)
            {
                int selectedIndex = grid.SelectedRows[0].Index;
                int avail = Convert.ToInt16(grid["avail", selectedIndex].Value);

                FormOperation form = new FormOperation();
                form.RegPlate = grid["registration_plate", selectedIndex].Value.ToString();
                form.CarId = Convert.ToInt32(grid["id", selectedIndex].Value);
                form.OperBack = (avail == 0);

                if (form.ShowDialog()==DialogResult.OK)
                {
                    RefreshData();
                }

            }
        }
    }
}
