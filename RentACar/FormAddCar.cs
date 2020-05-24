using MySql.Data.MySqlClient;
using RentACar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentACar
{
    public partial class FormAddCar : Form
    {

        //pole do przekazywania ID rekordu
        public int RowId = 0;

        public FormAddCar()
        {
            InitializeComponent();
        }

        private void FormAddCar_Load(object sender, EventArgs e)
        {
            LoadDictionaryData();
            numYear.Maximum = DateTime.Now.Year; //ograniczenie do biezacego roku
            if (RowId>0)
            {
                // do refaktoryzacji
                String sql = @"
                    SELECT c.*, m.brand_id
                    FROM cars c , car_models m  
                    WHERE c.id = {0}  AND c.model_id = m.id";
                sql = String.Format(sql, RowId);
                MySqlCommand cmd = new MySqlCommand(sql, GlobalData.connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    numEngine.Value = Convert.ToInt32( reader["engine"] );
                    numYear.Value = Convert.ToInt32(reader["manufacturer_year"]);
                    tbRegPlate.Text = reader["registration_plate"].ToString();
                    cbFuel.SelectedIndex = cbFuel.Items.IndexOf(reader["fuel"].ToString());

                    cbTypes.SelectedValue = reader["type_id"];
                    cbBrands.SelectedValue = reader["brand_id"];
                    cbModels.SelectedValue = reader["model_id"];
                    cbModels.Enabled = true;

                    if (!(reader["image"] is DBNull))
                    {
                        byte[] b = (byte[])reader["image"];
                        using(var ms = new MemoryStream(b))
                        {
                            picCar.Image = Image.FromStream(ms);
                        }
                    }

                    reader.Close();
                }

            }

            // tunning GUI
            if (RowId>0)
            {
                btnOK.Text = "Zapisz zmiany";
                this.Text = "Edycja pojazdu";
            } else
            {
                btnOK.Text = "Dodaj nowy";
                this.Text = "Nowy pojazd";
            }

        }

        BindingSource bsBrands = new BindingSource();
        BindingSource bsModels = new BindingSource();
        BindingSource bsTypes = new BindingSource();

        private void LoadDictionaryData()
        {
            try
            {
                // ładowanie słownika producentów
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                String sql = "select id, name from car_brands order by name";
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                bsBrands.DataSource = dt;
                cbBrands.DataSource = bsBrands;
                cbBrands.DisplayMember = "name";
                cbBrands.ValueMember = "id";
                cbBrands.SelectedIndex = -1;
                // podpinanie oblsugi zdarzenia
                cbBrands.SelectedIndexChanged += CbBrands_SelectedIndexChanged;

                // ładowanie słownika modeli
                adapter = new MySqlDataAdapter();
                sql = "select id, brand_id, name from car_models order BY brand_id asc, NAME asc";
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                dt = new DataTable();
                adapter.Fill(dt);

                bsModels.DataSource = dt;
                cbModels.DataSource = bsModels;
                cbModels.DisplayMember = "name";
                cbModels.ValueMember = "id";
                cbModels.SelectedIndex = -1;
                cbModels.Enabled = false;


                // ładowanie słownika typów własnosci
                adapter = new MySqlDataAdapter();
                sql = "select id, name from car_types order BY NAME asc";
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                dt = new DataTable();
                adapter.Fill(dt);

                bsTypes.DataSource = dt;
                cbTypes.DataSource = bsTypes;
                cbTypes.DisplayMember = "name";
                cbTypes.ValueMember = "id";
                cbTypes.SelectedIndex = -1;

            } catch (Exception exc)
            {
                DialogHelper.Error(exc.Message);
            }
        }

        private void CbBrands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBrands.SelectedIndex>-1)
            {
                bsModels.Filter = "brand_id = "+ cbBrands.SelectedValue;
                cbModels.DataSource = bsModels;
                cbModels.SelectedIndex = -1;
                cbModels.Enabled = true;

            }
        }

        private void tbRegPlate_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Char.ToUpper(e.KeyChar);
        }

        private void btnRemovePic_Click(object sender, EventArgs e)
        {
            if (picCar.Image != null)
            {
                picCar.Image.Dispose();
                picCar.Image = null; //dobra praktyka
                pictureFileName = null;
            }
        }

        private String pictureFileName = null;
        private void btnInsertPic_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Pliki graficzne|*.jpg;*.png;*.jpeg;*.gif;*.bmp";
            if (dialog.ShowDialog()==DialogResult.OK)
            {
                // ładujemy grafikę do komponentu
                picCar.Image = new Bitmap(dialog.FileName);
                pictureFileName = dialog.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                // zapis do bazy
                SaveData();
            }
        }

        private void SaveData()
        {
           try
            {
                String sql = "";
                if (RowId > 0)
                {
                    // update SQL
                    sql = @" UPDATE cars SET
                                model_id=@model_id, type_id=@type_id, registration_plate=@reg_plate,
                                engine=@engine, manufacturer_year=@year, fuel=@fuel, image=@image
                             WHERE
                                id = @row_id
                    ";
                }
                else
                {
                    sql = @"
                    INSERT INTO cars
                    (model_id, type_id, registration_plate, engine, manufacturer_year, 
                      image, fuel, avail )
                    VALUES
                    (@model_id, @type_id, @reg_plate, @engine, @year, @image, @fuel, 1)
                ";
                }
                MySqlCommand cmd = new MySqlCommand(sql, GlobalData.connection);
                cmd.Parameters.Add("@model_id", MySqlDbType.Int32);
                cmd.Parameters.Add("@type_id", MySqlDbType.Int32);
                cmd.Parameters.Add("@reg_plate", MySqlDbType.VarChar, 50);
                cmd.Parameters.Add("@engine", MySqlDbType.Int32);
                cmd.Parameters.Add("@year", MySqlDbType.Int32);
                cmd.Parameters.Add("@fuel", MySqlDbType.VarChar, 10);
                cmd.Parameters.Add("@image", MySqlDbType.MediumBlob);
                cmd.Parameters.Add("@row_id", MySqlDbType.Int32);

                cmd.Parameters["@model_id"].Value = cbModels.SelectedValue;
                cmd.Parameters["@type_id"].Value = cbTypes.SelectedValue;
                cmd.Parameters["@reg_plate"].Value = tbRegPlate.Text.Trim();
                cmd.Parameters["@year"].Value = numYear.Value;
                cmd.Parameters["@engine"].Value = numEngine.Value;
                cmd.Parameters["@fuel"].Value = cbFuel.SelectedItem;
                cmd.Parameters["@row_id"].Value = RowId;

                if (pictureFileName!=null)
                {
                    cmd.Parameters["@image"].Value = File.ReadAllBytes(pictureFileName);
                } else
                {
                    cmd.Parameters["@image"].Value = null;
                }
                cmd.ExecuteNonQuery();

                DialogResult = DialogResult.OK;
                Close();

            } catch (Exception exc)
            {
                DialogHelper.Error(exc.Message);
            }
        }

        private bool ValidateData()
        {
            if (cbBrands.SelectedIndex > -1 &&
                cbModels.SelectedIndex > -1 &&
                cbTypes.SelectedIndex > -1 &&
                cbFuel.SelectedIndex>-1 &&
                tbRegPlate.Text.Replace(" ","").Length>0 )
            {
                return true;
            } else
            {
                DialogHelper.Error("Sprawdź formularz");
                return false;
            }
        }
    }
}
